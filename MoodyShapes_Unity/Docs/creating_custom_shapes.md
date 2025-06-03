# Creating Custom Shapes Guide

This guide provides step-by-step instructions for creating new shapes with unique personalities and emotional behaviors in Moody Shapes.

## Overview

Creating custom shapes involves:
1. Setting up the base GameObject with required components
2. Defining emotional personality traits
3. Creating visual representations for different emotions
4. Configuring interaction behaviors
5. Integrating with puzzles and game mechanics

## Basic Shape Setup

### 1. Create Base GameObject

```
1. In the Hierarchy window, right-click and select "Create Empty"
2. Rename it to something descriptive (e.g., "Shape_Circle_Shy")
3. Add a collider component (SphereCollider, BoxCollider, etc.)
4. Add a Rigidbody component (if the shape will use physics)
5. Set the layer to "EmotionalShapes" for proper targeting
```

### 2. Add Core Components

```
Add the following components to your GameObject:

1. EmotionSystem (manages overall emotion processing)
2. EmotionalState (tracks current emotional state)
3. EmotionMemory (remembers past interactions)
4. EmotionProfileHandler (manages personality traits)
5. ShapeInteractionHandler (handles player interactions)
```

### 3. Create Visual Representation

You have several options for visuals:

#### Option A: Using Basic Meshes
```
1. Add a MeshFilter component
2. Select a basic shape (Sphere, Cube, etc.)
3. Add a MeshRenderer component
4. Create a material for the shape
```

#### Option B: Using Sprites (2D)
```
1. Add a SpriteRenderer component
2. Assign a base sprite
```

#### Option C: Using Custom Models
```
1. Import your custom model
2. Drag the model as a child of your shape GameObject
3. Configure materials and renderers
```

### 4. Add Visualization Components

```
Add one or more of these components:

1. Shape_Animator (for animation-based visualization)
2. ShapeVisualController (for material/color-based visualization)
3. Your own custom IShapeVisuals implementation
```

## Creating an Emotion Profile

### 1. Create a ScriptableObject Asset

```
1. In Project window, right-click and select:
   Create > Moody Shapes > Emotion Profile
2. Name it appropriately (e.g., "ShyCircle_Profile")
```

### 2. Configure Base Personality

```
In the Inspector for your new Emotion Profile:

1. Set Base Emotion (the default emotional state)
   - Emotion Type: (e.g., Neutral, Calm, Curious)
   - Intensity: (0.0-1.0, typically start at 0.5)

2. Configure Personality Traits
   - Vulnerabilities (emotions this shape is sensitive to)
   - Resistances (emotions this shape doesn't respond to strongly)

3. Set Emotion Decay Rate
   - How quickly emotions fade over time
   - (0.0-1.0, smaller values = slower decay)
```

### 3. Assign Profile to Shape

```
1. Select your shape GameObject
2. Find the EmotionProfileHandler component
3. Drag your Emotion Profile asset to the "Emotion Profile" field
```

## Visual Emotion Expression

### Method 1: Using Shape_Animator

#### 1. Set Up Animations
```
1. Create animations for each emotional state:
   - Neutral_Idle
   - Happy_Idle
   - Sad_Idle
   - etc.

2. Create an Animator Controller
   - Right-click in Project window: Create > Animator Controller
   - Name it after your shape (e.g., "ShyCircle_Controller")

3. Set up Animator transitions:
   - Create parameters for each emotion type and intensity
   - Create transitions between states based on emotions
```

#### 2. Configure Shape_Animator
```
1. Select your shape GameObject
2. In the Shape_Animator component:
   - Assign the Animator Controller
   - Configure Emotion Parameter Names:
     - Emotion Parameter: "Emotion" (or your custom name)
     - Intensity Parameter: "Intensity" (or your custom name)
```

### Method 2: Using ShapeVisualController

#### 1. Create Materials for Emotions
```
1. Create materials for each emotional state:
   - Neutral_Material
   - Happy_Material
   - Sad_Material
   - etc.

2. Configure each material with appropriate:
   - Colors
   - Emission
   - Other visual properties
```

#### 2. Configure ShapeVisualController
```
1. Select your shape GameObject
2. In the ShapeVisualController component:
   - Assign Renderer: drag your MeshRenderer/SpriteRenderer
   - Add Emotion Materials:
     - For each emotion type, assign the corresponding material
```

## Adding Special Behaviors

### 1. Create a Custom Behavior Script
```csharp
public class MyShapeBehavior : MonoBehaviour
{
    public EmotionSystem emotionSystem;
    public float moveSpeed = 2f;
    
    private void Start()
    {
        emotionSystem = GetComponent<EmotionSystem>();
        
        // Subscribe to emotion changes
        emotionSystem.OnEmotionChanged += OnEmotionChanged;
    }
    
    private void OnEmotionChanged(EmotionType emotion, float intensity)
    {
        // Custom behavior based on emotions
        if (emotion.HasFlag(EmotionType.Happy) && intensity > 0.7f)
        {
            // Happy behavior (e.g., jump, spin)
            StartCoroutine(JumpForJoy());
        }
        else if (emotion.HasFlag(EmotionType.Sad) && intensity > 0.6f)
        {
            // Sad behavior (e.g., shrink, darken)
            transform.localScale = Vector3.one * (1.0f - (intensity * 0.3f));
        }
    }
    
    private IEnumerator JumpForJoy()
    {
        Vector3 startPos = transform.position;
        float jumpHeight = 1.5f;
        float jumpDuration = 0.5f;
        float startTime = Time.time;
        
        while (Time.time < startTime + jumpDuration)
        {
            float progress = (Time.time - startTime) / jumpDuration;
            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            transform.position = new Vector3(
                startPos.x, 
                startPos.y + height, 
                startPos.z
            );
            yield return null;
        }
        
        transform.position = startPos;
    }
}
```

### 2. Create Custom Interaction Responses
```csharp
public class MyShapeInteraction : MonoBehaviour
{
    public EmotionSystem emotionSystem;
    public ParticleSystem interactionParticles;
    
    private void Start()
    {
        emotionSystem = GetComponent<EmotionSystem>();
        
        // Get or add ShapeInteractionHandler
        ShapeInteractionHandler interactionHandler = 
            GetComponent<ShapeInteractionHandler>() ?? 
            gameObject.AddComponent<ShapeInteractionHandler>();
            
        // Subscribe to interaction events
        interactionHandler.OnInteractionStarted += OnInteraction;
    }
    
    private void OnInteraction(GameObject interactor)
    {
        // Respond to player interaction
        EmotionalStimulus stimulus = new EmotionalStimulus(
            EmotionType.Curious,
            0.6f,
            interactor,
            3.0f
        );
        
        emotionSystem.ReceiveStimulus(stimulus);
        
        // Visual feedback
        if (interactionParticles != null)
        {
            interactionParticles.Play();
        }
    }
}
```

## Shape Examples

### Example 1: Shy Circle

```
GameObject: Shape_Circle_Shy

Components:
- SphereCollider
- Rigidbody (kinematic)
- EmotionSystem
- EmotionalState
- EmotionMemory
- EmotionProfileHandler
- ShapeInteractionHandler
- Shape_Animator
- MeshRenderer
- MeshFilter (Sphere)

Emotion Profile:
- Base Emotion: Neutral (0.5 intensity)
- Vulnerabilities: Scared (1.5x), Surprised (1.3x)
- Resistances: Angry (0.7x), Joyful (0.8x)
- Decay Rate: 0.2 (slow)

Behavior:
- Hides (shrinks) when Scared > 0.6
- Gradually approaches player when Happy > 0.7
- Moves away from other shapes when Surprised
```

### Example 2: Angry Triangle

```
GameObject: Shape_Triangle_Angry

Components:
- MeshCollider
- Rigidbody
- EmotionSystem
- EmotionalState
- EmotionMemory
- EmotionProfileHandler
- ShapeInteractionHandler
- ShapeVisualController
- MeshRenderer
- MeshFilter (Custom triangle mesh)

Emotion Profile:
- Base Emotion: Angry (0.4 intensity)
- Vulnerabilities: Angry (1.4x), Frustrated (1.3x)
- Resistances: Happy (0.6x), Calm (0.7x)
- Decay Rate: 0.4 (medium)

Behavior:
- Grows spikes when Angry > 0.8
- Charges at nearest shape when Frustrated > 0.7
- Calms down (reduces Angry) when exposed to Calm for > 5 seconds
```

### Example 3: Curious Square

```
GameObject: Shape_Square_Curious

Components:
- BoxCollider
- Rigidbody
- EmotionSystem
- EmotionalState
- EmotionMemory
- EmotionProfileHandler
- ShapeInteractionHandler
- Shape_Animator
- ShapeVisualController
- MeshRenderer
- MeshFilter (Cube)

Emotion Profile:
- Base Emotion: Curious (0.6 intensity)
- Vulnerabilities: Surprised (1.3x), Joyful (1.2x)
- Resistances: Sad (0.8x), Frustrated (0.9x)
- Decay Rate: 0.3 (medium-slow)

Behavior:
- Follows player when Curious > 0.7
- Spins when Joyful > 0.8
- Changes color based on dominant emotion
- Remembers and seeks shapes that previously made it Happy
```

## Integration with Puzzles

### 1. Emotion-Triggered Mechanisms

```csharp
public class EmotionTrigger : MonoBehaviour
{
    public EmotionType requiredEmotion;
    public float requiredIntensity = 0.7f;
    public EmotionSystem targetShape;
    
    public UnityEvent onTriggered;
    
    private bool activated = false;
    
    private void Update()
    {
        if (!activated && 
            targetShape.CurrentState.HasEmotion(requiredEmotion) && 
            targetShape.CurrentState.GetEmotionIntensity(requiredEmotion) >= requiredIntensity)
        {
            activated = true;
            onTriggered.Invoke();
        }
        else if (activated && 
                (!targetShape.CurrentState.HasEmotion(requiredEmotion) || 
                targetShape.CurrentState.GetEmotionIntensity(requiredEmotion) < requiredIntensity))
        {
            activated = false;
            // Optional: Add event for deactivation
        }
    }
}
```

### 2. Emotional Chain Reactions

```csharp
public class EmotionalChainReaction : MonoBehaviour
{
    public EmotionSystem[] shapes;
    public EmotionType triggerEmotion;
    public float triggerIntensity = 0.8f;
    public EmotionType chainEmotion;
    public float chainIntensity = 0.7f;
    
    private void Start()
    {
        if (shapes.Length == 0) return;
        
        // Subscribe to first shape's emotion changes
        shapes[0].OnEmotionChanged += (emotion, intensity) => {
            if (emotion.HasFlag(triggerEmotion) && intensity >= triggerIntensity)
            {
                StartChainReaction();
            }
        };
    }
    
    private void StartChainReaction()
    {
        StartCoroutine(ChainReactionSequence());
    }
    
    private IEnumerator ChainReactionSequence()
    {
        for (int i = 1; i < shapes.Length; i++)
        {
            EmotionalStimulus stimulus = new EmotionalStimulus(
                chainEmotion,
                chainIntensity,
                shapes[i-1].gameObject,
                5.0f
            );
            
            shapes[i].ReceiveStimulus(stimulus);
            
            // Wait between each shape in chain
            yield return new WaitForSeconds(0.5f);
        }
    }
}
```

### 3. Emotion Matching Puzzles

```csharp
public class EmotionMatchingPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class EmotionTarget
    {
        public EmotionSystem shape;
        public EmotionType targetEmotion;
        public float minIntensity = 0.7f;
        public bool isMatched = false;
    }
    
    public EmotionTarget[] targets;
    public UnityEvent onAllMatched;
    
    private void Update()
    {
        bool allMatched = true;
        
        foreach (var target in targets)
        {
            target.isMatched = target.shape.CurrentState.HasEmotion(target.targetEmotion) && 
                              target.shape.CurrentState.GetEmotionIntensity(target.targetEmotion) >= target.minIntensity;
                              
            if (!target.isMatched)
            {
                allMatched = false;
            }
        }
        
        if (allMatched)
        {
            onAllMatched.Invoke();
            enabled = false; // Disable this script once solved
        }
    }
}
```

## Advanced Shape Creation

### 1. Memory-Based Behaviors

```csharp
public class MemoryBasedBehavior : MonoBehaviour
{
    public EmotionSystem emotionSystem;
    public EmotionMemory memory;
    public float memoryThreshold = 0.7f;
    
    private void Start()
    {
        emotionSystem = GetComponent<EmotionSystem>();
        memory = GetComponent<EmotionMemory>();
    }
    
    private void Update()
    {
        // Find all shapes with positive memories
        var positiveShapes = memory.GetAllMemories()
            .Where(m => m.OverallSentiment > memoryThreshold)
            .Select(m => m.ShapeId)
            .ToList();
            
        // Custom behavior based on memories
        // ...
    }
}
```

### 2. Emotional Contagion

```csharp
public class EmotionalContagion : MonoBehaviour
{
    public EmotionSystem emotionSystem;
    public float contagionRadius = 5f;
    public float contagionStrength = 0.5f;
    public LayerMask shapeLayer;
    
    private void Start()
    {
        emotionSystem = GetComponent<EmotionSystem>();
        emotionSystem.OnEmotionChanged += SpreadEmotion;
    }
    
    private void SpreadEmotion(EmotionType emotion, float intensity)
    {
        // Only spread strong emotions
        if (intensity < 0.7f) return;
        
        // Find nearby shapes
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position, 
            contagionRadius, 
            shapeLayer
        );
        
        // Create a weaker stimulus to spread
        EmotionalStimulus stimulus = new EmotionalStimulus(
            emotion,
            intensity * contagionStrength,
            gameObject,
            3.0f
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
```

### 3. Custom Emotion Expressions

For truly unique shapes, create custom implementations of the `IShapeVisuals` interface:

```csharp
public class CustomShapeExpression : MonoBehaviour, IShapeVisuals
{
    public ParticleSystem emotionParticles;
    public TrailRenderer emotionTrail;
    public Light emotionLight;
    
    private Dictionary<EmotionType, Color> emotionColors = new Dictionary<EmotionType, Color>
    {
        { EmotionType.Happy, Color.yellow },
        { EmotionType.Sad, Color.blue },
        { EmotionType.Angry, Color.red },
        { EmotionType.Scared, Color.magenta },
        { EmotionType.Calm, Color.cyan },
        { EmotionType.Surprised, Color.green }
    };
    
    public void OnEmotionChanged(EmotionType emotion, float intensity)
    {
        // Get the appropriate color based on emotion
        Color targetColor = GetEmotionColor(emotion);
        
        // Apply color to visual elements
        if (emotionParticles != null)
        {
            var mainModule = emotionParticles.main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(targetColor);
            
            // Adjust emission rate based on intensity
            var emission = emotionParticles.emission;
            emission.rateOverTime = intensity * 20f;
        }
        
        if (emotionTrail != null)
        {
            emotionTrail.startColor = targetColor;
            emotionTrail.endColor = targetColor * 0.5f;
            emotionTrail.time = intensity * 0.5f;
        }
        
        if (emotionLight != null)
        {
            emotionLight.color = targetColor;
            emotionLight.intensity = intensity * 2f;
        }
    }
    
    private Color GetEmotionColor(EmotionType emotion)
    {
        // Try to find a direct match
        foreach (var pair in emotionColors)
        {
            if (emotion.HasFlag(pair.Key))
            {
                return pair.Value;
            }
        }
        
        // Default to white if no match
        return Color.white;
    }
}
```

## Testing Your Shape

### 1. Create a Test Scene

```
1. Create a new scene or use an existing test scene
2. Add your shape GameObject
3. Add a player controller or interaction mechanism
4. Add UI for debugging emotions (optional)
```

### 2. Use Debug Tools

```csharp
public class EmotionDebugger : MonoBehaviour
{
    public EmotionSystem targetSystem;
    public TMPro.TextMeshProUGUI debugText;
    
    private void Update()
    {
        if (targetSystem == null || debugText == null) return;
        
        string text = $"Current Emotion: {targetSystem.CurrentState.CurrentEmotion}\n" +
                     $"Intensity: {targetSystem.CurrentState.Intensity:F2}\n" +
                     $"Memory Records: {targetSystem.GetComponent<EmotionMemory>().GetAllMemories().Count}";
                     
        debugText.text = text;
    }
}
```

### 3. Test Interactions

```
1. Use the EmotionSystem's inspector to manually set emotions
2. Use the Skills system to apply emotional effects
3. Create test stimuli to verify responses:

EmotionalStimulus stimulus = new EmotionalStimulus(
    EmotionType.Happy,
    0.8f,
    gameObject,
    5.0f
);

targetShape.GetComponent<EmotionSystem>().ReceiveStimulus(stimulus);
```

## Best Practices

1. **Keep Emotions Distinct**
   - Each emotion should have clear visual differences
   - Avoid subtle changes that players might miss
   - Use multiple visual cues (color, shape, movement, particles)

2. **Create Distinctive Personalities**
   - Give each shape a unique emotional profile
   - Make vulnerabilities and resistances meaningful
   - Consider adding backstory or context for complex behaviors

3. **Balance Complexity**
   - Start with simple shapes and emotions
   - Gradually introduce more complex emotional behaviors
   - Avoid overwhelming players with too many emotional states

4. **Visual Consistency**
   - Maintain a consistent visual language for emotions
   - Similar emotions should have similar visual cues
   - Help players understand the meaning of visual changes

5. **Iterative Testing**
   - Regularly test shape behaviors with actual gameplay
   - Observe how players interpret emotional expressions
   - Refine visuals and behaviors based on feedback

## Troubleshooting

### Common Issues

1. **Emotions Not Displaying Correctly**
   - Check that visual components implement IShapeVisuals
   - Verify animator parameters match what Shape_Animator expects
   - Ensure materials are correctly assigned in ShapeVisualController

2. **Shapes Not Responding to Stimuli**
   - Check that the EmotionSystem component is enabled
   - Verify the EmotionProfileHandler has a profile assigned
   - Look for conflicting stimuli or emotional shields

3. **Animation Issues**
   - Check that animation clips are properly created
   - Verify animator controller transitions are correctly set up
   - Check that animator parameters are being updated

4. **Performance Concerns**
   - Reduce the number of shapes with complex behaviors
   - Optimize particle effects and visual feedback
   - Consider using LOD (Level of Detail) for distant shapes

## Next Steps

Now that you know how to create custom shapes, explore:
- Creating full puzzles using emotional shapes
- Developing narrative elements with emotional progression
- Creating environments that respond to emotional states
- Building complex multi-shape puzzles
