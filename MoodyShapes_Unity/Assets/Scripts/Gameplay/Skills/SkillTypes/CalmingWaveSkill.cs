using UnityEngine;

/// <summary>
/// A calming skill that reduces the intensity of negative emotions
/// </summary>
[CreateAssetMenu(fileName = "CalmingWave", menuName = "MoodyShapes/Skills/Calming Wave", order = 1)]
public class CalmingWaveSkill : EmotionSkillSO
{
    [Header("Calming Settings")]
    [SerializeField] private float intensityReduction = 0.3f;
    [SerializeField] private EmotionType targetEmotions = EmotionType.Angry | EmotionType.Scared | EmotionType.Frustrated;
    
    protected override void ApplyTargetEffect(GameObject user, GameObject target, EmotionSkillController controller)
    {
        // Get target's emotional state
        if (target.TryGetComponent<EmotionalState>(out var targetState))
        {
            // Check if target has any of the target emotions
            if ((targetState.CurrentEmotion & targetEmotions) != 0)
            {
                // Reduce the intensity of the negative emotion
                targetState.ModifyIntensity(-intensityReduction);
                
                // If this would make the emotion too weak, try to apply calm instead
                if (targetState.Intensity < 0.2f && (targetState.CurrentEmotion & targetEmotions) != 0)
                {
                    // Create a calming stimulus
                    StimulusEffect effect = new StimulusEffect
                    {
                        EmotionToTrigger = EmotionType.Calm,
                        IntensityMultiplier = effectPower,
                        DurationModifier = effectDuration
                    };
                    
                    EmotionalStimulus stimulus = new EmotionalStimulus(
                        StimulusType.EmpathicResponse,
                        effectPower,
                        effect,
                        user,
                        target
                    );
                    
                    // Apply the stimulus
                    if (target.TryGetComponent<EmotionSystem>(out var emotionSystem))
                    {
                        emotionSystem.ProcessStimulus(stimulus);
                    }
                    else
                    {
                        targetState.ProcessStimulus(stimulus);
                    }
                }
            }
        }
        
        // Call base implementation for visuals
        base.ApplyTargetEffect(user, target, controller);
    }
    
    protected override void ApplyAreaEffect(GameObject user, EmotionSkillController controller)
    {
        // Call base to handle targeting
        base.ApplyAreaEffect(user, controller);
    }
}
