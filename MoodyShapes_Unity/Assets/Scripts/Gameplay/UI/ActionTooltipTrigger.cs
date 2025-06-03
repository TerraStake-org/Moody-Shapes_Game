using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attaches to UI action buttons to provide tooltips about emotional effects.
/// Connects the player actions to the ShapeEmotionFeedbackUI system.
/// </summary>
public class ActionTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private StimulusType actionType;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 1f);
    
    [TextArea]
    [SerializeField] private string tooltipDescription;
    
    private ShapeEmotionFeedbackUI feedbackUI;
    private Color originalColor;
    
    private void Start()
    {
        // Find the feedback UI in the scene
        feedbackUI = FindObjectOfType<ShapeEmotionFeedbackUI>();
        
        // Store the original color if we have a text component
        if (tooltipText != null)
        {
            originalColor = tooltipText.color;
        }
    }
    
    /// <summary>
    /// Called when pointer enters this UI element
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Change text color to indicate hover
        if (tooltipText != null)
        {
            tooltipText.color = hoverColor;
            
            // Show the tooltip if we have one
            if (!string.IsNullOrEmpty(tooltipDescription))
            {
                tooltipText.text += $"\n<size=80%><color=#AAAAAA>{tooltipDescription}</color></size>";
            }
        }
        
        // Notify the feedback UI about this action
        if (feedbackUI != null)
        {
            feedbackUI.OnActionHover(actionType);
        }
    }
    
    /// <summary>
    /// Called when pointer exits this UI element
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Restore original color
        if (tooltipText != null)
        {
            tooltipText.color = originalColor;
            
            // Remove the tooltip description
            string baseText = tooltipText.text.Split('\n')[0];
            tooltipText.text = baseText;
        }
        
        // Notify the feedback UI that we're no longer hovering
        if (feedbackUI != null)
        {
            feedbackUI.OnActionExit();
        }
    }
}
