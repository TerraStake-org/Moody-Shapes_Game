using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility for creating enhanced emotion test prefabs with both
/// ShapeAnimator and EmotionPhysics components properly configured.
/// </summary>
public class EnhancedShapePrefabBuilder : EditorWindow
{
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private string prefabName = "EnhancedEmotionShape";
    [SerializeField] private bool includePhysicsAccessories = true;
    [SerializeField] private bool includeAdvancedAnimator = true;
    [SerializeField] private bool includeTestComponents = true;

    [MenuItem("Moody Shapes/Create Enhanced Shape Prefab")]
    public static void ShowWindow()
    {
        GetWindow<EnhancedShapePrefabBuilder>("Enhanced Shape Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("Enhanced Emotion Shape Prefab Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        basePrefab = EditorGUILayout.ObjectField("Base Prefab", basePrefab, typeof(GameObject), false) as GameObject;
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);

        EditorGUILayout.Space();
        GUILayout.Label("Components to Include:", EditorStyles.boldLabel);
        
        includeAdvancedAnimator = EditorGUILayout.Toggle("Enhanced Shape Animator", includeAdvancedAnimator);
        includePhysicsAccessories = EditorGUILayout.Toggle("Emotion Physics System", includePhysicsAccessories);
        includeTestComponents = EditorGUILayout.Toggle("Test Integration Components", includeTestComponents);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Enhanced Prefab"))
        {
            CreateEnhancedPrefab();
        }

        if (GUILayout.Button("Create Test Scene Setup"))
        {
            CreateTestSceneSetup();
        }
    }

    void CreateEnhancedPrefab()
    {
        if (basePrefab == null)
        {
            CreateBasicShapeFromScratch();
        }
        else
        {
            EnhanceExistingPrefab();
        }
    }

    void CreateBasicShapeFromScratch()
    {
        // Create base shape
        GameObject shapeObj = new GameObject(prefabName);
        
        // Add basic shape geometry
        GameObject visualMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualMesh.name = "Visual";
        visualMesh.transform.SetParent(shapeObj.transform);
        visualMesh.transform.localPosition = Vector3.zero;
        
        // Add core emotion components
        AddCoreEmotionComponents(shapeObj);
        
        // Add enhanced components
        if (includeAdvancedAnimator)
        {
            AddEnhancedAnimator(shapeObj);
        }
        
        if (includePhysicsAccessories)
        {
            AddEmotionPhysics(shapeObj);
        }
        
        if (includeTestComponents)
        {
            AddTestComponents(shapeObj);
        }
        
        // Save as prefab
        SaveAsPrefab(shapeObj);
        
        DestroyImmediate(shapeObj);
        
        EditorUtility.DisplayDialog("Success", $"Enhanced prefab '{prefabName}' created successfully!", "OK");
    }

    void EnhanceExistingPrefab()
    {
        GameObject instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
        
        if (includeAdvancedAnimator && instance.GetComponent<ShapeAnimator>() == null)
        {
            AddEnhancedAnimator(instance);
        }
        
        if (includePhysicsAccessories && instance.GetComponent<EmotionPhysics>() == null)
        {
            AddEmotionPhysics(instance);
        }
        
        if (includeTestComponents)
        {
            AddTestComponents(instance);
        }
        
        SaveAsPrefab(instance);
        
        DestroyImmediate(instance);
        
        EditorUtility.DisplayDialog("Success", $"Enhanced prefab '{prefabName}' created from existing prefab!", "OK");
    }

    void AddCoreEmotionComponents(GameObject obj)
    {
        // Add core emotion system components
        if (obj.GetComponent<EmotionSystem>() == null)
            obj.AddComponent<EmotionSystem>();
            
        if (obj.GetComponent<EmotionalState>() == null)
            obj.AddComponent<EmotionalState>();
            
        if (obj.GetComponent<EmotionMemory>() == null)
            obj.AddComponent<EmotionMemory>();
            
        // Add animator if needed
        if (obj.GetComponent<Animator>() == null)
        {
            obj.AddComponent<Animator>();
            // Note: In a real implementation, you'd assign an AnimatorController here
        }
    }

    void AddEnhancedAnimator(GameObject obj)
    {
        var animator = obj.GetComponent<ShapeAnimator>();
        if (animator == null)
        {
            animator = obj.AddComponent<ShapeAnimator>();
        }
        
        // Create some test accessories for animation rigging
        GameObject rigContainer = new GameObject("Animation Rig");
        rigContainer.transform.SetParent(obj.transform);
        rigContainer.transform.localPosition = Vector3.zero;
        
        // Add some visual elements for animation testing
        for (int i = 0; i < 2; i++)
        {
            GameObject eyeTarget = new GameObject($"EyeTarget_{i}");
            eyeTarget.transform.SetParent(rigContainer.transform);
            eyeTarget.transform.localPosition = new Vector3(i == 0 ? -0.3f : 0.3f, 0.2f, 0.5f);
        }
    }

    void AddEmotionPhysics(GameObject obj)
    {
        var physics = obj.GetComponent<EmotionPhysics>();
        if (physics == null)
        {
            physics = obj.AddComponent<EmotionPhysics>();
        }
        
        // Create physics accessories
        GameObject accessoryContainer = new GameObject("Physics Accessories");
        accessoryContainer.transform.SetParent(obj.transform);
        accessoryContainer.transform.localPosition = Vector3.zero;
        
        // Create floating orbs as physics accessories
        for (int i = 0; i < 3; i++)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = $"Accessory_Orb_{i}";
            orb.transform.SetParent(accessoryContainer.transform);
            orb.transform.localScale = Vector3.one * 0.2f;
            
            // Position in a circle around the shape
            float angle = (i / 3f) * 360f * Mathf.Deg2Rad;
            orb.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 1.5f,
                Random.Range(-0.5f, 0.5f),
                Mathf.Sin(angle) * 1.5f
            );
            
            // Remove collider to avoid physics interference
            DestroyImmediate(orb.GetComponent<Collider>());
        }
    }

    void AddTestComponents(GameObject obj)
    {
        // Add a simple test marker component
        if (obj.GetComponent<TestShapeMarker>() == null)
        {
            obj.AddComponent<TestShapeMarker>();
        }
    }

    void SaveAsPrefab(GameObject obj)
    {
        // Ensure prefabs directory exists
        string prefabDir = "Assets/Prefabs/Enhanced";
        if (!AssetDatabase.IsValidFolder(prefabDir))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enhanced");
        }
        
        string prefabPath = $"{prefabDir}/{prefabName}.prefab";
        
        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        
        Debug.Log($"Enhanced emotion shape prefab created at: {prefabPath}");
    }

    void CreateTestSceneSetup()
    {
        // Create test manager in current scene
        GameObject testManager = new GameObject("Advanced Emotion Test Manager");
        var component = testManager.AddComponent<AdvancedEmotionTestManager>();
        
        // Set up basic test arena
        GameObject arena = new GameObject("Test Arena");
        arena.transform.position = Vector3.zero;
        
        // Create some test UI (basic setup)
        CreateBasicTestUI(testManager);
        
        Selection.activeGameObject = testManager;
        
        EditorUtility.DisplayDialog("Test Scene", "Advanced emotion test manager added to scene. Configure the UI references in the inspector.", "OK");
    }

    void CreateBasicTestUI(GameObject parent)
    {
        // This would create a basic canvas with test buttons
        // In a real implementation, you'd create a proper UI hierarchy
        GameObject canvas = new GameObject("Test UI Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        Debug.Log("Basic test UI canvas created. Add specific UI elements manually.");
    }
}

/// <summary>
/// Simple marker component to identify test shapes
/// </summary>
public class TestShapeMarker : MonoBehaviour
{
    [SerializeField] private bool isTestShape = true;
    [SerializeField] private string testNotes = "Enhanced emotion test shape";
    
    public bool IsTestShape => isTestShape;
    public string TestNotes => testNotes;
}