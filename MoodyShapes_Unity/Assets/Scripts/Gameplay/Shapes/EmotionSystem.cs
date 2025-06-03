using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(EmotionMemory))]
public class EmotionSystem : MonoBehaviour
{
    [Header("Core Configuration")]
    [SerializeField] private EmotionProfileSO _emotionProfile;
    [SerializeField] private bool _enableDecay = true;
    [SerializeField] private float _decayTickInterval = 0.2f;    [Header("State Monitoring")]
    [SerializeField, ReadOnly]
    private EmotionalState _currentEmotionalState;
    [SerializeField, ReadOnly]
    private List<ActiveEmotionModifier> _activeModifiers = new List<ActiveEmotionModifier>();
      // Public accessors
    public EmotionalState CurrentState => _currentEmotionalState;
    public EmotionProfileSO EmotionProfile => _emotionProfile;
    
    /// <summary>
    /// Sets the emotion profile for this system.
    /// </summary>
    /// <param name="profile">The emotion profile to use</param>
    public void SetEmotionProfile(EmotionProfileSO profile)
    {
        _emotionProfile = profile;
        if (_currentEmotionalState != null)
        {
            InitializeState();
        }
    }
    
    // Events
    public event System.Action<EmotionChangeEvent> OnEmotionChanged;
    public static event System.Action<EmotionChangeEvent> OnAnyEmotionChanged;
    public event System.Action<EmotionalStimulus, ref bool> OnStimulusReceived;

    private float _nextDecayTick;
    private EmotionMemory _memory;

    #region Lifecycle
    void Awake()
    {
        _memory = GetComponent<EmotionMemory>();
        ValidateProfile();
        InitializeState();
    }

    void Update()
    {
        if (_enableDecay && Time.time >= _nextDecayTick)
        {
            ProcessDecay();
            _nextDecayTick = Time.time + _decayTickInterval;
        }
    }
    #endregion    #region Public API
    public void ProcessStimulus(EmotionalStimulus stimulus)
    {
        if (!enabled || _emotionProfile == null) return;

        // Allow emotion shields or other effects to filter stimuli
        bool shouldProcess = true;
        if (OnStimulusReceived != null)
        {
            OnStimulusReceived.Invoke(stimulus, ref shouldProcess);
        }
        
        if (!shouldProcess) return;

        ReactionRule rule = _emotionProfile.GetReactionRule(stimulus.Type, _currentEmotionalState.CurrentEmotion);
        if (rule == null) return;

        StartCoroutine(ProcessStimulusDelayed(stimulus, rule));
    }

    public void ApplyModifier(EmotionModifier modifier)
    {
        var activeMod = new ActiveEmotionModifier(modifier);
        _activeModifiers.Add(activeMod);
       
        // Immediate effect
        RecalculateState();
    }

    public void ForceEmotion(EmotionType emotion, float intensity, float duration = 0f)
    {
        var tempMod = new EmotionModifier {
            emotionType = emotion,
            intensityChange = intensity - _currentEmotionalState.Intensity,
            duration = duration
        };
        ApplyModifier(tempMod);
    }
    #endregion

    #region Core Logic
    private IEnumerator ProcessStimulusDelayed(EmotionalStimulus stimulus, ReactionRule rule)
    {
        if (rule.reactionDelay > 0)
            yield return new WaitForSeconds(rule.reactionDelay);

        EmotionChangeEvent change = CalculateEmotionChange(stimulus, rule);
       
        if (change.hasChanged)
        {
            _currentEmotionalState.SetEmotion(change.newEmotion, change.newIntensity);
            _memory?.RecordEmotionEvent(change);
            NotifyChange(change);

            if (rule.sustainedDuration > 0)
            {
                ApplyModifier(new EmotionModifier {
                    emotionType = change.newEmotion,
                    intensityChange = 0,
                    duration = rule.sustainedDuration,
                    blocksDecay = true
                });
            }
        }
    }    private void ProcessDecay()
    {
        bool changed = _currentEmotionalState.Decay(
            _emotionProfile.emotionalDecayRate * GetDecayMultiplier(),
            _decayTickInterval
        );

        if (changed)
        {
            var change = new EmotionChangeEvent(
                gameObject,
                _currentEmotionalState.CurrentEmotion,
                _currentEmotionalState.Intensity,
                _currentEmotionalState.CurrentEmotion,
                _currentEmotionalState.Intensity,
                EmotionChangeSource.Decay
            );
            NotifyChange(change);
        }

        UpdateActiveModifiers();
    }

    private void RecalculateState()
    {
        // Reapply all active modifiers to get current state
        // (Implementation depends on your modifier stacking logic)
    }
    #endregion

    #region Helper Methods    private EmotionChangeEvent CalculateEmotionChange(EmotionalStimulus stimulus, ReactionRule rule)
    {
        EmotionType newEmotion = rule.overrideCurrentEmotion ?
            rule.resultingEmotion : _currentEmotionalState.CurrentEmotion;

        float baseIntensity = rule.overrideCurrentEmotion ?
            0 : _currentEmotionalState.Intensity;

        float intensityChange = rule.intensityChangeAmount *
                              stimulus.GetEffectivePotency(_emotionProfile) *
                              _emotionProfile.sensitivity;

        float newIntensity = Mathf.Clamp(
            baseIntensity + intensityChange,
            0f,
            rule.maxIntensityForThisReaction
        );

        var change = new EmotionChangeEvent(
            gameObject,
            _currentEmotionalState.CurrentEmotion,
            _currentEmotionalState.Intensity,
            newEmotion,
            newIntensity,
            EmotionChangeSource.Stimulus
        );
        change.stimulus = stimulus;
        return change;
    }

    private float GetDecayMultiplier()
    {
        float multiplier = 1f;
        foreach (var mod in _activeModifiers)
        {
            if (mod.modifier.blocksDecay)
                multiplier *= 0.2f; // Reduce decay rate
        }
        return multiplier;
    }

    private void UpdateActiveModifiers()
    {
        for (int i = _activeModifiers.Count - 1; i >= 0; i--)
        {
            _activeModifiers[i].remainingDuration -= _decayTickInterval;
            if (_activeModifiers[i].remainingDuration <= 0)
            {
                _activeModifiers.RemoveAt(i);
                RecalculateState();
            }
        }
    }    private void NotifyChange(EmotionChangeEvent change)
    {
        OnEmotionChanged?.Invoke(change);
        OnAnyEmotionChanged?.Invoke(change);

        // Trigger screen effects for significant emotional changes
        if (change.newIntensity > 0.8f && change.newIntensity > change.oldIntensity + 0.2f)
        {
            // Trigger more intense effects for peak emotions
            if (change.newIntensity > 0.95f)
            {
                // Very intense emotion - trigger combined effect
                EmotionScreenEffects.TriggerCombinedEffect(
                    change.newEmotion, 
                    change.newIntensity, 
                    transform.position, 
                    1.5f
                );
            }
            else
            {
                // Moderately intense emotion - just post-processing
                EmotionScreenEffects.TriggerEmotionPulse(
                    change.newEmotion,
                    change.newIntensity * 0.8f,
                    1.0f
                );
            }
        }

        // Threshold behavior check
        var thresholdBehavior = _emotionProfile.GetThresholdBehavior(
            change.newEmotion,
            change.newIntensity
        );
        if (thresholdBehavior != null)
        {
            ProcessThresholdBehavior(thresholdBehavior, change);
        }
    }
    
    private void ProcessThresholdBehavior(EmotionThresholdBehavior behavior, EmotionChangeEvent change)
    {
        // Check if this is a one-time behavior that has already triggered
        if (behavior.isOneTimeOnly && behavior.hasTriggered)
            return;
            
        // Play animation if specified
        if (!string.IsNullOrEmpty(behavior.animationTrigger))
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(behavior.animationTrigger);
            }
        }
        
        // Play particles if specified
        if (behavior.particles != null)
        {
            behavior.particles.Play();
        }
        
        // Play sound effect if specified
        if (behavior.soundEffect != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = behavior.soundEffect;
                audioSource.Play();
            }
        }
        
        // Mark as triggered if one-time only
        if (behavior.isOneTimeOnly)
        {
            behavior.hasTriggered = true;
        }
    }
    #endregion

    #region Initialization
    private void ValidateProfile()
    {
        if (_emotionProfile == null)
        {
            Debug.LogError($"Missing EmotionProfile on {gameObject.name}", this);
            enabled = false;
        }
    }    private void InitializeState()
    {
        _currentEmotionalState = new EmotionalState(
            _emotionProfile.dominantTrait,
            _emotionProfile.baselineIntensity
        );

        NotifyChange(new EmotionChangeEvent(
            gameObject,
            EmotionType.Neutral,
            0f,
            _currentEmotionalState.CurrentEmotion,
            _currentEmotionalState.Intensity,
            EmotionChangeSource.Initialization
        ));
    }
    #endregion
}

#region Supporting Types
public struct EmotionChangeEvent
{
    public GameObject entity;
    public EmotionType oldEmotion;
    public float oldIntensity;
    public EmotionType newEmotion;
    public float newIntensity;
    public EmotionChangeSource source;
    public EmotionalStimulus stimulus;
    public float changeTime; // When this change occurred (Time.time)
    public bool hasChanged => oldEmotion != newEmotion || !Mathf.Approximately(oldIntensity, newIntensity);
    
    public EmotionChangeEvent(GameObject entity, EmotionType oldEmotion, float oldIntensity, 
                             EmotionType newEmotion, float newIntensity, EmotionChangeSource source)
    {
        this.entity = entity;
        this.oldEmotion = oldEmotion;
        this.oldIntensity = oldIntensity;
        this.newEmotion = newEmotion;
        this.newIntensity = newIntensity;
        this.source = source;
        this.stimulus = null;
        this.changeTime = Time.time;
    }
}

public enum EmotionChangeSource { Initialization, Stimulus, Decay, Script }

public struct ActiveEmotionModifier
{
    public EmotionModifier modifier;
    public float remainingDuration;
   
    public ActiveEmotionModifier(EmotionModifier mod)
    {
        modifier = mod;
        remainingDuration = mod.duration;
    }
}

[System.Serializable]
public struct EmotionModifier
{
    public EmotionType emotionType;
    public float intensityChange;
    public float duration;
    public bool blocksDecay;
    public AnimationCurve falloffCurve;
}
#endregion
