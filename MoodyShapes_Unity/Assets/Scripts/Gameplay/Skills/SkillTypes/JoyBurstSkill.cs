using UnityEngine;

/// <summary>
/// A joy-sharing skill that spreads happiness to nearby shapes
/// </summary>
[CreateAssetMenu(fileName = "JoyBurst", menuName = "MoodyShapes/Skills/Joy Burst", order = 2)]
public class JoyBurstSkill : EmotionSkillSO
{
    [Header("Joy Burst Settings")]
    [SerializeField] private float minimumJoyToTransfer = 0.5f;
    [SerializeField] private float joyMultiplier = 1.2f;
    [SerializeField] private bool consumeAllJoy = false;
    
    public override bool CanUseSkill(EmotionalState emotionalState, float currentCooldown)
    {
        // Can only use if happy or joyful and above minimum intensity
        bool hasJoy = emotionalState.HasEmotion(EmotionType.Happy) || 
                      emotionalState.HasEmotion(EmotionType.Joyful);
                      
        return base.CanUseSkill(emotionalState, currentCooldown) && 
               hasJoy && 
               emotionalState.Intensity >= minimumJoyToTransfer;
    }
    
    protected override void ApplyAreaEffect(GameObject user, EmotionSkillController controller)
    {
        if (user.TryGetComponent<EmotionalState>(out var userState))
        {
            // Create joyful stimulus
            StimulusEffect effect = new StimulusEffect
            {
                EmotionToTrigger = EmotionType.Joyful,
                IntensityMultiplier = userState.Intensity * joyMultiplier,
                DurationModifier = effectDuration
            };
            
            EmotionalStimulus stimulus = new EmotionalStimulus(
                StimulusType.EmotionalContagion,
                effectPower,
                effect,
                user
            );
            
            // If we should consume all joy from the user
            if (consumeAllJoy)
            {
                userState.ModifyIntensity(-userState.Intensity);
            }
            
            // Find and affect all shapes in range
            Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, effectRadius, targetLayers);
            foreach (var hitCollider in hitColliders)
            {
                // Skip self
                if (hitCollider.gameObject == user)
                    continue;
                    
                // Check if target has required components
                if (hitCollider.TryGetComponent<EmotionalState>(out var targetState))
                {
                    // Process the stimulus
                    if (hitCollider.TryGetComponent<EmotionSystem>(out var emotionSystem))
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
        
        // Create visual effect
        if (skillParticles != null)
        {
            ParticleSystem particles = Instantiate(skillParticles, user.transform.position, Quaternion.identity);
            
            // Adjust particle system for area effect
            var shape = particles.shape;
            if (shape.enabled)
            {
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = effectRadius;
            }
            
            var main = particles.main;
            main.startColor = effectColor;
            
            particles.Play();
        }
    }
}
