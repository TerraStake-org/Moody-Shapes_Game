# Moody Shapes - Project Architecture Overview

This document provides a comprehensive overview of the Moody Shapes game architecture, explaining how all systems work together to create an emotionally-driven puzzle experience.

## Architecture Overview

Moody Shapes is built on a modular, component-based architecture with the following core systems:

1. **Emotion System**: The core system for emotional states and responses
2. **Skill System**: Player abilities for emotional manipulation
3. **Visual Feedback System**: Multiple layers of visual representation
4. **UI System**: Player interface and information display
5. **Puzzle System**: Gameplay mechanics and challenges

These systems communicate through events, interfaces, and direct references, creating a flexible and extensible game framework.

## System Relationships

![System Architecture Diagram]

The relationships between systems can be summarized as:

- **Emotion System** ↔ **Visual Feedback**: Emotions trigger visual changes
- **Skill System** → **Emotion System**: Skills modify emotional states
- **Emotion System** → **Screen Effects**: Strong emotions trigger screen-wide effects
- **Player** → **Skill System**: Player triggers skills
- **Puzzle System** → **Emotion System**: Puzzles track emotional states

## Core Systems Breakdown

### 1. Emotion System

**Primary Responsibility**: Managing emotional states, personalities, and responses

**Key Components**:
- `EmotionType`: Flag-based enum defining all possible emotions
- `EmotionalState`: Tracks current emotion and intensity
- `EmotionTraits`: Defines personality (vulnerabilities, resistances)
- `EmotionSystem`: Central manager coordinating emotional processing
- `EmotionProfileSO`: ScriptableObject defining a shape's personality
- `EmotionMemory`: Tracks history of emotional interactions
- `EmotionalStimulus`: Data structure for emotional triggers
- `EmotionChangeEvent`: Struct for tracking emotion state changes

**Core Interactions**:
1. External sources create an `EmotionalStimulus`
2. The stimulus is passed to an `EmotionSystem`
3. `EmotionSystem` processes the stimulus based on `EmotionTraits`
4. `EmotionalState` is updated with new emotions
5. `EmotionSystem` triggers events to notify other systems
6. Visual systems update to reflect the new emotional state

### 2. Skill System

**Primary Responsibility**: Providing player abilities to influence emotional states

**Key Components**:
- `EmotionSkillSO`: ScriptableObject defining skill properties
- `EmotionSkillController`: Manages skill usage and cooldowns
- `SkillTargetingSystem`: Handles target selection for skills
- `EmotionSkillUI`: Provides user interface for skills
- `SkillLoader`: Loads skill configurations from JSON

**Specialized Skill Types**:
- `CalmingWaveSkill`: Area-effect calming ability
- `JoyBurstSkill`: Chain-reaction joy ability
- `EmotionalShieldSkill`: Protection from negative emotions
- `EmpathyLinkSkill`: Creates emotional connections between shapes

**Core Interactions**:
1. Player selects a skill through `EmotionSkillUI`
2. `SkillTargetingSystem` activates for target selection
3. Player chooses a target shape
4. `EmotionSkillController` executes the skill
5. Skill creates appropriate `EmotionalStimulus` objects
6. Target shapes receive stimuli through their `EmotionSystem`
7. Visual effects provide feedback on skill execution

### 3. Visual Feedback System

**Primary Responsibility**: Visualizing emotional states at multiple levels

**Key Components**:
- `IShapeVisuals`: Interface for components that visualize emotions
- `Shape_Animator`: Animation-based visualization
- `ShapeVisualController`: Material/color-based visualization
- `EmotionPostProcessingController`: Camera effects for emotions
- `EmotionScreenShakeController`: Screen shake effects
- `EmotionScreenEffects`: Utility for triggering screen effects

**Visualization Layers**:
1. **Shape-level**: Direct visual changes to emotional shapes
2. **Particle Effects**: Emotional particle systems
3. **Screen Effects**: Global post-processing effects
4. **Screen Shake**: Camera movement for emotional impact

**Core Interactions**:
1. `EmotionSystem` triggers emotion change events
2. Shape-level visual components (`IShapeVisuals`) update appearances
3. Strong emotions trigger `EmotionScreenEffects`
4. Screen-wide visual effects provide global emotional context

### 4. Audio System

**Primary Responsibility**: Providing dynamic audio that responds to emotional states

**Key Components**:
- `MoodMusicManager`: Adaptive music system that responds to emotions
- Emotion-based audio layers
- Stinger/transition system for emotional changes

**Core Interactions**:
1. `MoodMusicManager` monitors emotional states of shapes in the scene
2. Dominant emotions influence which audio layers are active
3. Dramatic emotional changes trigger audio stingers
4. Audio atmosphere adapts to match the overall emotional tone

**Advanced Features**:
- Dynamic mixing between different emotional music layers
- Distance-based weighting for emotion influence
- Smooth transitions between emotional states
- Immediate stinger responses to dramatic emotional changes

### 4. UI System

**Primary Responsibility**: Providing information and controls to the player

**Key Components**:
- `ShapeEmotionFeedbackUI`: Displays shape's emotional state
- `EmotionSkillUI`: Shows available skills and cooldowns
- `SkillTooltipTrigger`: Provides information about skills
- `ActionTooltipTrigger`: Shows available interactions

**Core Interactions**:
1. UI components register with relevant systems
2. Systems notify UI components of state changes
3. UI updates to reflect current game state
4. Player interacts with UI elements to trigger actions

### 5. Puzzle System

**Primary Responsibility**: Creating gameplay challenges based on emotions

While not fully implemented in the current codebase, the puzzle system is designed to:
- Track emotional states for puzzle completion
- Create emotion-based challenges
- Provide level progression mechanics

## Data Flow

### Emotion Processing Flow

1. **Stimulus Creation**:
   ```csharp
   EmotionalStimulus stimulus = new EmotionalStimulus(
       EmotionType.Happy,    // Type of emotion
       0.8f,                 // Base intensity
       sourceObject,         // Source GameObject
       5.0f                  // Duration in seconds
   );
   ```

2. **Stimulus Reception**:
   ```csharp
   targetEmotionSystem.ReceiveStimulus(stimulus);
   ```

3. **Trait-Based Processing**:
   ```csharp
   // Inside EmotionSystem
   float modifiedIntensity = ApplyEmotionalTraits(stimulus.EmotionType, stimulus.Intensity);
   ```

4. **State Update**:
   ```csharp
   emotionalState.BlendEmotion(stimulus.EmotionType, modifiedIntensity);
   ```

5. **Event Notification**:
   ```csharp
   OnEmotionChanged?.Invoke(CurrentState.CurrentEmotion, CurrentState.Intensity);
   ```

6. **Visual Update**:
   ```csharp
   // Inside IShapeVisuals implementations
   void OnEmotionChanged(EmotionType emotion, float intensity)
   {
       // Update visuals based on emotion and intensity
   }
   ```

7. **Screen Effects** (for strong emotions):
   ```csharp
   EmotionScreenEffects.TriggerEmotionPulse(emotion, intensity);
   ```

### Skill Execution Flow

1. **Skill Selection**:
   ```csharp
   // Player selects skill through UI
   skillUI.OnSkillSelected(skillIndex);
   ```

2. **Targeting**:
   ```csharp
   // Begin targeting mode
   targetingSystem.StartTargeting(skill, OnTargetSelected);
   ```

3. **Target Selection**:
   ```csharp
   // Player selects target
   targetingSystem.SelectTarget(hitInfo.collider.gameObject);
   ```

4. **Skill Execution**:
   ```csharp
   // Execute the skill
   skillController.UseSkill(skillIndex, targetEmotionSystem);
   ```

5. **Skill Implementation**:
   ```csharp
   // Inside skill implementation
   public override void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem)
   {
       // Create and apply emotional stimuli
       EmotionalStimulus stimulus = new EmotionalStimulus(...);
       targetSystem.ReceiveStimulus(stimulus);
   }
   ```

6. **Visual Feedback**:
   ```csharp
   // Show skill effects
   EmotionScreenEffects.TriggerCombinedEffect(emotion, intensity, targetPosition);
   ```

## Extension Points

The architecture is designed to be extended in several ways:

### 1. New Emotion Types

To add new emotions:
1. Extend the `EmotionType` enum with new values
2. Update visualization systems to handle new emotions
3. Create new stimuli using the new emotion types

### 2. New Skill Types

To add new skills:
1. Create a new class deriving from `EmotionSkillSO`
2. Implement custom `ExecuteSkill` method
3. Create JSON configuration or ScriptableObject instance
4. Register the skill type in `SkillLoader`

### 3. Custom Visual Feedback

To add new visualization methods:
1. Create a new component implementing `IShapeVisuals`
2. Implement the `OnEmotionChanged` method
3. Add the component to shapes

### 4. New Puzzle Mechanics

To add new puzzle types:
1. Create components that observe emotional states
2. Implement puzzle logic based on emotional conditions
3. Create feedback systems for puzzle progress

## Implementation Details

### Emotion System Implementation

The emotion system uses a flags-based enum to represent emotions:

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

This allows:
- Multiple emotions simultaneously (e.g., Happy | Surprised)
- Checking for specific emotions (`emotion.HasFlag(EmotionType.Happy)`)
- Predefined composite emotions (e.g., Bittersweet)

Emotional states are managed by the `EmotionalState` class, which:
- Tracks current dominant emotion and intensity
- Handles emotion blending and decay
- Provides methods to query emotional state

### Skill System Implementation

Skills are implemented as ScriptableObjects with custom behavior:

```csharp
public class EmotionSkillSO : ScriptableObject
{
    // Base properties
    public string skillName;
    public string description;
    public Sprite icon;
    public float cooldownTime;
    public float range;
    public EmotionType emotionType;
    public float basePower;
    public TargetingType targeting;
    
    // Virtual method for skill execution
    public virtual void ExecuteSkill(EmotionSystem targetSystem, EmotionSystem sourceSystem = null)
    {
        // Base implementation - can be overridden
    }
}
```

Specific skill types override the `ExecuteSkill` method to implement custom behavior.

For flexibility, skills can also be loaded from JSON configurations using the `SkillLoader` class.

### Visual Feedback Implementation

Visual feedback is implemented through the `IShapeVisuals` interface:

```csharp
public interface IShapeVisuals
{
    void OnEmotionChanged(EmotionType emotion, float intensity);
}
```

This interface is implemented by various components, including:
- `Shape_Animator`: Uses animations to show emotions
- `ShapeVisualController`: Uses materials and colors

For global effects, the system uses:
- `EmotionPostProcessingController`: URP post-processing effects
- `EmotionScreenShakeController`: Cinemachine impulse effects

### UI Implementation

The UI system uses several specialized components:
- `EmotionSkillUI`: Displays and manages skill slots
- `ShapeEmotionFeedbackUI`: Shows a shape's emotional state
- `SkillTooltipTrigger`: Displays skill information on hover

These components register with appropriate systems to receive updates when game state changes.

## Technical Requirements

### Unity Requirements

- **Unity Version**: 2022.3 LTS or later
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Required Packages**:
  - Universal Render Pipeline (URP)
  - Cinemachine
  - TextMeshPro
  - Visual Effect Graph

### Runtime Performance

The system is designed with performance in mind:
- Emotion processing is event-driven to reduce constant updates
- Screen effects are only triggered for significant emotional changes
- Visual feedback can be simplified for distant/less important shapes

### Memory Considerations

- Emotion profiles are shared ScriptableObjects to reduce memory usage
- Skills are loaded on demand from JSON when possible
- Animation controllers are shared between similar shapes

## Development Patterns

### Observer Pattern

Used extensively for notification of state changes:
- `EmotionSystem.OnEmotionChanged` event
- `EmotionSystem.OnStimulusReceived` event
- `ShapeInteractionHandler.OnInteractionStarted` event

### Component-Based Design

The architecture uses Unity's component system:
- Each responsibility is in a separate component
- Components communicate through references and events
- New behaviors can be added by adding/replacing components

### ScriptableObject-Based Configuration

Configuration data is stored in ScriptableObjects:
- `EmotionProfileSO`: Personality traits and behaviors
- `EmotionSkillSO`: Skill properties and effects

### Interface-Based Polymorphism

Interfaces allow multiple implementations:
- `IShapeVisuals`: Different ways to visualize emotions
- Custom skill types deriving from `EmotionSkillSO`

## Integration Guide

### Adding the Emotion System to a New Project

1. Import the Emotion System scripts
2. Create emotion profiles for your characters
3. Add required components to character objects
4. Set up visual feedback systems

### Creating Emotion-Based Gameplay

1. Use `EmotionSystem` to track emotional states
2. Create puzzles that respond to emotional changes
3. Use skills to give players ways to influence emotions
4. Create visual feedback for emotional states

### Extending with New Features

1. Create new emotion types for specialized gameplay
2. Implement new skill types for unique interactions
3. Create custom visualization components
4. Build new puzzle mechanisms based on emotions

## Best Practices

1. **Consistent Emotional Language**
   - Use consistent visual cues for emotions
   - Establish clear patterns for emotional states
   - Help players understand the emotional system

2. **Layered Feedback**
   - Use multiple feedback channels (visuals, animation, effects)
   - Scale feedback with emotional intensity
   - Use screen effects sparingly for impact

3. **Balanced Gameplay**
   - Create meaningful differences between shapes
   - Balance skill effects and cooldowns
   - Allow multiple solutions to emotional puzzles

4. **Performance Optimization**
   - Limit screen effects to significant emotional events
   - Use simplified visual feedback for distant shapes
   - Share resources (profiles, animations) when possible

## Future Development

Potential areas for system expansion:

1. **Advanced Emotional AI**
   - More complex emotional reactions
   - Learning-based emotional memory
   - Procedural personality generation

2. **Enhanced Visual Feedback**
   - Procedural animations based on emotions
   - Advanced particle systems for emotional expression
   - Dynamic environment responses to emotions

3. **Expanded Skill System**
   - Skill trees and progression
   - Combinable skills for complex effects
   - Context-sensitive skill effects

4. **Narrative Integration**
   - Emotion-driven dialogue systems
   - Story progression based on emotional states
   - Character development through emotional experiences

## Documentation Resources

For more detailed information, refer to these documents:

- [Emotion System Tutorial](emotion_system_tutorial.md)
- [Skill System Tutorial](skill_system_tutorial.md)
- [Creating Custom Shapes](creating_custom_shapes.md)
- [Installation Guide](installation_guide.md)
- [Screen Effects Setup](screen_effects_setup.md)

## Conclusion

The Moody Shapes architecture provides a flexible, extensible framework for creating emotion-based gameplay. By separating concerns into distinct systems and using event-based communication, the architecture supports a wide range of gameplay possibilities while maintaining clean code organization.

The modular design allows for easy extension with new emotions, skills, and visual feedback mechanisms, enabling developers to create rich, emotionally-driven game experiences.
