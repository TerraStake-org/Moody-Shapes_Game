using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component that displays emotion-based skills and allows player interaction
/// </summary>
public class EmotionSkillUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EmotionSkillController skillController;
    [SerializeField] private GameObject skillButtonPrefab;
    [SerializeField] private Transform skillButtonContainer;
    [SerializeField] private GameObject skillTooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipTitle;
    [SerializeField] private TextMeshProUGUI tooltipDescription;
    [SerializeField] private TextMeshProUGUI tooltipEmotionRequirement;
    [SerializeField] private Image tooltipIcon;
    [SerializeField] private Image tooltipEmotionIcon;
    
    [Header("UI Settings")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color unavailableColor = Color.gray;
    [SerializeField] private Color activePassiveColor = Color.green;
    
    // Dictionary to track skill buttons
    private Dictionary<EmotionSkillSO, SkillButtonInfo> skillButtons = new Dictionary<EmotionSkillSO, SkillButtonInfo>();
    private EmotionSkillSO selectedSkill;
    
    private void Start()
    {
        // Hide tooltip on start
        if (skillTooltipPanel) 
            skillTooltipPanel.SetActive(false);
        
        // Auto-find skill controller if not set
        if (skillController == null)
        {
            skillController = FindObjectOfType<EmotionSkillController>();
        }
        
        if (skillController != null)
        {
            // Subscribe to events
            skillController.OnSkillCooldownChanged += UpdateSkillCooldown;
            skillController.OnSkillActivated += OnSkillActivated;
            skillController.OnSkillDeactivated += OnSkillDeactivated;
            
            // Initialize UI
            RefreshSkillUI();
        }
        else
        {
            Debug.LogError("EmotionSkillUI: No EmotionSkillController found!");
        }
    }
    
    private void OnDestroy()
    {
        if (skillController != null)
        {
            skillController.OnSkillCooldownChanged -= UpdateSkillCooldown;
            skillController.OnSkillActivated -= OnSkillActivated;
            skillController.OnSkillDeactivated -= OnSkillDeactivated;
        }
    }
    
    /// <summary>
    /// Refreshes the entire skill UI
    /// </summary>
    public void RefreshSkillUI()
    {
        // Clear existing buttons
        ClearSkillButtons();
        
        // Get available skills
        List<EmotionSkillSO> skills = skillController.GetAvailableSkills();
        
        // Create buttons for each skill
        foreach (var skill in skills)
        {
            CreateSkillButton(skill);
        }
    }
    
    /// <summary>
    /// Creates a UI button for a skill
    /// </summary>
    private void CreateSkillButton(EmotionSkillSO skill)
    {
        if (skillButtonPrefab == null || skillButtonContainer == null)
            return;
            
        // Instantiate button prefab
        GameObject buttonObj = Instantiate(skillButtonPrefab, skillButtonContainer);
        Button button = buttonObj.GetComponent<Button>();
        
        // Setup components
        Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = buttonObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        Image cooldownOverlay = buttonObj.transform.Find("CooldownOverlay")?.GetComponent<Image>();
        TextMeshProUGUI cooldownText = buttonObj.transform.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();
        Image passiveIndicator = buttonObj.transform.Find("PassiveIndicator")?.GetComponent<Image>();
        
        // Set button data
        if (iconImage) iconImage.sprite = skill.icon;
        if (nameText) nameText.text = skill.skillName;
        if (cooldownOverlay) cooldownOverlay.fillAmount = 0;
        if (cooldownText) cooldownText.text = "";
        if (passiveIndicator) 
        {
            passiveIndicator.gameObject.SetActive(skill.isPassive);
            passiveIndicator.color = unavailableColor;
        }
        
        // Store button info
        SkillButtonInfo buttonInfo = new SkillButtonInfo
        {
            Button = button,
            IconImage = iconImage,
            NameText = nameText,
            CooldownOverlay = cooldownOverlay,
            CooldownText = cooldownText,
            PassiveIndicator = passiveIndicator,
            Skill = skill
        };
        
        skillButtons[skill] = buttonInfo;
        
        // Set click handler
        button.onClick.AddListener(() => OnSkillButtonClicked(skill));
        
        // Set hover handlers
        AddHoverHandlers(buttonObj, skill);
        
        // Update button state
        UpdateButtonState(skill);
    }
    
    /// <summary>
    /// Adds hover event handlers to show tooltips
    /// </summary>
    private void AddHoverHandlers(GameObject buttonObj, EmotionSkillSO skill)
    {
        if (buttonObj.TryGetComponent<UnityEngine.EventSystems.IPointerEnterHandler>(out var enterHandler))
        {
            // Already has handlers
            return;
        }
        
        // Add component for hover events
        SkillTooltipTrigger tooltipTrigger = buttonObj.AddComponent<SkillTooltipTrigger>();
        tooltipTrigger.Initialize(skill, this);
    }
    
    /// <summary>
    /// Shows the tooltip for a skill
    /// </summary>
    public void ShowTooltip(EmotionSkillSO skill, Vector2 position)
    {
        if (skillTooltipPanel == null)
            return;
            
        // Set tooltip content
        if (tooltipTitle) tooltipTitle.text = skill.skillName;
        if (tooltipDescription) tooltipDescription.text = skill.description;
        if (tooltipIcon) tooltipIcon.sprite = skill.icon;
        
        // Set emotion requirement text
        if (tooltipEmotionRequirement)
        {
            string emotionText = skill.requiredEmotion.ToString();
            string intensityText = Mathf.RoundToInt(skill.minimumIntensity * 100).ToString() + "%";
            tooltipEmotionRequirement.text = $"Requires: {emotionText} ({intensityText} intensity)";
            
            // Set color based on availability
            bool available = skillController.IsSkillAvailable(skill);
            tooltipEmotionRequirement.color = available ? Color.green : Color.red;
        }
        
        // Position the tooltip
        RectTransform tooltipRect = skillTooltipPanel.GetComponent<RectTransform>();
        if (tooltipRect != null)
        {
            // Position near the button but ensure it stays on screen
            tooltipRect.position = position;
            
            // Ensure tooltip is visible
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                Vector2 tooltipSize = tooltipRect.sizeDelta;
                Vector2 viewportPos = canvas.worldCamera != null 
                    ? RectTransformUtility.WorldToViewportPoint(canvas.worldCamera, tooltipRect.position)
                    : new Vector2(tooltipRect.position.x / Screen.width, tooltipRect.position.y / Screen.height);
                    
                // If tooltip is off the right edge
                if (viewportPos.x + (tooltipSize.x / canvasRect.sizeDelta.x) > 0.95f)
                {
                    tooltipRect.position += new Vector3(-tooltipSize.x, 0, 0);
                }
                
                // If tooltip is off the top edge
                if (viewportPos.y + (tooltipSize.y / canvasRect.sizeDelta.y) > 0.95f)
                {
                    tooltipRect.position += new Vector3(0, -tooltipSize.y, 0);
                }
            }
        }
        
        // Show the tooltip
        skillTooltipPanel.SetActive(true);
    }
    
    /// <summary>
    /// Hides the skill tooltip
    /// </summary>
    public void HideTooltip()
    {
        if (skillTooltipPanel != null)
            skillTooltipPanel.SetActive(false);
    }
    
    /// <summary>
    /// Called when a skill button is clicked
    /// </summary>
    private void OnSkillButtonClicked(EmotionSkillSO skill)
    {
        // If it's a passive skill, don't try to activate it
        if (skill.isPassive) return;
        
        // Check if it's a targeted skill
        if (skill.effectType == SkillEffectType.Target)
        {
            // Select the skill for targeting
            SelectSkillForTargeting(skill);
        }
        else
        {
            // Directly activate non-targeted skills
            skillController.ActivateSkill(skill);
        }
    }
    
    /// <summary>
    /// Selects a skill for targeting
    /// </summary>
    private void SelectSkillForTargeting(EmotionSkillSO skill)
    {
        // Toggle selection if already selected
        if (selectedSkill == skill)
        {
            selectedSkill = null;
            SetTargetingMode(false);
        }
        else
        {
            selectedSkill = skill;
            SetTargetingMode(true);
        }
        
        // Highlight the selected skill button
        foreach (var pair in skillButtons)
        {
            pair.Value.Button.GetComponent<Image>().color = 
                (pair.Key == selectedSkill) ? Color.yellow : availableColor;
        }
    }
    
    /// <summary>
    /// Activates or deactivates targeting mode
    /// </summary>
    private void SetTargetingMode(bool active)
    {
        // Here you would implement cursor changes or other UI to indicate targeting mode
        
        // For now, just change the cursor
        Cursor.SetCursor(active ? null : null, Vector2.zero, CursorMode.Auto);
    }
    
    /// <summary>
    /// Attempt to use the selected skill on a target
    /// </summary>
    public void UseSelectedSkillOnTarget(GameObject target)
    {
        if (selectedSkill != null)
        {
            skillController.ActivateSkill(selectedSkill, target);
            selectedSkill = null;
            SetTargetingMode(false);
            
            // Reset button highlighting
            foreach (var pair in skillButtons)
            {
                UpdateButtonState(pair.Key);
            }
        }
    }
    
    /// <summary>
    /// Updates a skill button's visual state
    /// </summary>
    private void UpdateButtonState(EmotionSkillSO skill)
    {
        if (!skillButtons.TryGetValue(skill, out SkillButtonInfo info))
            return;
            
        bool isAvailable = skillController.IsSkillAvailable(skill);
        float cooldown = skillController.GetSkillCooldown(skill);
        
        // Set button interactability
        info.Button.interactable = isAvailable || skill.isPassive;
        
        // Set button color
        if (skill.isPassive)
        {
            // For passive skills, color indicates active/inactive
            bool isActive = false;
            if (info.PassiveIndicator != null)
            {
                isActive = info.PassiveIndicator.color == activePassiveColor;
                info.Button.GetComponent<Image>().color = isActive ? activePassiveColor : unavailableColor;
            }
        }
        else
        {
            // For active skills, color indicates availability
            info.Button.GetComponent<Image>().color = isAvailable ? availableColor : unavailableColor;
        }
        
        // Update cooldown display
        UpdateCooldownVisual(skill, cooldown);
    }
    
    /// <summary>
    /// Updates the cooldown display for a skill
    /// </summary>
    private void UpdateCooldownVisual(EmotionSkillSO skill, float remainingCooldown)
    {
        if (!skillButtons.TryGetValue(skill, out SkillButtonInfo info))
            return;
            
        // Skip if passive
        if (skill.isPassive) return;
        
        // Calculate cooldown percentage
        float cooldownPercent = remainingCooldown / skill.cooldownTime;
        
        // Update overlay fill
        if (info.CooldownOverlay != null)
        {
            info.CooldownOverlay.fillAmount = cooldownPercent;
            info.CooldownOverlay.gameObject.SetActive(cooldownPercent > 0);
        }
        
        // Update text
        if (info.CooldownText != null)
        {
            if (remainingCooldown > 0)
            {
                info.CooldownText.text = remainingCooldown.ToString("F1");
                info.CooldownText.gameObject.SetActive(true);
            }
            else
            {
                info.CooldownText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Called when a skill cooldown changes
    /// </summary>
    private void UpdateSkillCooldown(EmotionSkillSO skill, float remainingCooldown)
    {
        UpdateCooldownVisual(skill, remainingCooldown);
        UpdateButtonState(skill);
    }
    
    /// <summary>
    /// Called when a skill is activated
    /// </summary>
    private void OnSkillActivated(EmotionSkillSO skill)
    {
        if (!skillButtons.TryGetValue(skill, out SkillButtonInfo info))
            return;
            
        // For passive skills, update the indicator
        if (skill.isPassive && info.PassiveIndicator != null)
        {
            info.PassiveIndicator.color = activePassiveColor;
            info.Button.GetComponent<Image>().color = activePassiveColor;
        }
        
        UpdateButtonState(skill);
    }
    
    /// <summary>
    /// Called when a skill is deactivated
    /// </summary>
    private void OnSkillDeactivated(EmotionSkillSO skill)
    {
        if (!skillButtons.TryGetValue(skill, out SkillButtonInfo info))
            return;
            
        // For passive skills, update the indicator
        if (skill.isPassive && info.PassiveIndicator != null)
        {
            info.PassiveIndicator.color = unavailableColor;
            info.Button.GetComponent<Image>().color = unavailableColor;
        }
        
        UpdateButtonState(skill);
    }
    
    /// <summary>
    /// Clears all skill buttons
    /// </summary>
    private void ClearSkillButtons()
    {
        foreach (var buttonInfo in skillButtons.Values)
        {
            if (buttonInfo.Button != null)
            {
                Destroy(buttonInfo.Button.gameObject);
            }
        }
        
        skillButtons.Clear();
    }
    
    /// <summary>
    /// Helper class to store references to button UI elements
    /// </summary>
    private class SkillButtonInfo
    {
        public Button Button;
        public Image IconImage;
        public TextMeshProUGUI NameText;
        public Image CooldownOverlay;
        public TextMeshProUGUI CooldownText;
        public Image PassiveIndicator;
        public EmotionSkillSO Skill;
    }
}
