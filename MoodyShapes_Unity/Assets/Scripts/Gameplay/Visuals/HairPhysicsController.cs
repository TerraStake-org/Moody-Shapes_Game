using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Specialized controller for hair physics that integrates with the EmotionPhysics system.
/// Manages hair strands, flow maps, and emotion-responsive hair behavior.
/// </summary>
[RequireComponent(typeof(EmotionPhysics))]
public class HairPhysicsController : MonoBehaviour
{
    [System.Serializable]
    public class HairStrand
    {
        [Header("Strand Configuration")]
        public Transform rootTransform;
        public Transform[] segments;
        public Material hairMaterial;
        
        [Header("Physics Properties")]
        public float stiffness = 0.8f;
        public float damping = 0.9f;
        public float gravity = 1f;
        public float windResponse = 1f;
        
        [Header("Emotion Response")]
        public float emotionIntensityMultiplier = 1f;
        public Vector3 emotionDirection = Vector3.up;
        public AnimationCurve emotionResponse = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Runtime data
        [HideInInspector] public Vector3[] positions;
        [HideInInspector] public Vector3[] previousPositions;
        [HideInInspector] public Vector3[] velocities;
        [HideInInspector] public bool isInitialized;
    }

    [Header("Hair Configuration")]
    [SerializeField] private HairStrand[] _hairStrands;
    [SerializeField] private Transform _headTransform;
    
    [Header("Physics Settings")]
    [SerializeField] private float _globalStiffness = 1f;
    [SerializeField] private float _globalDamping = 0.95f;
    [SerializeField] private float _airResistance = 0.02f;
    [SerializeField] private int _physicsIterations = 3;
    
    [Header("Emotion Integration")]
    [SerializeField] private bool _enableEmotionResponse = true;
    [SerializeField] private float _emotionResponseStrength = 2f;
    [SerializeField] private float _emotionTransitionSpeed = 5f;
    
    [Header("Shader Integration")]
    [SerializeField] private bool _updateShaderProperties = true;
    [SerializeField] private string _flowIntensityProperty = "_FlowIntensity";
    [SerializeField] private string _emotionColorProperty = "_EmotionColorTint";
    [SerializeField] private string _emotionGlowProperty = "_EmotionGlow";
    
    // Component references
    private EmotionPhysics _emotionPhysics;
    private EmotionSystem _emotionSystem;
    
    // Runtime state
    private Vector3 _currentEmotionForce;
    private float _currentEmotionIntensity;
    private Color _currentEmotionColor;
    private Dictionary<Material, MaterialPropertyBlock> _materialPropertyBlocks;

    void Awake()
    {
        _emotionPhysics = GetComponent<EmotionPhysics>();
        _emotionSystem = GetComponent<EmotionSystem>();
        _materialPropertyBlocks = new Dictionary<Material, MaterialPropertyBlock>();
        
        InitializeHairStrands();
    }

    void Start()
    {
        SetupShaderProperties();
    }

    void FixedUpdate()
    {
        if (!_emotionPhysics.EnableAccessories) return;
        
        UpdateEmotionState();
        SimulateHairPhysics();
        
        if (_updateShaderProperties)
        {
            UpdateShaderProperties();
        }
    }

    #region Initialization

    void InitializeHairStrands()
    {
        foreach (var strand in _hairStrands)
        {
            if (strand.segments == null || strand.segments.Length == 0) continue;
            
            int segmentCount = strand.segments.Length;
            strand.positions = new Vector3[segmentCount];
            strand.previousPositions = new Vector3[segmentCount];
            strand.velocities = new Vector3[segmentCount];
            
            // Initialize positions from current transforms
            for (int i = 0; i < segmentCount; i++)
            {
                if (strand.segments[i] != null)
                {
                    strand.positions[i] = strand.segments[i].position;
                    strand.previousPositions[i] = strand.positions[i];
                }
            }
            
            strand.isInitialized = true;
        }
    }

    void SetupShaderProperties()
    {
        foreach (var strand in _hairStrands)
        {
            if (strand.hairMaterial != null)
            {
                if (!_materialPropertyBlocks.ContainsKey(strand.hairMaterial))
                {
                    _materialPropertyBlocks[strand.hairMaterial] = new MaterialPropertyBlock();
                }
            }
        }
    }

    #endregion

    #region Physics Simulation

    void SimulateHairPhysics()
    {
        foreach (var strand in _hairStrands)
        {
            if (!strand.isInitialized) continue;
            
            SimulateStrand(strand);
        }
    }

    void SimulateStrand(HairStrand strand)
    {
        if (strand.segments == null || strand.positions == null) return;
        
        int segmentCount = strand.positions.Length;
        
        // Multiple iterations for stability
        for (int iteration = 0; iteration < _physicsIterations; iteration++)
        {
            // Apply forces and constraints
            for (int i = 0; i < segmentCount; i++)
            {
                if (strand.segments[i] == null) continue;
                
                Vector3 force = CalculateForces(strand, i);
                
                // Verlet integration
                Vector3 newPosition = strand.positions[i] + (strand.positions[i] - strand.previousPositions[i]) * _globalDamping + force * Time.fixedDeltaTime * Time.fixedDeltaTime;
                
                strand.previousPositions[i] = strand.positions[i];
                strand.positions[i] = newPosition;
                
                // Apply constraints (distance and collision)
                ApplyConstraints(strand, i);
            }
        }
        
        // Update transform positions
        for (int i = 0; i < segmentCount; i++)
        {
            if (strand.segments[i] != null)
            {
                strand.segments[i].position = strand.positions[i];
                
                // Update rotation to look along hair direction
                if (i > 0)
                {
                    Vector3 direction = (strand.positions[i] - strand.positions[i - 1]).normalized;
                    if (direction != Vector3.zero)
                    {
                        strand.segments[i].rotation = Quaternion.LookRotation(direction, Vector3.up);
                    }
                }
            }
        }
    }

    Vector3 CalculateForces(HairStrand strand, int segmentIndex)
    {
        Vector3 totalForce = Vector3.zero;
        
        // Gravity
        totalForce += Vector3.down * strand.gravity;
        
        // Air resistance
        totalForce -= strand.velocities[segmentIndex] * _airResistance;
        
        // Emotion-based force
        if (_enableEmotionResponse)
        {
            Vector3 emotionForce = _currentEmotionForce * strand.emotionIntensityMultiplier;
            emotionForce = Vector3.Scale(emotionForce, strand.emotionDirection);
            
            // Apply emotion response curve
            float normalizedIndex = (float)segmentIndex / (strand.positions.Length - 1);
            float responseMultiplier = strand.emotionResponse.Evaluate(normalizedIndex);
            
            totalForce += emotionForce * responseMultiplier;
        }
        
        // Wind force (from EmotionPhysics wind system)
        // This integrates with the existing wind system
        totalForce += GetWindForce() * strand.windResponse;
        
        return totalForce;
    }

    void ApplyConstraints(HairStrand strand, int segmentIndex)
    {
        // Distance constraint to previous segment
        if (segmentIndex > 0)
        {
            Vector3 delta = strand.positions[segmentIndex] - strand.positions[segmentIndex - 1];
            float distance = delta.magnitude;
            float restDistance = CalculateRestDistance(strand, segmentIndex);
            
            if (distance > restDistance)
            {
                Vector3 correction = delta.normalized * (distance - restDistance) * strand.stiffness * _globalStiffness;
                strand.positions[segmentIndex] -= correction * 0.5f;
                
                if (segmentIndex > 1)
                {
                    strand.positions[segmentIndex - 1] += correction * 0.5f;
                }
            }
        }
        
        // Keep root fixed to head
        if (segmentIndex == 0 && strand.rootTransform != null)
        {
            strand.positions[0] = strand.rootTransform.position;
            strand.previousPositions[0] = strand.positions[0];
        }
        
        // Collision with head (simple sphere collision)
        if (_headTransform != null)
        {
            float headRadius = 0.5f; // Adjust based on your character
            Vector3 toSegment = strand.positions[segmentIndex] - _headTransform.position;
            
            if (toSegment.magnitude < headRadius)
            {
                strand.positions[segmentIndex] = _headTransform.position + toSegment.normalized * headRadius;
            }
        }
    }

    float CalculateRestDistance(HairStrand strand, int segmentIndex)
    {
        if (segmentIndex == 0 || strand.segments == null) return 0.1f;
        
        // Calculate based on original segment spacing
        if (strand.segments[segmentIndex] != null && strand.segments[segmentIndex - 1] != null)
        {
            return Vector3.Distance(strand.segments[segmentIndex].localPosition, strand.segments[segmentIndex - 1].localPosition);
        }
        
        return 0.1f; // Default segment length
    }

    #endregion

    #region Emotion Integration

    void UpdateEmotionState()
    {
        if (_emotionSystem == null) return;
        
        // Get current emotion data
        EmotionType currentEmotion = _emotionSystem.CurrentState.CurrentEmotion;
        float intensity = _emotionSystem.CurrentState.Intensity;
        
        // Calculate emotion force
        Vector3 targetEmotionForce = GetEmotionForce(currentEmotion, intensity);
        _currentEmotionForce = Vector3.Lerp(_currentEmotionForce, targetEmotionForce, 
            _emotionTransitionSpeed * Time.fixedDeltaTime);
        
        // Update emotion intensity for shader
        _currentEmotionIntensity = Mathf.Lerp(_currentEmotionIntensity, intensity, 
            _emotionTransitionSpeed * Time.fixedDeltaTime);
        
        // Update emotion color
        Color targetColor = GetEmotionColor(currentEmotion);
        _currentEmotionColor = Color.Lerp(_currentEmotionColor, targetColor, 
            _emotionTransitionSpeed * Time.fixedDeltaTime);
    }

    Vector3 GetEmotionForce(EmotionType emotion, float intensity)
    {
        Vector3 baseForce = emotion switch
        {
            EmotionType.Happy => Vector3.up * 2f,
            EmotionType.Excited => new Vector3(0.5f, 1.5f, 0.5f),
            EmotionType.Angry => new Vector3(
                UnityEngine.Random.Range(-1f, 1f), 
                UnityEngine.Random.Range(-0.5f, 1f), 
                UnityEngine.Random.Range(-1f, 1f)
            ) * 3f,
            EmotionType.Sad => Vector3.down * 1.5f,
            EmotionType.Scared => Vector3.back * 2f,
            EmotionType.Surprised => Vector3.up * 3f,
            EmotionType.Disgusted => new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f), 
                -1f, 
                UnityEngine.Random.Range(-0.5f, 0.5f)
            ),
            EmotionType.Calm => Vector3.zero,
            _ => Vector3.zero
        };
        
        return baseForce * intensity * _emotionResponseStrength;
    }

    Color GetEmotionColor(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => Color.yellow,
            EmotionType.Excited => Color.magenta,
            EmotionType.Angry => Color.red,
            EmotionType.Sad => Color.blue,
            EmotionType.Scared => Color.gray,
            EmotionType.Surprised => Color.white,
            EmotionType.Disgusted => Color.green,
            EmotionType.Calm => new Color(0.8f, 0.9f, 1f),
            _ => Color.white
        };
    }

    #endregion

    #region Shader Integration

    void UpdateShaderProperties()
    {
        foreach (var strand in _hairStrands)
        {
            if (strand.hairMaterial == null) continue;
            
            if (_materialPropertyBlocks.TryGetValue(strand.hairMaterial, out MaterialPropertyBlock propertyBlock))
            {
                // Update flow intensity based on emotion
                float flowIntensity = Mathf.Lerp(0.5f, 2f, _currentEmotionIntensity);
                propertyBlock.SetFloat(_flowIntensityProperty, flowIntensity);
                
                // Update emotion color tint
                propertyBlock.SetColor(_emotionColorProperty, _currentEmotionColor);
                
                // Update emotion glow
                float emotionGlow = _currentEmotionIntensity * 0.5f;
                propertyBlock.SetFloat(_emotionGlowProperty, emotionGlow);
                
                // Apply property block to all renderers using this material
                ApplyPropertyBlockToRenderers(strand.hairMaterial, propertyBlock);
            }
        }
    }

    void ApplyPropertyBlockToRenderers(Material material, MaterialPropertyBlock propertyBlock)
    {
        // Find all renderers in hair strands that use this material
        foreach (var strand in _hairStrands)
        {
            if (strand.segments == null) continue;
            
            foreach (var segment in strand.segments)
            {
                if (segment == null) continue;
                
                Renderer renderer = segment.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial == material)
                {
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
    }

    #endregion

    #region Utility Methods    Vector3 GetWindForce()
    {
        // Integrate with the EmotionPhysics wind system
        if (_emotionPhysics != null && _emotionPhysics.enabled)
        {
            // Access the wind force from EmotionPhysics using reflection or public properties
            // Since EmotionPhysics has wind functionality, we'll try to get the wind direction
            return GetEmotionPhysicsWindForce();
        }
        
        // Fallback to simple wind effect if EmotionPhysics wind is not available
        return new Vector3(
            Mathf.Sin(Time.time * 0.5f) * 0.2f,
            0,
            Mathf.Cos(Time.time * 0.3f) * 0.1f
        );
    }

    Vector3 GetEmotionPhysicsWindForce()
    {
        // Use reflection to access the private _windDirection field from EmotionPhysics
        var windDirectionField = typeof(EmotionPhysics).GetField("_windDirection", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var windStrengthField = typeof(EmotionPhysics).GetField("_windStrength", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var enableWindField = typeof(EmotionPhysics).GetField("_enableWind", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (windDirectionField != null && windStrengthField != null && enableWindField != null)
        {
            bool enableWind = (bool)enableWindField.GetValue(_emotionPhysics);
            if (enableWind)
            {
                Vector3 windDirection = (Vector3)windDirectionField.GetValue(_emotionPhysics);
                float windStrength = (float)windStrengthField.GetValue(_emotionPhysics);
                return windDirection * windStrength * 0.3f; // Scale for hair physics
            }
        }
        
        return Vector3.zero;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Add a new hair strand at runtime
    /// </summary>
    public void AddHairStrand(HairStrand newStrand)
    {
        var strandList = _hairStrands.ToList();
        strandList.Add(newStrand);
        _hairStrands = strandList.ToArray();
        
        // Initialize the new strand
        if (newStrand.segments != null && newStrand.segments.Length > 0)
        {
            int segmentCount = newStrand.segments.Length;
            newStrand.positions = new Vector3[segmentCount];
            newStrand.previousPositions = new Vector3[segmentCount];
            newStrand.velocities = new Vector3[segmentCount];
            
            for (int i = 0; i < segmentCount; i++)
            {
                if (newStrand.segments[i] != null)
                {
                    newStrand.positions[i] = newStrand.segments[i].position;
                    newStrand.previousPositions[i] = newStrand.positions[i];
                }
            }
            
            newStrand.isInitialized = true;
        }
    }

    /// <summary>
    /// Remove a hair strand
    /// </summary>
    public void RemoveHairStrand(int index)
    {
        if (index >= 0 && index < _hairStrands.Length)
        {
            var strandList = _hairStrands.ToList();
            strandList.RemoveAt(index);
            _hairStrands = strandList.ToArray();
        }
    }

    /// <summary>
    /// Set the emotion response strength for all strands
    /// </summary>
    public void SetEmotionResponseStrength(float strength)
    {
        _emotionResponseStrength = strength;
    }

    /// <summary>
    /// Enable or disable hair physics
    /// </summary>
    public void SetHairPhysicsEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    #endregion

    #region Editor Integration

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_hairStrands == null) return;
        
        foreach (var strand in _hairStrands)
        {
            if (!strand.isInitialized || strand.positions == null) continue;
            
            // Draw hair strand connections
            Gizmos.color = Color.yellow;
            for (int i = 1; i < strand.positions.Length; i++)
            {
                Gizmos.DrawLine(strand.positions[i - 1], strand.positions[i]);
            }
            
            // Draw segment positions
            Gizmos.color = Color.red;
            foreach (var position in strand.positions)
            {
                Gizmos.DrawWireSphere(position, 0.02f);
            }
        }
        
        // Draw emotion force
        if (_enableEmotionResponse)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + _currentEmotionForce);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw more detailed information when selected
        if (_headTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_headTransform.position, 0.5f); // Head collision sphere
        }
    }
#endif

    #endregion
}
