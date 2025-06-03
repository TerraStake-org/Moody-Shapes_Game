using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages emotional influence between shapes, allowing emotions to spread
/// based on proximity, intensity, and social relationships.
/// </summary>
public class EmotionInfluenceSystem : MonoBehaviour
{
    [System.Serializable]
    public class InfluenceSettings
    {
        public float baseInfluenceRadius = 5f;
        public float intensityFalloff = 0.7f;
        public float influenceInterval = 1.5f;
        public LayerMask targetLayers;
        public bool affectPlayerShapes = true;
        public bool affectNPCShapes = true;
    }
    
    [Header("Influence Settings")]
    [SerializeField] private InfluenceSettings settings;
    [SerializeField] private bool _enableInfluence = true;
    [SerializeField] private bool _debugMode = false;
    
    [Header("Social Settings")]
    [SerializeField] private bool _useSocialRelationships = true;
    [SerializeField] private SocialRelationshipManager _relationshipManager;
    
    // Public accessors
    public bool EnableInfluence { get => _enableInfluence; set => _enableInfluence = value; }
    public InfluenceSettings Settings => settings;
    
    private List<EmotionSystem> _trackedSystems = new List<EmotionSystem>();
    private float _nextInfluenceTime;
    
    private void Start()
    {
        RefreshTrackedSystems();
        _nextInfluenceTime = Time.time + settings.influenceInterval;
    }
    
    private void Update()
    {
        if (!_enableInfluence) return;
        
        if (Time.time >= _nextInfluenceTime)
        {
            ProcessInfluence();
            _nextInfluenceTime = Time.time + settings.influenceInterval;
        }
    }
      /// <summary>
    /// Refreshes the list of tracked emotional systems in the scene.
    /// </summary>
    public void RefreshTrackedSystems()
    {
        _trackedSystems.Clear();
        _trackedSystems.AddRange(FindObjectsOfType<EmotionSystem>());
        
        if (_debugMode)
        {
            Debug.Log($"EmotionInfluenceSystem: Found {_trackedSystems.Count} emotion systems");
        }
    }
    
    /// <summary>
    /// Sets up the influence system with the specified parameters.
    /// </summary>
    /// <param name="radius">The base influence radius</param>
    /// <param name="interval">The interval at which influence is processed</param>
    /// <param name="targetLayers">The layers to target for influence</param>
    public void SetupInfluenceSystem(float radius, float interval, LayerMask targetLayers)
    {
        if (settings == null)
        {
            settings = new InfluenceSettings();
        }
        
        settings.baseInfluenceRadius = radius;
        settings.influenceInterval = interval;
        settings.targetLayers = targetLayers;
    }
    
    /// <summary>
    /// Sets the social relationship manager reference.
    /// </summary>
    public void SetSocialRelationshipManager(SocialRelationshipManager manager)
    {
        _relationshipManager = manager;
        _useSocialRelationships = (manager != null);
    }
    public void RefreshTrackedSystems()
    {
        _trackedSystems.Clear();
        EmotionSystem[] systems = FindObjectsOfType<EmotionSystem>();
        
        foreach (var system in systems)
        {
            if (system.enabled)
            {
                _trackedSystems.Add(system);
            }
        }
        
        if (_debugMode)
        {
            Debug.Log($"EmotionInfluenceSystem: Tracking {_trackedSystems.Count} emotion systems");
        }
    }
    
    /// <summary>
    /// Processes emotional influence between all tracked systems.
    /// </summary>
    private void ProcessInfluence()
    {
        if (_trackedSystems.Count < 2) return;
        
        // Refresh list occasionally in case new objects have been created
        if (Random.value < 0.1f)
        {
            RefreshTrackedSystems();
        }
        
        // Check each pair of systems for potential influence
        for (int i = 0; i < _trackedSystems.Count; i++)
        {
            EmotionSystem source = _trackedSystems[i];
            if (source == null || !source.enabled || source.CurrentState.Intensity < 0.3f)
                continue;
                
            for (int j = 0; j < _trackedSystems.Count; j++)
            {
                if (i == j) continue;
                
                EmotionSystem target = _trackedSystems[j];
                if (target == null || !target.enabled)
                    continue;
                    
                if (ShouldInfluence(source, target))
                {
                    ApplyInfluence(source, target);
                }
            }
        }
    }
    
    /// <summary>
    /// Determines if a source shape should influence a target shape.
    /// </summary>
    private bool ShouldInfluence(EmotionSystem source, EmotionSystem target)
    {
        // Check layer mask
        if (((1 << target.gameObject.layer) & settings.targetLayers) == 0)
            return false;
            
        // Check distance
        float maxDistance = settings.baseInfluenceRadius * source.CurrentState.Intensity;
        float distance = Vector3.Distance(source.transform.position, target.transform.position);
        if (distance > maxDistance)
            return false;
            
        // Additional checks could be added here (line of sight, etc.)
        
        return true;
    }
    
    void ApplyInfluence(EmotionSystem source, EmotionSystem target)
    {
        float distance = Vector3.Distance(source.transform.position, target.transform.position);
        float distanceFactor = 1f - Mathf.Clamp01(
            distance / (settings.baseInfluenceRadius * source.CurrentState.Intensity)
        );

        float effectiveIntensity = source.CurrentState.Intensity * distanceFactor * settings.intensityFalloff;

        // Apply social modifier if enabled
        if (_useSocialRelationships && _relationshipManager != null)
        {
            float socialModifier = _relationshipManager.GetInfluenceModifier(source, target);
            effectiveIntensity *= socialModifier;
            _relationshipManager.UpdateRelationship(source, source.CurrentState.CurrentEmotion, effectiveIntensity);
        }

        var stimulus = new EmotionalStimulus(
            type: GetStimulusTypeForInfluence(source.CurrentState.CurrentEmotion),
            basePotency: effectiveIntensity,
            source: source.gameObject,
            target: target.gameObject
        );

        target.ProcessStimulus(stimulus);
    }
    
    /// <summary>
    /// Converts an emotion type to an appropriate stimulus type for influence.
    /// </summary>
    private StimulusType GetStimulusTypeForInfluence(EmotionType emotion)
    {
        // Map emotion types to stimulus types
        switch (emotion)
        {
            case EmotionType.Happy:
            case EmotionType.Joyful:
                return StimulusType.Encouragement;
                
            case EmotionType.Sad:
                return StimulusType.Discouragement;
                
            case EmotionType.Angry:
            case EmotionType.Frustrated:
                return StimulusType.Aggression;
                
            case EmotionType.Scared:
                return StimulusType.Fear;
                
            case EmotionType.Surprised:
                return StimulusType.Surprise;
                
            case EmotionType.Calm:
                return StimulusType.Comfort;
                
            case EmotionType.Curious:
                return StimulusType.Curiosity;
                
            default:
                return StimulusType.Social;
        }
    }
    
    /// <summary>
    /// Forces an immediate influence check.
    /// </summary>
    public void ForceInfluenceCheck()
    {
        RefreshTrackedSystems();
        ProcessInfluence();
    }
    
    /// <summary>
    /// Enables or disables the influence system.
    /// </summary>
    public void SetInfluenceEnabled(bool enabled)
    {
        _enableInfluence = enabled;
    }
}
