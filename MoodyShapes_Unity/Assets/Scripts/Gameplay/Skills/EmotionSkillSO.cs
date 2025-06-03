using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Defines a skill or ability that can be triggered based on emotional state.
/// </summary>
[CreateAssetMenu(fileName = "NewEmotionSkill", menuName = "MoodyShapes/Emotion Skill", order = 2)]
public class EmotionSkillSO : ScriptableObject
{
    [Header("Skill Identity")]
    public string skillName = "Unnamed Skill";
    [TextArea]
    public string description = "Skill description";
    public Sprite icon;
    
    [Header("Emotion Requirements")]
    public EmotionType requiredEmotion = EmotionType.Neutral;
    [Range(0f, 1f)]
    public float minimumIntensity = 0.5f;
    public bool consumesEmotion = false;
    [Range(0f, 1f)]
    public float emotionConsumptionAmount = 0.3f;
    
    [Header("Skill Properties")]
    public float cooldownTime = 5f;
    public float castTime = 0.5f;
    public bool isPassive = false;
    
    [Header("Effects")]
    public SkillEffectType effectType = SkillEffectType.Self;
    public float effectRadius = 3f;
    public float effectDuration = 3f;
    public float effectPower = 1f;
    
    [Header("Visual and Audio")]
    public ParticleSystem skillParticles;
    public AudioClip skillSound;
    public string animationTrigger;
    public Color effectColor = Color.white;
    
    [Header("Advanced")]
    public LayerMask targetLayers = -1; // Default to everything
    public bool requiresLineOfSight = false;
    public bool emotionTransferable = false; // Can transfer emotion to target
    
    /// <summary>
    /// Validates if the entity's emotional state meets the requirements to use this skill
    /// </summary>
    public virtual bool CanUseSkill(EmotionalState emotionalState, float currentCooldown)
    {
        if (currentCooldown > 0)
            return false;
            
        if (!emotionalState.HasEmotion(requiredEmotion))
            return false;
            
        if (emotionalState.Intensity < minimumIntensity)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Hook for child classes to implement custom effects
    /// </summary>
    public virtual void ApplySkillEffect(GameObject user, GameObject target, EmotionSkillController controller)
    {
        // Base implementation for common effects
        switch (effectType)
        {
            case SkillEffectType.Self:
                ApplySelfEffect(user, controller);
                break;
                
            case SkillEffectType.Target:
                if (target != null)
                    ApplyTargetEffect(user, target, controller);
                break;
                
            case SkillEffectType.Area:
                ApplyAreaEffect(user, controller);
                break;
        }
    }
    
    protected virtual void ApplySelfEffect(GameObject user, EmotionSkillController controller)
    {
        // Default self-effect: Boost intensity of current emotion
        if (user.TryGetComponent<EmotionalState>(out var emotionalState))
        {
            emotionalState.ModifyIntensity(effectPower * 0.2f);
        }
        
        // Spawn particles if provided
        if (skillParticles != null)
        {
            ParticleSystem particles = Instantiate(skillParticles, user.transform.position, Quaternion.identity);
            particles.Play();
            
            // Set main module properties for customization
            var main = particles.main;
            main.startColor = effectColor;
        }
    }
    
    protected virtual void ApplyTargetEffect(GameObject user, GameObject target, EmotionSkillController controller)
    {
        // Default target effect: Transfer emotion
        if (emotionTransferable && 
            user.TryGetComponent<EmotionalState>(out var userEmotionalState) &&
            target.TryGetComponent<EmotionalState>(out var targetEmotionalState))
        {
            // Create an emotional stimulus that transfers emotion
            StimulusEffect effect = new StimulusEffect
            {
                EmotionToTrigger = userEmotionalState.CurrentEmotion,
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
            
            // Process the stimulus
            if (target.TryGetComponent<EmotionSystem>(out var emotionSystem))
            {
                emotionSystem.ProcessStimulus(stimulus);
            }
            else
            {
                targetEmotionalState.ProcessStimulus(stimulus);
            }
        }
        
        // Visual effects between user and target
        if (skillParticles != null)
        {
            // Create a line of particles between user and target
            Vector3 direction = (target.transform.position - user.transform.position).normalized;
            float distance = Vector3.Distance(user.transform.position, target.transform.position);
            
            ParticleSystem particles = Instantiate(skillParticles, user.transform.position, 
                Quaternion.LookRotation(direction));
            
            // Adjust particle system to reach the target
            var shape = particles.shape;
            if (shape.enabled)
            {
                shape.radius = distance;
            }
            
            var main = particles.main;
            main.startColor = effectColor;
            
            particles.Play();
        }
    }
    
    protected virtual void ApplyAreaEffect(GameObject user, EmotionSkillController controller)
    {
        // Find all entities in effect radius
        Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, effectRadius, targetLayers);
        List<GameObject> validTargets = new List<GameObject>();
        
        foreach (var hitCollider in hitColliders)
        {
            // Skip self
            if (hitCollider.gameObject == user)
                continue;
                
            // Check line of sight if required
            if (requiresLineOfSight)
            {
                Vector3 direction = hitCollider.transform.position - user.transform.position;
                if (Physics.Raycast(user.transform.position, direction.normalized, 
                    out RaycastHit hit, direction.magnitude))
                {
                    // Something is blocking line of sight
                    if (hit.collider != hitCollider)
                        continue;
                }
            }
            
            // Check if target has required components
            if (hitCollider.TryGetComponent<EmotionalState>(out _))
            {
                validTargets.Add(hitCollider.gameObject);
            }
        }
        
        // Apply effect to all valid targets
        foreach (var target in validTargets)
        {
            ApplyTargetEffect(user, target, controller);
        }
        
        // Area visual effect
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
    
    /// <summary>
    /// Consumes emotion if this skill requires it
    /// </summary>
    public virtual void ConsumeEmotion(EmotionalState emotionalState)
    {
        if (consumesEmotion && emotionalState != null)
        {
            emotionalState.ModifyIntensity(-emotionConsumptionAmount);
        }
    }
}

/// <summary>
/// Defines the type of effect a skill has
/// </summary>
public enum SkillEffectType
{
    Self,       // Affects only the user
    Target,     // Affects a specific target
    Area        // Affects all valid targets in an area
}
