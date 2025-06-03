# MoodyShapes Audio Mixer Setup

This file provides instructions for setting up the Audio Mixer for the Moody Shapes game.

## Creating the Audio Mixer

1. In Unity, go to the Project window
2. Right-click in the Assets/Audio/Mixers folder
3. Select Create > Audio Mixer
4. Name it "MoodyShapesMixer"

## Setting up the Mixer Groups

Create the following group structure:

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

## Configuring the Groups

### Music Group
- Volume: 0dB
- Effects:
  - Add a Lowpass Filter (disabled by default)
  - Add a Reverb (disabled by default)

### MusicLayers Group
- Volume: -3dB
- Effects:
  - Add a Compressor (Threshold: -15dB, Ratio: 3:1)

### MusicDefault Group
- Volume: -2dB

### SFX Group
- Volume: -1dB
- Effects:
  - Add a Compressor (Threshold: -10dB, Ratio: 2:1)

### EmotionSFX Group
- Volume: 0dB
- Effects:
  - Add a Reverb (Room Size: 0.3, Dry Level: 0.7, Wet Level: 0.3)

## Creating Exposed Parameters

Create the following exposed parameters to control through scripts:

1. MasterVolume (linked to Master group volume)
2. MusicVolume (linked to Music group volume)
3. SFXVolume (linked to SFX group volume)
4. MusicLowpassCutoff (linked to Music group lowpass filter cutoff)
5. EmotionReverbLevel (linked to EmotionSFX group reverb wet level)

## Snapshots

Create the following snapshots:

1. Default
   - All parameters at default values
   
2. EmotionalPeak
   - MusicLowpassCutoff: Reduced to 4000Hz
   - EmotionReverbLevel: Increased to 0.5
   
3. Calm
   - MusicVolume: Reduced by 2dB
   - SFXVolume: Reduced by 3dB
   
4. Intense
   - MusicVolume: Increased by 1dB
   - EmotionReverbLevel: Increased to 0.4

## Connecting to Scripts

Once the Audio Mixer is set up, assign it to:

1. MoodMusicManager._musicMixerGroup (use the Music group)
2. ShapeAudioFeedback components (use the EmotionSFX group)

## Testing

Use the AudioTestScene to test the audio system with the mixer.
