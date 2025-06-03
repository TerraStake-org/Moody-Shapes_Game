# Moody Shapes - Installation and Setup Guide

This guide provides detailed instructions for setting up the Moody Shapes game project in Unity.

## Prerequisites

### Software Requirements
- Unity 2022.3 LTS or later
- Git (for version control)
- Visual Studio 2019/2022 or Visual Studio Code (recommended IDE)

### Required Unity Packages
The following packages will be automatically installed via the manifest.json:
- Universal Render Pipeline (URP) 14.0.8+
- Cinemachine 2.9.7+
- TextMeshPro 3.0.6+
- Visual Effect Graph 14.0.8+
- Unity Test Framework
- Unity UI (UGUI)

### Recommended Optional Packages
- DOTween (for smooth animations)
- Shapes (for procedural geometry)
- Color Studio (for color palette management)

## Project Setup

### Basic Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/your-username/moody-shapes.git
   ```

2. **Open with Unity Hub**
   - Open Unity Hub
   - Click "Add" and select the MoodyShapes_Unity folder
   - Select Unity 2022.3 LTS or later
   - Click "Open"

3. **Initial Configuration**
   - Wait for Unity to import and configure all packages
   - Required packages will be automatically installed via the manifest.json
   - This process may take several minutes

4. **Import Optional Assets (if needed)**
   - DOTween: Available on the Asset Store or via OpenUPM
   - Shapes: Available on the Asset Store
   - Color Studio: Available on the Asset Store

### System-Specific Setup

## 1. Emotion System Setup

The Emotion System is the core of Moody Shapes and manages all emotional states and responses.

1. **Create a Shape GameObject**
   - Create an empty GameObject
   - Add the following components:
     - `EmotionSystem`
     - `EmotionalState`
     - `EmotionMemory`
     - `EmotionProfileHandler`
     - `ShapeInteractionHandler`

2. **Configure Emotion Profile**
   - Create an Emotion Profile Scriptable Object:
     - Right-click in Project window → Create → Moody Shapes → Emotion Profile
   - Configure base emotions, traits, and response patterns
   - Assign the created profile to the `EmotionProfileHandler` component

3. **Set Up Visual Feedback**
   - Add visual components based on your shape type:
     - `Shape_Animator` for animation-based feedback
     - `ShapeVisualController` for material/color-based feedback
   - Ensure they implement the `IShapeVisuals` interface

## 2. Screen Effects Setup

The Screen Effects system provides global visual feedback for emotional events.

1. **Automatic Setup**
   - Add an `EmotionScreenEffectsInitializer` component to your main camera
   - Click "Set Up Effects" in the Inspector
   - This will automatically create and configure all required components

2. **Manual Setup (Alternative)**
   - Create a Volume GameObject with post-processing effects:
     - Bloom, Color Adjustments, Vignette, Film Grain, Depth of Field
   - Add `EmotionPostProcessingController` to the Volume GameObject
   - Create a screen shake controller GameObject
   - Add `CinemachineImpulseSource` and `EmotionScreenShakeController` components
   - Configure detection settings and effect intensities

## 3. Skill System Setup

The Skill System allows player interaction with emotional shapes.

1. **Set Up Skill Controller**
   - Create a GameObject for skill management
   - Add `EmotionSkillController` component
   - Add `SkillTargetingSystem` component
   - Configure targeting layers and interaction settings

2. **Create Skill UI**
   - Create a Canvas for the skill UI
   - Add `EmotionSkillUI` component to a panel
   - Configure skill slot prefabs and layout settings
   - Link the UI to the Skill Controller

3. **Configure Skills**
   - Skills can be configured via:
     - ScriptableObjects: Create → Moody Shapes → Emotion Skill
     - JSON: Edit files in Resources/Skills/
   - Use the `SkillLoader` to load skills from JSON at runtime

## 4. Demo Scene Setup

1. **Basic Scene Elements**
   - Main Camera with post-processing and Cinemachine
   - Directional Light
   - Environment/Background
   - UI Canvas for game UI

2. **Add Required Managers**
   - Create an empty GameObject named "GameManager"
   - Add `SkillDemoManager` component
   - Configure references to player, shapes, and UI elements

3. **Add Shapes**
   - Create example emotional shapes with different personalities
   - Configure their emotion profiles for different responses
   - Set up interaction triggers and colliders

4. **Player Setup**
   - Create a player controller GameObject
   - Add movement and interaction scripts
   - Configure player skill loadout

## Troubleshooting

### Common Issues

1. **Missing References**
   - If you see "Missing Reference" errors, check that all required components are added
   - Make sure scriptable objects are properly assigned

2. **Post-Processing Not Working**
   - Ensure URP is properly set up in Project Settings → Graphics
   - Check that the Volume component is set to "Global"
   - Verify that post-processing is enabled in the URP Asset

3. **Skills Not Loading**
   - Check that JSON files are in the Resources/Skills/ folder
   - Verify JSON formatting is correct
   - Check for errors in the SkillLoader component

4. **Screen Shake Not Working**
   - Ensure Cinemachine is properly installed
   - Check that CinemachineImpulseSource is properly configured
   - Make sure the main camera has a CinemachineBrain component

## Advanced Configuration

For more advanced configuration options, refer to:
- `screen_effects_setup.md` for detailed post-processing setup
- `requirements.txt` for package version requirements
- Individual script documentation for component-specific settings

## Project Structure

The project follows a modular architecture:
- Emotion System: Core emotional processing and response
- Skill System: Player interaction and abilities
- Visual Feedback: Animation and screen effects
- UI System: Player interface and information display

Each system is designed to be modular and can be extended or replaced as needed.
