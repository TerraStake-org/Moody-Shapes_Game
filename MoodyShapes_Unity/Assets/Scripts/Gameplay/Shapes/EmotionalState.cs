using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class EmotionalState : MonoBehaviour
{
    public EmotionType CurrentEmotion { get; private set; }
    public float Intensity { get; private set; }
    public EmotionTraits Personality { get; private set; }
   
    public event Action<EmotionType, float> OnEmotionChanged;
    public event Action<EmotionType> OnEmotionPeaked;  // When intensity reaches 1.0
   
    private float decayTimer;
    private const float decayCheckInterval = 0.2f;

    public EmotionalState(EmotionTraits personality)
    {
        Personality = personality;
        ResetToDefault();
    }

    public void ResetToDefault()
    {
        SetEmotion(Personality.DominantTrait, Personality.Expressiveness * 0.5f, true);
    }

    public void SetEmotion(EmotionType newEmotion, float newIntensity, bool force = false)
    {
        // Check for resistance or vulnerability
        if (!force && (newEmotion & Personality.ResistantTrait) != 0)
        {
            newIntensity *= 0.5f;  // Half effect for resisted emotions
        }
        else if (!force && (newEmotion & Personality.VulnerableTrait) != 0)
        {
            newIntensity *= 1.5f;  // Amplify vulnerable emotions
        }

        newIntensity = Mathf.Clamp01(newIntensity * Personality.Expressiveness);
       
        bool emotionChanged = CurrentEmotion != newEmotion;
        bool intensityChanged = !Mathf.Approximately(Intensity, newIntensity);

        CurrentEmotion = newEmotion;
        Intensity = newIntensity;        if (emotionChanged || intensityChanged)
        {
            OnEmotionChanged?.Invoke(CurrentEmotion, Intensity);
           
            if (Intensity >= 0.99f)
            {
                OnEmotionPeaked?.Invoke(CurrentEmotion);
                
                // Trigger screen effects for peaked emotions
                EmotionScreenEffects.TriggerCombinedEffect(
                    CurrentEmotion, 
                    1.0f, 
                    transform.position, 
                    1.8f
                );
            }
            else if (Intensity >= 0.85f && intensityChanged && Intensity > 0.2f)
            {
                // Trigger less intense effects for high but not peak emotions
                EmotionScreenEffects.TriggerEmotionPulse(
                    CurrentEmotion,
                    Intensity * 0.7f,
                    1.0f
                );
            }
        }
    }

    public void ModifyIntensity(float amount)
    {
        SetEmotion(CurrentEmotion, Intensity + amount);
    }    // This method has been replaced by the version below that respects the decaySuspended flag
    private void ApplyDecay(float deltaTime)
    {
        if (CurrentEmotion == Personality.DominantTrait &&
            Mathf.Approximately(Intensity, Personality.Expressiveness * 0.5f))
        {
            return;  // Already at baseline
        }

        float decayAmount = deltaTime * (1.1f - Personality.EmotionalStability);
        Intensity = Mathf.Clamp01(Intensity - decayAmount);

        if (Intensity <= 0.01f)
        {
            SetEmotion(Personality.DominantTrait, Personality.Expressiveness * 0.5f);
        }
        else
        {
            OnEmotionChanged?.Invoke(CurrentEmotion, Intensity);
        }
    }

    public bool HasEmotion(EmotionType emotion)
    {
        return (CurrentEmotion & emotion) != 0;
    }    public bool IsDominantEmotion()
    {
        return CurrentEmotion == Personality.DominantTrait;
    }
    
    public void ProcessStimulus(EmotionalStimulus stimulus)
    {
        if (!stimulus.ShouldAffect(gameObject)) return;

        float potency = stimulus.GetEffectivePotency(Personality);
       
        // Apply primary effect
        if ((stimulus.PrimaryEffect.EmotionToTrigger & Personality.ResistantTrait) == 0)
        {
            float intensity = potency * stimulus.PrimaryEffect.IntensityMultiplier;
            SetEmotion(stimulus.PrimaryEffect.EmotionToTrigger, intensity);
           
            // Modify decay rate temporarily
            if (stimulus.PrimaryEffect.DurationModifier > 0)
            {
                StartCoroutine(TemporaryDecayModifier(stimulus.PrimaryEffect.DurationModifier));
            }
        }

        // Process secondary effects
        foreach (var effect in stimulus.SecondaryEffects)
        {
            // Similar processing with potentially reduced impact
            if ((effect.EmotionToTrigger & Personality.ResistantTrait) == 0)
            {
                float intensity = potency * effect.IntensityMultiplier * 0.7f; // Secondary effects are weaker
                SetEmotion(effect.EmotionToTrigger, intensity);
            }
        }
    }
      private IEnumerator TemporaryDecayModifier(float durationModifier)
    {
        // Save original stability
        float originalStability = Personality.EmotionalStability;
        
        // Create a modified personality with longer decay time (higher stability)
        EmotionTraits modifiedTraits = Personality;
        modifiedTraits.EmotionalStability = Mathf.Clamp01(originalStability * durationModifier);
        Personality = modifiedTraits;
        
        // Wait for the modified duration
        float modifiedDuration = 5f * durationModifier; // Base duration of 5 seconds, modified by the stimulus
        yield return new WaitForSeconds(modifiedDuration);
        
        // Restore original stability
        modifiedTraits.EmotionalStability = originalStability;
        Personality = modifiedTraits;
    }
    
    /// <summary>
    /// Suspends emotion decay temporarily
    /// </summary>
    private bool decaySuspended = false;
    
    /// <summary>
    /// Suspends the natural decay of emotions temporarily.
    /// Useful for sustained emotional reactions.
    /// </summary>
    public void SuspendDecay()
    {
        decaySuspended = true;
    }
    
    /// <summary>
    /// Resumes the natural decay of emotions after being suspended.
    /// </summary>
    public void ResumeDecay()
    {
        decaySuspended = false;
    }
    
    /// <summary>
    /// Overrides the standard UpdateDecay method to respect suspension
    /// </summary>
    public void UpdateDecay(float deltaTime)
    {
        if (decaySuspended) return;
        
        decayTimer += deltaTime;
        if (decayTimer >= decayCheckInterval)
        {
            decayTimer = 0;
            ApplyDecay(decayCheckInterval);
        }
    }
}
