using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Applies screen-space post-processing effects based on intense emotions in the scene
/// </summary>
[RequireComponent(typeof(Volume))]
public class EmotionPostProcessingController : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask shapeLayerMask;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minIntensityThreshold = 0.7f;
    [SerializeField] private float effectFalloffDistance = 5f;
    
    [Header("Effect Intensity")]
    [SerializeField] private float maxEffectStrength = 1.0f;
    [SerializeField] private float effectBlendSpeed = 3.0f;
    [SerializeField] private float effectDecaySpeed = 1.5f;
    
    [Header("Effect Settings")]
    [SerializeField] private bool useVignette = true;
    [SerializeField] private bool useColorAdjustments = true;
    [SerializeField] private bool useBloom = true;
    [SerializeField] private bool useDepthOfField = true;
    [SerializeField] private bool useFilmGrain = true;
    
    // Cached components
    private Volume postProcessVolume;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private Bloom bloom;
    private DepthOfField depthOfField;
    private FilmGrain filmGrain;
    
    // Runtime state
    private EmotionType currentDominantEmotion = EmotionType.Neutral;
    private float currentIntensity = 0f;
    private float targetIntensity = 0f;
    private Color targetColorTint = Color.white;
    private Coroutine pulseEffectCoroutine;
    
    // Cached values for reset
    private float defaultVignetteIntensity;
    private float defaultBloomIntensity;
    private float defaultGrainIntensity;
    private float defaultSaturation;
    
    // Emotion color mapping
    private Dictionary<EmotionType, Color> emotionColors = new Dictionary<EmotionType, Color>
    {
        { EmotionType.Happy, new Color(1f, 0.9f, 0.2f) },    // Yellow
        { EmotionType.Sad, new Color(0.2f, 0.4f, 0.8f) },    // Blue
        { EmotionType.Angry, new Color(0.9f, 0.2f, 0.2f) },  // Red
        { EmotionType.Scared, new Color(0.8f, 0.3f, 0.8f) }, // Purple
        { EmotionType.Surprised, new Color(0.9f, 0.9f, 0.9f) }, // White
        { EmotionType.Calm, new Color(0.4f, 0.8f, 0.9f) },   // Light Blue
        { EmotionType.Frustrated, new Color(0.8f, 0.4f, 0.2f) }, // Orange
        { EmotionType.Joyful, new Color(1f, 0.8f, 0.0f) },   // Gold
        { EmotionType.Curious, new Color(0.4f, 0.8f, 0.4f) } // Green
    };
    
    private void Awake()
    {
        postProcessVolume = GetComponent<Volume>();
        
        // Cache player transform if not set
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
        
        // Get profile settings
        if (postProcessVolume.profile != null)
        {
            if (useVignette && postProcessVolume.profile.TryGet(out vignette))
            {
                defaultVignetteIntensity = vignette.intensity.value;
            }
            
            if (useColorAdjustments && postProcessVolume.profile.TryGet(out colorAdjustments))
            {
                defaultSaturation = colorAdjustments.saturation.value;
            }
            
            if (useBloom && postProcessVolume.profile.TryGet(out bloom))
            {
                defaultBloomIntensity = bloom.intensity.value;
            }
            
            if (useDepthOfField && !postProcessVolume.profile.TryGet(out depthOfField))
            {
                depthOfField = null;
            }
            
            if (useFilmGrain && postProcessVolume.profile.TryGet(out filmGrain))
            {
                defaultGrainIntensity = filmGrain.intensity.value;
            }
        }
        
        // Subscribe to global emotion changes
        EmotionSystem.OnAnyEmotionChanged += OnAnyEmotionChanged;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        EmotionSystem.OnAnyEmotionChanged -= OnAnyEmotionChanged;
        
        // Reset effects
        ResetEffects();
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        // Check for shapes with intense emotions near the player
        ScanForIntenseEmotions();
        
        // Blend effect intensity
        BlendEffects();
    }
    
    private void ScanForIntenseEmotions()
    {
        // Reset target intensity if no emotions were detected this frame
        bool emotionsDetected = false;
        targetIntensity = 0f;
        
        // Find all shapes in radius
        Collider[] hitColliders = Physics.OverlapSphere(playerTransform.position, detectionRadius, shapeLayerMask);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<EmotionalState>(out var emotionalState))
            {
                // Only consider intense emotions
                float intensity = emotionalState.Intensity;
                if (intensity < minIntensityThreshold) continue;
                
                // Calculate distance factor (closer = stronger effect)
                float distance = Vector3.Distance(playerTransform.position, hitCollider.transform.position);
                if (distance > detectionRadius) continue;
                
                float distanceFactor = 1f - Mathf.Clamp01(distance / effectFalloffDistance);
                float effectStrength = intensity * distanceFactor;
                
                // If this is stronger than current target, use this emotion
                if (effectStrength > targetIntensity)
                {
                    targetIntensity = effectStrength;
                    emotionsDetected = true;
                    
                    // Update dominant emotion
                    EmotionType dominantEmotion = emotionalState.CurrentEmotion;
                    if (dominantEmotion != currentDominantEmotion)
                    {
                        currentDominantEmotion = dominantEmotion;
                        
                        // Get color for this emotion
                        if (emotionColors.TryGetValue(dominantEmotion, out Color color))
                        {
                            targetColorTint = color;
                        }
                        else
                        {
                            // Handle composite emotions by blending or using primary component
                            targetColorTint = GetCompositeEmotionColor(dominantEmotion);
                        }
                    }
                }
            }
        }
        
        // Cap the effect strength
        targetIntensity = Mathf.Min(targetIntensity, maxEffectStrength);
        
        // Decay if no emotions were detected
        if (!emotionsDetected && currentIntensity > 0)
        {
            targetIntensity = 0f;
        }
    }
    
    private void BlendEffects()
    {
        // Smoothly transition to target intensity
        float blendSpeed = (targetIntensity > currentIntensity) ? effectBlendSpeed : effectDecaySpeed;
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * blendSpeed);
        
        // Apply effects based on current intensity
        ApplyEffects(currentIntensity, targetColorTint);
    }
    
    private void ApplyEffects(float intensity, Color color)
    {
        if (intensity < 0.01f)
        {
            ResetEffects();
            return;
        }
        
        // Apply vignette effect
        if (useVignette && vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(defaultVignetteIntensity, 0.5f, intensity);
            vignette.color.value = Color.Lerp(Color.black, color, intensity * 0.7f);
        }
        
        // Apply color adjustments
        if (useColorAdjustments && colorAdjustments != null)
        {
            // Increase saturation for happy/joyful, decrease for sad/scared
            float saturationMod = 0;
            if ((currentDominantEmotion & (EmotionType.Happy | EmotionType.Joyful)) != 0)
            {
                saturationMod = 20f; // More colorful
            }
            else if ((currentDominantEmotion & (EmotionType.Sad | EmotionType.Scared)) != 0)
            {
                saturationMod = -20f; // Less colorful
            }
            
            colorAdjustments.saturation.value = defaultSaturation + (saturationMod * intensity);
            
            // Subtle color filter
            colorAdjustments.colorFilter.value = Color.Lerp(Color.white, color, intensity * 0.3f);
        }
        
        // Apply bloom for happy/joyful emotions
        if (useBloom && bloom != null)
        {
            float bloomIntensityMod = 0;
            if ((currentDominantEmotion & (EmotionType.Happy | EmotionType.Joyful | EmotionType.Surprised)) != 0)
            {
                bloomIntensityMod = 1f; // More bloom for positive emotions
            }
            
            bloom.intensity.value = defaultBloomIntensity + (bloomIntensityMod * intensity);
            bloom.tint.value = Color.Lerp(Color.white, color, intensity * 0.5f);
        }
        
        // Apply depth of field for focused emotions like anger or fear
        if (useDepthOfField && depthOfField != null)
        {
            if ((currentDominantEmotion & (EmotionType.Angry | EmotionType.Scared | EmotionType.Frustrated)) != 0)
            {
                depthOfField.active = intensity > 0.3f;
                if (depthOfField.active)
                {
                    depthOfField.focusDistance.value = 5f; // Focus in middle distance
                    depthOfField.aperture.value = Mathf.Lerp(5.6f, 2.8f, intensity); // Narrower aperture = less bokeh
                }
            }
            else
            {
                depthOfField.active = false;
            }
        }
        
        // Apply film grain for negative emotions
        if (useFilmGrain && filmGrain != null)
        {
            if ((currentDominantEmotion & (EmotionType.Angry | EmotionType.Sad | EmotionType.Scared | EmotionType.Frustrated)) != 0)
            {
                filmGrain.intensity.value = defaultGrainIntensity + (0.3f * intensity);
            }
            else
            {
                filmGrain.intensity.value = defaultGrainIntensity;
            }
        }
    }
    
    private void ResetEffects()
    {
        // Reset all effects to default values
        if (useVignette && vignette != null)
        {
            vignette.intensity.value = defaultVignetteIntensity;
            vignette.color.value = Color.black;
        }
        
        if (useColorAdjustments && colorAdjustments != null)
        {
            colorAdjustments.saturation.value = defaultSaturation;
            colorAdjustments.colorFilter.value = Color.white;
        }
        
        if (useBloom && bloom != null)
        {
            bloom.intensity.value = defaultBloomIntensity;
            bloom.tint.value = Color.white;
        }
        
        if (useDepthOfField && depthOfField != null)
        {
            depthOfField.active = false;
        }
        
        if (useFilmGrain && filmGrain != null)
        {
            filmGrain.intensity.value = defaultGrainIntensity;
        }
    }
    
    /// <summary>
    /// Manually trigger a pulse of post-processing effects in response to a significant emotion event
    /// </summary>
    public void TriggerEmotionPulseEffect(EmotionType emotion, float intensity, float duration = 1.0f)
    {
        if (pulseEffectCoroutine != null)
        {
            StopCoroutine(pulseEffectCoroutine);
        }
        
        pulseEffectCoroutine = StartCoroutine(PulseEffect(emotion, intensity, duration));
    }
    
    private IEnumerator PulseEffect(EmotionType emotion, float intensity, float duration)
    {
        // Save current values
        float originalIntensity = currentIntensity;
        EmotionType originalEmotion = currentDominantEmotion;
        Color originalColor = targetColorTint;
        
        // Get color for pulse emotion
        Color pulseColor = Color.white;
        if (emotionColors.TryGetValue(emotion, out Color color))
        {
            pulseColor = color;
        }
        else
        {
            pulseColor = GetCompositeEmotionColor(emotion);
        }
        
        // Quick ramp up
        float pulseTime = 0f;
        float peakTime = duration * 0.3f;
        while (pulseTime < peakTime)
        {
            pulseTime += Time.deltaTime;
            float t = pulseTime / peakTime;
            
            // Blend to peak intensity
            currentIntensity = Mathf.Lerp(originalIntensity, intensity, t);
            currentDominantEmotion = emotion;
            targetColorTint = pulseColor;
            
            // Apply the effects
            ApplyEffects(currentIntensity, targetColorTint);
            
            yield return null;
        }
        
        // Hold at peak briefly
        yield return new WaitForSeconds(duration * 0.1f);
        
        // Decay back to normal
        pulseTime = 0f;
        float decayTime = duration * 0.6f;
        while (pulseTime < decayTime)
        {
            pulseTime += Time.deltaTime;
            float t = pulseTime / decayTime;
            
            // Blend back to original values
            currentIntensity = Mathf.Lerp(intensity, originalIntensity, t);
            targetColorTint = Color.Lerp(pulseColor, originalColor, t);
            
            // Apply the effects
            ApplyEffects(currentIntensity, targetColorTint);
            
            yield return null;
        }
        
        // Restore original values
        currentIntensity = originalIntensity;
        currentDominantEmotion = originalEmotion;
        targetColorTint = originalColor;
        
        pulseEffectCoroutine = null;
    }
    
    private void OnAnyEmotionChanged(EmotionChangeEvent change)
    {
        // React to extreme emotion changes anywhere in the scene
        if (change.newIntensity > 0.9f && change.oldIntensity < 0.7f)
        {
            // Check if this is within range
            if (playerTransform != null && change.entity != null)
            {
                float distance = Vector3.Distance(playerTransform.position, change.entity.transform.position);
                if (distance <= detectionRadius)
                {
                    // Trigger a pulse effect proportional to distance
                    float distanceFactor = 1f - Mathf.Clamp01(distance / effectFalloffDistance);
                    TriggerEmotionPulseEffect(change.newEmotion, distanceFactor * 0.8f, 1.2f);
                }
            }
        }
    }
    
    private Color GetCompositeEmotionColor(EmotionType emotion)
    {
        // For composite emotions, blend the components
        Color result = Color.gray; // Default
        int components = 0;
        
        foreach (EmotionType baseEmotion in System.Enum.GetValues(typeof(EmotionType)))
        {
            // Skip neutral and complex composites
            if (baseEmotion == EmotionType.Neutral || 
                baseEmotion == EmotionType.Bittersweet || 
                baseEmotion == EmotionType.Anxious)
                continue;
                
            // Check if this base emotion is part of the composite
            if ((emotion & baseEmotion) != 0)
            {
                if (emotionColors.TryGetValue(baseEmotion, out Color component))
                {
                    if (components == 0)
                        result = component;
                    else
                        result = Color.Lerp(result, component, 0.5f);
                        
                    components++;
                }
            }
        }
        
        return result;
    }
}
