// Assets/Scripts/Gameplay/Visuals/AdvancedEmotionPhysics.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EmotionSystem))]
public class AdvancedEmotionPhysics : MonoBehaviour
{
    [System.Serializable]
    public class PhysicsStrand
    {
        public Transform root;
        public Transform tip;
        public float width = 0.1f;
        public int resolution = 3;
        [HideInInspector] public List<Transform> joints = new List<Transform>();
        [HideInInspector] public List<Vector3> velocities = new List<Vector3>();
    }

    [Header("Hair Physics")]
    [SerializeField] private PhysicsStrand[] _hairStrands;
    [SerializeField] private float _stiffness = 50f;
    [SerializeField] private float _damping = 5f;
    [SerializeField] private float _collisionRadius = 0.2f;

    [Header("Emotion Effects")]
    [SerializeField] private ParticleSystem _steamParticles;
    [SerializeField] private ParticleSystem _waterDroplets;
    [SerializeField] private float _steamRateAngry = 50f;
    [SerializeField] private float _dripRateSad = 10f;

    [Header("Material Effects")]
    [SerializeField] private Renderer _targetRenderer;
    [SerializeField] private string _wetnessProperty = "_Wetness";
    [SerializeField] private string _shineProperty = "_Shine";
    [SerializeField] private float _maxWetness = 0.8f;
    [SerializeField] private float _maxShine = 2f;

    // Private state
    private EmotionSystem _emotionSystem;
    private MaterialPropertyBlock _propBlock;
    private float _currentWetness;
    private float _currentShine;
    private List<SphereCollider> _colliders = new List<SphereCollider>();

    void Awake()
    {
        _emotionSystem = GetComponent<EmotionSystem>();
        _propBlock = new MaterialPropertyBlock();
       
        InitializeHairStrands();
        CreateCollisionSystem();
    }

    void OnEnable()
    {
        StartCoroutine(UpdateHairPhysics());
        StartCoroutine(UpdateEmotionEffects());
    }

    #region Initialization
    private void InitializeHairStrands()
    {
        foreach (var strand in _hairStrands)
        {
            strand.joints.Clear();
            strand.velocities.Clear();

            if (strand.root != null && strand.tip != null && strand.resolution > 1)
            {
                for (int i = 0; i < strand.resolution; i++)
                {
                    float t = i / (float)(strand.resolution - 1);
                    Vector3 pos = Vector3.Lerp(strand.root.position, strand.tip.position, t);
                    var joint = new GameObject($"Joint_{i}").transform;
                    joint.position = pos;
                    joint.parent = strand.root;
                    strand.joints.Add(joint);
                    strand.velocities.Add(Vector3.zero);
                }
            }
        }
    }

    private void CreateCollisionSystem()
    {
        foreach (var strand in _hairStrands)
        {
            foreach (var joint in strand.joints)
            {
                var collider = joint.gameObject.AddComponent<SphereCollider>();
                collider.radius = _collisionRadius;
                collider.isTrigger = true;
                _colliders.Add(collider);
            }
        }
    }
    #endregion

    #region Core Physics
    private IEnumerator UpdateHairPhysics()
    {
        while (true)
        {
            float deltaTime = Time.deltaTime;
            float emotionIntensity = _emotionSystem.CurrentState.Intensity;
            EmotionType emotion = _emotionSystem.CurrentState.CurrentEmotion;

            // Apply emotion-based modifiers
            float stiffness = _stiffness * GetStiffnessModifier(emotion);
            float damping = _damping * GetDampingModifier(emotion);

            foreach (var strand in _hairStrands)
            {
                for (int i = 0; i < strand.joints.Count; i++)
                {
                    Transform joint = strand.joints[i];
                    Vector3 targetPos = GetTargetPosition(strand, i);
                    Vector3 force = (targetPos - joint.position) * stiffness;
                    force -= strand.velocities[i] * damping;

                    // Add emotional forces
                    force += GetEmotionForce(emotion, emotionIntensity) * 0.1f;

                    // Update velocity and position
                    strand.velocities[i] += force * deltaTime;
                    joint.position += strand.velocities[i] * deltaTime;

                    // Hair-to-hair collision
                    HandleCollisions(strand, i);
                }
            }
            yield return null;
        }
    }

    private Vector3 GetTargetPosition(PhysicsStrand strand, int index)
    {
        if (index == 0) return strand.root.position;

        Transform prevJoint = strand.joints[index - 1];
        Transform currentJoint = strand.joints[index];
        float segmentLength = Vector3.Distance(strand.root.position, strand.tip.position) / (strand.joints.Count - 1);
        return prevJoint.position + (currentJoint.position - prevJoint.position).normalized * segmentLength;
    }

    private void HandleCollisions(PhysicsStrand currentStrand, int currentIndex)
    {
        Transform currentJoint = currentStrand.joints[currentIndex];
        Vector3 totalRepulsion = Vector3.zero;
        int collisionCount = 0;

        foreach (var strand in _hairStrands)
        {
            for (int i = 0; i < strand.joints.Count; i++)
            {
                if (strand == currentStrand && i == currentIndex) continue;

                Transform otherJoint = strand.joints[i];
                float distance = Vector3.Distance(currentJoint.position, otherJoint.position);
                if (distance < _collisionRadius * 2f)
                {
                    Vector3 dir = (currentJoint.position - otherJoint.position).normalized;
                    float pushForce = (_collisionRadius * 2f - distance) * 0.5f;
                    totalRepulsion += dir * pushForce;
                    collisionCount++;
                }
            }
        }

        if (collisionCount > 0)
        {
            currentJoint.position += totalRepulsion / collisionCount;
        }
    }
    #endregion

    #region Emotion Effects
    private IEnumerator UpdateEmotionEffects()
    {
        while (true)
        {
            EmotionType emotion = _emotionSystem.CurrentState.CurrentEmotion;
            float intensity = _emotionSystem.CurrentState.Intensity;

            // Steam when angry
            if (emotion == EmotionType.Angry && _steamParticles != null)
            {
                var emission = _steamParticles.emission;
                emission.rateOverTime = intensity * _steamRateAngry;
               
                if (intensity > 0.7f && !_steamParticles.isPlaying)
                    _steamParticles.Play();
                else if (intensity < 0.3f && _steamParticles.isPlaying)
                    _steamParticles.Stop();
            }

            // Drips when sad
            if (emotion == EmotionType.Sad && _waterDroplets != null)
            {
                var emission = _waterDroplets.emission;
                emission.rateOverTime = intensity * _dripRateSad;
            }

            // Material effects
            UpdateMaterialEffects(emotion, intensity);

            yield return new WaitForSeconds(0.1f); // Optimized update rate
        }
    }

    private void UpdateMaterialEffects(EmotionType emotion, float intensity)
    {
        if (_targetRenderer == null) return;

        // Wetness increases when sad
        float targetWetness = emotion == EmotionType.Sad ? intensity * _maxWetness : 0f;
        _currentWetness = Mathf.Lerp(_currentWetness, targetWetness, Time.deltaTime * 2f);

        // Shine increases when happy
        float targetShine = emotion == EmotionType.Happy ? intensity * _maxShine : 1f;
        _currentShine = Mathf.Lerp(_currentShine, targetShine, Time.deltaTime * 3f);

        _targetRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(_wetnessProperty, _currentWetness);
        _propBlock.SetFloat(_shineProperty, _currentShine);
        _targetRenderer.SetPropertyBlock(_propBlock);
    }
    #endregion

    #region Helper Methods
    private float GetStiffnessModifier(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Angry => 0.8f, // Looser when angry
            EmotionType.Sad => 1.2f,   // Stiffer when sad
            _ => 1f
        };
    }

    private float GetDampingModifier(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => 0.7f, // More bouncy when happy
            EmotionType.Surprised => 0.5f,
            _ => 1f
        };
    }

    private Vector3 GetEmotionForce(EmotionType emotion, float intensity)
    {
        return emotion switch
        {
            EmotionType.Happy => Vector3.up * intensity * 2f,
            EmotionType.Angry => new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0f, 0.5f),
                Random.Range(-1f, 1f)
            ) * intensity * 3f,
            EmotionType.Sad => Vector3.down * intensity * 0.5f,
            _ => Vector3.zero
        };
    }
    #endregion

    #region Editor Debugging
    private void OnDrawGizmosSelected()
    {
        if (_hairStrands == null) return;

        Gizmos.color = Color.magenta;
        foreach (var strand in _hairStrands)
        {
            if (strand.root != null && strand.tip != null)
            {
                for (int i = 0; i < strand.joints.Count - 1; i++)
                {
                    if (strand.joints[i] != null && strand.joints[i + 1] != null)
                    {
                        Gizmos.DrawLine(strand.joints[i].position, strand.joints[i + 1].position);
                        Gizmos.DrawWireSphere(strand.joints[i].position, _collisionRadius);
                    }
                }
            }
        }
    }
    #endregion
}
