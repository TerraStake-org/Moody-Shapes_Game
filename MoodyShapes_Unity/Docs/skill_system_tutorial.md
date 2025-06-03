# Skill System Tutorial

This tutorial explains how to work with the Emotion Skill System in Moody Shapes, covering skill creation, configuration, and implementation.

## Introduction to the Skill System

The Skill System in Moody Shapes provides the player with abilities to:
- Influence the emotional states of shapes
- Solve puzzles through emotional manipulation
- Create relationships between shapes
- Defend shapes from unwanted emotional stimuli

## Core Components

### 1. EmotionSkillSO

The `EmotionSkillSO` ScriptableObject defines a skill's properties:

```csharp
// Properties of EmotionSkillSO
public string skillName;          // Name of the skill
public string description;        // What the skill does
public Sprite icon;               // Visual representation
public float cooldownTime;        // Time between uses
public float range;               // Skill's reach
public EmotionType emotionType;   // Primary emotion affected
public float basePower;           // Base effectiveness
public TargetingType targeting;   // How targets are selected
```

### 2. EmotionSkillController

The `EmotionSkillController` manages skill execution and cooldowns:

```csharp
// Basic usage example
skillController.EquipSkill(calmingWaveSkill, 0); // Equip to slot 0
skillController.UseSkill(0, targetShape);        // Use skill in slot 0
```

### 3. SkillTargetingSystem

The `SkillTargetingSystem` handles target selection:

```csharp
// Targeting types
public enum TargetingType
{
    SingleTarget,    // Affects one shape
    AOE,             // Area of effect, multiple shapes
    Self,            // Affects the player or player's shape
    Beam,            // Line-based effect
    Global           // Affects all valid targets in scene
}
```

### 4. JSON Configuration

Skills can be defined in JSON files for easy configuration:

```json
{
  "skillName": "Calming Wave",
  "description": "Reduces anger and fear in the target shape",
  "iconPath": "SkillIcons/calming_wave",
  "cooldownTime": 5.0,
  "range": 8.0,
  "emotionType": "Calm",
  "basePower": 0.7,
  "targeting": "AOE",
  "effectRadius": 5.0,
  "skillType": "CalmingWaveSkill"
}
```

## Working with Skills

### Creating and Configuring Skills

#### Method 1: ScriptableObject Approach

1. **Create a Skill Asset**:
   ```
   Assets > Create > Moody Shapes > Emotion Skill
   ```

2. **Configure the Skill**:
   - Fill in the skill properties in the Inspector
   - Assign an icon sprite
   - Select the appropriate targeting type
   - Configure skill-specific properties

#### Method 2: JSON Approach

1. **Create a JSON File**:
   - Create a new JSON file in Resources/Skills/
   - Follow the structure shown above

2. **Load at Runtime**:
   ```csharp
   // Load skills from JSON
   List<EmotionSkillSO> skills = SkillLoader.LoadSkillsFromJSON();
   
   // Or load a specific skill
   EmotionSkillSO skill = SkillLoader.LoadSkillFromJSON("CalmingWave");
   ```

### Implementing Custom Skill Types

1. **Create a Skill Class**:
   ```csharp
   public class MyCustomSkill : EmotionSkillSO
   {
       [Header("Custom Skill Settings")]
       public float specialValue;
       public int chargeCount;
       
       public override void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem = null)
       {
           base.ExecuteSkill(targetSystem, sourceSystem);
           
           // Create your custom skill logic here
           EmotionalStimulus stimulus = new EmotionalStimulus(
               emotionType,         // From base class
               basePower,           // From base class
               sourceSystem?.gameObject,
               5f                   // Duration
           );
           
           // Apply the stimulus
           targetSystem.ReceiveStimulus(stimulus);
           
           // Custom effects
           // ...
       }
   }
   ```

2. **Register the Skill Type**:
   Update the `SkillLoader` to recognize your custom skill type:

   ```csharp
   // In SkillLoader.cs
   private static EmotionSkillSO CreateSkillInstance(string skillType)
   {
       switch (skillType)
       {
           // Existing cases...
           
           case "MyCustomSkill":
               return ScriptableObject.CreateInstance<MyCustomSkill>();
               
           default:
               Debug.LogError($"Unknown skill type: {skillType}");
               return null;
       }
   }
   ```

### Using Skills in the Game

#### Equipping Skills

```csharp
// Get the skill controller
EmotionSkillController skillController = GetComponent<EmotionSkillController>();

// Load a skill
EmotionSkillSO calmingWave = SkillLoader.LoadSkillFromJSON("CalmingWave");

// Equip the skill to a slot
skillController.EquipSkill(calmingWave, 0); // Slot 0
```

#### Using Skills

```csharp
// Method 1: Direct usage
skillController.UseSkill(0, targetEmotionSystem); // Use skill in slot 0

// Method 2: Through targeting system
skillController.BeginTargeting(0); // Begin targeting mode for skill in slot 0
// Player selects target...
// Targeting system calls UseSkill internally when target is confirmed
```

#### Handling Cooldowns

```csharp
// Check if skill is ready
bool isReady = skillController.IsSkillReady(0);

// Get remaining cooldown time
float cooldown = skillController.GetRemainingCooldown(0);

// Reset cooldown (for special events)
skillController.ResetCooldown(0);
```

## Example Skill Implementations

### 1. Calming Wave

Reduces anger and fear in the target and nearby shapes:

```csharp
public class CalmingWaveSkill : EmotionSkillSO
{
    public float effectRadius = 5f;
    public LayerMask shapeLayerMask;
    
    public override void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem = null)
    {
        base.ExecuteSkill(targetSystem, sourceSystem);
        
        // Find all shapes in radius
        Collider[] hitColliders = Physics.OverlapSphere(
            targetSystem.transform.position, 
            effectRadius, 
            shapeLayerMask
        );
        
        // Create calming stimulus
        EmotionalStimulus stimulus = new EmotionalStimulus(
            EmotionType.Calm,
            basePower,
            sourceSystem?.gameObject,
            5f
        );
        
        // Apply to all affected shapes
        foreach (var hitCollider in hitColliders)
        {
            EmotionSystem affectedSystem = hitCollider.GetComponent<EmotionSystem>();
            if (affectedSystem != null)
            {
                affectedSystem.ReceiveStimulus(stimulus);
                
                // Additionally reduce negative emotions
                EmotionalState state = affectedSystem.CurrentState;
                if (state.HasEmotion(EmotionType.Angry))
                {
                    state.ReduceEmotion(EmotionType.Angry, basePower * 0.5f);
                }
                
                if (state.HasEmotion(EmotionType.Scared))
                {
                    state.ReduceEmotion(EmotionType.Scared, basePower * 0.5f);
                }
            }
        }
        
        // Visual feedback
        EmotionScreenEffects.TriggerEmotionPulse(EmotionType.Calm, basePower, 1.0f);
    }
}
```

### 2. Joy Burst

Creates a burst of joy that spreads from shape to shape:

```csharp
public class JoyBurstSkill : EmotionSkillSO
{
    public int maxJumpCount = 3;
    public float jumpRadius = 4f;
    public float intensityDecay = 0.2f;
    public LayerMask shapeLayerMask;
    
    public override void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem = null)
    {
        base.ExecuteSkill(targetSystem, sourceSystem);
        
        // Start the joy chain
        StartJoyChain(targetSystem, basePower, maxJumpCount, new HashSet<EmotionSystem>());
    }
    
    private void StartJoyChain(EmotionSystem system, float power, int jumpsLeft, HashSet<EmotionSystem> affected)
    {
        // Apply joy to current target
        EmotionalStimulus stimulus = new EmotionalStimulus(
            EmotionType.Joyful,
            power,
            gameObject,
            5f
        );
        
        system.ReceiveStimulus(stimulus);
        affected.Add(system);
        
        // Visual feedback at this point
        EmotionScreenEffects.TriggerEmotionPulse(EmotionType.Joyful, power, 0.5f);
        
        // Stop if no more jumps
        if (jumpsLeft <= 0) return;
        
        // Find nearby shapes for chain reaction
        Collider[] hitColliders = Physics.OverlapSphere(
            system.transform.position, 
            jumpRadius, 
            shapeLayerMask
        );
        
        // Randomize to get unpredictable chains
        List<Collider> potentialTargets = hitColliders.ToList();
        potentialTargets.RemoveAll(c => {
            EmotionSystem es = c.GetComponent<EmotionSystem>();
            return es == null || affected.Contains(es);
        });
        
        if (potentialTargets.Count == 0) return;
        
        // Pick a random target and continue chain
        int randomIndex = UnityEngine.Random.Range(0, potentialTargets.Count);
        EmotionSystem nextTarget = potentialTargets[randomIndex].GetComponent<EmotionSystem>();
        
        // Add slight delay for visual effect
        system.StartCoroutine(DelayedJoyChain(nextTarget, power - intensityDecay, jumpsLeft - 1, affected));
    }
    
    private IEnumerator DelayedJoyChain(EmotionSystem nextTarget, float power, int jumpsLeft, HashSet<EmotionSystem> affected)
    {
        yield return new WaitForSeconds(0.5f);
        StartJoyChain(nextTarget, power, jumpsLeft, affected);
    }
}
```

### 3. Emotional Shield

Protects a shape from negative emotions:

```csharp
public class EmotionalShieldSkill : EmotionSkillSO
{
    public float shieldDuration = 10f;
    public EmotionType[] blockedEmotions = { EmotionType.Angry, EmotionType.Sad, EmotionType.Scared };
    
    public override void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem = null)
    {
        base.ExecuteSkill(targetSystem, sourceSystem);
        
        // Apply shield effect
        targetSystem.StartCoroutine(ApplyShield(targetSystem));
    }
    
    private IEnumerator ApplyShield(EmotionSystem system)
    {
        // Store original stimulus callback
        System.Action<EmotionalStimulus> originalCallback = system.OnStimulusReceived;
        
        // Replace with filtered callback
        system.OnStimulusReceived = (stimulus) => {
            // Check if stimulus contains blocked emotions
            bool blocked = false;
            foreach (var blockedType in blockedEmotions)
            {
                if (stimulus.EmotionType.HasFlag(blockedType))
                {
                    blocked = true;
                    break;
                }
            }
            
            // If not blocked, pass to original handler
            if (!blocked)
            {
                originalCallback?.Invoke(stimulus);
            }
            else
            {
                // Visual feedback for blocked emotion
                EmotionScreenEffects.TriggerEmotionPulse(EmotionType.Calm, 0.3f, 0.5f);
            }
        };
        
        // Visual shield effect
        // (Implementation depends on your visual system)
        
        // Wait for duration
        yield return new WaitForSeconds(shieldDuration);
        
        // Restore original callback
        system.OnStimulusReceived = originalCallback;
    }
}
```

### 4. Empathy Link

Creates a connection between two shapes where emotions are shared:

```csharp
public class EmpathyLinkSkill : EmotionSkillSO
{
    public float linkDuration = 15f;
    public float syncStrength = 0.5f;
    
    private readonly List<(EmotionSystem, EmotionSystem, Coroutine)> activeLinks = new List<(EmotionSystem, EmotionSystem, Coroutine)>();
    
    public override void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem)
    {
        base.ExecuteSkill(targetSystem, sourceSystem);
        
        if (sourceSystem == null)
        {
            Debug.LogError("Empathy Link requires a source system!");
            return;
        }
        
        // Start empathy link
        Coroutine linkCoroutine = sourceSystem.StartCoroutine(
            MaintainEmpathyLink(sourceSystem, targetSystem)
        );
        
        // Add to active links
        activeLinks.Add((sourceSystem, targetSystem, linkCoroutine));
        
        // Visual feedback
        EmotionScreenEffects.TriggerEmotionPulse(EmotionType.Curious, basePower, 1.0f);
    }
    
    private IEnumerator MaintainEmpathyLink(EmotionSystem source, EmotionSystem target)
    {
        float endTime = Time.time + linkDuration;
        
        while (Time.time < endTime && source != null && target != null)
        {
            // Share emotions in both directions
            SyncEmotions(source, target);
            SyncEmotions(target, source);
            
            // Wait before next sync
            yield return new WaitForSeconds(0.5f);
        }
        
        // Remove from active links
        activeLinks.RemoveAll(link => link.Item1 == source && link.Item2 == target);
    }
    
    private void SyncEmotions(EmotionSystem from, EmotionSystem to)
    {
        // Get dominant emotion from source
        EmotionType dominantEmotion = from.CurrentState.CurrentEmotion;
        float intensity = from.CurrentState.Intensity;
        
        // Skip if neutral or too weak
        if (dominantEmotion == EmotionType.Neutral || intensity < 0.3f)
            return;
            
        // Transfer emotion at reduced strength
        EmotionalStimulus stimulus = new EmotionalStimulus(
            dominantEmotion,
            intensity * syncStrength,
            from.gameObject,
            2f
        );
        
        to.ReceiveStimulus(stimulus);
    }
}
```

## UI Integration

The `EmotionSkillUI` class provides a user interface for skill management:

```csharp
// Connect UI to skill controller
skillUI.Initialize(skillController);

// Update cooldown display
skillUI.UpdateCooldowns();

// Highlight active skill
skillUI.SetActiveSkill(0);
```

## Skill Tooltips

The `SkillTooltipTrigger` class adds informative tooltips to skill buttons:

```csharp
// Add tooltip to a skill button
SkillTooltipTrigger tooltip = skillButton.AddComponent<SkillTooltipTrigger>();
tooltip.skill = skill;
tooltip.tooltipPrefab = tooltipPrefab;
```

## Targeting System

The `SkillTargetingSystem` handles target selection:

```csharp
// Begin targeting mode
targetingSystem.StartTargeting(skill, (target) => {
    // This callback is called when a target is selected
    skillController.UseSkill(skillIndex, target);
});

// Cancel targeting
targetingSystem.CancelTargeting();
```

## Demo Setup

The `SkillDemoManager` provides a simple testbed for experimenting with skills:

```csharp
// Setup a basic skill demo
SkillDemoManager demo = gameObject.AddComponent<SkillDemoManager>();
demo.availableSkills = SkillLoader.LoadSkillsFromJSON();
demo.playerSystem = playerEmotionSystem;
demo.shapeLayer = LayerMask.GetMask("EmotionalShapes");
```

## Best Practices

1. **Balance Skill Power**
   - More powerful skills should have longer cooldowns
   - Consider adding resource costs for powerful skills
   - Create counters for particularly strong effects

2. **Design Complementary Skills**
   - Skills should work together in interesting ways
   - Create skill combinations for solving complex puzzles
   - Allow multiple approaches to emotional challenges

3. **Clear Visual Feedback**
   - Each skill should have distinct visual effects
   - Use screen effects for dramatic skill moments
   - Provide clear success/failure feedback

4. **Skill Progression**
   - Start with basic emotion-setting skills
   - Introduce more complex skills as the game progresses
   - Allow skill upgrades or modifications

## Troubleshooting

### Common Issues

1. **Skills Not Affecting Targets**
   - Check that target has an EmotionSystem component
   - Verify skill power is sufficient to overcome resistances
   - Check for shields or immunities

2. **Skills Not Loading from JSON**
   - Verify JSON files are in Resources/Skills/
   - Check JSON syntax for errors
   - Ensure skill types match registered types in SkillLoader

3. **Targeting Issues**
   - Check that target is on the correct layer
   - Verify target is within skill range
   - Ensure targeting raycast is unobstructed

## Next Steps

Now that you understand the Skill System, explore these related tutorials:
- Emotion System Tutorial
- Creating Custom Skill Types
- Advanced UI Integration
- Designing Emotional Puzzles
