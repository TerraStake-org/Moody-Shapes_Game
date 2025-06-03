# Emotion Aura Shader

This shader creates a glowing aura effect that can be used to visualize a shape's emotional state.

## How to Use

1. Add the `EmotionAuraEffect` component to any shape with an `EmotionSystem` component
2. The aura will automatically adjust its color and intensity based on the shape's current emotion

## Properties

- **Emission Color**: The base color of the aura
- **Emission Intensity**: Controls the brightness of the glow effect

## EmotionAuraEffect Component

The `EmotionAuraEffect` component provides these additional customization options:

- **Emotion Colors**: Map colors to specific emotion types
- **Pulse Speed**: How quickly the aura pulses
- **Pulse Amount**: How much the aura changes during pulsing
- **Intensity Multiplier**: Overall brightness of the effect

## Example Usage

```csharp
// Get the EmotionAuraEffect component
EmotionAuraEffect auraEffect = GetComponent<EmotionAuraEffect>();

// Customize the effect
auraEffect.SetPulseSpeed(2.0f);
auraEffect.SetIntensityMultiplier(1.5f);
```

## Integration with Emotion System

The aura automatically responds to emotional changes by:
- Changing color based on the current emotion type
- Adjusting intensity based on the emotion intensity
- Pulsing more rapidly during intense emotional states

For custom emotion visualization, you can extend the EmotionAuraEffect class and override the GetColorForEmotion method.
