using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component that handles showing/hiding tooltips when hovering over skill buttons
/// </summary>
public class SkillTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EmotionSkillSO skill;
    private EmotionSkillUI skillUI;
    
    /// <summary>
    /// Initialize the trigger with skill data and UI reference
    /// </summary>
    public void Initialize(EmotionSkillSO skillData, EmotionSkillUI ui)
    {
        skill = skillData;
        skillUI = ui;
    }
    
    /// <summary>
    /// Called when pointer enters the button
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skill != null && skillUI != null)
        {
            // Show tooltip at pointer position
            skillUI.ShowTooltip(skill, eventData.position);
        }
    }
    
    /// <summary>
    /// Called when pointer exits the button
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (skillUI != null)
        {
            skillUI.HideTooltip();
        }
    }
}
