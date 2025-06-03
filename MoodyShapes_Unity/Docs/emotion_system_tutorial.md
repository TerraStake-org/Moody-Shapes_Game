# Emotion System Tutorial

This tutorial guide explains how to work with the Emotion System in Moody Shapes, including examples, best practices, and advanced usage patterns.

## Introduction to the Emotion System

The Emotion System is the heart of Moody Shapes, providing shapes with the ability to:
- Experience and express emotions
- Remember emotional interactions
- Develop unique personalities
- Respond to player actions and other shapes

## Core Components

### 1. EmotionType

The `EmotionType` enum defines all possible emotions in the game:

```csharp
[Flags]
public enum EmotionType
{
    Neutral = 0,
    Happy = 1 << 0,
    Sad = 1 << 1,
    Angry = 1 << 2,
    // ...other emotions
    
    // Composite emotions
    Bittersweet = Happy | Sad,
    Anxious = Scared | Curious
}
```

Being a flags enum, it allows shapes to experience multiple emotions simultaneously (composite emotions).

### 2. EmotionalState

The `EmotionalState` class manages the current emotional state of a shape:

```csharp
// Basic usage example
emotionalState.SetEmotion(EmotionType.Happy, 0.8f);
emotionalState.BlendEmotion(EmotionType.Curious, 0.4f);

// Get current emotion
EmotionType currentEmotion = emotionalState.CurrentEmotion;
float intensity = emotionalState.Intensity;
```

### 3. EmotionTraits

The `EmotionTraits` class defines personality traits that affect how shapes respond to stimuli:

```csharp
// Example traits setup
traits.AddVulnerability(EmotionType.Sad, 1.5f); // 50% more susceptible to sadness
traits.AddResistance(EmotionType.Angry, 0.7f);  // 30% more resistant to anger
```

### 4. EmotionProfileSO

A ScriptableObject that defines a shape's emotional personality:

```csharp
// Create a new emotion profile asset
// Assets > Create > Moody Shapes > Emotion Profile
```

## Working with Emotions

### Creating Emotional Shapes

1. **Basic Setup**:
   ```csharp
   // Add these components to your shape GameObject
   EmotionSystem emotionSystem = gameObject.AddComponent<EmotionSystem>();
   EmotionalState emotionalState = gameObject.AddComponent<EmotionalState>();
   EmotionMemory emotionMemory = gameObject.AddComponent<EmotionMemory>();
   EmotionProfileHandler profileHandler = gameObject.AddComponent<EmotionProfileHandler>();
   
   // Assign an emotion profile
   profileHandler.EmotionProfile = yourEmotionProfileSO;
   ```

2. **Visual Feedback**:
   ```csharp
   // Add visual components to show emotions
   Shape_Animator animator = gameObject.AddComponent<Shape_Animator>();
   ShapeVisualController visualController = gameObject.AddComponent<ShapeVisualController>();
   ```

### Triggering Emotions

1. **Using Direct Methods**:
   ```csharp
   // Set a new emotion (replaces current emotion)
   emotionalState.SetEmotion(EmotionType.Happy, 0.8f);
   
   // Blend with existing emotion
   emotionalState.BlendEmotion(EmotionType.Curious, 0.4f);
   
   // Clear all emotions
   emotionalState.ClearEmotions();
   ```

2. **Using Stimuli** (Recommended):
   ```csharp
   // Create an emotional stimulus
   EmotionalStimulus stimulus = new EmotionalStimulus(
       EmotionType.Joyful,   // Type of emotion
       0.75f,                // Base intensity
       gameObject,           // Source of the stimulus
       10f                   // Duration in seconds
   );
   
   // Apply the stimulus to a shape
   targetShapeEmotionSystem.ReceiveStimulus(stimulus);
   ```

3. **Using Skills**:
   ```csharp
   // See the Skill System Tutorial for examples
   ```

### Reading Emotional States

```csharp
// Get current dominant emotion
EmotionType currentEmotion = emotionalState.CurrentEmotion;

// Get current intensity
float intensity = emotionalState.Intensity;

// Check if a shape is feeling a specific emotion
bool isHappy = emotionalState.HasEmotion(EmotionType.Happy);

// Get intensity of a specific emotion
float sadnessLevel = emotionalState.GetEmotionIntensity(EmotionType.Sad);
```

### Emotional Memory

Shapes can remember past emotional interactions:

```csharp
// Get a shape's memory of another shape
EmotionMemoryRecord memory = emotionMemory.GetMemoryOfShape(otherShape);

// Check if a shape has positive memories of another
bool hasPositiveMemory = memory != null && memory.OverallSentiment > 0.5f;

// Record a new interaction
emotionMemory.RecordInteraction(otherShape, EmotionType.Happy, 0.8f);
```

## Advanced Usage

### Creating Custom Emotion Profiles

1. Create a new Emotion Profile asset (Assets > Create > Moody Shapes > Emotion Profile)
2. Configure base emotions and traits:
   - **Base Emotions**: Default emotional state
   - **Emotional Traits**: Vulnerabilities and resistances
   - **Response Patterns**: How to respond to specific stimuli
   - **Decay Rate**: How quickly emotions fade over time

### Implementing Custom Visual Feedback

Create your own implementation of the `IShapeVisuals` interface:

```csharp
public class MyCustomVisuals : MonoBehaviour, IShapeVisuals
{
    public void OnEmotionChanged(EmotionType emotion, float intensity)
    {
        // Your custom visual effects here
        // Examples:
        // - Change colors based on emotion
        // - Play particle effects
        // - Modify shape geometry
    }
}
```

### Creating Composite Emotions

You can create complex emotional states by combining basic emotions:

```csharp
// Create a complex emotional state
emotionalState.SetEmotion(EmotionType.Happy, 0.6f);
emotionalState.BlendEmotion(EmotionType.Sad, 0.4f);
// Result approximates EmotionType.Bittersweet

// Or use predefined composite emotions
emotionalState.SetEmotion(EmotionType.Bittersweet, 0.7f);
```

### Integration with Screen Effects

For dramatic moments, trigger screen-wide effects:

```csharp
// Trigger a screen-wide emotion pulse
EmotionScreenEffects.TriggerEmotionPulse(EmotionType.Surprised, 0.9f, 1.5f);

// Trigger screen shake
EmotionScreenEffects.TriggerScreenShake(EmotionType.Angry, 0.8f, transform.position);

// Trigger combined effects
EmotionScreenEffects.TriggerCombinedEffect(EmotionType.Joyful, 1.0f, transform.position);
```

## Common Patterns and Examples

### Creating a Mood-Changing Puzzle

```csharp
// Example: A puzzle that requires changing a shape's mood
public class MoodPuzzle : MonoBehaviour
{
    public EmotionSystem targetShape;
    public EmotionType requiredEmotion;
    public float requiredIntensity = 0.7f;
    public GameObject rewardObject;
    
    private void Update()
    {
        // Check if shape has the required emotion at sufficient intensity
        if (targetShape.CurrentState.HasEmotion(requiredEmotion) && 
            targetShape.CurrentState.GetEmotionIntensity(requiredEmotion) >= requiredIntensity)
        {
            // Puzzle solved!
            rewardObject.SetActive(true);
            enabled = false; // Disable this script
        }
    }
}
```

### Creating Emotional Chain Reactions

```csharp
// Example: When one shape gets happy, it spreads to nearby shapes
public class EmotionSpreader : MonoBehaviour
{
    public EmotionSystem emotionSystem;
    public float spreadRadius = 5f;
    public LayerMask shapeLayer;
    
    private void Start()
    {
        emotionSystem.OnEmotionChanged += SpreadEmotion;
    }
    
    private void SpreadEmotion(EmotionType emotion, float intensity)
    {
        // Only spread happiness
        if (emotion.HasFlag(EmotionType.Happy) && intensity > 0.6f)
        {
            // Find nearby shapes
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, spreadRadius, shapeLayer);
            
            // Create a happiness stimulus
            EmotionalStimulus stimulus = new EmotionalStimulus(
                EmotionType.Happy,
                intensity * 0.7f, // Slightly weaker than original
                gameObject,
                5f // Duration
            );
            
            // Apply to all nearby shapes
            foreach (var hitCollider in hitColliders)
            {
                EmotionSystem otherSystem = hitCollider.GetComponent<EmotionSystem>();
                if (otherSystem != null && otherSystem != emotionSystem)
                {
                    otherSystem.ReceiveStimulus(stimulus);
                }
            }
        }
    }
}
```

## Troubleshooting

### Common Issues

1. **Emotions Not Showing Visually**
   - Ensure shape has visual components (Shape_Animator, ShapeVisualController)
   - Check that animations are properly set up
   - Verify emotion intensity is high enough (usually > 0.5f)

2. **Emotions Changing Too Quickly/Slowly**
   - Adjust decay rates in EmotionalState
   - Check for conflicting stimuli

3. **Unexpected Emotional Responses**
   - Review emotion profile settings
   - Check for vulnerabilities/resistances
   - Look for interaction history in EmotionMemory

## Best Practices

1. **Use Stimuli Instead of Direct Emotion Setting**
   - Stimuli respect the shape's personality traits
   - Stimuli can be tracked in memory
   - Stimuli provide more natural transitions

2. **Design Personality-Driven Puzzles**
   - Different shapes should respond differently to the same stimulus
   - Create puzzles that require understanding personality traits
   - Allow multiple solutions based on emotional approaches

3. **Provide Clear Visual Feedback**
   - Each emotion should have distinct visual characteristics
   - Intensity should be clearly communicated
   - Use screen effects for dramatic emotional moments

## Next Steps

Now that you understand the Emotion System, explore these related tutorials:
- Skill System Tutorial
- Creating Custom Emotion Profiles
- Animation and Visual Feedback Systems
