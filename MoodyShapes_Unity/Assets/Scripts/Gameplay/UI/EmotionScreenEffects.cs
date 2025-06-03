using UnityEngine;

/// <summary>
/// Utility class for easily triggering emotion-based screen effects from anywhere in the game
/// </summary>
public static class EmotionScreenEffects
{
    private static EmotionPostProcessingController postProcessing;
    private static EmotionScreenShakeController screenShake;
    
    /// <summary>
    /// Initialize the references to effect controllers
    /// </summary>
    public static void Initialize()
    {
        postProcessing = Object.FindObjectOfType<EmotionPostProcessingController>();
        screenShake = Object.FindObjectOfType<EmotionScreenShakeController>();
    }
    
    /// <summary>
    /// Trigger a pulse of post-processing effects for a specific emotion
    /// </summary>
    public static void TriggerEmotionPulse(EmotionType emotion, float intensity, float duration = 1.0f)
    {
        if (postProcessing == null)
        {
            postProcessing = Object.FindObjectOfType<EmotionPostProcessingController>();
        }
        
        if (postProcessing != null)
        {
            postProcessing.TriggerEmotionPulseEffect(emotion, intensity, duration);
        }
    }
    
    /// <summary>
    /// Trigger a screen shake effect for a specific emotion
    /// </summary>
    public static void TriggerScreenShake(EmotionType emotion, float intensity, Vector3 position)
    {
        if (screenShake == null)
        {
            screenShake = Object.FindObjectOfType<EmotionScreenShakeController>();
        }
        
        if (screenShake != null)
        {
            screenShake.TriggerScreenShake(emotion, intensity, position);
        }
    }
    
    /// <summary>
    /// Trigger both post-processing and screen shake effects together
    /// </summary>
    public static void TriggerCombinedEffect(EmotionType emotion, float intensity, Vector3 position, float duration = 1.0f)
    {
        TriggerEmotionPulse(emotion, intensity, duration);
        TriggerScreenShake(emotion, intensity, position);
    }
}
