using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages all emotion-related systems in a scene, ensuring they are properly initialized
/// and connected. This is the main entry point for setting up the emotional ecosystem.
/// </summary>
public class EmotionSystemsManager : MonoBehaviour
{
    [System.Serializable]
    public class EmotionSettings
    {
        [Header("Influence Settings")]
        public float baseInfluenceRadius = 5f;
        public float influenceInterval = 1.5f;
        public LayerMask shapesLayerMask;
        
        [Header("Social Settings")]
        public float baseRelationshipMultiplier = 1f;
        public float familiarityGrowthRate = 0.05f;
        
        [Header("Visual Settings")]
        public Material auraBaseMaterial;
        public float auraIntensityMultiplier = 1.5f;
        public float auraPulseSpeed = 1.5f;
    }
    
    [SerializeField] private EmotionSettings _settings;
    [SerializeField] private bool _autoSetupScene = true;
    [SerializeField] private bool _createMissingComponents = true;
    
    // References to core systems
    private SocialRelationshipManager _socialManager;
    private EmotionInfluenceSystem _influenceSystem;
    
    // List of all tracked emotion systems in the scene
    private List<EmotionSystem> _allEmotionSystems = new List<EmotionSystem>();
    
    private void Awake()
    {
        if (_autoSetupScene)
        {
            SetupScene();
        }
    }
    
    private void Start()
    {
        // Find all EmotionSystem components after any other scripts have had a chance to create them
        RefreshTrackedSystems();
    }
    
    /// <summary>
    /// Sets up all required emotion-related systems in the scene.
    /// </summary>
    public void SetupScene()
    {
        CreateCoreSystems();
        RefreshTrackedSystems();
        SetupVisualEffects();
    }
    
    /// <summary>
    /// Creates the core system components if they don't already exist.
    /// </summary>
    private void CreateCoreSystems()
    {
        // Create SocialRelationshipManager if needed
        _socialManager = FindObjectOfType<SocialRelationshipManager>();
        if (_socialManager == null && _createMissingComponents)
        {
            GameObject socialObj = new GameObject("Social Relationship Manager");
            socialObj.transform.SetParent(transform);
            _socialManager = socialObj.AddComponent<SocialRelationshipManager>();
            
            // Apply settings
            _socialManager.SetBaseInfluenceMultiplier(_settings.baseRelationshipMultiplier);
            _socialManager.SetFamiliarityGrowthRate(_settings.familiarityGrowthRate);
            
            Debug.Log("Created Social Relationship Manager");
        }
        
        // Create EmotionInfluenceSystem if needed
        _influenceSystem = FindObjectOfType<EmotionInfluenceSystem>();
        if (_influenceSystem == null && _createMissingComponents)
        {
            GameObject influenceObj = new GameObject("Emotion Influence System");
            influenceObj.transform.SetParent(transform);
            _influenceSystem = influenceObj.AddComponent<EmotionInfluenceSystem>();
            
            // Apply settings
            _influenceSystem.SetupInfluenceSystem(_settings.baseInfluenceRadius, 
                                                 _settings.influenceInterval,
                                                 _settings.shapesLayerMask);
            
            // Connect to social manager
            if (_socialManager != null)
            {
                _influenceSystem.SetSocialRelationshipManager(_socialManager);
            }
            
            Debug.Log("Created Emotion Influence System");
        }
    }
    
    /// <summary>
    /// Finds all EmotionSystem components in the scene.
    /// </summary>
    public void RefreshTrackedSystems()
    {
        _allEmotionSystems.Clear();
        _allEmotionSystems.AddRange(FindObjectsOfType<EmotionSystem>());
        
        Debug.Log($"Found {_allEmotionSystems.Count} emotion systems in the scene");
    }
    
    /// <summary>
    /// Sets up visual effects for all emotion systems in the scene.
    /// </summary>
    private void SetupVisualEffects()
    {
        if (_settings.auraBaseMaterial == null)
        {
            Debug.LogWarning("Aura base material is not set. Skipping visual effects setup.");
            return;
        }
        
        foreach (var emotionSystem in _allEmotionSystems)
        {
            // Add EmotionAuraEffect if it doesn't exist
            if (_createMissingComponents && emotionSystem.GetComponent<EmotionAuraEffect>() == null)
            {
                var auraEffect = emotionSystem.gameObject.AddComponent<EmotionAuraEffect>();
                auraEffect.SetupAura(_settings.auraBaseMaterial, 
                                   _settings.auraIntensityMultiplier,
                                   _settings.auraPulseSpeed);
                
                Debug.Log($"Added aura effect to {emotionSystem.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Creates a basic shape with emotion systems attached.
    /// </summary>
    public GameObject CreateEmotionalShape(Vector3 position, EmotionProfileSO profile = null)
    {
        GameObject shapeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shapeObj.transform.position = position;
        shapeObj.name = "Emotional Shape";
        
        // Add emotion system
        var emotionSystem = shapeObj.AddComponent<EmotionSystem>();
        var emotionMemory = shapeObj.AddComponent<EmotionMemory>();
        
        // Set profile if provided
        if (profile != null)
        {
            emotionSystem.SetEmotionProfile(profile);
        }
        
        // Add aura effect
        if (_settings.auraBaseMaterial != null)
        {
            var auraEffect = shapeObj.AddComponent<EmotionAuraEffect>();
            auraEffect.SetupAura(_settings.auraBaseMaterial, 
                               _settings.auraIntensityMultiplier,
                               _settings.auraPulseSpeed);
        }
        
        // Add to tracked systems
        _allEmotionSystems.Add(emotionSystem);
        
        return shapeObj;
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Creates a prefab from a shape in the scene.
    /// </summary>
    public void CreateShapePrefab(GameObject shapeObject, string prefabName = "EmotionalShape")
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Shapes"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Shapes");
        }
        
        string prefabPath = $"Assets/Prefabs/Shapes/{prefabName}.prefab";
        
        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(shapeObject, prefabPath);
        Debug.Log($"Created prefab at {prefabPath}");
    }
    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(EmotionSystemsManager))]
public class EmotionSystemsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EmotionSystemsManager manager = (EmotionSystemsManager)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Scene"))
        {
            manager.SetupScene();
        }
        
        if (GUILayout.Button("Refresh Tracked Systems"))
        {
            manager.RefreshTrackedSystems();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Create Test Shape", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Emotional Shape"))
        {
            var shape = manager.CreateEmotionalShape(Vector3.zero);
            Selection.activeGameObject = shape;
        }
    }
}
#endif
