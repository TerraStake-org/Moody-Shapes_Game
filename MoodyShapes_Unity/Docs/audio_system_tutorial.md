# Adaptive Audio System Tutorial

This tutorial explains how to work with the Adaptive Audio System in Moody Shapes, which creates a dynamic soundscape that responds to the emotional states of characters in the game.

## Introduction to the Adaptive Audio System

The Adaptive Audio System in Moody Shapes creates a living soundscape that:
- Adapts to the dominant emotions in the scene
- Responds to dramatic emotional changes
- Provides audio feedback for player actions
- Creates an immersive emotional atmosphere

## Core Components

### 1. MoodMusicManager

The `MoodMusicManager` is the central component that manages the adaptive music:

```csharp
// Key properties of MoodMusicManager
public class MoodMusicManager : MonoBehaviour
{
    // Music layers for different emotions
    public List<MoodMusicLayer> moodLayers;
    
    // Emotional stingers for dramatic changes
    public List<EmotionStinger> emotionStingers;
    
    // Base music for neutral states
    public AudioClip defaultMusic;
    
    // Analysis settings
    public float dominanceThreshold = 0.4f;
    public bool weightByDistance = true;
    public float listenerRange = 20f;
}
```

### 2. MoodMusicLayer

Each `MoodMusicLayer` represents a specific emotional music element:

```csharp
// Definition of a music layer
[System.Serializable]
public class MoodMusicLayer
{
    public EmotionType associatedMood;  // The emotion this layer represents
    public AudioClip clip;              // The audio to play
    public float volume = 1f;           // Base volume
    public float fadeTime = 1f;         // Transition time
    public AnimationCurve fadeCurve;    // How the fade occurs
}
```

### 3. Audio Mixer Integration

The system uses Unity's Audio Mixer for blending layers:

```
- Master Group
  └─ Music Group
     ├─ Base Layer
     └─ Emotional Layers
```

## Setting Up the System

### 1. Create Audio Mixer

First, set up an Audio Mixer with appropriate groups:

1. **Create an Audio Mixer Asset**:
   ```
   Assets > Create > Audio Mixer
   ```

2. **Add Groups**:
   - Right-click in the Mixer window > Add Group
   - Create a "Music" group
   - Create a "SFX" group
   - Create an "Emotional" subgroup under Music

### 2. Set Up MoodMusicManager

1. **Add the Component**:
   ```
   1. Create an empty GameObject named "AudioManager"
   2. Add the MoodMusicManager component
   3. Add an AudioSource component (will be configured automatically)
   ```

2. **Configure the Manager**:
   - Assign the Audio Mixer Group
   - Set the default music
   - Configure mood check interval (how often to analyze emotions)
   - Set global music volume

3. **Add Emotion Layers**:
   - For each emotion you want to support:
     - Add a new element to the moodLayers list
     - Assign the associated EmotionType
     - Assign the audio clip for this emotion
     - Configure volume and fade settings
     - Create an appropriate fade curve

### 3. Prepare Audio Assets

Organize your audio assets for the system:

1. **Base Music**:
   - Loopable, neutral background music
   - Should work well when layered with emotional tracks

2. **Emotional Layers**:
   - Music layers that represent specific emotions
   - Should be designed to blend with the base track
   - Example layers:
     - Happy: Bright, major key elements
     - Sad: Melancholic, minor key elements
     - Angry: Intense, percussive elements
     - Scared: Tense, dissonant elements

3. **Stingers**:
   - Short musical phrases for emotional transitions
   - Used for dramatic emotional shifts
   - Should align musically with your layers

## Using the System

### Basic Setup

```csharp
// Minimum viable setup
public class GameAudioSetup : MonoBehaviour
{
    public AudioClip defaultMusic;
    public AudioClip happyLayer;
    public AudioClip sadLayer;
    public AudioMixerGroup musicGroup;
    
    private void Start()
    {
        // Create audio manager if not present
        if (FindObjectOfType<MoodMusicManager>() == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            MoodMusicManager manager = audioObj.AddComponent<MoodMusicManager>();
            
            // Configure base settings
            manager.defaultMusic = defaultMusic;
            manager._musicMixerGroup = musicGroup;
            
            // Add basic emotion layers
            manager._moodLayers.Add(new MoodMusicLayer
            {
                associatedMood = EmotionType.Happy,
                clip = happyLayer,
                volume = 0.8f,
                fadeTime = 2.0f
            });
            
            manager._moodLayers.Add(new MoodMusicLayer
            {
                associatedMood = EmotionType.Sad,
                clip = sadLayer,
                volume = 0.7f,
                fadeTime = 3.0f
            });
        }
    }
}
```

### Runtime Control

You can dynamically control the audio system during gameplay:

```csharp
// Get reference to manager
MoodMusicManager audioManager = FindObjectOfType<MoodMusicManager>();

// Adjust global volume
audioManager.SetGlobalVolume(0.5f);

// Change analysis settings
audioManager._dominanceThreshold = 0.3f;  // Make it more sensitive
audioManager._moodCheckInterval = 1.0f;   // Check more frequently
```

## How the System Works

### 1. Emotion Analysis

The `MoodMusicManager` periodically analyzes the emotional state of all shapes in the scene:

```csharp
private void AnalyzeMood()
{
    // Get all emotion systems
    _emotionalEntities = FindObjectsOfType<EmotionSystem>()
        .Where(es => es != null && es.enabled)
        .ToList();
    
    // Calculate mood scores
    Dictionary<EmotionType, float> moodScores = new Dictionary<EmotionType, float>();
    
    // For each entity, add its emotion to the scores
    foreach (var entity in _emotionalEntities)
    {
        // Calculate weight based on distance if enabled
        float weight = CalculateWeight(entity);
        
        // Add to scores
        AddToMoodScores(moodScores, entity.CurrentState.CurrentEmotion, 
                       entity.CurrentState.Intensity * weight);
    }
    
    // Find dominant mood and adjust audio layers
    SetDominantMood(FindDominantMood(moodScores));
}
```

### 2. Audio Layer Management

Based on the analysis, the system adjusts the volume of each layer:

```csharp
private void AdjustLayerVolumes(Dictionary<EmotionType, float> moodScores, int totalWeight)
{
    foreach (var layer in _moodLayers)
    {
        // Calculate target volume based on emotion presence
        float targetVolume = CalculateTargetVolume(layer, moodScores, totalWeight);
        
        // Set target volume for smooth transitions
        _targetVolumes[layer.associatedMood] = targetVolume * _globalMusicVolume;
    }
}
```

### 3. Smooth Transitions

The system smoothly transitions between volume levels:

```csharp
private void UpdateLayerVolumes()
{
    foreach (var layer in _moodLayers)
    {
        if (_layerSources.TryGetValue(layer.associatedMood, out AudioSource source))
        {
            // Get current and target volumes
            float currentVol = source.volume;
            float targetVol = _targetVolumes[layer.associatedMood];
            
            // If a change is needed
            if (!Mathf.Approximately(currentVol, targetVol))
            {
                // Calculate fade speed
                float fadeSpeed = 1f / layer.fadeTime;
                
                // Apply fade curve for natural transitions
                float curveValue = layer.fadeCurve.Evaluate(
                    Mathf.Abs(targetVol - currentVol));
                
                // Move toward target
                float newVol = Mathf.MoveTowards(currentVol, targetVol,
                    fadeSpeed * Time.deltaTime * curveValue);
                    
                source.volume = newVol;
            }
        }
    }
}
```

### 4. Event-Based Responses

The system listens for significant emotional changes to trigger immediate responses:

```csharp
private void HandleEmotionChange(EmotionChangeEvent change)
{
    // React to dramatic changes
    if (change.newIntensity > 0.8f && change.oldIntensity < 0.5f)
    {
        // Play appropriate stinger
        PlayStinger(change.newEmotion);
    }
}
```

## Advanced Customization

### 1. Creating Custom Fade Curves

Design custom fade curves for different emotional transitions:

1. **Quick Rise, Slow Fall (Joy)**:
   - Create a curve that rises quickly and falls slowly
   - Good for happiness that lingers

2. **Slow Rise, Quick Fall (Fear)**:
   - Create a curve that rises slowly but falls quickly
   - Good for tension that resolves suddenly

3. **Oscillating Curve (Anxiety)**:
   - Create a curve that oscillates up and down
   - Good for unstable or anxious emotions

### 2. Distance-Based Weighting

Configure how distance affects emotional influence:

```csharp
// In Inspector
weightByDistance = true;
listenerRange = 15f;  // Adjust based on level scale

// Behind the scenes
float weight = 1f;
if (_weightByDistance && _listenerTransform != null)
{
    float distance = Vector3.Distance(_listenerTransform.position, entity.transform.position);
    weight = Mathf.Clamp01(1 - (distance / _listenerRange));
}
```

### 3. Implementing Stingers

Add dramatic musical accents for significant emotional changes:

```csharp
[System.Serializable]
public class EmotionalStinger
{
    public EmotionType emotion;
    public AudioClip stingerClip;
    public float volume = 1f;
}

// Add to MoodMusicManager
public List<EmotionalStinger> stingers;
private AudioSource stingerSource;

private void PlayStinger(EmotionType emotion)
{
    // Find matching stinger
    var stinger = stingers.Find(s => s.emotion == emotion);
    if (stinger != null && stingerSource != null)
    {
        stingerSource.volume = stinger.volume * _globalMusicVolume;
        stingerSource.PlayOneShot(stinger.stingerClip);
    }
}
```

## Additional Audio Components

### 1. AudioPlaceholderGenerator

During development, you may not have all audio assets ready. The `AudioPlaceholderGenerator` creates procedural audio for testing:

```csharp
// Generate placeholder audio for all emotions
AudioPlaceholderGenerator generator = GetComponent<AudioPlaceholderGenerator>();
generator.GenerateAllEmotionSounds();

// Get a specific emotion sound
AudioClip happyClip = generator.GetEmotionClip(EmotionType.Happy);

// Play it immediately
generator.PlayEmotionSound(EmotionType.Happy);
```

This component generates simple procedural audio that matches the emotional characteristics (happy tones are major and uplifting, sad tones are minor and slow, etc.)

### 2. AudioMixer Setup

The game uses a structured AudioMixer with the following groups:

```
Master
├── Music
│   ├── MusicLayers
│   └── MusicDefault
├── SFX
│   ├── EmotionSFX
│   ├── UiSFX
│   └── AmbientSFX
```

To set up the AudioMixer:
1. Use the AudioMixerSetup utility in the Editor menu (MoodyShapes > Audio > Create Audio Mixer)
2. Follow the instructions in `Assets/Audio/Mixers/MixerSetupInstructions.md`
3. Assign the created mixer groups to the audio components

## Testing and Debugging

### 1. Using the Audio Test Scene

The project includes a comprehensive audio test scene that lets you:
- Spawn test shapes with different emotions
- Trigger specific emotions at controlled intensities
- Toggle between placeholder and real audio assets
- Adjust global volume
- Randomize emotions across multiple shapes

To use the test scene:
1. Open the scene at `Assets/Scenes/Test/AudioTestScene`
2. Press Play
3. Use the UI controls to spawn shapes and set emotions
4. Watch the console for debug information

### 2. Common Audio Issues

#### No audio playing
- Ensure AudioSources aren't muted
- Check that the AudioMixer groups are properly assigned
- Verify that the component has the appropriate audio clips assigned
- If using placeholder audio, ensure the AudioPlaceholderGenerator is active

#### Audio not responding to emotion changes
- Check the EmotionSystem is properly connected to the audio components
- Ensure the intensity thresholds aren't set too high
- Verify that the dominant emotion threshold in MoodMusicManager isn't too restrictive

#### Placeholder audio sounds strange
- The placeholder audio is generated procedurally and is meant only for testing
- Replace with proper audio assets when available

### 3. Runtime Audio Debugging

The MoodMusicManager provides runtime debug information:

```csharp
// Enable debug logging
moodMusicManager.EnableDebugLogging = true;

// Get current dominant mood
EmotionType currentMood = moodMusicManager.GetCurrentDominantMood();

// Check if a specific layer is active
bool isHappyActive = moodMusicManager.IsLayerActive(EmotionType.Happy);

// Get current layer volumes (for visualization)
Dictionary<EmotionType, float> volumes = moodMusicManager.GetCurrentLayerVolumes();
```

## Integration with Puzzle Design

### 1. Emotion-Driven Puzzle Atmosphere

Use the audio system to reinforce puzzle states:

```csharp
public class PuzzleAudioController : MonoBehaviour
{
    public MoodMusicManager audioManager;
    
    public void OnPuzzleStarted()
    {
        // Lower dominance threshold to make music more responsive
        audioManager._dominanceThreshold = 0.3f;
    }
    
    public void OnPuzzleCompleted()
    {
        // Play happy stinger regardless of overall mood
        audioManager.PlayStinger(EmotionType.Happy);
        
        // Force happy layer to be prominent for a moment
        StartCoroutine(TemporaryMoodBoost(EmotionType.Happy, 0.8f, 5f));
    }
    
    private IEnumerator TemporaryMoodBoost(EmotionType emotion, float intensity, float duration)
    {
        // Create temporary emotion entity with desired emotion
        GameObject tempEntity = new GameObject("TempEmotion");
        EmotionSystem system = tempEntity.AddComponent<EmotionSystem>();
        system.ForceEmotion(emotion, intensity, duration);
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Clean up
        Destroy(tempEntity);
    }
}
```

### 2. Emotional Transitions Between Areas

Use the audio system to signal changes in game areas:

```csharp
public class AreaTransitionManager : MonoBehaviour
{
    public MoodMusicManager audioManager;
    
    public void OnAreaEnter(AreaType newArea)
    {
        switch (newArea)
        {
            case AreaType.HappyZone:
                // Boost happy emotions in this area
                audioManager._dominanceThreshold = 0.2f; // More sensitive
                audioManager._moodLayers.Find(m => m.associatedMood == EmotionType.Happy)
                    .volume = 1.2f; // Boost volume
                break;
                
            case AreaType.ScaryZone:
                // Make scared emotion more prominent
                audioManager._dominanceThreshold = 0.3f;
                audioManager._moodLayers.Find(m => m.associatedMood == EmotionType.Scared)
                    .volume = 1.1f;
                break;
        }
    }
}
```

## Best Practices

### 1. Audio Design

- **Layer Compatibility**: Design emotional layers to complement, not clash
- **Loop Points**: Ensure all loops are clean and grid-aligned
- **Dynamic Range**: Leave headroom for layers to stack
- **Frequency Space**: Assign different frequency ranges to different emotions:
  - Happy: Higher frequencies
  - Sad: Mid-range, warm frequencies
  - Angry: Lower frequencies, percussion
  - Scared: Wide frequency range, dissonant

### 2. Technical Setup

- **Audio Settings**: Use appropriate compression settings:
  - Streaming for longer tracks
  - Compressed in memory for stingers
  - Appropriate sample rates (44.1kHz typically sufficient)
  
- **Performance**: Use object pooling for frequent stingers
  ```csharp
  // Simple stinger pool
  private List<AudioSource> stingerPool = new List<AudioSource>();
  
  private AudioSource GetStingerSource()
  {
      // Find available source
      var available = stingerPool.Find(s => !s.isPlaying);
      if (available != null)
          return available;
          
      // Create new if needed
      var newSource = gameObject.AddComponent<AudioSource>();
      newSource.outputAudioMixerGroup = _musicMixerGroup;
      stingerPool.Add(newSource);
      return newSource;
  }
  ```

### 3. Emotional Design

- **Subtlety**: Use subtle changes for regular emotions, bigger changes for dramatic moments
- **Response Time**: Balance responsiveness with stability:
  - Faster response for action-focused gameplay
  - Slower, more stable transitions for puzzle/exploration
  
- **Audio Hierarchy**: Consider the emotional importance hierarchy:
  1. Player emotions and actions (most important)
  2. Current puzzle state emotions
  3. Background/ambient emotions

## Troubleshooting

### Common Issues

1. **No Music Playing**
   - Check that AudioSource components are enabled
   - Verify AudioMixer routing
   - Check that clips are assigned and not null

2. **No Dynamic Response**
   - Verify that EmotionSystem components exist in the scene
   - Check dominanceThreshold isn't too high
   - Ensure weightByDistance settings are appropriate for your scene scale

3. **Choppy Transitions**
   - Increase fade times for smoother transitions
   - Adjust fade curves to be less abrupt
   - Check for performance issues affecting Time.deltaTime

4. **Music Too Chaotic**
   - Increase moodCheckInterval for less frequent updates
   - Increase dominanceThreshold to require stronger emotions
   - Use smoother fade curves

## Example: Complete Setup

Here's a complete example of setting up the adaptive audio system:

```csharp
public class GameAudioSetup : MonoBehaviour
{
    // Audio assets
    public AudioClip baseMusic;
    public AudioClip happyLayer;
    public AudioClip sadLayer;
    public AudioClip angryLayer;
    public AudioClip scaredLayer;
    
    // Stingers
    public AudioClip happyStinger;
    public AudioClip sadStinger;
    public AudioClip angryStinger;
    public AudioClip scaredStinger;
    
    // Audio mixer
    public AudioMixerGroup musicMixerGroup;
    
    void Awake()
    {
        SetupAudioSystem();
    }
    
    private void SetupAudioSystem()
    {
        // Create manager object
        GameObject audioObj = new GameObject("AudioManager");
        DontDestroyOnLoad(audioObj);
        
        // Add and configure MoodMusicManager
        MoodMusicManager manager = audioObj.AddComponent<MoodMusicManager>();
        manager._musicMixerGroup = musicMixerGroup;
        manager._defaultMusic = baseMusic;
        manager._globalMusicVolume = 0.8f;
        manager._moodCheckInterval = 2.0f;
        manager._dominanceThreshold = 0.4f;
        manager._weightByDistance = true;
        manager._listenerRange = 15f;
        
        // Add emotion layers
        manager._moodLayers = new List<MoodMusicLayer>
        {
            new MoodMusicLayer
            {
                associatedMood = EmotionType.Happy,
                clip = happyLayer,
                volume = 0.8f,
                fadeTime = 2.0f,
                fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1)
            },
            new MoodMusicLayer
            {
                associatedMood = EmotionType.Sad,
                clip = sadLayer,
                volume = 0.7f,
                fadeTime = 3.0f,
                fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1)
            },
            new MoodMusicLayer
            {
                associatedMood = EmotionType.Angry,
                clip = angryLayer,
                volume = 0.75f,
                fadeTime = 1.5f,
                fadeCurve = CreateQuickRiseCurve()
            },
            new MoodMusicLayer
            {
                associatedMood = EmotionType.Scared,
                clip = scaredLayer,
                volume = 0.65f,
                fadeTime = 2.5f,
                fadeCurve = CreateSlowRiseFastFallCurve()
            }
        };
    }
    
    private AnimationCurve CreateQuickRiseCurve()
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, 0);
        curve.AddKey(0.3f, 0.8f);
        curve.AddKey(1, 1);
        return curve;
    }
    
    private AnimationCurve CreateSlowRiseFastFallCurve()
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, 0);
        curve.AddKey(0.7f, 0.3f);
        curve.AddKey(1, 1);
        return curve;
    }
}
```

## Next Steps

Now that you understand the Adaptive Audio System, you can:

1. **Create custom emotional music assets**
2. **Design area-specific audio behaviors**
3. **Implement puzzle-specific audio responses**
4. **Integrate with the Skill System for audio feedback**

For more information, see:
- Emotion System Tutorial
- Skill System Tutorial
- Unity Audio Mixer documentation
