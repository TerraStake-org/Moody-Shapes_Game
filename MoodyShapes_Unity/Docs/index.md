# Moody Shapes Documentation Index

Welcome to the Moody Shapes documentation. This index provides links to all available documentation files to help you navigate the project.

## Getting Started

- [README](../README.md) - Project overview and basic setup
- [Installation Guide](installation_guide.md) - Detailed setup instructions
- [Requirements](requirements.txt) - Technical requirements and dependencies

## Core Systems

- [Architecture Overview](architecture_overview.md) - High-level overview of all systems
- [Emotion System Tutorial](emotion_system_tutorial.md) - Working with the emotion system
- [Skill System Tutorial](skill_system_tutorial.md) - Working with the skill system
- [Audio System Tutorial](audio_system_tutorial.md) - Working with the adaptive audio system
- [Screen Effects Setup](screen_effects_setup.md) - Setting up post-processing and screen effects

## Development Guides

- [Creating Custom Shapes](creating_custom_shapes.md) - Creating new shapes with custom behaviors
- [Troubleshooting Guide](troubleshooting_guide.md) - Solutions for common issues

## System Diagrams

### Emotion System Flow

```
[EmotionalStimulus] → [EmotionSystem] → [EmotionalState]
         ↓                    ↓                ↓
     (filters)           (processes)       (updates)
         ↓                    ↓                ↓
   [EmotionTraits]    [EmotionChangeEvent] → [Visual Feedback]
                              ↓
                    [Screen Effects, Audio]
```

### Skill System Flow

```
[Player Input] → [EmotionSkillUI] → [SkillTargetingSystem]
                        ↓                    ↓
            [EmotionSkillController] → [EmotionSkillSO]
                                             ↓
                                    [Target EmotionSystem]
```

### Audio System Flow

```
[EmotionChangeEvent] → [MoodMusicManager] → [Analyze Scene Emotions]
                             ↓                      ↓
                     [Adjust Layer Volumes]    [Trigger Stingers]
                             ↓
                    [Adaptive Music Output]

[EmotionChangeEvent] → [ShapeAudioFeedback] → [Play Sound Effects]
                             ↓
                     [Influence Global Audio]
```

## Implementation Details

- [Audio System Implementation](audio_system_implementation.md) - Details of the audio system implementation
                              ↓                      ↓
                  [Dynamic Layer Volumes] ← [Dominant Emotion Calculation]
                              ↓
                     [Audio Output Mixer]
```

## Component Reference

### Emotion System Components

- `EmotionType` - Flag-based enum for all emotions
- `EmotionalState` - Manages current emotion and intensity
- `EmotionSystem` - Main controller for emotional processing
- `EmotionProfileSO` - ScriptableObject for personality configuration
- `EmotionMemory` - Tracks emotional interaction history
- `EmotionalStimulus` - Data structure for triggering emotions

### Skill System Components

- `EmotionSkillSO` - Base ScriptableObject for all skills
- `EmotionSkillController` - Manages skill execution and cooldowns
- `SkillTargetingSystem` - Handles target selection for skills
- `EmotionSkillUI` - UI components for skill interaction
- `SkillLoader` - Utility for loading skills from JSON

### Visual Feedback Components

- `IShapeVisuals` - Interface for visualizing emotions
- `Shape_Animator` - Animation-based visualization
- `ShapeVisualController` - Material-based visualization
- `EmotionPostProcessingController` - Camera effects
- `EmotionScreenShakeController` - Screen shake effects
- `EmotionScreenEffects` - Utility for triggering effects

### Audio System Components

- `MoodMusicManager` - Manages adaptive music system
- `MoodMusicLayer` - Configuration for emotion-specific music

## Common Tasks

### Creating a New Shape

1. See [Creating Custom Shapes](creating_custom_shapes.md)
2. Add required components:
   - `EmotionSystem`
   - `EmotionalState`
   - `EmotionMemory`
   - `EmotionProfileHandler`
   - `Shape_Animator` or `ShapeVisualController`

### Creating a New Skill

1. See [Skill System Tutorial](skill_system_tutorial.md)
2. Create a class extending `EmotionSkillSO`
3. Implement custom logic in `ExecuteSkill()`
4. Create JSON configuration or ScriptableObject

### Setting Up Screen Effects

1. See [Screen Effects Setup](screen_effects_setup.md)
2. Use `EmotionScreenEffectsInitializer` for automatic setup
3. Configure effect intensities for different emotions

### Configuring Adaptive Audio

1. See [Audio System Tutorial](audio_system_tutorial.md)
2. Set up `MoodMusicManager` with appropriate layers
3. Configure emotion-based music assets

## Troubleshooting

For common issues and solutions, refer to the [Troubleshooting Guide](troubleshooting_guide.md).

## Contributing

If you wish to expand this documentation, please follow these guidelines:

1. Use clear, concise language
2. Include practical code examples
3. Add diagrams for complex systems
4. Link to related documentation
5. Include troubleshooting sections for common issues
