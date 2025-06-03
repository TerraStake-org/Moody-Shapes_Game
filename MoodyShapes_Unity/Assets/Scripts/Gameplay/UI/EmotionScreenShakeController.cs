using UnityEngine;
using System.Collections;
using Cinemachine;

/// <summary>
/// Applies camera shake effects in response to intense emotions
/// </summary>
[RequireComponent(typeof(CinemachineImpulseSource))]
public class EmotionScreenShakeController : MonoBehaviour
{
    [Header("Emotion Detection")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float minIntensityThreshold = 0.75f;
    [SerializeField] private LayerMask shapeLayerMask;
    
    [Header("Shake Settings")]
    [SerializeField] private float baseShakeIntensity = 0.5f;
    [SerializeField] private float maxShakeIntensity = 1.5f;
    [SerializeField] private float shakeFrequency = 0.2f;
    [SerializeField] private float emotionIntensityMultiplier = 2.0f;
    
    [Header("Emotion Type Modifiers")]
    [SerializeField] private float angryShakeMultiplier = 1.5f;
    [SerializeField] private float scaredShakeMultiplier = 1.2f;
    [SerializeField] private float surprisedShakeMultiplier = 2.0f;
    [SerializeField] private float sadShakeMultiplier = 0.7f;
    [SerializeField] private float joyfulShakeMultiplier = 0.9f;
    
    // Cached components
    private CinemachineImpulseSource impulseSource;
    private Transform playerTransform;
    private float nextShakeTime;
    
    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
        // Find player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        
        // Subscribe to global emotion events
        EmotionSystem.OnAnyEmotionChanged += OnAnyEmotionChanged;
    }
    
    private void OnDestroy()
    {
        EmotionSystem.OnAnyEmotionChanged -= OnAnyEmotionChanged;
    }
    
    private void Update()
    {
        // We can also check for ongoing intense emotions and create subtle ambient shakes
        if (Time.time >= nextShakeTime && playerTransform != null)
        {
            CheckForIntenseEmotions();
        }
    }
    
    private void CheckForIntenseEmotions()
    {
        bool foundIntenseEmotion = false;
        float strongestIntensity = 0f;
        EmotionType strongestEmotion = EmotionType.Neutral;
        GameObject closestIntenseShape = null;
        float closestDistance = float.MaxValue;
        
        // Find shapes with intense emotions near the player
        Collider[] hitColliders = Physics.OverlapSphere(playerTransform.position, detectionRadius, shapeLayerMask);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<EmotionalState>(out var emotionalState))
            {
                // Check intensity
                float intensity = emotionalState.Intensity;
                if (intensity < minIntensityThreshold) continue;
                
                // Calculate distance
                float distance = Vector3.Distance(playerTransform.position, hitCollider.transform.position);
                if (distance > detectionRadius) continue;
                
                // Check if this is more intense than what we've found so far
                if (intensity > strongestIntensity || 
                    (intensity == strongestIntensity && distance < closestDistance))
                {
                    strongestIntensity = intensity;
                    strongestEmotion = emotionalState.CurrentEmotion;
                    closestIntenseShape = hitCollider.gameObject;
                    closestDistance = distance;
                    foundIntenseEmotion = true;
                }
            }
        }
        
        // If we found an intense emotion, trigger ambient shake
        if (foundIntenseEmotion && closestIntenseShape != null)
        {
            // Calculate shake parameters based on emotion type and intensity
            float distanceFactor = 1f - Mathf.Clamp01(closestDistance / detectionRadius);
            float shakeIntensity = CalculateShakeIntensity(strongestEmotion, strongestIntensity, distanceFactor);
            
            // Only shake if intensity is significant
            if (shakeIntensity > 0.1f)
            {
                // Generate shake from the direction of the emotional shape
                Vector3 direction = (closestIntenseShape.transform.position - playerTransform.position).normalized;
                
                // Apply subtle shake
                GenerateImpulse(direction, shakeIntensity * 0.5f);
                
                // Set time for next ambient shake
                nextShakeTime = Time.time + shakeFrequency;
            }
        }
    }
    
    private void OnAnyEmotionChanged(EmotionChangeEvent change)
    {
        // Only respond to significant increases in intensity
        if (change.newIntensity > minIntensityThreshold && 
            change.newIntensity > change.oldIntensity + 0.2f)
        {
            // Check if this happened within range
            if (playerTransform != null && change.entity != null)
            {
                float distance = Vector3.Distance(playerTransform.position, change.entity.transform.position);
                if (distance <= detectionRadius)
                {
                    // Calculate shake intensity based on emotion and distance
                    float distanceFactor = 1f - Mathf.Clamp01(distance / detectionRadius);
                    float shakeIntensity = CalculateShakeIntensity(change.newEmotion, change.newIntensity, distanceFactor);
                    
                    // Generate directional impulse from the emotion source
                    Vector3 direction = (change.entity.transform.position - playerTransform.position).normalized;
                    GenerateImpulse(direction, shakeIntensity);
                }
            }
        }
    }
    
    private float CalculateShakeIntensity(EmotionType emotion, float intensity, float distanceFactor)
    {
        // Base intensity based on emotion intensity and distance
        float baseIntensity = baseShakeIntensity * intensity * distanceFactor * emotionIntensityMultiplier;
        
        // Apply modifiers based on emotion type
        float emotionMultiplier = 1.0f;
        
        if ((emotion & EmotionType.Angry) != 0)
        {
            emotionMultiplier = angryShakeMultiplier;
        }
        else if ((emotion & EmotionType.Scared) != 0)
        {
            emotionMultiplier = scaredShakeMultiplier;
        }
        else if ((emotion & EmotionType.Surprised) != 0)
        {
            emotionMultiplier = surprisedShakeMultiplier;
        }
        else if ((emotion & EmotionType.Sad) != 0)
        {
            emotionMultiplier = sadShakeMultiplier;
        }
        else if ((emotion & (EmotionType.Happy | EmotionType.Joyful)) != 0)
        {
            emotionMultiplier = joyfulShakeMultiplier;
        }
        
        return Mathf.Min(baseIntensity * emotionMultiplier, maxShakeIntensity);
    }
    
    private void GenerateImpulse(Vector3 direction, float intensity)
    {
        if (impulseSource != null)
        {
            // Set the force of the impulse
            impulseSource.m_ImpulseDefinition.m_AmplitudeGain = intensity;
            
            // Generate impulse in the direction of the emotion source
            impulseSource.GenerateImpulseAt(transform.position, direction);
        }
    }
    
    /// <summary>
    /// Manually trigger a screen shake with specified parameters
    /// </summary>
    public void TriggerScreenShake(EmotionType emotion, float intensity, Vector3 position)
    {
        if (playerTransform == null) return;
        
        // Calculate direction and distance
        Vector3 direction = (position - playerTransform.position).normalized;
        float distance = Vector3.Distance(playerTransform.position, position);
        float distanceFactor = 1f - Mathf.Clamp01(distance / detectionRadius);
        
        // Calculate shake intensity
        float shakeIntensity = CalculateShakeIntensity(emotion, intensity, distanceFactor);
        
        // Generate impulse
        GenerateImpulse(direction, shakeIntensity);
    }
}
