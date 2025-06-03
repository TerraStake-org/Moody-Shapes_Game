using System;
using UnityEngine;

[System.Serializable]
public class EmotionalState
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
        Intensity = newIntensity;

        if (emotionChanged || intensityChanged)
        {
            OnEmotionChanged?.Invoke(CurrentEmotion, Intensity);
           
            if (Intensity >= 0.99f)
            {
                OnEmotionPeaked?.Invoke(CurrentEmotion);
            }
        }
    }

    public void ModifyIntensity(float amount)
    {
        SetEmotion(CurrentEmotion, Intensity + amount);
    }

    public void UpdateDecay(float deltaTime)
    {
        decayTimer += deltaTime;
        if (decayTimer >= decayCheckInterval)
        {
            decayTimer = 0;
            ApplyDecay(decayCheckInterval);
        }
    }

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
    }

    public bool IsDominantEmotion()
    {
        return CurrentEmotion == Personality.DominantTrait;
    }
}
