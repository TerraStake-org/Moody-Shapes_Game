using UnityEngine;

/// <summary>
/// Applies an emotion-based aura effect to a shape using the EmotionAura shader.
/// </summary>
[RequireComponent(typeof(EmotionSystem))]
public class EmotionAuraEffect : MonoBehaviour
{
    [System.Serializable]
    public class EmotionColorMapping
    {
        public EmotionType emotion;
        public Color color = Color.white;
        [Range(0.5f, 3f)] public float baseIntensity = 1f;
    }
    
    [Header("Visual Settings")]
    [SerializeField] private EmotionColorMapping[] _emotionColors;
    [SerializeField] private float _pulseSpeed = 1.5f;
    [SerializeField] private float _pulseAmount = 0.2f;
    [SerializeField] private float _intensityMultiplier = 1.5f;
    
    [Header("References")]
    [SerializeField] private Material _auraMaterial;
    [SerializeField] private MeshRenderer _auraRenderer;
    
    private EmotionSystem _emotionSystem;
    private MaterialPropertyBlock _propertyBlock;
    private float _time;
    
    // Shader property IDs
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionIntensityProperty = Shader.PropertyToID("_EmissionIntensity");
    
    private void Awake()
    {
        _emotionSystem = GetComponent<EmotionSystem>();
        _propertyBlock = new MaterialPropertyBlock();
        
        if (_auraMaterial == null)
        {
            _auraMaterial = new Material(Shader.Find("Custom/EmotionAura"));
        }
        
        SetupAuraRenderer();
    }
    
    private void OnEnable()
    {
        if (_emotionSystem != null)
        {
            _emotionSystem.OnEmotionChanged += HandleEmotionChanged;
        }
    }
    
    private void OnDisable()
    {
        if (_emotionSystem != null)
        {
            _emotionSystem.OnEmotionChanged -= HandleEmotionChanged;
        }
    }
    
    private void Update()
    {
        if (_auraRenderer == null || _emotionSystem == null) return;
        
        _time += Time.deltaTime * _pulseSpeed;
        
        // Calculate pulsing effect
        float pulse = 1f + Mathf.Sin(_time) * _pulseAmount;
        
        // Get current emotion intensity
        float emotionIntensity = _emotionSystem.CurrentState.Intensity;
        
        // Apply intensity to the aura
        _auraRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(EmissionIntensityProperty, 
            emotionIntensity * _intensityMultiplier * pulse);
        _auraRenderer.SetPropertyBlock(_propertyBlock);
    }
    
    private void HandleEmotionChanged(EmotionChangeEvent change)
    {
        if (_auraRenderer == null) return;
        
        // Get color for current emotion
        Color emotionColor = GetColorForEmotion(change.newEmotion);
        
        // Set alpha based on intensity
        emotionColor.a = Mathf.Clamp01(change.newIntensity * 0.8f);
        
        // Apply color to the aura
        _auraRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(EmissionColorProperty, emotionColor);
        _auraRenderer.SetPropertyBlock(_propertyBlock);
    }
    
    private Color GetColorForEmotion(EmotionType emotion)
    {
        if (_emotionColors != null)
        {
            foreach (var mapping in _emotionColors)
            {
                if ((mapping.emotion & emotion) != 0)
                {
                    return mapping.color;
                }
            }
        }
        
        // Default colors if no mapping found
        if ((emotion & EmotionType.Happy) != 0 || (emotion & EmotionType.Joyful) != 0)
            return new Color(1f, 0.9f, 0.2f);
        else if ((emotion & EmotionType.Sad) != 0)
            return new Color(0.2f, 0.4f, 0.8f);
        else if ((emotion & EmotionType.Angry) != 0 || (emotion & EmotionType.Frustrated) != 0)
            return new Color(0.9f, 0.2f, 0.2f);
        else if ((emotion & EmotionType.Scared) != 0)
            return new Color(0.8f, 0.2f, 0.8f);
        else if ((emotion & EmotionType.Surprised) != 0)
            return new Color(0.2f, 0.8f, 0.8f);
        else if ((emotion & EmotionType.Calm) != 0)
            return new Color(0.2f, 0.8f, 0.4f);
        else if ((emotion & EmotionType.Curious) != 0)
            return new Color(0.6f, 0.8f, 0.9f);
            
        return Color.white; // Default for Neutral
    }
    
    /// <summary>
    /// Sets up the aura with the specified parameters.
    /// </summary>
    /// <param name="baseMaterial">The base material for the aura</param>
    /// <param name="intensityMultiplier">The intensity multiplier for the aura</param>
    /// <param name="pulseSpeed">The pulse speed for the aura</param>
    public void SetupAura(Material baseMaterial, float intensityMultiplier, float pulseSpeed)
    {
        _auraMaterial = new Material(baseMaterial);
        _intensityMultiplier = intensityMultiplier;
        _pulseSpeed = pulseSpeed;
        
        // Setup the renderer if it doesn't exist
        if (_auraRenderer == null)
        {
            SetupAuraRenderer();
        }
        else
        {
            _auraRenderer.material = _auraMaterial;
        }
    }
    
    private void SetupAuraRenderer()
    {
        // If no aura renderer assigned, create one
        if (_auraRenderer == null)
        {
            // Create a child object for the aura
            GameObject auraObject = new GameObject("EmotionAura");
            auraObject.transform.SetParent(transform);
            auraObject.transform.localPosition = Vector3.zero;
            auraObject.transform.localRotation = Quaternion.identity;
            
            // Add mesh filter and renderer
            MeshFilter meshFilter = auraObject.AddComponent<MeshFilter>();
            _auraRenderer = auraObject.AddComponent<MeshRenderer>();
            
            // Set up a simple quad mesh slightly larger than the shape
            meshFilter.mesh = CreateAuraMesh();
            
            // Assign material
            _auraRenderer.material = _auraMaterial;
            
            // Set up initial property block
            _propertyBlock = new MaterialPropertyBlock();
            _auraRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(EmissionColorProperty, Color.white);
            _propertyBlock.SetFloat(EmissionIntensityProperty, 0f);
            _auraRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
    
    private Mesh CreateAuraMesh()
    {
        // Create a simple quad that surrounds the shape
        Mesh mesh = new Mesh();
        
        // Determine the size based on the shape's collider or renderer
        float size = 1.5f; // Default size
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            if (collider is SphereCollider sphereCollider)
            {
                size = sphereCollider.radius * 2.2f;
            }
            else if (collider is BoxCollider boxCollider)
            {
                size = Mathf.Max(boxCollider.size.x, boxCollider.size.y) * 1.2f;
            }
        }
        
        // Create quad vertices
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-size, -size, 0),
            new Vector3(size, -size, 0),
            new Vector3(-size, size, 0),
            new Vector3(size, size, 0)
        };
        
        // Create triangles
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        // Create UVs
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}
