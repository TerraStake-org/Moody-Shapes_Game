# Moody Shapes - Troubleshooting Guide

This guide provides solutions to common issues you might encounter when working with the Moody Shapes systems.

## Table of Contents

1. [Emotion System Issues](#emotion-system-issues)
2. [Skill System Issues](#skill-system-issues)
3. [Visual Feedback Issues](#visual-feedback-issues)
4. [Screen Effects Issues](#screen-effects-issues)
5. [Performance Issues](#performance-issues)
6. [Unity-Specific Issues](#unity-specific-issues)
7. [Common Error Messages](#common-error-messages)

## Emotion System Issues

### Shapes Not Responding to Emotional Stimuli

**Symptoms:**
- Shapes don't change emotional state when expected
- No visual response to interactions

**Possible Causes and Solutions:**

1. **Missing Components**
   - Check that the shape has all required components: `EmotionSystem`, `EmotionalState`, `EmotionProfileHandler`
   - Solution: Add any missing components to the GameObject

2. **No Emotion Profile Assigned**
   - Check if the `EmotionProfileHandler` has a profile assigned
   - Solution: Assign an `EmotionProfileSO` in the inspector

3. **High Resistance to Emotion**
   - The shape might have high resistance to the specific emotion
   - Solution: Check the emotion profile and lower resistance values, or increase stimulus intensity

4. **Emotion Decay Too Fast**
   - Emotions might be decaying too quickly to notice
   - Solution: Reduce the decay rate in the `EmotionalState` component

5. **Low Stimulus Intensity**
   - The stimulus might be too weak to overcome the current emotional state
   - Solution: Increase the intensity of the stimulus

```csharp
// Example of creating a stronger stimulus
EmotionalStimulus stimulus = new EmotionalStimulus(
    EmotionType.Happy,
    0.9f,  // Increase intensity
    gameObject,
    10.0f  // Increase duration
);
targetSystem.ReceiveStimulus(stimulus);
```

### Unexpected Emotional Responses

**Symptoms:**
- Shape responds with unexpected emotions
- Emotional responses seem incorrect or disproportionate

**Possible Causes and Solutions:**

1. **Conflicting Emotion Flags**
   - Check if multiple emotion types are being applied simultaneously
   - Solution: Verify the `EmotionType` values being used and ensure proper flags handling

2. **Unintended Emotion Traits**
   - The shape's personality traits might be affecting responses
   - Solution: Review and adjust vulnerabilities and resistances in the emotion profile

3. **Incorrect Emotion Processing**
   - The `EmotionSystem` might be processing emotions incorrectly
   - Solution: Add debug logging to track emotion processing flow:

```csharp
// Add to EmotionSystem.ReceiveStimulus method
Debug.Log($"Received stimulus: {stimulus.EmotionType}, Intensity: {stimulus.Intensity}");
Debug.Log($"After traits processing: Intensity: {modifiedIntensity}");
```

4. **Memory Influence**
   - Past interactions might be influencing current responses
   - Solution: Check the `EmotionMemory` component for past interactions, or reset memory:

```csharp
// Clear memory of a specific shape
emotionMemory.ClearMemoryOfShape(otherShape);

// Or clear all memories
emotionMemory.ClearAllMemories();
```

### Emotion Decay Issues

**Symptoms:**
- Emotions don't fade over time
- Emotions fade too quickly

**Possible Causes and Solutions:**

1. **Incorrect Decay Rate**
   - Check the decay rate in the `EmotionalState` component
   - Solution: Adjust the decay rate (lower = slower decay)

2. **Time Scale Issues**
   - Unity's time scale might be affecting decay
   - Solution: Check if `Time.timeScale` has been modified

3. **Continuous Stimuli**
   - Something might be constantly applying stimuli
   - Solution: Debug to check for stimulus sources:

```csharp
// Add to EmotionSystem
public void ReceiveStimulus(EmotionalStimulus stimulus)
{
    Debug.Log($"Stimulus received from: {stimulus.Source?.name ?? "unknown"}");
    // Regular processing...
}
```

## Skill System Issues

### Skills Not Executing

**Symptoms:**
- Skills don't activate when used
- No effect on target shapes

**Possible Causes and Solutions:**

1. **Skill Controller Issues**
   - Check if the `EmotionSkillController` is properly set up
   - Solution: Verify references and configuration

2. **Cooldown Active**
   - The skill might be on cooldown
   - Solution: Check remaining cooldown time:

```csharp
float remainingCooldown = skillController.GetRemainingCooldown(skillIndex);
Debug.Log($"Skill cooldown: {remainingCooldown}");
```

3. **Target Out of Range**
   - Target might be outside the skill's range
   - Solution: Check distance and range calculations

4. **Missing Skill Implementation**
   - The skill type might not be properly implemented
   - Solution: Verify the skill class exists and is properly registered

5. **Null References**
   - Missing references in the skill implementation
   - Solution: Add null checks and debug logs:

```csharp
public override void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem)
{
    if (targetSystem == null)
    {
        Debug.LogError("Target system is null!");
        return;
    }
    
    // Regular implementation...
}
```

### Skills Not Loading from JSON

**Symptoms:**
- Skills aren't available or don't load
- Errors when trying to load skills

**Possible Causes and Solutions:**

1. **JSON File Location**
   - Files might not be in the correct Resources folder
   - Solution: Ensure JSON files are in `Resources/Skills/`

2. **JSON Format Errors**
   - JSON syntax might be incorrect
   - Solution: Validate JSON format using an online validator

3. **Missing Skill Type**
   - The skill type specified in JSON might not be registered
   - Solution: Add the skill type to the `SkillLoader.CreateSkillInstance` method

4. **Resources Path Issues**
   - The path to the JSON files might be incorrect
   - Solution: Check the path and ensure it's correct:

```csharp
// Debug path issues
TextAsset jsonAsset = Resources.Load<TextAsset>("Skills/YourSkillName");
Debug.Log($"JSON asset loaded: {jsonAsset != null}");
```

### Targeting System Problems

**Symptoms:**
- Can't select targets
- Wrong targets selected

**Possible Causes and Solutions:**

1. **Layer Mask Issues**
   - Target objects might not be on the expected layer
   - Solution: Check and update layer settings

2. **Raycast Problems**
   - Raycasts might be blocked by other colliders
   - Solution: Verify raycast parameters and debug with:

```csharp
// Debug raycasts
Debug.DrawRay(rayOrigin, rayDirection * maxDistance, Color.red, 1.0f);
```

3. **Camera Setup**
   - Camera might not be properly set up for raycasting
   - Solution: Check camera references and settings

4. **Missing Colliders**
   - Target objects might not have colliders
   - Solution: Add appropriate colliders to target objects

## Visual Feedback Issues

### No Visual Changes for Emotions

**Symptoms:**
- Shapes don't visually express emotions
- No animation or color changes

**Possible Causes and Solutions:**

1. **Missing Visual Components**
   - Check for `Shape_Animator` or `ShapeVisualController` components
   - Solution: Add appropriate visual components

2. **Interface Implementation**
   - Components might not implement `IShapeVisuals`
   - Solution: Ensure visual components implement the interface

3. **Animation Issues**
   - Animator Controller might not be properly set up
   - Solution: Check Animator Controller parameters and transitions

4. **Material Reference Issues**
   - Materials might not be assigned in the `ShapeVisualController`
   - Solution: Assign appropriate materials for each emotion

5. **Event Connection**
   - Visual components might not be connected to emotion events
   - Solution: Verify event connections:

```csharp
// In visual component's Start method
EmotionSystem emotionSystem = GetComponent<EmotionSystem>();
if (emotionSystem != null)
{
    emotionSystem.OnEmotionChanged += OnEmotionChanged;
    Debug.Log("Connected to emotion events");
}
```

### Animation Issues

**Symptoms:**
- Animations don't play
- Wrong animations for emotions

**Possible Causes and Solutions:**

1. **Animator Configuration**
   - Animator parameters might not match what `Shape_Animator` expects
   - Solution: Check parameter names in Animator and `Shape_Animator`

2. **Missing Animation Clips**
   - Animation clips might be missing
   - Solution: Create and assign required animation clips

3. **Transition Conditions**
   - Animation transitions might have incorrect conditions
   - Solution: Check and fix Animator transition conditions

4. **Animation Parameters**
   - Parameters might not be properly updated
   - Solution: Add debug logging to track parameter updates:

```csharp
// In Shape_Animator
void OnEmotionChanged(EmotionType emotion, float intensity)
{
    Debug.Log($"Setting animation parameters: Emotion={emotion}, Intensity={intensity}");
    // Regular implementation...
}
```

## Screen Effects Issues

### Post-Processing Effects Not Working

**Symptoms:**
- No screen-wide visual effects for emotions
- Post-processing seems disabled

**Possible Causes and Solutions:**

1. **URP Setup**
   - URP might not be properly configured
   - Solution: Check URP settings in Project Settings > Graphics

2. **Volume Component**
   - Volume component might be missing or not global
   - Solution: Add a Volume component and check "Is Global"

3. **Missing Profile**
   - Volume might not have a profile assigned
   - Solution: Create and assign a Volume Profile

4. **Effect Overrides**
   - Required effects might not be added to the profile
   - Solution: Add needed effects (Bloom, Color Adjustments, etc.)

5. **Threshold Settings**
   - Effect thresholds might be too high
   - Solution: Adjust threshold settings in `EmotionPostProcessingController`

```csharp
// Lower threshold to see effects more easily
postProcessingController.minIntensityThreshold = 0.5f;
```

### Screen Shake Not Working

**Symptoms:**
- No camera shake for intense emotions
- Shake seems disabled

**Possible Causes and Solutions:**

1. **Cinemachine Setup**
   - Cinemachine might not be properly set up
   - Solution: Check that the main camera has a CinemachineBrain component

2. **Impulse Source**
   - CinemachineImpulseSource might be missing
   - Solution: Add a CinemachineImpulseSource component

3. **Impulse Listener**
   - Camera might not have an impulse listener
   - Solution: Add a CinemachineImpulseListener to the camera

4. **Shake Settings**
   - Shake intensity might be too low
   - Solution: Increase base shake intensity in `EmotionScreenShakeController`

```csharp
// Increase shake intensity for debugging
screenShakeController.baseShakeIntensity = 1.0f;
```

5. **Position Issues**
   - Shake position might be incorrect
   - Solution: Verify position parameters in shake calls

### EmotionScreenEffects Utility Not Working

**Symptoms:**
- Utility methods don't trigger effects
- No response from utility calls

**Possible Causes and Solutions:**

1. **Initialization**
   - Utility might not be initialized
   - Solution: Call `EmotionScreenEffects.Initialize()` before use

2. **Missing Controllers**
   - Required controllers might be missing
   - Solution: Add `EmotionScreenEffectsInitializer` to set up components

3. **Reference Issues**
   - Utility might not find required components
   - Solution: Add debug logging:

```csharp
// Debug controller references
Debug.Log($"Post-processing controller: {EmotionScreenEffects.postProcessing != null}");
Debug.Log($"Screen shake controller: {EmotionScreenEffects.screenShake != null}");
```

## Performance Issues

### Slowdown with Many Emotional Shapes

**Symptoms:**
- Frame rate drops with multiple shapes
- Game becomes sluggish

**Possible Causes and Solutions:**

1. **Too Many Stimuli**
   - Many shapes might be constantly exchanging stimuli
   - Solution: Limit stimulus frequency or range

2. **Excessive Visual Effects**
   - Too many particle effects or post-processing
   - Solution: Simplify visual effects or use LOD

3. **Animation Overhead**
   - Too many animated shapes
   - Solution: Implement animation LOD based on distance or importance

4. **Physics Interactions**
   - Complex physics between shapes
   - Solution: Simplify colliders or use triggers instead

5. **Screen Effects Overuse**
   - Too frequent screen-wide effects
   - Solution: Increase thresholds for global effects

```csharp
// Example of emotion system optimization
public class EmotionSystemOptimizer : MonoBehaviour
{
    public float updateInterval = 0.2f;
    private EmotionSystem emotionSystem;
    
    private void Start()
    {
        emotionSystem = GetComponent<EmotionSystem>();
        StartCoroutine(OptimizedUpdate());
    }
    
    private IEnumerator OptimizedUpdate()
    {
        while (true)
        {
            // Update emotions less frequently
            emotionSystem.ProcessDecay();
            
            yield return new WaitForSeconds(updateInterval);
        }
    }
}
```

### Memory Leaks

**Symptoms:**
- Increasing memory usage over time
- Performance degradation with playtime

**Possible Causes and Solutions:**

1. **Uncleaned Event Subscriptions**
   - Event subscriptions might not be properly removed
   - Solution: Unsubscribe from events in OnDisable/OnDestroy:

```csharp
private void OnDisable()
{
    if (emotionSystem != null)
    {
        emotionSystem.OnEmotionChanged -= OnEmotionChanged;
    }
}
```

2. **Accumulating Memory Records**
   - `EmotionMemory` might be storing too many records
   - Solution: Implement memory limits or cleanup

3. **Coroutine Leaks**
   - Coroutines might not be stopped properly
   - Solution: Store and stop coroutines when objects are disabled

## Unity-Specific Issues

### Serialization Problems

**Symptoms:**
- Settings don't save between play sessions
- Inspector values reset unexpectedly

**Possible Causes and Solutions:**

1. **Serialization Attributes**
   - Fields might not be properly serialized
   - Solution: Mark fields with `[SerializeField]` if private

2. **Complex Types**
   - Complex types might not be serializable
   - Solution: Use `[System.Serializable]` or create custom editors

3. **Missing References**
   - References might be lost during serialization
   - Solution: Ensure references are properly established in Awake/Start

### Prefab Issues

**Symptoms:**
- Prefab instances behave differently
- Changes to prefabs don't propagate

**Possible Causes and Solutions:**

1. **Prefab Overrides**
   - Instance might have overridden values
   - Solution: Check for and reset overrides

2. **Missing Components**
   - Components might be missing in prefabs
   - Solution: Ensure all required components are on the prefab

3. **Scene References**
   - Prefab might have scene-specific references
   - Solution: Use runtime reference finding instead of serialized references

### Script Execution Order

**Symptoms:**
- Components initialize in wrong order
- Dependencies not ready when needed

**Possible Causes and Solutions:**

1. **Initialization Dependencies**
   - Scripts might assume dependencies are already initialized
   - Solution: Use explicit initialization order or callbacks

2. **Script Execution Settings**
   - Script execution order might need adjustment
   - Solution: Set script execution order in Project Settings

```csharp
// Example of safe initialization
private void Awake()
{
    // Register components but don't use them yet
    emotionSystem = GetComponent<EmotionSystem>();
}

private void Start()
{
    // Now safe to use dependencies
    if (emotionSystem != null)
    {
        emotionSystem.Initialize();
        emotionSystem.OnEmotionChanged += OnEmotionChanged;
    }
}
```

## Common Error Messages

### "NullReferenceException in EmotionSystem"

**Causes and Solutions:**

1. **Missing Component References**
   - Solution: Check that all required components are attached and references are set

2. **Uninitialized Objects**
   - Solution: Ensure objects are properly initialized before use

```csharp
// Safe component access
private EmotionalState _emotionalState;
public EmotionalState CurrentState 
{
    get
    {
        if (_emotionalState == null)
        {
            _emotionalState = GetComponent<EmotionalState>();
            if (_emotionalState == null)
            {
                Debug.LogError("EmotionalState component missing!");
                _emotionalState = gameObject.AddComponent<EmotionalState>();
            }
        }
        return _emotionalState;
    }
}
```

### "Cannot find skill type in SkillLoader"

**Causes and Solutions:**

1. **Unregistered Skill Type**
   - Solution: Add the skill type to the `SkillLoader.CreateSkillInstance` method

2. **Typo in JSON**
   - Solution: Check the spelling of skill type in JSON files

3. **Missing Script File**
   - Solution: Ensure the skill class exists in the project

### "Post-processing not available"

**Causes and Solutions:**

1. **URP Not Set Up**
   - Solution: Properly set up URP in Project Settings

2. **Missing URP Package**
   - Solution: Install URP package via Package Manager

3. **Global Volume Missing**
   - Solution: Add a Volume component and set it as global

## Advanced Troubleshooting

### Diagnostic Logging

Add comprehensive logging to track system behavior:

```csharp
public static class MoodyShapesDebug
{
    public static bool VerboseLogging = false;
    
    public static void Log(string message)
    {
        if (VerboseLogging)
        {
            Debug.Log($"[MoodyShapes] {message}");
        }
    }
    
    public static void LogEmotion(string source, EmotionType emotion, float intensity)
    {
        if (VerboseLogging)
        {
            Debug.Log($"[MoodyShapes] {source}: Emotion={emotion}, Intensity={intensity:F2}");
        }
    }
    
    public static void LogError(string message)
    {
        Debug.LogError($"[MoodyShapes] {message}");
    }
}
```

### Component Validation

Create a validation tool to check for common setup issues:

```csharp
public class MoodyShapesValidator : MonoBehaviour
{
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        // Check for emotion system components
        EmotionSystem[] emotionSystems = FindObjectsOfType<EmotionSystem>();
        Debug.Log($"Found {emotionSystems.Length} EmotionSystem components");
        
        foreach (var es in emotionSystems)
        {
            ValidateEmotionSystem(es);
        }
        
        // Check for screen effects
        ValidateScreenEffects();
        
        // Check for skill system
        ValidateSkillSystem();
    }
    
    private void ValidateEmotionSystem(EmotionSystem system)
    {
        bool valid = true;
        
        // Check for required components
        if (system.GetComponent<EmotionalState>() == null)
        {
            Debug.LogError($"Missing EmotionalState on {system.name}");
            valid = false;
        }
        
        // Check for profile
        EmotionProfileHandler handler = system.GetComponent<EmotionProfileHandler>();
        if (handler == null || handler.EmotionProfile == null)
        {
            Debug.LogError($"Missing or empty EmotionProfile on {system.name}");
            valid = false;
        }
        
        // Check for visual components
        var visuals = system.GetComponents<MonoBehaviour>().OfType<IShapeVisuals>();
        if (!visuals.Any())
        {
            Debug.LogWarning($"No IShapeVisuals implementations on {system.name}");
        }
        
        if (valid)
        {
            Debug.Log($"EmotionSystem on {system.name} is properly configured");
        }
    }
    
    private void ValidateScreenEffects()
    {
        // Check for post-processing
        EmotionPostProcessingController ppController = FindObjectOfType<EmotionPostProcessingController>();
        if (ppController == null)
        {
            Debug.LogError("Missing EmotionPostProcessingController in scene");
        }
        else
        {
            Volume volume = ppController.GetComponent<Volume>();
            if (volume == null || volume.profile == null)
            {
                Debug.LogError("Post-processing Volume not properly configured");
            }
            else if (!volume.isGlobal)
            {
                Debug.LogWarning("Post-processing Volume is not set as global");
            }
        }
        
        // Check for screen shake
        EmotionScreenShakeController shakeController = FindObjectOfType<EmotionScreenShakeController>();
        if (shakeController == null)
        {
            Debug.LogError("Missing EmotionScreenShakeController in scene");
        }
        else
        {
            var impulseSource = shakeController.GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                Debug.LogError("Missing CinemachineImpulseSource on shake controller");
            }
        }
    }
    
    private void ValidateSkillSystem()
    {
        EmotionSkillController skillController = FindObjectOfType<EmotionSkillController>();
        if (skillController == null)
        {
            Debug.LogWarning("No EmotionSkillController found in scene");
            return;
        }
        
        // Check targeting system
        SkillTargetingSystem targetingSystem = FindObjectOfType<SkillTargetingSystem>();
        if (targetingSystem == null)
        {
            Debug.LogWarning("Missing SkillTargetingSystem in scene");
        }
        
        // Check UI
        EmotionSkillUI skillUI = FindObjectOfType<EmotionSkillUI>();
        if (skillUI == null)
        {
            Debug.LogWarning("Missing EmotionSkillUI in scene");
        }
    }
}
```

### Runtime State Inspection

Create a runtime inspector for debugging:

```csharp
public class MoodyShapesInspector : MonoBehaviour
{
    public bool showWindow = true;
    public EmotionSystem selectedSystem;
    
    private Vector2 scrollPosition;
    
    private void OnGUI()
    {
        if (!showWindow) return;
        
        Rect windowRect = new Rect(10, 10, 300, 400);
        windowRect = GUILayout.Window(0, windowRect, DrawWindow, "Moody Shapes Inspector");
    }
    
    private void DrawWindow(int id)
    {
        GUILayout.Label("Emotion Systems");
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        EmotionSystem[] systems = FindObjectsOfType<EmotionSystem>();
        foreach (var system in systems)
        {
            if (GUILayout.Button(system.name))
            {
                selectedSystem = system;
            }
        }
        
        GUILayout.EndScrollView();
        
        if (selectedSystem != null)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Selected: {selectedSystem.name}");
            
            EmotionalState state = selectedSystem.CurrentState;
            if (state != null)
            {
                GUILayout.Label($"Current Emotion: {state.CurrentEmotion}");
                GUILayout.Label($"Intensity: {state.Intensity:F2}");
                
                // Show all active emotions
                GUILayout.Label("Active Emotions:");
                foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
                {
                    if (emotion == EmotionType.Neutral) continue;
                    
                    float intensity = state.GetEmotionIntensity(emotion);
                    if (intensity > 0)
                    {
                        GUILayout.Label($"- {emotion}: {intensity:F2}");
                    }
                }
            }
            
            // Debug actions
            if (GUILayout.Button("Clear Emotions"))
            {
                state?.ClearEmotions();
            }
            
            if (GUILayout.Button("Apply Happy Stimulus"))
            {
                EmotionalStimulus stimulus = new EmotionalStimulus(
                    EmotionType.Happy,
                    0.8f,
                    gameObject,
                    5.0f
                );
                selectedSystem.ReceiveStimulus(stimulus);
            }
        }
        
        GUI.DragWindow();
    }
}
```

## Contact Support

If you encounter persistent issues not covered in this guide, please:

1. Check the Moody Shapes GitHub repository for known issues
2. Ensure you're using the latest version of all packages
3. Create a detailed bug report with:
   - Unity version
   - Project configuration
   - Step-by-step reproduction instructions
   - Screenshots or videos demonstrating the issue
