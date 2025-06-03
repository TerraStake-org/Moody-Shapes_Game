# Screen-Space Effects Setup Guide

This guide explains how to set up and configure the emotion-based screen effects system in Moody Shapes.

## Prerequisites
- Unity 2022.3 LTS or later
- Universal Render Pipeline (URP) package installed
- Cinemachine package installed

## Automatic Setup
The easiest way to set up the screen effects system is to use the provided `EmotionScreenEffectsInitializer` component:

1. Add the `EmotionScreenEffectsInitializer` component to your main camera or a dedicated manager GameObject
2. The initializer will automatically create:
   - A Volume with post-processing effects
   - A CinemachineImpulseSource for screen shake

## Manual Setup

### Post-Processing Effects Setup
1. **Install Universal Render Pipeline (URP)**
   - Open Package Manager (Window > Package Manager)
   - Find and install "Universal RP" package
   - Create a URP Asset (Assets > Create > Rendering > Universal Render Pipeline > Pipeline Asset)
   - Set the URP Asset in Project Settings > Graphics

2. **Add Volume Component**
   - Create a new GameObject in your scene
   - Add a Volume component to it (Component > Rendering > Volume)
   - Check "Is Global" checkbox
   - Create a new Volume Profile (click "New" button next to the Profile field)

3. **Add Required Effects**
   - In the Volume component, click "Add Override"
   - Add the following effects:
     - Bloom
     - Color Adjustments
     - Vignette
     - Film Grain
     - Depth of Field

4. **Add EmotionPostProcessingController**
   - Add the EmotionPostProcessingController component to the same GameObject
   - Configure the detection settings:
     - Detection Radius: How far emotions are detected (e.g., 10)
     - Min Intensity Threshold: Minimum emotion intensity to trigger effects (e.g., 0.7)
     - Shape Layer Mask: Layer for emotional shapes
   - Configure effect intensities as desired

### Screen Shake Setup
1. **Install Cinemachine**
   - Open Package Manager (Window > Package Manager)
   - Find and install "Cinemachine" package

2. **Set Up Cinemachine**
   - Add a CinemachineBrain component to your main camera
   - Create a new GameObject for the screen shake controller
   - Add a CinemachineImpulseSource component to it

3. **Add EmotionScreenShakeController**
   - Add the EmotionScreenShakeController component to the same GameObject
   - Configure the detection settings (similar to post-processing)
   - Adjust shake settings:
     - Base Shake Intensity: Base strength of shakes
     - Emotion-specific multipliers for different emotion types

## Usage
Once set up, the screen effects system will automatically respond to emotional changes in your game:

1. Strong emotions (intensity > 0.8) will trigger subtle post-processing effects
2. Peak emotions (intensity > 0.95) will trigger both post-processing and screen shake
3. Different emotions will produce different visual effects (colors, patterns)

## Manual Triggering
You can also manually trigger effects from your code:

```csharp
// Trigger post-processing effects
EmotionScreenEffects.TriggerEmotionPulse(EmotionType.Angry, 0.8f, 1.5f);

// Trigger screen shake
EmotionScreenEffects.TriggerScreenShake(EmotionType.Surprised, 0.9f, transform.position);

// Trigger both together
EmotionScreenEffects.TriggerCombinedEffect(EmotionType.Joyful, 1.0f, transform.position, 2.0f);
```

## Troubleshooting
- **No effects visible**: Make sure URP is properly set up and post-processing is enabled
- **No screen shake**: Check that Cinemachine is properly installed and impulse source is configured
- **Effects too strong/weak**: Adjust the intensity settings in the controller components

## Advanced Configuration
For more advanced configuration, you can modify:

- The color mappings for different emotions in EmotionPostProcessingController
- The shake patterns for different emotions in EmotionScreenShakeController
- The effect thresholds and falloff distances for proximity-based effects
