using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the skill demo scene, allowing players to test emotional skills
/// </summary>
public class SkillDemoManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject playerShape;
    [SerializeField] private List<GameObject> targetShapes = new List<GameObject>();
    [SerializeField] private EmotionSkillUI skillUI;
    [SerializeField] private SkillTargetingSystem targetingSystem;
    
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown playerEmotionDropdown;
    [SerializeField] private Slider playerIntensitySlider;
    [SerializeField] private TMP_Dropdown targetEmotionDropdown;
    [SerializeField] private Slider targetIntensitySlider;
    [SerializeField] private TextMeshProUGUI statusText;
    
    private EmotionSystem playerEmotionSystem;
    private EmotionalState playerEmotionalState;
    private EmotionSkillController playerSkillController;
    private List<EmotionSkillSO> availableSkills;
    
    private void Start()
    {
        // Get references
        playerEmotionSystem = playerShape.GetComponent<EmotionSystem>();
        playerEmotionalState = playerShape.GetComponent<EmotionalState>();
        playerSkillController = playerShape.GetComponent<EmotionSkillController>();
        
        // Load skills
        availableSkills = SkillLoader.LoadAllSkills();
        
        // Add skills to player
        foreach (var skill in availableSkills)
        {
            playerSkillController.AddSkill(skill);
        }
        
        // Initialize UI
        InitializeEmotionDropdowns();
        SetupUICallbacks();
        
        // Display status
        UpdateStatusText("Skill demo initialized. Select emotions and test skills!");
    }
    
    private void InitializeEmotionDropdowns()
    {
        // Clear existing options
        playerEmotionDropdown.ClearOptions();
        targetEmotionDropdown.ClearOptions();
        
        // Create emotion options
        List<string> emotionOptions = new List<string>();
        foreach (EmotionType emotion in System.Enum.GetValues(typeof(EmotionType)))
        {
            // Skip complex composite emotions
            if (emotion == EmotionType.Neutral || 
                emotion == EmotionType.Happy || 
                emotion == EmotionType.Sad || 
                emotion == EmotionType.Angry || 
                emotion == EmotionType.Scared || 
                emotion == EmotionType.Surprised || 
                emotion == EmotionType.Calm || 
                emotion == EmotionType.Joyful || 
                emotion == EmotionType.Curious)
            {
                emotionOptions.Add(emotion.ToString());
            }
        }
        
        // Add options to dropdowns
        playerEmotionDropdown.AddOptions(emotionOptions);
        targetEmotionDropdown.AddOptions(emotionOptions);
        
        // Select default values
        playerEmotionDropdown.value = emotionOptions.IndexOf("Neutral");
        targetEmotionDropdown.value = emotionOptions.IndexOf("Neutral");
    }
    
    private void SetupUICallbacks()
    {
        // Set up player emotion controls
        playerEmotionDropdown.onValueChanged.AddListener(OnPlayerEmotionChanged);
        playerIntensitySlider.onValueChanged.AddListener(OnPlayerIntensityChanged);
        
        // Set up target emotion controls
        targetEmotionDropdown.onValueChanged.AddListener(OnTargetEmotionChanged);
        targetIntensitySlider.onValueChanged.AddListener(OnTargetIntensityChanged);
    }
    
    private void OnPlayerEmotionChanged(int index)
    {
        if (playerEmotionDropdown.options.Count <= index) return;
        
        string emotionName = playerEmotionDropdown.options[index].text;
        if (System.Enum.TryParse<EmotionType>(emotionName, out EmotionType emotion))
        {
            // Set player emotion
            playerEmotionalState.SetEmotion(emotion, playerIntensitySlider.value, true);
            
            UpdateStatusText($"Player emotion set to {emotion} at {playerIntensitySlider.value:F2} intensity");
        }
    }
    
    private void OnPlayerIntensityChanged(float intensity)
    {
        if (playerEmotionalState != null)
        {
            // Get current emotion
            string emotionName = playerEmotionDropdown.options[playerEmotionDropdown.value].text;
            if (System.Enum.TryParse<EmotionType>(emotionName, out EmotionType emotion))
            {
                // Set player emotion with new intensity
                playerEmotionalState.SetEmotion(emotion, intensity, true);
                
                UpdateStatusText($"Player emotion intensity set to {intensity:F2}");
            }
        }
    }
    
    private void OnTargetEmotionChanged(int index)
    {
        if (targetEmotionDropdown.options.Count <= index) return;
        
        string emotionName = targetEmotionDropdown.options[index].text;
        if (System.Enum.TryParse<EmotionType>(emotionName, out EmotionType emotion))
        {
            // Set all target emotions
            foreach (var targetShape in targetShapes)
            {
                if (targetShape.TryGetComponent<EmotionalState>(out var state))
                {
                    state.SetEmotion(emotion, targetIntensitySlider.value, true);
                }
            }
            
            UpdateStatusText($"Target emotions set to {emotion} at {targetIntensitySlider.value:F2} intensity");
        }
    }
    
    private void OnTargetIntensityChanged(float intensity)
    {
        // Get current emotion
        string emotionName = targetEmotionDropdown.options[targetEmotionDropdown.value].text;
        if (System.Enum.TryParse<EmotionType>(emotionName, out EmotionType emotion))
        {
            // Set all target emotions with new intensity
            foreach (var targetShape in targetShapes)
            {
                if (targetShape.TryGetComponent<EmotionalState>(out var state))
                {
                    state.SetEmotion(emotion, intensity, true);
                }
            }
            
            UpdateStatusText($"Target emotion intensity set to {intensity:F2}");
        }
    }
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            
            // Clear the message after a delay
            StartCoroutine(ClearStatusAfterDelay(5f));
        }
    }
    
    private IEnumerator ClearStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (statusText != null)
        {
            statusText.text = "";
        }
    }
    
    /// <summary>
    /// Creates a random stimulus between two shapes for testing
    /// </summary>
    public void CreateRandomStimulus()
    {
        if (targetShapes.Count == 0) return;
        
        // Pick a random target
        GameObject target = targetShapes[Random.Range(0, targetShapes.Count)];
        
        // Create a random emotion
        EmotionType emotion = (EmotionType)(1 << Random.Range(0, 9));
        float intensity = Random.Range(0.3f, 1.0f);
        
        // Create stimulus
        StimulusEffect effect = new StimulusEffect
        {
            EmotionToTrigger = emotion,
            IntensityMultiplier = intensity,
            DurationModifier = 1.0f
        };
        
        EmotionalStimulus stimulus = new EmotionalStimulus(
            StimulusType.EnvironmentalTrigger,
            intensity,
            effect,
            null,
            target
        );
        
        // Apply to target
        if (target.TryGetComponent<EmotionSystem>(out var emotionSystem))
        {
            emotionSystem.ProcessStimulus(stimulus);
        }
        else if (target.TryGetComponent<EmotionalState>(out var emotionalState))
        {
            emotionalState.ProcessStimulus(stimulus);
        }
        
        UpdateStatusText($"Created random {emotion} stimulus with {intensity:F2} intensity on {target.name}");
    }
}
