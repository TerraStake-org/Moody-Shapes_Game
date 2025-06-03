using UnityEngine;

/// <summary>
/// Manages environmental changes based on player emotion usage.
/// </summary>
public class WorldAdaptationManager : MonoBehaviour
{
    [Header("Environment Settings")]
    [Tooltip("Reference to the main light in the scene.")]
    [SerializeField] private Light mainLight;

    [Tooltip("Default light intensity.")]
    [SerializeField] private float defaultLightIntensity = 1f;

    [Tooltip("Light intensity for anger.")]
    [SerializeField] private float angerLightIntensity = 0.5f;

    [Tooltip("Light intensity for joy.")]
    [SerializeField] private float joyLightIntensity = 1.5f;

    /// <summary>
    /// Applies environmental changes based on the dominant emotion.
    /// </summary>
    public void AdaptEnvironment(EmotionType dominantEmotion)
    {
        if (mainLight == null) return;

        switch (dominantEmotion)
        {
            case EmotionType.Anger:
                mainLight.intensity = angerLightIntensity;
                break;
            case EmotionType.Joy:
                mainLight.intensity = joyLightIntensity;
                break;
            default:
                mainLight.intensity = defaultLightIntensity;
                break;
        }
    }
}
