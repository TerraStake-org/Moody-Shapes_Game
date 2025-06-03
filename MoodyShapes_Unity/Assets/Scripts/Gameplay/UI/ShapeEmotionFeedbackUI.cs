using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Provides visual feedback to players about how their actions will affect shape emotions.
/// This UI component shows potential emotional responses when players hover over shapes
/// or consider specific actions.
/// </summary>
public class ShapeEmotionFeedbackUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private Image emotionIcon;
    [SerializeField] private TextMeshProUGUI emotionNameText;
    [SerializeField] private Slider intensitySlider;
    [SerializeField] private TextMeshProUGUI actionDescriptionText;
    
    [Header("Visual Elements")]
    [SerializeField] private Dictionary<EmotionType, Sprite> emotionIcons = new Dictionary<EmotionType, Sprite>();
    [SerializeField] private Dictionary<EmotionType, Color> emotionColors = new Dictionary<EmotionType, Color>();
    
    [Header("Tooltip Settings")]
    [SerializeField] private float displayDelay = 0.5f;
    [SerializeField] private float tooltipOffset = 25f;
    [SerializeField] private bool followCursor = true;
    
    private GameObject currentHoveredShape;
    private EmotionalState shapeEmotionalState;
    private EmotionProfileSO shapeProfile;
    private StimulusType pendingAction;
    private float hoverTimer;
    private bool isShowingTooltip;
    
    private void Start()
    {
        // Hide the feedback panel initially
        feedbackPanel.SetActive(false);
        isShowingTooltip = false;
    }
    
    private void Update()
    {
        // Update hover timer and show tooltip if needed
        if (currentHoveredShape != null && !isShowingTooltip)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= displayDelay)
            {
                ShowEmotionFeedback();
            }
        }
        
        // Update tooltip position if following cursor
        if (isShowingTooltip && followCursor)
        {
            UpdateTooltipPosition();
        }
    }
    
    /// <summary>
    /// Call this when player hovers over a shape to start showing feedback
    /// </summary>
    public void OnShapeHover(GameObject shape)
    {
        // Get the shape's emotional components
        if (shape.TryGetComponent<EmotionalState>(out var emotionalState))
        {
            currentHoveredShape = shape;
            shapeEmotionalState = emotionalState;
            
            // Try to get the emotion profile as well
            shape.TryGetComponent<EmotionProfileHandler>(out var profileHandler);
            if (profileHandler != null)
            {
                shapeProfile = profileHandler.Profile;
            }
            
            hoverTimer = 0f;
        }
    }
    
    /// <summary>
    /// Call this when player hovers over an action button
    /// </summary>
    public void OnActionHover(StimulusType actionType)
    {
        pendingAction = actionType;
        
        // If already hovering over a shape, update immediately
        if (currentHoveredShape != null && shapeEmotionalState != null)
        {
            ShowEmotionFeedback();
        }
    }
    
    /// <summary>
    /// Call when player stops hovering over a shape
    /// </summary>
    public void OnShapeExit()
    {
        currentHoveredShape = null;
        shapeEmotionalState = null;
        shapeProfile = null;
        feedbackPanel.SetActive(false);
        isShowingTooltip = false;
    }
    
    /// <summary>
    /// Call when player stops hovering over an action
    /// </summary>
    public void OnActionExit()
    {
        pendingAction = StimulusType.PlayerPresenceNearby; // Default to just presence
        
        // Update feedback if still hovering over a shape
        if (currentHoveredShape != null && isShowingTooltip)
        {
            ShowEmotionFeedback();
        }
    }
    
    private void ShowEmotionFeedback()
    {
        if (shapeEmotionalState == null) return;
        
        // Show the feedback panel
        feedbackPanel.SetActive(true);
        isShowingTooltip = true;
        
        if (shapeProfile != null && pendingAction != StimulusType.PlayerPresenceNearby)
        {
            // If we have a profile and a specific action, show detailed prediction
            ShowDetailedEmotionPrediction();
        }
        else
        {
            // Otherwise just show current emotional state
            ShowCurrentEmotionalState();
        }
        
        UpdateTooltipPosition();
    }
    
    private void ShowDetailedEmotionPrediction()
    {
        // Get the reaction rule for this action type
        ReactionRule rule = shapeProfile.GetReactionRule(pendingAction, shapeEmotionalState.CurrentEmotion);
        
        if (rule != null)
        {
            // Show the predicted emotion
            EmotionType predictedEmotion = rule.resultingEmotion;
            
            // Calculate approximate intensity
            float currentIntensity = shapeEmotionalState.Intensity;
            float predictedIntensity = rule.overrideCurrentEmotion 
                ? Mathf.Clamp01(rule.intensityChangeAmount)
                : Mathf.Clamp01(currentIntensity + rule.intensityChangeAmount);
            
            // Update UI elements
            emotionNameText.text = $"{predictedEmotion} ({(rule.overrideCurrentEmotion ? "New" : "Modified")})";
            intensitySlider.value = predictedIntensity;
            
            // Set action description
            string actionName = GetFriendlyActionName(pendingAction);
            actionDescriptionText.text = $"{actionName} will likely cause this shape to feel {predictedEmotion}.";
            
            // Update color and icon if available
            if (emotionColors.ContainsKey(predictedEmotion))
            {
                emotionIcon.color = emotionColors[predictedEmotion];
            }
            
            if (emotionIcons.ContainsKey(predictedEmotion))
            {
                emotionIcon.sprite = emotionIcons[predictedEmotion];
            }
        }
        else
        {
            // No specific rule found, show current state with warning
            ShowCurrentEmotionalState();
            actionDescriptionText.text += "\n(Reaction to this action is uncertain)";
        }
    }
    
    private void ShowCurrentEmotionalState()
    {
        // Just display current emotional state
        emotionNameText.text = shapeEmotionalState.CurrentEmotion.ToString();
        intensitySlider.value = shapeEmotionalState.Intensity;
        
        // Set basic description
        actionDescriptionText.text = $"Current emotional state: {shapeEmotionalState.CurrentEmotion}";
        
        // Update color and icon if available
        if (emotionColors.ContainsKey(shapeEmotionalState.CurrentEmotion))
        {
            emotionIcon.color = emotionColors[shapeEmotionalState.CurrentEmotion];
        }
        
        if (emotionIcons.ContainsKey(shapeEmotionalState.CurrentEmotion))
        {
            emotionIcon.sprite = emotionIcons[shapeEmotionalState.CurrentEmotion];
        }
    }
    
    private void UpdateTooltipPosition()
    {
        if (!followCursor) return;
        
        // Position near mouse cursor
        Vector3 mousePos = Input.mousePosition;
        feedbackPanel.transform.position = new Vector3(
            mousePos.x + tooltipOffset,
            mousePos.y + tooltipOffset,
            feedbackPanel.transform.position.z
        );
    }
    
    private string GetFriendlyActionName(StimulusType action)
    {
        switch (action)
        {
            case StimulusType.PlayerPositiveGesture:
                return "Petting/Praising";
            case StimulusType.PlayerNegativeGesture:
                return "Scolding";
            case StimulusType.PlayerGiftPositive:
                return "Giving a nice gift";
            case StimulusType.PlayerGiftNegative:
                return "Giving an unwanted gift";
            case StimulusType.PlayerPresenceNearby:
                return "Being nearby";
            default:
                return action.ToString();
        }
    }
}
