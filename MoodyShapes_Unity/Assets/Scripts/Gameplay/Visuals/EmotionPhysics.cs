using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EmotionSystem))]
public class EmotionPhysics : MonoBehaviour
{
    [System.Serializable]
    public class PhysicsAccessory
    {
        public Transform target;
        public Vector3 basePosition;
        public Quaternion baseRotation;
        public float springStrength = 50f;
        public float damping = 5f;
        public float mass = 0.1f;
        public Vector3 emotionResponseAxis = Vector3.up;
       
        [Header("Emotion Multipliers")]
        public float happyMultiplier = 1.5f;
        public float angryMultiplier = 2f;
        public float sadMultiplier = 0.3f;
       
        // Runtime physics
        [HideInInspector] public Vector3 velocity;
        [HideInInspector] public Vector3 angularVelocity;
    }

    [Header("Configuration")]
    [SerializeField] private PhysicsAccessory[] _accessories;
    [SerializeField] private float _gravity = 2f;
    [SerializeField] private bool _enableWind = true;
    [SerializeField] private float _windStrength = 0.5f;
    [SerializeField] private float _windChangeInterval = 3f;

    // Emotion state
    private EmotionSystem _emotionSystem;
    private Vector3 _windDirection;
    private float _nextWindChange;

    void Awake()
    {
        _emotionSystem = GetComponent<EmotionSystem>();
       
        // Initialize accessories
        foreach (var accessory in _accessories)
        {
            if (accessory.target != null)
            {
                accessory.basePosition = accessory.target.localPosition;
                accessory.baseRotation = accessory.target.localRotation;
            }
        }
    }

    void OnEnable()
    {
        StartCoroutine(WindChangeRoutine());
    }    void Update()
    {
        if (!EnableAccessories || _accessories == null) return;

        float emotionMultiplier = GetCurrentEmotionMultiplier();
        Vector3 emotionalForce = GetEmotionalForce(emotionMultiplier);

        foreach (var accessory in _accessories)
        {
            if (accessory.target == null) continue;

            // Calculate forces
            Vector3 position = accessory.target.localPosition;
            Quaternion rotation = accessory.target.localRotation;

            // Spring force (return to base)
            Vector3 positionForce = (accessory.basePosition - position) * accessory.springStrength;
            Quaternion rotationForce = Quaternion.FromToRotation(rotation * Vector3.forward, accessory.baseRotation * Vector3.forward);

            // Damping
            positionForce -= accessory.velocity * accessory.damping;
            Vector3 angularForce = new Vector3(
                rotationForce.eulerAngles.x > 180 ? rotationForce.eulerAngles.x - 360 : rotationForce.eulerAngles.x,
                rotationForce.eulerAngles.y > 180 ? rotationForce.eulerAngles.y - 360 : rotationForce.eulerAngles.y,
                rotationForce.eulerAngles.z > 180 ? rotationForce.eulerAngles.z - 360 : rotationForce.eulerAngles.z
            ) * accessory.springStrength;
            angularForce -= accessory.angularVelocity * accessory.damping;

            // Apply emotion-based force
            positionForce += emotionalForce * emotionMultiplier * accessory.emotionResponseAxis.magnitude;

            // Apply wind if enabled
            if (_enableWind)
            {
                positionForce += _windDirection * _windStrength;
            }

            // Apply gravity
            positionForce += Vector3.down * _gravity;

            // Update physics
            accessory.velocity += positionForce * Time.deltaTime / accessory.mass;
            accessory.angularVelocity += angularForce * Time.deltaTime / accessory.mass;

            // Apply movement
            accessory.target.localPosition += accessory.velocity * Time.deltaTime;
            accessory.target.localRotation = Quaternion.Euler(
                accessory.target.localRotation.eulerAngles + accessory.angularVelocity * Time.deltaTime
            );
        }
    }

    private float GetCurrentEmotionMultiplier()
    {
        if (_emotionSystem == null) return 1f;

        return _emotionSystem.CurrentState.CurrentEmotion switch
        {
            EmotionType.Happy => _accessories.Length > 0 ? _accessories[0].happyMultiplier : 1f,
            EmotionType.Angry => _accessories.Length > 0 ? _accessories[0].angryMultiplier : 1f,
            EmotionType.Sad => _accessories.Length > 0 ? _accessories[0].sadMultiplier : 1f,
            _ => 1f
        };
    }

    private Vector3 GetEmotionalForce(float multiplier)
    {
        if (_emotionSystem == null) return Vector3.zero;

        float intensity = _emotionSystem.CurrentState.Intensity;
       
        return _emotionSystem.CurrentState.CurrentEmotion switch
        {
            EmotionType.Happy => Vector3.up * intensity * 2f * multiplier,
            EmotionType.Angry => new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0f, 1f),
                Random.Range(-1f, 1f)
            ) * intensity * multiplier,
            EmotionType.Surprised => Vector3.back * intensity * multiplier,
            _ => Vector3.zero
        };
    }

    private IEnumerator WindChangeRoutine()
    {
        while (_enableWind)
        {
            _windDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.2f, 0.2f),
                Random.Range(-1f, 1f)
            ).normalized;

            _nextWindChange = Time.time + _windChangeInterval;
            yield return new WaitForSeconds(_windChangeInterval);
        }
    }

    // Public properties for testing integration
    public bool IsActive => _emotionSystem != null && _emotionSystem.CurrentState.Intensity > 0.1f;
    public bool EnableAccessories { get; set; } = true;
    public int AccessoryCount => _accessories?.Length ?? 0;
    
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_enableWind)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + _windDirection * 2f);
            Gizmos.DrawSphere(transform.position + _windDirection * 2f, 0.1f);
        }
    }
    #endif
}
