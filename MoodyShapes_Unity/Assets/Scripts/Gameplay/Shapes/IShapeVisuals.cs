using UnityEngine;

/// <summary>
/// Interface for components that can visualize emotional states
/// </summary>
public interface IShapeVisuals
{
    /// <summary>
    /// Updates the visual representation based on an emotion type and intensity
    /// </summary>
    /// <param name="newEmotion">The emotion type to visualize</param>
    /// <param name="newIntensity">The intensity of the emotion (0-1)</param>
    void UpdateVisuals(EmotionType newEmotion, float newIntensity);
}
