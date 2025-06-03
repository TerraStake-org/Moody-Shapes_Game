using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to create advanced integration test scenes with all necessary components
/// </summary>
public class AdvancedIntegrationTestSceneBuilder : EditorWindow
{
    [MenuItem("Tools/Advanced Emotion Systems/Create Integration Test Scene")]
    public static void CreateIntegrationTestScene()
    {
        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Setup camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 5, -10);
            mainCamera.transform.rotation = Quaternion.Euler(20, 0, 0);
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }
        
        // Create lighting
        Light mainLight = FindObjectOfType<Light>();
        if (mainLight != null)
        {
            mainLight.type = LightType.Directional;
            mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            mainLight.intensity = 1.2f;
        }
        
        // Create test arena
        GameObject testArena = new GameObject("Test Arena");
        testArena.transform.position = Vector3.zero;
        
        // Add floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Test Floor";
        floor.transform.parent = testArena.transform;
        floor.transform.localScale = new Vector3(2, 1, 2);
        
        // Create systems manager
        GameObject systemsManager = new GameObject("Emotion Systems Manager");
        systemsManager.AddComponent<EmotionSystemsManager>();
        
        // Create test manager gameobject
        GameObject testManagerObj = new GameObject("Advanced Test Manager");
        testManagerObj.AddComponent<AdvancedEmotionTestManager>();
        testManagerObj.AddComponent<AdvancedEmotionIntegrationTest>();
        
        // Position test manager
        testManagerObj.transform.position = new Vector3(0, 1, 0);
        
        // Create UI Canvas
        CreateTestUI(testManagerObj);
        
        // Add test shapes prefab location
        GameObject prefabContainer = new GameObject("Test Shape Prefabs");
        prefabContainer.transform.parent = testArena.transform;
        
        // Create some example enhanced shapes
        CreateExampleShapes(prefabContainer.transform);
        
        // Save the scene
        string scenePath = "Assets/Scenes/Demo/AdvancedIntegrationTest.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes/Demo");
        
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        Debug.Log($"Advanced Integration Test Scene created at: {scenePath}");
        
        // Select the test manager for easy access
        Selection.activeGameObject = testManagerObj;
    }
    
    static void CreateTestUI(GameObject testManagerObj)
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Test UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create main panel
        GameObject mainPanel = new GameObject("Main Panel");
        mainPanel.transform.SetParent(canvasObj.transform, false);
        
        var rectTransform = mainPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0.7f);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        var panelImage = mainPanel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Create test buttons panel
        CreateTestButtons(mainPanel.transform);
        
        // Create status panel
        CreateStatusPanel(canvasObj.transform);
    }
    
    static void CreateTestButtons(Transform parent)
    {
        GameObject buttonPanel = new GameObject("Button Panel");
        buttonPanel.transform.SetParent(parent, false);
        
        var rectTransform = buttonPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        var layout = buttonPanel.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        
        // Create test buttons
        string[] buttonNames = {
            "Run Integration Test",
            "Spawn Enhanced Shape", 
            "Test Animations",
            "Test Physics",
            "Stress Test",
            "Clear All"
        };
        
        foreach (string buttonName in buttonNames)
        {
            CreateButton(buttonPanel.transform, buttonName);
        }
    }
    
    static void CreateButton(Transform parent, string buttonText)
    {
        GameObject buttonObj = new GameObject(buttonText + " Button");
        buttonObj.transform.SetParent(parent, false);
        
        var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
        var buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
        buttonImage.color = new Color(0.2f, 0.3f, 0.8f, 0.8f);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        var text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    static void CreateStatusPanel(Transform parent)
    {
        GameObject statusPanel = new GameObject("Status Panel");
        statusPanel.transform.SetParent(parent, false);
        
        var rectTransform = statusPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 0.3f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        var panelImage = statusPanel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        // Create status text
        GameObject statusText = new GameObject("Status Text");
        statusText.transform.SetParent(statusPanel.transform, false);
        
        var text = statusText.AddComponent<UnityEngine.UI.Text>();
        text.text = "Advanced Emotion Systems Integration Test Ready\nPress buttons above to run tests";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        
        var textRect = statusText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
    }
    
    static void CreateExampleShapes(Transform parent)
    {
        Vector3[] positions = {
            new Vector3(-4, 0, 0),
            new Vector3(-2, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(4, 0, 0)
        };
        
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject shape = new GameObject($"Enhanced Shape {i + 1}");
            shape.transform.SetParent(parent);
            shape.transform.position = positions[i];
            
            // Add essential components
            shape.AddComponent<EmotionSystem>();
            shape.AddComponent<ShapeAnimator>();
            shape.AddComponent<EmotionPhysics>();
            shape.AddComponent<ShapeVisualController>();
            
            // Add basic visuals
            var renderer = shape.AddComponent<MeshRenderer>();
            var meshFilter = shape.AddComponent<MeshFilter>();
            
            // Use different primitive shapes
            PrimitiveType[] primitives = { 
                PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Capsule, 
                PrimitiveType.Cylinder, PrimitiveType.Cube 
            };
            
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>(primitives[i].ToString() + ".fbx");
            
            // Create colorful materials
            Material mat = new Material(Shader.Find("Standard"));
            Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
            mat.color = colors[i];
            renderer.material = mat;
            
            // Add collider for physics
            switch (primitives[i])
            {
                case PrimitiveType.Cube:
                    shape.AddComponent<BoxCollider>();
                    break;
                case PrimitiveType.Sphere:
                    shape.AddComponent<SphereCollider>();
                    break;
                case PrimitiveType.Capsule:
                    shape.AddComponent<CapsuleCollider>();
                    break;
                default:
                    shape.AddComponent<BoxCollider>();
                    break;
            }
            
            // Add rigidbody for physics
            var rb = shape.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 2f;
            rb.angularDrag = 5f;
        }
    }
    
    [MenuItem("Tools/Advanced Emotion Systems/Setup Integration Test Environment")]
    public static void SetupIntegrationTestEnvironment()
    {
        // Create directories
        string[] directories = {
            "Assets/Scenes/Demo",
            "Assets/Prefabs/Enhanced",
            "Assets/Materials/Emotion",
            "Assets/Testing"
        };
        
        foreach (string dir in directories)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string parentPath = System.IO.Path.GetDirectoryName(dir);
                string folderName = System.IO.Path.GetFileName(dir);
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("Integration test environment setup completed!");
    }
}
