using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the emotion-based skills for a shape, handling cooldowns, activation, and effects.
/// </summary>
public class EmotionSkillController : MonoBehaviour
{
    [SerializeField] private EmotionalState emotionalState;
    [SerializeField] private List<EmotionSkillSO> availableSkills = new List<EmotionSkillSO>();
    [SerializeField] private float globalCooldownTime = 0.5f;
    
    private Dictionary<EmotionSkillSO, float> skillCooldowns = new Dictionary<EmotionSkillSO, float>();
    private bool isGlobalCooldown = false;
    private Dictionary<EmotionSkillSO, bool> passiveSkillsActive = new Dictionary<EmotionSkillSO, bool>();
    
    public event Action<EmotionSkillSO, float> OnSkillCooldownChanged;
    public event Action<EmotionSkillSO> OnSkillActivated;
    public event Action<EmotionSkillSO> OnSkillDeactivated;
    
    private void Awake()
    {
        // Auto-find emotional state if not set
        if (emotionalState == null)
        {
            emotionalState = GetComponent<EmotionalState>();
        }
        
        // Initialize cooldown tracking
        foreach (var skill in availableSkills)
        {
            skillCooldowns[skill] = 0f;
            passiveSkillsActive[skill] = false;
        }
    }
    
    private void Start()
    {
        // Subscribe to emotion change events to enable/disable passive skills
        emotionalState.OnEmotionChanged += OnEmotionChanged;
        
        // Activate any passive skills that are valid at start
        CheckPassiveSkills();
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
        // Update cooldowns
        List<EmotionSkillSO> cooldownsToUpdate = new List<EmotionSkillSO>(skillCooldowns.Keys);
        foreach (var skill in cooldownsToUpdate)
        {
            if (skillCooldowns[skill] > 0)
            {
                skillCooldowns[skill] -= Time.deltaTime;
                if (skillCooldowns[skill] <= 0)
                {
                    skillCooldowns[skill] = 0;
                    OnSkillCooldownChanged?.Invoke(skill, 0f);
                }
                else
                {
                    OnSkillCooldownChanged?.Invoke(skill, skillCooldowns[skill]);
                }
            }
        }
    }
    
    /// <summary>
    /// Attempts to activate a skill if requirements are met
    /// </summary>
    public bool ActivateSkill(EmotionSkillSO skill, GameObject target = null)
    {
        // Don't activate passive skills directly
        if (skill.isPassive) return false;
        
        // Check if on cooldown or GCD
        if (skillCooldowns[skill] > 0 || isGlobalCooldown)
            return false;
            
        // Check emotional requirements
        if (!skill.CanUseSkill(emotionalState, skillCooldowns[skill]))
            return false;
            
        // Start the skill activation
        StartCoroutine(ActivateSkillCoroutine(skill, target));
        return true;
    }
    
    private IEnumerator ActivateSkillCoroutine(EmotionSkillSO skill, GameObject target)
    {
        // Trigger global cooldown
        isGlobalCooldown = true;
        
        // If there's a cast time, wait for it
        if (skill.castTime > 0)
        {
            // TODO: Trigger cast animation/visuals
            yield return new WaitForSeconds(skill.castTime);
        }
        
        // Apply skill effect
        skill.ApplySkillEffect(gameObject, target, this);
        
        // Consume emotion if configured to do so
        skill.ConsumeEmotion(emotionalState);
        
        // Put skill on cooldown
        skillCooldowns[skill] = skill.cooldownTime;
        OnSkillCooldownChanged?.Invoke(skill, skill.cooldownTime);
        
        // Notify listeners that skill was activated
        OnSkillActivated?.Invoke(skill);
        
        // End global cooldown
        yield return new WaitForSeconds(globalCooldownTime);
        isGlobalCooldown = false;
    }
    
    /// <summary>
    /// Checks if a skill is available for use
    /// </summary>
    public bool IsSkillAvailable(EmotionSkillSO skill)
    {
        if (!availableSkills.Contains(skill))
            return false;
            
        if (skillCooldowns[skill] > 0 || isGlobalCooldown)
            return false;
            
        return skill.CanUseSkill(emotionalState, skillCooldowns[skill]);
    }
    
    /// <summary>
    /// Gets remaining cooldown time for a skill
    /// </summary>
    public float GetSkillCooldown(EmotionSkillSO skill)
    {
        if (skillCooldowns.TryGetValue(skill, out float cooldown))
            return cooldown;
            
        return 0f;
    }
    
    /// <summary>
    /// Adds a new skill to the available skills list
    /// </summary>
    public void AddSkill(EmotionSkillSO skill)
    {
        if (!availableSkills.Contains(skill))
        {
            availableSkills.Add(skill);
            skillCooldowns[skill] = 0f;
            passiveSkillsActive[skill] = false;
            
            // If it's passive, check if it should be active
            if (skill.isPassive && skill.CanUseSkill(emotionalState, 0f))
            {
                ActivatePassiveSkill(skill);
            }
        }
    }
    
    /// <summary>
    /// Removes a skill from the available skills list
    /// </summary>
    public void RemoveSkill(EmotionSkillSO skill)
    {
        if (availableSkills.Contains(skill))
        {
            // Deactivate if it's an active passive skill
            if (skill.isPassive && passiveSkillsActive[skill])
            {
                DeactivatePassiveSkill(skill);
            }
            
            availableSkills.Remove(skill);
            skillCooldowns.Remove(skill);
            passiveSkillsActive.Remove(skill);
        }
    }
    
    /// <summary>
    /// Get all available skills
    /// </summary>
    public List<EmotionSkillSO> GetAvailableSkills()
    {
        return new List<EmotionSkillSO>(availableSkills);
    }
    
    /// <summary>
    /// Called when the emotional state changes
    /// </summary>
    private void OnEmotionChanged(EmotionType newEmotion, float intensity)
    {
        CheckPassiveSkills();
    }
    
    /// <summary>
    /// Checks all passive skills and activates/deactivates as needed
    /// </summary>
    private void CheckPassiveSkills()
    {
        foreach (var skill in availableSkills)
        {
            if (!skill.isPassive) continue;
            
            bool shouldBeActive = skill.CanUseSkill(emotionalState, 0f);
            bool isActive = passiveSkillsActive.ContainsKey(skill) && passiveSkillsActive[skill];
            
            if (shouldBeActive && !isActive)
            {
                ActivatePassiveSkill(skill);
            }
            else if (!shouldBeActive && isActive)
            {
                DeactivatePassiveSkill(skill);
            }
        }
    }
    
    /// <summary>
    /// Activates a passive skill
    /// </summary>
    private void ActivatePassiveSkill(EmotionSkillSO skill)
    {
        if (!skill.isPassive) return;
        
        // Apply skill effect
        skill.ApplySkillEffect(gameObject, gameObject, this);
        
        // Mark as active
        passiveSkillsActive[skill] = true;
        
        // Notify listeners
        OnSkillActivated?.Invoke(skill);
    }
    
    /// <summary>
    /// Deactivates a passive skill
    /// </summary>
    private void DeactivatePassiveSkill(EmotionSkillSO skill)
    {
        if (!skill.isPassive) return;
        
        // Mark as inactive
        passiveSkillsActive[skill] = false;
        
        // Notify listeners
        OnSkillDeactivated?.Invoke(skill);
    }
}
