# Moody Shapes - Unity Game Project

## Overview
An emotional puzzle game where geometric shapes express different moods and emotions. Players interact with these shapes to solve puzzles while the game adapts to their emotional preferences.

## Project Structure

### Assets/
- **Animations/**: Shape emotion animations (e.g., "HappyCircle.controller")
- **Audio/**: 
  - **Music/**: Adaptive tracks (e.g., "JoyfulTheme.ogg")
  - **SFX/**: Emotion sounds (e.g., "AngryTriangle.wav")
- **Materials/**: Shaders/VFX (e.g., "Glow_Happy.mat")
- **Prefabs/**: Reusable objects (e.g., "Shape_Emotional.prefab")
- **Plugins/**: Third-party SDKs (e.g., FMOD, AdMob)
- **Resources/**: Dynamically loaded assets (e.g., "Levels/level1.json")
- **Scenes/**:
  - **Core/**: Base scenes (e.g., "MainMenu.unity")
  - **Levels/**: Gameplay scenes (e.g., "World1_Level5.unity")
- **Scripts/**:
  - **Gameplay/**:
    - **Shapes/**: Shape behaviors (e.g., "EmotionSystem.cs")
    - **Levels/**: Puzzle logic (e.g., "LevelManager.cs")
    - **UI/**: HUD/buttons (e.g., "EmotionWheelUI.cs")
  - **Systems/**:
    - **Save/**: Player prefs/JSON (e.g., "SaveManager.cs")
    - **Audio/**: Adaptive sound (e.g., "MoodMusicManager.cs")
    - **AI/**: Mood adaptation (e.g., "PlayerBehaviorAnalyzer.cs")
  - **Utility/**: Helpers (e.g., "CoroutineUtils.cs")
- **Textures/**: Sprites/UI art (e.g., "UI_EmotionIcons.png")

### Other Folders/
- **Docs/**: Design documents, Game Design Document (GDD)
- **ProjectSettings/**: Unity's auto-generated settings
- **Packages/**: Unity Package Manager manifests

## Setup Instructions
1. Open Unity Hub
2. Add this project folder
3. Open with Unity 2022.3 LTS or later
4. Required packages will be installed automatically via the manifest

## Requirements
### Core Requirements
- Unity 2022.3 LTS or later
- Universal Render Pipeline (URP) 14.0.8 or later
- Cinemachine 2.9.7 or later
- TextMeshPro 3.0.6 or later
- Visual Effect Graph 14.0.8 or later

### System Requirements
- OS: Windows 10/11, macOS 12+, or Linux
- GPU: Graphics card with DX11 (Shader Model 4.5) or higher capabilities
- CPU: SSE2 instruction set support
- RAM: 8GB minimum, 16GB recommended
- Storage: 2GB available space

### Post-Processing Effects Setup
1. Ensure URP is properly set up in your project
2. Make sure your camera has a Volume component attached
3. Create a Volume Profile with the following effects:
   - Bloom
   - Color Adjustments
   - Vignette
   - Film Grain
   - Depth of Field
4. The EmotionScreenEffectsInitializer can automatically set up these components

### Screen Shake Setup
1. Make sure Cinemachine is installed
2. The main camera should use a CinemachineBrain component
3. The EmotionScreenEffectsInitializer will create the necessary CinemachineImpulseSource

## Credits
- Game Design: [Your Name]
- Programming: [Team Members]
- Art & Animation: [Artists]
- Audio: [Sound Designers]
