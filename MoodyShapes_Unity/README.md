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
4. Install required packages from Package Manager

## Development Notes
- This game focuses on emotional AI and adaptive gameplay
- Shape emotions drive both visual presentation and puzzle mechanics
- Music and sound effects adapt to player behavior and preferences

## Credits
- Game Design: [Your Name]
- Programming: [Team Members]
- Art & Animation: [Artists]
- Audio: [Sound Designers]
