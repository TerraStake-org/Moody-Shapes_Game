using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive integration test manager for the enhanced emotion systems:
/// - ShapeAnimator with rigging and procedural motion
/// - EmotionPhysics with accessories and spring dynamics
/// - AnimationSetupHelper and EmotionClipBuilder integration
/// </summary>
public class AdvancedEmotionTestManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private EmotionSystemsManager _systemsManager;
    [SerializeField] private Transform _testArena;
    
    [Header("Test Configuration")]
    [SerializeField] private GameObject _enhancedShapePrefab;
    [SerializeField] private int _maxTestShapes = 8;
    [SerializeField] private float _spawnRadius = 6f;
    [SerializeField] private float _testDuration = 30f;
    
    [Header("UI References")]
    [SerializeField] private Button[] _emotionTestButtons;
    [SerializeField] private Button _spawnButton;
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _animationTestButton;
    [SerializeField] private Button _physicsTestButton;
    [SerializeField] private Button _stressTestButton;
    [SerializeField] private Slider _intensitySlider;
    [SerializeField] private Toggle _proceduralMotionToggle;
    [SerializeField] private Toggle _physicsAccessoriesToggle;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _performanceText;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool _showDebugGizmos = true;
    [SerializeField] private Material _debugAccessoryMaterial;
    
    // Test State
    private List<EmotionSystem> _testShapes = new List<EmotionSystem>();
    private List<ShapeAnimator> _shapeAnimators = new List<ShapeAnimator>();
    private List<EmotionPhysics> _emotionPhysics = new List<EmotionPhysics>();
    private bool _isRunningTest = false;
    private float _testStartTime;
    private int _frameCount;
    private float _averageFPS;
    
    // Test Results
    private Dictionary<EmotionType, TestResult> _testResults = new Dictionary<EmotionType, TestResult>();
    
    [System.Serializable]
    public class TestResult
    {
        public EmotionType emotion;
        public float animationResponseTime;
        public float physicsResponseTime;
        public bool animationCompleted;
        public bool physicsActivated;
        public float performanceImpact;
    }

    void Start()
    {
        InitializeTest();
        SetupUI();
        UpdateStatus("Advanced Emotion Systems Test Manager Ready");
    }

    void Update()
    {
        if (_isRunningTest)
        {
            UpdatePerformanceMetrics();
            UpdateTestProgress();
        }
    }

    #region Initialization

    void InitializeTest()
    {
        if (_systemsManager == null)
        {
            _systemsManager = FindObjectOfType<EmotionSystemsManager>();
        }
        
        if (_testArena == null)
        {
            GameObject arena = new GameObject("Test Arena");
            arena.transform.position = Vector3.zero;
            _testArena = arena.transform;
        }
        
        // Initialize test results for all emotions
        foreach (EmotionType emotion in System.Enum.GetValues(typeof(EmotionType)))
        {
            _testResults[emotion] = new TestResult { emotion = emotion };
        }
    }

    void SetupUI()
    {
        // Setup emotion test buttons
        if (_emotionTestButtons != null)
        {
            EmotionType[] emotions = new EmotionType[]
            {
                EmotionType.Happy, EmotionType.Sad, EmotionType.Angry,
                EmotionType.Scared, EmotionType.Surprised, EmotionType.Calm
            };
            
            for (int i = 0; i < _emotionTestButtons.Length && i < emotions.Length; i++)
            {
                int index = i;
                EmotionType emotion = emotions[i];
                _emotionTestButtons[i].onClick.AddListener(() => TestSingleEmotion(emotion));
                
                // Set button color
                var buttonImage = _emotionTestButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = GetEmotionColor(emotion);
                }
                
                // Set button text
                var buttonText = _emotionTestButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = emotion.ToString();
                }
            }
        }
        
        // Setup control buttons
        _spawnButton?.onClick.AddListener(SpawnEnhancedShape);
        _clearButton?.onClick.AddListener(ClearAllShapes);
        _animationTestButton?.onClick.AddListener(RunAnimationTest);
        _physicsTestButton?.onClick.AddListener(RunPhysicsTest);
        _stressTestButton?.onClick.AddListener(RunStressTest);
        
        // Setup sliders and toggles
        _intensitySlider?.onValueChanged.AddListener(OnIntensityChanged);
        _proceduralMotionToggle?.onValueChanged.AddListener(OnProceduralMotionToggled);
        _physicsAccessoriesToggle?.onValueChanged.AddListener(OnPhysicsAccessoriesToggled);
    }

    #endregion

    #region Shape Management

    void SpawnEnhancedShape()
    {
        if (_testShapes.Count >= _maxTestShapes)
        {
            UpdateStatus($"Maximum test shapes reached ({_maxTestShapes})");
            return;
        }
        
        Vector3 spawnPos = GetRandomSpawnPosition();
        
        GameObject shapeObj;
        if (_enhancedShapePrefab != null)
        {
            shapeObj = Instantiate(_enhancedShapePrefab, spawnPos, Quaternion.identity, _testArena);
        }
        else
        {
            // Create enhanced shape using system manager
            shapeObj = _systemsManager.CreateEmotionalShape(spawnPos);
            EnhanceShapeWithAdvancedSystems(shapeObj);
        }
        
        RegisterTestShape(shapeObj);
        UpdateStatus($"Spawned enhanced shape ({_testShapes.Count}/{_maxTestShapes})");
    }

    void EnhanceShapeWithAdvancedSystems(GameObject shapeObj)
    {
        // Add enhanced animator if not present
        if (shapeObj.GetComponent<ShapeAnimator>() == null)
        {
            var animator = shapeObj.AddComponent<ShapeAnimator>();
            // Configure animator with test settings
        }
        
        // Add emotion physics if not present
        if (shapeObj.GetComponent<EmotionPhysics>() == null)
        {
            var physics = shapeObj.AddComponent<EmotionPhysics>();
            // Configure physics with test accessories
            CreateTestAccessories(physics);
        }
    }

    void CreateTestAccessories(EmotionPhysics physics)
    {
        // Create test accessories for physics simulation
        var accessories = new List<GameObject>();
        
        // Create floating orbs around the shape
        for (int i = 0; i < 3; i++)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.transform.localScale = Vector3.one * 0.2f;
            orb.transform.SetParent(physics.transform);
            orb.transform.localPosition = Random.insideUnitSphere * 1.5f;
            
            if (_debugAccessoryMaterial != null)
            {
                orb.GetComponent<Renderer>().material = _debugAccessoryMaterial;
            }
            
            accessories.Add(orb);
        }
        
        // Note: This would be configured through the EmotionPhysics component
        // physics.SetAccessories(accessories);
    }

    void RegisterTestShape(GameObject shapeObj)
    {
        var emotionSystem = shapeObj.GetComponent<EmotionSystem>();
        var shapeAnimator = shapeObj.GetComponent<ShapeAnimator>();
        var emotionPhysics = shapeObj.GetComponent<EmotionPhysics>();
        
        if (emotionSystem != null) _testShapes.Add(emotionSystem);
        if (shapeAnimator != null) _shapeAnimators.Add(shapeAnimator);
        if (emotionPhysics != null) _emotionPhysics.Add(emotionPhysics);
        
        // Subscribe to events for testing
        if (emotionSystem != null)
        {
            emotionSystem.OnEmotionChanged += OnShapeEmotionChanged;
        }
    }

    void ClearAllShapes()
    {
        foreach (var shape in _testShapes)
        {
            if (shape != null)
            {
                Destroy(shape.gameObject);
            }
        }
        
        _testShapes.Clear();
        _shapeAnimators.Clear();
        _emotionPhysics.Clear();
        
        UpdateStatus("All test shapes cleared");
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
        return _testArena.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    #endregion

    #region Individual Tests

    void TestSingleEmotion(EmotionType emotion)
    {
        if (_testShapes.Count == 0)
        {
            UpdateStatus("No test shapes available. Spawn some first!");
            return;
        }
        
        float intensity = _intensitySlider != null ? _intensitySlider.value : 0.8f;
        float testStartTime = Time.time;
        
        StartCoroutine(TestEmotionResponse(emotion, intensity, testStartTime));
    }

    IEnumerator TestEmotionResponse(EmotionType emotion, float intensity, float startTime)
    {
        UpdateStatus($"Testing {emotion} at intensity {intensity:F2}...");
        
        var result = _testResults[emotion];
        result.animationCompleted = false;
        result.physicsActivated = false;
        
        // Apply emotion to all test shapes
        foreach (var shape in _testShapes)
        {
            if (shape != null)
            {
                shape.ForceEmotion(emotion, intensity, 5f);
            }
        }
        
        // Wait for animation response
        float animationStartTime = Time.time;
        bool animationResponseDetected = false;
        
        while (Time.time - animationStartTime < 2f && !animationResponseDetected)
        {
            // Check if animations have started
            foreach (var animator in _shapeAnimators)
            {
                if (animator != null && animator.IsAnimating)
                {
                    animationResponseDetected = true;
                    result.animationResponseTime = Time.time - animationStartTime;
                    break;
                }
            }
            yield return null;
        }
        
        // Wait for physics response
        float physicsStartTime = Time.time;
        bool physicsResponseDetected = false;
        
        while (Time.time - physicsStartTime < 2f && !physicsResponseDetected)
        {
            // Check if physics systems have activated
            foreach (var physics in _emotionPhysics)
            {
                if (physics != null && physics.IsActive)
                {
                    physicsResponseDetected = true;
                    result.physicsResponseTime = Time.time - physicsStartTime;
                    break;
                }
            }
            yield return null;
        }
        
        result.animationCompleted = animationResponseDetected;
        result.physicsActivated = physicsResponseDetected;
        
        string resultText = $"{emotion} Test: Anim={result.animationCompleted}, Physics={result.physicsActivated}";
        UpdateStatus(resultText);
    }

    void RunAnimationTest()
    {
        StartCoroutine(AnimationSystemTest());
    }

    IEnumerator AnimationSystemTest()
    {
        UpdateStatus("Running comprehensive animation system test...");
        _isRunningTest = true;
        _testStartTime = Time.time;
        
        EmotionType[] testEmotions = {
            EmotionType.Happy, EmotionType.Sad, EmotionType.Angry,
            EmotionType.Surprised, EmotionType.Calm
        };
        
        foreach (var emotion in testEmotions)
        {
            UpdateStatus($"Testing animation transitions to {emotion}...");
            
            // Test at different intensities
            float[] intensities = { 0.3f, 0.6f, 1.0f };
            
            foreach (var intensity in intensities)
            {
                foreach (var shape in _testShapes)
                {
                    if (shape != null)
                    {
                        shape.ForceEmotion(emotion, intensity, 2f);
                    }
                }
                
                yield return new WaitForSeconds(3f);
            }
        }
        
        _isRunningTest = false;
        UpdateStatus("Animation system test completed!");
    }

    void RunPhysicsTest()
    {
        StartCoroutine(PhysicsSystemTest());
    }

    IEnumerator PhysicsSystemTest()
    {
        UpdateStatus("Running physics system test...");
        _isRunningTest = true;
        _testStartTime = Time.time;
        
        // Test physics responses to different emotions
        EmotionType[] physicsEmotions = {
            EmotionType.Angry,    // Should create aggressive physics
            EmotionType.Scared,   // Should create defensive physics
            EmotionType.Happy,    // Should create bouncy physics
            EmotionType.Calm      // Should create gentle physics
        };
        
        foreach (var emotion in physicsEmotions)
        {
            UpdateStatus($"Testing physics response to {emotion}...");
            
            foreach (var shape in _testShapes)
            {
                if (shape != null)
                {
                    shape.ForceEmotion(emotion, 0.9f, 5f);
                }
            }
            
            // Let physics systems respond
            yield return new WaitForSeconds(4f);
        }
        
        _isRunningTest = false;
        UpdateStatus("Physics system test completed!");
    }

    void RunStressTest()
    {
        StartCoroutine(StressTestRoutine());
    }

    IEnumerator StressTestRoutine()
    {
        UpdateStatus("Running stress test - rapid emotion changes...");
        _isRunningTest = true;
        _testStartTime = Time.time;
        
        EmotionType[] emotions = System.Enum.GetValues(typeof(EmotionType)).Cast<EmotionType>().ToArray();
        
        float testDuration = 15f;
        float endTime = Time.time + testDuration;
        
        while (Time.time < endTime)
        {
            // Rapidly change emotions
            EmotionType randomEmotion = emotions[Random.Range(0, emotions.Length)];
            float randomIntensity = Random.Range(0.2f, 1.0f);
            
            foreach (var shape in _testShapes)
            {
                if (shape != null)
                {
                    shape.ForceEmotion(randomEmotion, randomIntensity, 1f);
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        _isRunningTest = false;
        UpdateStatus($"Stress test completed! Average FPS: {_averageFPS:F1}");
    }

    #endregion

    #region Event Handlers

    void OnShapeEmotionChanged(EmotionChangeEvent changeEvent)
    {
        // Log emotion changes for testing
        Debug.Log($"Shape emotion changed: {changeEvent.oldEmotion} -> {changeEvent.newEmotion}, Intensity: {changeEvent.newIntensity:F2}");
    }

    void OnIntensityChanged(float intensity)
    {
        UpdateStatus($"Intensity set to {intensity:F2}");
    }

    void OnProceduralMotionToggled(bool enabled)
    {
        foreach (var animator in _shapeAnimators)
        {
            if (animator != null)
            {
                animator.EnableProceduralMotion = enabled;
            }
        }
        UpdateStatus($"Procedural motion {(enabled ? "enabled" : "disabled")}");
    }

    void OnPhysicsAccessoriesToggled(bool enabled)
    {
        foreach (var physics in _emotionPhysics)
        {
            if (physics != null)
            {
                physics.EnableAccessories = enabled;
            }
        }
        UpdateStatus($"Physics accessories {(enabled ? "enabled" : "disabled")}");
    }

    #endregion

    #region Performance Monitoring

    void UpdatePerformanceMetrics()
    {
        _frameCount++;
        float timeSinceStart = Time.time - _testStartTime;
        _averageFPS = _frameCount / timeSinceStart;
        
        if (_performanceText != null)
        {
            _performanceText.text = $"FPS: {_averageFPS:F1}\nShapes: {_testShapes.Count}\nAnimators: {_shapeAnimators.Count}\nPhysics: {_emotionPhysics.Count}";
        }
    }

    void UpdateTestProgress()
    {
        float elapsed = Time.time - _testStartTime;
        if (elapsed > _testDuration)
        {
            _isRunningTest = false;
        }
    }

    #endregion

    #region Utilities

    void UpdateStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
        Debug.Log($"[AdvancedEmotionTest] {message}");
    }

    Color GetEmotionColor(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => Color.yellow,
            EmotionType.Sad => Color.blue,
            EmotionType.Angry => Color.red,
            EmotionType.Scared => Color.magenta,
            EmotionType.Surprised => Color.white,
            EmotionType.Calm => Color.green,
            _ => Color.gray
        };
    }

    #endregion

    #region Debug Visualization

    void OnDrawGizmos()
    {
        if (!_showDebugGizmos) return;
        
        // Draw test arena
        if (_testArena != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_testArena.position, _spawnRadius);
        }
        
        // Draw connections between shapes
        Gizmos.color = Color.cyan;
        for (int i = 0; i < _testShapes.Count; i++)
        {
            if (_testShapes[i] == null) continue;
            
            for (int j = i + 1; j < _testShapes.Count; j++)
            {
                if (_testShapes[j] == null) continue;
                
                float distance = Vector3.Distance(_testShapes[i].transform.position, _testShapes[j].transform.position);
                if (distance < 5f) // Show close connections
                {
                    Gizmos.DrawLine(_testShapes[i].transform.position, _testShapes[j].transform.position);
                }
            }
        }
    }

    #endregion
}
