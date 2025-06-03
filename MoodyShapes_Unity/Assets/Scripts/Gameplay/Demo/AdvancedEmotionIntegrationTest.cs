using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive integration test suite for the advanced emotion systems.
/// Tests the interaction between ShapeAnimator, EmotionPhysics, AnimationSetupHelper, 
/// and EmotionClipBuilder to ensure seamless integration.
/// </summary>
public class AdvancedEmotionIntegrationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool _autoRunOnStart = false;
    [SerializeField] private float _testInterval = 2f;
    [SerializeField] private int _maxTestIterations = 50;
    [SerializeField] private bool _enableDetailedLogging = true;
    
    [Header("Performance Thresholds")]
    [SerializeField] private float _maxAnimationResponseTime = 0.5f;
    [SerializeField] private float _maxPhysicsResponseTime = 0.3f;
    [SerializeField] private float _minAcceptableFPS = 30f;
    [SerializeField] private int _maxMemoryUsageMB = 500;
    
    // Test State
    private AdvancedEmotionTestManager _testManager;
    private EmotionSystemsManager _systemsManager;
    private List<IntegrationTestResult> _testResults = new List<IntegrationTestResult>();
    private bool _isRunningIntegrationTest = false;
    private float _testStartTime;
    private int _passedTests = 0;
    private int _failedTests = 0;
    
    // Performance Monitoring
    private float _frameAccumulator = 0f;
    private int _frameCount = 0;
    private float _lastMemoryCheck = 0f;
    private float _initialMemoryUsage = 0f;
    
    [System.Serializable]
    public class IntegrationTestResult
    {
        public string testName;
        public bool passed;
        public float executionTime;
        public string details;
        public float averageFPS;
        public float memoryUsage;
        public System.DateTime timestamp;
        
        public IntegrationTestResult(string name)
        {
            testName = name;
            timestamp = System.DateTime.Now;
        }
    }

    void Start()
    {
        InitializeIntegrationTest();
        
        if (_autoRunOnStart)
        {
            StartCoroutine(RunCompleteIntegrationTest());
        }
    }

    void Update()
    {
        if (_isRunningIntegrationTest)
        {
            UpdatePerformanceMonitoring();
        }
    }

    #region Initialization

    void InitializeIntegrationTest()
    {
        _testManager = GetComponent<AdvancedEmotionTestManager>();
        if (_testManager == null)
        {
            _testManager = FindObjectOfType<AdvancedEmotionTestManager>();
        }
        
        _systemsManager = FindObjectOfType<EmotionSystemsManager>();
        
        _initialMemoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        
        LogTest($"Integration Test System Initialized. Initial Memory: {_initialMemoryUsage:F2} MB");
    }

    #endregion

    #region Public Test Interface

    [ContextMenu("Run Complete Integration Test")]
    public void RunCompleteIntegrationTestMenu()
    {
        StartCoroutine(RunCompleteIntegrationTest());
    }

    [ContextMenu("Run System Compatibility Test")]
    public void RunSystemCompatibilityTest()
    {
        StartCoroutine(TestSystemCompatibility());
    }

    [ContextMenu("Run Performance Stress Test")]
    public void RunPerformanceStressTest()
    {
        StartCoroutine(TestPerformanceUnderStress());
    }

    [ContextMenu("Generate Test Report")]
    public void GenerateTestReport()
    {
        GenerateDetailedTestReport();
    }

    #endregion

    #region Core Integration Tests

    public IEnumerator RunCompleteIntegrationTest()
    {
        if (_isRunningIntegrationTest)
        {
            LogTest("Integration test already running!");
            yield break;
        }
        
        _isRunningIntegrationTest = true;
        _testStartTime = Time.time;
        _testResults.Clear();
        _passedTests = 0;
        _failedTests = 0;
        
        LogTest("=== STARTING ADVANCED EMOTION SYSTEMS INTEGRATION TEST ===");
        
        // Test 1: System Initialization
        yield return StartCoroutine(TestSystemInitialization());
        
        // Test 2: Component Compatibility
        yield return StartCoroutine(TestComponentCompatibility());
        
        // Test 3: Animation-Physics Integration
        yield return StartCoroutine(TestAnimationPhysicsIntegration());
        
        // Test 4: Emotion State Synchronization
        yield return StartCoroutine(TestEmotionStateSynchronization());
        
        // Test 5: Procedural Animation Generation
        yield return StartCoroutine(TestProceduralAnimationGeneration());
        
        // Test 6: Physics Accessory Management
        yield return StartCoroutine(TestPhysicsAccessoryManagement());
        
        // Test 7: Performance Under Load
        yield return StartCoroutine(TestPerformanceUnderLoad());
        
        // Test 8: Memory Management
        yield return StartCoroutine(TestMemoryManagement());
        
        // Test 9: Error Recovery
        yield return StartCoroutine(TestErrorRecovery());
        
        // Test 10: Multi-Shape Interaction
        yield return StartCoroutine(TestMultiShapeInteraction());
        
        _isRunningIntegrationTest = false;
        GenerateDetailedTestReport();
        
        LogTest($"=== INTEGRATION TEST COMPLETED ===");
        LogTest($"Total Tests: {_testResults.Count}, Passed: {_passedTests}, Failed: {_failedTests}");
        LogTest($"Success Rate: {(_passedTests * 100f / _testResults.Count):F1}%");
    }

    IEnumerator TestSystemInitialization()
    {
        var result = new IntegrationTestResult("System Initialization");
        float startTime = Time.time;
        
        try
        {
            // Test if all required components can be found
            bool systemsManagerFound = _systemsManager != null;
            bool testManagerFound = _testManager != null;
            
            // Test if enhanced systems can be instantiated
            GameObject testShape = new GameObject("Test Shape");
            var emotionSystem = testShape.AddComponent<EmotionSystem>();
            var shapeAnimator = testShape.AddComponent<ShapeAnimator>();
            var emotionPhysics = testShape.AddComponent<EmotionPhysics>();
            
            bool componentsAdded = emotionSystem != null && shapeAnimator != null && emotionPhysics != null;
            
            // Test basic initialization
            yield return new WaitForEndOfFrame();
            
            bool systemsInitialized = shapeAnimator.enabled && emotionPhysics.enabled;
            
            result.passed = systemsManagerFound && testManagerFound && componentsAdded && systemsInitialized;
            result.details = $"SystemsManager: {systemsManagerFound}, TestManager: {testManagerFound}, " +
                           $"Components: {componentsAdded}, Initialized: {systemsInitialized}";
            
            DestroyImmediate(testShape);
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestComponentCompatibility()
    {
        var result = new IntegrationTestResult("Component Compatibility");
        float startTime = Time.time;
        
        try
        {
            // Create test shape with all enhanced components
            GameObject testShape = new GameObject("Compatibility Test Shape");
            var emotionSystem = testShape.AddComponent<EmotionSystem>();
            var shapeAnimator = testShape.AddComponent<ShapeAnimator>();
            var emotionPhysics = testShape.AddComponent<EmotionPhysics>();
            var visualController = testShape.AddComponent<ShapeVisualController>();
            
            yield return new WaitForEndOfFrame();
            
            // Test component references
            bool animatorHasEmotionSystem = shapeAnimator.GetComponent<EmotionSystem>() != null;
            bool physicsHasEmotionSystem = emotionPhysics.GetComponent<EmotionSystem>() != null;
            
            // Test component interaction
            emotionSystem.ForceEmotion(EmotionType.Happy, 0.8f, 1f);
            yield return new WaitForSeconds(0.5f);
            
            bool animatorResponded = shapeAnimator.IsAnimating;
            bool physicsResponded = emotionPhysics.IsActive;
            
            result.passed = animatorHasEmotionSystem && physicsHasEmotionSystem && 
                           animatorResponded && physicsResponded;
            result.details = $"References OK: {animatorHasEmotionSystem && physicsHasEmotionSystem}, " +
                           $"Animation Response: {animatorResponded}, Physics Response: {physicsResponded}";
            
            DestroyImmediate(testShape);
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestAnimationPhysicsIntegration()
    {
        var result = new IntegrationTestResult("Animation-Physics Integration");
        float startTime = Time.time;
        
        try
        {
            // Create enhanced test shape
            GameObject testShape = CreateEnhancedTestShape("AnimPhysics Test");
            var emotionSystem = testShape.GetComponent<EmotionSystem>();
            var shapeAnimator = testShape.GetComponent<ShapeAnimator>();
            var emotionPhysics = testShape.GetComponent<EmotionPhysics>();
            
            yield return new WaitForEndOfFrame();
            
            // Test different emotions and their integrated responses
            EmotionType[] testEmotions = { EmotionType.Happy, EmotionType.Angry, EmotionType.Scared };
            bool[] animationResponses = new bool[testEmotions.Length];
            bool[] physicsResponses = new bool[testEmotions.Length];
            
            for (int i = 0; i < testEmotions.Length; i++)
            {
                emotionSystem.ForceEmotion(testEmotions[i], 1f, 1f);
                yield return new WaitForSeconds(0.3f);
                
                animationResponses[i] = shapeAnimator.IsAnimating && 
                                       shapeAnimator.CurrentEmotion == testEmotions[i];
                physicsResponses[i] = emotionPhysics.IsActive;
                
                yield return new WaitForSeconds(0.7f);
            }
            
            bool allAnimationsWorked = animationResponses.All(r => r);
            bool allPhysicsWorked = physicsResponses.All(r => r);
            
            result.passed = allAnimationsWorked && allPhysicsWorked;
            result.details = $"Animation Success: {allAnimationsWorked}, Physics Success: {allPhysicsWorked}";
            
            DestroyImmediate(testShape);
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestEmotionStateSynchronization()
    {
        var result = new IntegrationTestResult("Emotion State Synchronization");
        float startTime = Time.time;
        
        try
        {
            GameObject testShape = CreateEnhancedTestShape("Sync Test");
            var emotionSystem = testShape.GetComponent<EmotionSystem>();
            var shapeAnimator = testShape.GetComponent<ShapeAnimator>();
            var emotionPhysics = testShape.GetComponent<EmotionPhysics>();
            
            yield return new WaitForEndOfFrame();
            
            // Test synchronization at different intensities
            float[] testIntensities = { 0.2f, 0.5f, 0.8f, 1.0f };
            bool synchronizationMaintained = true;
            
            foreach (float intensity in testIntensities)
            {
                emotionSystem.ForceEmotion(EmotionType.Happy, intensity, 1f);
                yield return new WaitForSeconds(0.5f);
                
                // Check if all systems reflect the same emotion state
                bool animatorSynced = Mathf.Approximately(shapeAnimator.CurrentIntensity, intensity);
                bool emotionMatches = shapeAnimator.CurrentEmotion == EmotionType.Happy;
                
                if (!animatorSynced || !emotionMatches)
                {
                    synchronizationMaintained = false;
                    break;
                }
                
                yield return new WaitForSeconds(0.5f);
            }
            
            result.passed = synchronizationMaintained;
            result.details = $"State synchronization maintained across intensity changes: {synchronizationMaintained}";
            
            DestroyImmediate(testShape);
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestProceduralAnimationGeneration()
    {
        var result = new IntegrationTestResult("Procedural Animation Generation");
        float startTime = Time.time;
        
        try
        {
            GameObject testShape = CreateEnhancedTestShape("Procedural Test");
            var shapeAnimator = testShape.GetComponent<ShapeAnimator>();
            
            yield return new WaitForEndOfFrame();
            
            // Enable procedural motion
            shapeAnimator.EnableProceduralMotion = true;
            
            // Test procedural generation for different emotions
            EmotionType[] testEmotions = { EmotionType.Happy, EmotionType.Sad, EmotionType.Angry };
            bool proceduralSystemWorking = true;
            
            foreach (var emotion in testEmotions)
            {
                var emotionSystem = testShape.GetComponent<EmotionSystem>();
                emotionSystem.ForceEmotion(emotion, 0.8f, 2f);
                
                yield return new WaitForSeconds(0.5f);
                
                // Check if procedural motion is active and responding
                if (!shapeAnimator.IsAnimating || !shapeAnimator.EnableProceduralMotion)
                {
                    proceduralSystemWorking = false;
                    break;
                }
                
                yield return new WaitForSeconds(1.5f);
            }
            
            result.passed = proceduralSystemWorking;
            result.details = $"Procedural animation generation working: {proceduralSystemWorking}";
            
            DestroyImmediate(testShape);
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestPhysicsAccessoryManagement()
    {
        var result = new IntegrationTestResult("Physics Accessory Management");
        float startTime = Time.time;
        
        try
        {
            GameObject testShape = CreateEnhancedTestShape("Accessory Test");
            var emotionPhysics = testShape.GetComponent<EmotionPhysics>();
            
            yield return new WaitForEndOfFrame();
            
            // Test accessory enable/disable
            emotionPhysics.EnableAccessories = true;
            yield return new WaitForSeconds(0.5f);
            
            int accessoryCountEnabled = emotionPhysics.AccessoryCount;
            
            emotionPhysics.EnableAccessories = false;
            yield return new WaitForSeconds(0.5f);
            
            int accessoryCountDisabled = emotionPhysics.AccessoryCount;
            
            // Test accessory response to emotions
            emotionPhysics.EnableAccessories = true;
            var emotionSystem = testShape.GetComponent<EmotionSystem>();
            emotionSystem.ForceEmotion(EmotionType.Angry, 1f, 1f);
            
            yield return new WaitForSeconds(1f);
            
            bool accessoriesResponded = emotionPhysics.IsActive;
            
            result.passed = accessoryCountEnabled > accessoryCountDisabled && accessoriesResponded;
            result.details = $"Accessories managed: {accessoryCountEnabled > accessoryCountDisabled}, " +
                           $"Response to emotion: {accessoriesResponded}";
            
            DestroyImmediate(testShape);
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestPerformanceUnderLoad()
    {
        var result = new IntegrationTestResult("Performance Under Load");
        float startTime = Time.time;
        
        try
        {
            List<GameObject> testShapes = new List<GameObject>();
            
            // Create multiple enhanced shapes
            for (int i = 0; i < 10; i++)
            {
                var shape = CreateEnhancedTestShape($"Performance Test {i}");
                shape.transform.position = Random.insideUnitSphere * 5f;
                testShapes.Add(shape);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Monitor performance during intensive operations
            float fpsStart = 1f / Time.unscaledDeltaTime;
            
            // Trigger all shapes simultaneously
            foreach (var shape in testShapes)
            {
                var emotionSystem = shape.GetComponent<EmotionSystem>();
                emotionSystem.ForceEmotion(EmotionType.Happy, Random.Range(0.5f, 1f), 2f);
            }
            
            yield return new WaitForSeconds(2f);
            
            float fpsEnd = 1f / Time.unscaledDeltaTime;
            float averageFPS = (fpsStart + fpsEnd) / 2f;
            
            bool performanceAcceptable = averageFPS >= _minAcceptableFPS;
            
            result.passed = performanceAcceptable;
            result.details = $"Average FPS: {averageFPS:F1}, Threshold: {_minAcceptableFPS}";
            result.averageFPS = averageFPS;
            
            // Cleanup
            foreach (var shape in testShapes)
            {
                DestroyImmediate(shape);
            }
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestMemoryManagement()
    {
        var result = new IntegrationTestResult("Memory Management");
        float startTime = Time.time;
        
        try
        {
            float memoryBefore = System.GC.GetTotalMemory(true) / (1024f * 1024f);
            
            // Create and destroy many shapes to test memory leaks
            for (int cycle = 0; cycle < 5; cycle++)
            {
                List<GameObject> shapes = new List<GameObject>();
                
                for (int i = 0; i < 20; i++)
                {
                    shapes.Add(CreateEnhancedTestShape($"Memory Test {i}"));
                }
                
                yield return new WaitForSeconds(0.5f);
                
                foreach (var shape in shapes)
                {
                    DestroyImmediate(shape);
                }
                
                yield return new WaitForSeconds(0.2f);
            }
            
            System.GC.Collect();
            yield return new WaitForSeconds(0.5f);
            
            float memoryAfter = System.GC.GetTotalMemory(true) / (1024f * 1024f);
            float memoryIncrease = memoryAfter - memoryBefore;
            
            bool memoryManagementGood = memoryIncrease < 50f; // Less than 50MB increase
            
            result.passed = memoryManagementGood;
            result.details = $"Memory increase: {memoryIncrease:F2} MB (Before: {memoryBefore:F2}, After: {memoryAfter:F2})";
            result.memoryUsage = memoryAfter;
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestErrorRecovery()
    {
        var result = new IntegrationTestResult("Error Recovery");
        float startTime = Time.time;
        
        try
        {
            GameObject testShape = CreateEnhancedTestShape("Error Recovery Test");
            var emotionSystem = testShape.GetComponent<EmotionSystem>();
            var shapeAnimator = testShape.GetComponent<ShapeAnimator>();
            
            yield return new WaitForEndOfFrame();
            
            bool recoverySuccessful = true;
            
            // Test recovery from invalid emotion values
            try
            {
                emotionSystem.ForceEmotion((EmotionType)999, -1f, -1f);
                yield return new WaitForSeconds(0.5f);
                
                // System should still be functional
                if (!shapeAnimator.enabled || testShape == null)
                {
                    recoverySuccessful = false;
                }
            }
            catch
            {
                // Exception handling is part of recovery
            }
            
            // Test recovery from component destruction
            if (recoverySuccessful)
            {
                DestroyImmediate(emotionSystem);
                yield return new WaitForSeconds(0.2f);
                
                // Animator should handle missing emotion system gracefully
                if (shapeAnimator != null && shapeAnimator.enabled)
                {
                    // System maintains stability
                }
                else
                {
                    recoverySuccessful = false;
                }
            }
            
            result.passed = recoverySuccessful;
            result.details = $"Error recovery successful: {recoverySuccessful}";
            
            if (testShape != null)
                DestroyImmediate(testShape);
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestMultiShapeInteraction()
    {
        var result = new IntegrationTestResult("Multi-Shape Interaction");
        float startTime = Time.time;
        
        try
        {
            List<GameObject> shapes = new List<GameObject>();
            
            // Create multiple interacting shapes
            for (int i = 0; i < 5; i++)
            {
                var shape = CreateEnhancedTestShape($"Interaction Test {i}");
                shape.transform.position = new Vector3(i * 2f, 0, 0);
                shapes.Add(shape);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Test cascading emotions
            var firstShape = shapes[0].GetComponent<EmotionSystem>();
            firstShape.ForceEmotion(EmotionType.Happy, 1f, 3f);
            
            yield return new WaitForSeconds(1f);
            
            // Check if emotions propagated (if social system is active)
            bool interactionWorking = true;
            foreach (var shape in shapes)
            {
                var animator = shape.GetComponent<ShapeAnimator>();
                var physics = shape.GetComponent<EmotionPhysics>();
                
                if (!animator.enabled || !physics.enabled)
                {
                    interactionWorking = false;
                    break;
                }
            }
            
            result.passed = interactionWorking;
            result.details = $"Multi-shape interaction systems stable: {interactionWorking}";
            
            // Cleanup
            foreach (var shape in shapes)
            {
                DestroyImmediate(shape);
            }
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    #endregion

    #region Additional Test Methods

    IEnumerator TestSystemCompatibility()
    {
        var result = new IntegrationTestResult("System Compatibility Check");
        float startTime = Time.time;
        
        try
        {
            // Test compatibility with existing systems
            bool emotionSystemCompatible = FindObjectOfType<EmotionSystem>() != null;
            bool visualControllerCompatible = FindObjectOfType<ShapeVisualController>() != null;
            bool audioSystemCompatible = FindObjectOfType<ShapeAudioFeedback>() != null;
            
            result.passed = emotionSystemCompatible && visualControllerCompatible;
            result.details = $"Emotion: {emotionSystemCompatible}, Visual: {visualControllerCompatible}, Audio: {audioSystemCompatible}";
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TestPerformanceUnderStress()
    {
        var result = new IntegrationTestResult("Performance Stress Test");
        float startTime = Time.time;
        
        try
        {
            List<GameObject> stressShapes = new List<GameObject>();
            
            // Create maximum number of shapes
            for (int i = 0; i < 25; i++)
            {
                var shape = CreateEnhancedTestShape($"Stress Test {i}");
                shape.transform.position = Random.insideUnitSphere * 10f;
                stressShapes.Add(shape);
            }
            
            yield return new WaitForSeconds(1f);
            
            float stressFPS = 0f;
            int samples = 0;
            
            // Run stress test for several seconds
            for (int frame = 0; frame < 120; frame++) // ~2 seconds at 60fps
            {
                // Trigger random emotions on random shapes
                var randomShape = stressShapes[Random.Range(0, stressShapes.Count)];
                var emotionSystem = randomShape.GetComponent<EmotionSystem>();
                var randomEmotion = (EmotionType)Random.Range(0, System.Enum.GetValues(typeof(EmotionType)).Length);
                
                emotionSystem.ForceEmotion(randomEmotion, Random.Range(0.1f, 1f), Random.Range(0.5f, 2f));
                
                stressFPS += 1f / Time.unscaledDeltaTime;
                samples++;
                
                yield return null;
            }
            
            float averageStressFPS = stressFPS / samples;
            bool stressTestPassed = averageStressFPS >= _minAcceptableFPS * 0.7f; // Allow 30% performance drop under stress
            
            result.passed = stressTestPassed;
            result.details = $"Stress test FPS: {averageStressFPS:F1} (Threshold: {_minAcceptableFPS * 0.7f:F1})";
            result.averageFPS = averageStressFPS;
            
            // Cleanup
            foreach (var shape in stressShapes)
            {
                DestroyImmediate(shape);
            }
        }
        catch (System.Exception e)
        {
            result.passed = false;
            result.details = $"Exception: {e.Message}";
        }
        
        result.executionTime = Time.time - startTime;
        RecordTestResult(result);
        
        yield return new WaitForSeconds(0.1f);
    }

    #endregion

    #region Utility Methods

    GameObject CreateEnhancedTestShape(string name)
    {
        GameObject shape = new GameObject(name);
        
        // Add essential components
        var emotionSystem = shape.AddComponent<EmotionSystem>();
        var shapeAnimator = shape.AddComponent<ShapeAnimator>();
        var emotionPhysics = shape.AddComponent<EmotionPhysics>();
        var visualController = shape.AddComponent<ShapeVisualController>();
        
        // Add basic renderer for visibility
        var renderer = shape.AddComponent<MeshRenderer>();
        var meshFilter = shape.AddComponent<MeshFilter>();
        
        // Use a simple cube mesh for testing
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        renderer.material = new Material(Shader.Find("Standard"));
        
        return shape;
    }

    void RecordTestResult(IntegrationTestResult result)
    {
        result.averageFPS = GetCurrentFPS();
        result.memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        
        _testResults.Add(result);
        
        if (result.passed)
        {
            _passedTests++;
        }
        else
        {
            _failedTests++;
        }
        
        LogTest($"[{(result.passed ? "PASS" : "FAIL")}] {result.testName}: {result.details} ({result.executionTime:F3}s)");
    }

    void UpdatePerformanceMonitoring()
    {
        _frameAccumulator += Time.unscaledDeltaTime;
        _frameCount++;
        
        if (Time.time - _lastMemoryCheck > 1f)
        {
            _lastMemoryCheck = Time.time;
            float currentMemory = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            
            if (currentMemory > _maxMemoryUsageMB)
            {
                LogTest($"WARNING: Memory usage exceeded threshold: {currentMemory:F2} MB");
            }
        }
    }

    float GetCurrentFPS()
    {
        if (_frameCount > 0)
        {
            return _frameCount / _frameAccumulator;
        }
        return 1f / Time.unscaledDeltaTime;
    }

    void GenerateDetailedTestReport()
    {
        LogTest("\n=== DETAILED INTEGRATION TEST REPORT ===");
        LogTest($"Test Run: {System.DateTime.Now}");
        LogTest($"Total Duration: {Time.time - _testStartTime:F2} seconds");
        LogTest($"Unity Version: {Application.unityVersion}");
        LogTest($"Platform: {Application.platform}");
        
        LogTest($"\nOVERALL RESULTS:");
        LogTest($"Total Tests: {_testResults.Count}");
        LogTest($"Passed: {_passedTests}");
        LogTest($"Failed: {_failedTests}");
        LogTest($"Success Rate: {(_passedTests * 100f / Mathf.Max(1, _testResults.Count)):F1}%");
        
        LogTest($"\nPERFORMANCE SUMMARY:");
        if (_testResults.Count > 0)
        {
            float avgFPS = _testResults.Where(r => r.averageFPS > 0).Average(r => r.averageFPS);
            float avgMemory = _testResults.Where(r => r.memoryUsage > 0).Average(r => r.memoryUsage);
            
            LogTest($"Average FPS: {avgFPS:F1}");
            LogTest($"Average Memory Usage: {avgMemory:F2} MB");
            LogTest($"Memory Increase: {(avgMemory - _initialMemoryUsage):F2} MB");
        }
        
        LogTest($"\nFAILED TESTS:");
        foreach (var result in _testResults.Where(r => !r.passed))
        {
            LogTest($"- {result.testName}: {result.details}");
        }
        
        LogTest($"\nTEST EXECUTION TIMES:");
        foreach (var result in _testResults.OrderByDescending(r => r.executionTime))
        {
            LogTest($"- {result.testName}: {result.executionTime:F3}s");
        }
        
        LogTest("=== END REPORT ===\n");
    }

    void LogTest(string message)
    {
        if (_enableDetailedLogging)
        {
            Debug.Log($"[IntegrationTest] {message}");
        }
    }

    #endregion

    #region Unity Editor Integration

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_isRunningIntegrationTest)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
            
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                $"Integration Test Running\nPassed: {_passedTests} | Failed: {_failedTests}");
        }
    }
#endif

    #endregion
}