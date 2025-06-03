# Social Emotion Systems Guide

This guide explains how to use the social emotion systems implemented in the Moody Shapes game.

## Overview

The social emotion systems allow shapes in the game to:
- Develop relationships with each other
- Influence each other's emotions based on these relationships
- Visualize emotions through aura effects
- Display relationship status in the UI

## Core Components

### 1. EmotionSystemsManager

The central manager that initializes and connects all emotion-related systems.

**Key Features:**
- Automatically sets up all required systems in a scene
- Creates core system components when needed
- Provides a simple API for creating emotional shapes
- Can be extended to configure scene-wide emotion settings

**How to Use:**
1. Add the EmotionSystemsManager component to a GameObject in your scene
2. Configure the settings in the inspector
3. Click "Setup Scene" to initialize all systems
4. Use "Create Emotional Shape" to add new shapes with emotion support

### 2. SocialRelationshipManager

Tracks and manages relationships between shapes.

**Key Features:**
- Stores relationship scores (-1 to 1) between entities
- Tracks familiarity (0 to 1) that grows with interactions
- Provides influence modifiers based on relationships
- Can be queried for visualization and gameplay mechanics

### 3. EmotionInfluenceSystem

Handles how emotions spread between shapes.

**Key Features:**
- Detects nearby shapes and applies emotional influence
- Uses social relationships to modify influence strength
- Configurable radius, falloff, and update intervals
- Can be filtered to affect specific types of shapes

### 4. EmotionAuraEffect

Visualizes emotions through shader effects.

**Key Features:**
- Creates a dynamic aura around shapes
- Changes color based on current emotion
- Pulses based on emotion intensity
- Customizable colors for each emotion type

### 5. SocialRelationshipUI

Displays UI visualization for social relationships.

**Key Features:**
- Shows relationship scores and familiarity
- Updates dynamically as relationships change
- Color-coded visualization of positive/negative relationships
- Displays selected entity's relationships

## Implementation Guide

### Setting Up a Scene

1. Create a new GameObject and name it "EmotionSystemsManager"
2. Add the `EmotionSystemsManager` component
3. Configure the settings:
   - Base influence radius (how far emotions spread)
   - Influence interval (how often emotions update)
   - Social relationship multipliers
   - Visual settings for auras
4. Click "Setup Scene" in the inspector

### Creating Emotional Shapes

**Method 1: Using the Manager**
1. With the EmotionSystemsManager selected, click "Create Emotional Shape"
2. Position the shape in the scene
3. Assign an emotion profile to configure its personality

**Method 2: Using Prefabs**
1. Drag the EmotionalShape prefab into your scene
2. Assign an emotion profile
3. The shape will automatically be detected by the emotion systems

### Configuring the UI

1. Create a Canvas in your scene
2. Add a Panel or other container for the relationship UI
3. Add the `SocialRelationshipUI` component to this container
4. Create a prefab instance of RelationshipDisplayPrefab
5. Assign the relationship display prefab to the UI component
6. Set the selected entity to view its relationships

### Testing the System

1. Create multiple shapes in proximity to each other
2. Run the scene
3. Trigger emotions on shapes (through gameplay or debug tools)
4. Observe how emotions spread based on proximity and relationships
5. Select a shape and view its relationships in the UI

## Advanced Usage

### Custom Emotion Profiles

Create emotion profiles (ScriptableObjects) to define:
- Default emotional states
- Personality traits that affect emotional responses
- Resistant and vulnerable emotions

### Integration with Gameplay

- Use emotions to drive character behavior
- Create gameplay mechanics based on relationships
- Use the relationship system for NPC interactions

### Performance Considerations

- Adjust influence intervals for performance
- Use layers to filter which objects participate in the system
- Consider disabling the system for distant or inactive shapes

## Troubleshooting

### Common Issues

**Emotions not spreading:**
- Check influence radius and layers
- Verify that shapes have EmotionSystem components
- Ensure EmotionInfluenceSystem is enabled

**UI not updating:**
- Check references to the relationship manager
- Verify the selected entity is valid
- Ensure prefab has correct UI elements

**Visual effects not showing:**
- Check that the aura material is assigned
- Verify the EmotionAuraEffect component is enabled
- Check for shader compatibility issues

## Extension Points

The system can be extended in several ways:

1. Add new emotion types and corresponding visual effects
2. Create more complex relationship models with additional parameters
3. Implement specialized behaviors based on relationships
4. Add memory effects that make relationships persist between sessions
