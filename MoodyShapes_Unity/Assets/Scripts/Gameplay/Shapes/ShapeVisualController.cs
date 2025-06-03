using UnityEngine;

/// <summary>
/// Simple component for configuring a shape's appearance based on its emotion
/// </summary>
[RequireComponent(typeof(EmotionalState))]
[RequireComponent(typeof(Renderer))]
public class ShapeVisualController : MonoBehaviour
{
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float pulseSpeed = 1.0f;
    [SerializeField] private float rotationSpeed = 30.0f;
    [SerializeField] private bool emotionAffectsRotation = true;
    
    private EmotionalState emotionalState;
    private Renderer shapeRenderer;
    private Material material;
    private Vector3 originalScale;
    private float pulsePhase;
    
    private void Start()
    {
        emotionalState = GetComponent<EmotionalState>();
        shapeRenderer = GetComponent<Renderer>();
        
        // Create a unique material instance
        material = new Material(shapeRenderer.material);
        shapeRenderer.material = material;
        
        originalScale = transform.localScale;
        
        // Subscribe to emotion changes
        emotionalState.OnEmotionChanged += OnEmotionChanged;
        
        // Set initial appearance
        UpdateVisuals(emotionalState.CurrentEmotion, emotionalState.Intensity);
    }
    
    private void OnDestroy()
    {
        if (emotionalState != null)
        {
            emotionalState.OnEmotionChanged -= OnEmotionChanged;
        }
    }
    
    private void Update()
    {
        // Pulse effect based on intensity
        pulsePhase += Time.deltaTime * pulseSpeed * (0.5f + emotionalState.Intensity);
        float pulseFactor = Mathf.Sin(pulsePhase) * 0.1f * emotionalState.Intensity;
        
        // Apply scale
        float baseScale = Mathf.Lerp(minScale, maxScale, emotionalState.Intensity);
        transform.localScale = originalScale * (baseScale + pulseFactor);
        
        // Apply rotation based on emotion
        if (emotionAffectsRotation)
        {
            float rotationAmount = rotationSpeed * Time.deltaTime;
            
            // Different emotions rotate differently
            if (emotionalState.HasEmotion(EmotionType.Happy) || emotionalState.HasEmotion(EmotionType.Joyful))
            {
                transform.Rotate(0, rotationAmount, 0);
            }
            else if (emotionalState.HasEmotion(EmotionType.Sad))
            {
                transform.Rotate(rotationAmount * 0.3f, 0, 0);
            }
            else if (emotionalState.HasEmotion(EmotionType.Angry))
            {
                transform.Rotate(rotationAmount, rotationAmount, 0);
            }
            else if (emotionalState.HasEmotion(EmotionType.Scared))
            {
                transform.Rotate(0, 0, rotationAmount * Mathf.Sin(Time.time * 5));
            }
            else if (emotionalState.HasEmotion(EmotionType.Surprised))
            {
                transform.Rotate(rotationAmount * Mathf.Sin(Time.time * 3), 
                                 rotationAmount * Mathf.Cos(Time.time * 3), 0);
            }
            else if (emotionalState.HasEmotion(EmotionType.Calm))
            {
                transform.Rotate(0, rotationAmount * 0.2f, 0);
            }
            else if (emotionalState.HasEmotion(EmotionType.Curious))
            {
                transform.Rotate(0, rotationAmount * Mathf.Sin(Time.time), 0);
            }
        }
    }
    
    private void OnEmotionChanged(EmotionType emotion, float intensity)
    {
        UpdateVisuals(emotion, intensity);
    }
    
    private void UpdateVisuals(EmotionType emotion, float intensity)
    {
        // Update color based on emotion
        Color emotionColor = GetEmotionColor(emotion);
        
        // Adjust color based on intensity
        Color finalColor = Color.Lerp(Color.gray, emotionColor, intensity);
        material.color = finalColor;
        
        // Emission for intense emotions
        if (intensity > 0.7f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emotionColor * (intensity - 0.7f) * 3.0f);
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }
    }
    
    private Color GetEmotionColor(EmotionType emotion)
    {
        if ((emotion & EmotionType.Happy) != 0)
            return new Color(1f, 0.9f, 0.2f); // Yellow
        
        if ((emotion & EmotionType.Joyful) != 0)
            return new Color(1f, 0.8f, 0.0f); // Gold
            
        if ((emotion & EmotionType.Sad) != 0)
            return new Color(0.2f, 0.4f, 0.8f); // Blue
            
        if ((emotion & EmotionType.Angry) != 0)
            return new Color(0.9f, 0.2f, 0.2f); // Red
            
        if ((emotion & EmotionType.Scared) != 0)
            return new Color(0.8f, 0.3f, 0.8f); // Purple
            
        if ((emotion & EmotionType.Surprised) != 0)
            return new Color(0.9f, 0.9f, 0.9f); // White
            
        if ((emotion & EmotionType.Calm) != 0)
            return new Color(0.4f, 0.8f, 0.9f); // Light Blue
            
        if ((emotion & EmotionType.Frustrated) != 0)
            return new Color(0.8f, 0.4f, 0.2f); // Orange
            
        if ((emotion & EmotionType.Curious) != 0)
            return new Color(0.4f, 0.8f, 0.4f); // Green
            
        return Color.gray; // Neutral
    }
}
