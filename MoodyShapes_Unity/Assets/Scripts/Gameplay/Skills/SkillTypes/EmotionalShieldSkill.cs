using UnityEngine;

/// <summary>
/// A defensive shield that protects against negative emotions
/// </summary>
[CreateAssetMenu(fileName = "EmotionalShield", menuName = "MoodyShapes/Skills/Emotional Shield", order = 3)]
public class EmotionalShieldSkill : EmotionSkillSO
{
    [Header("Shield Settings")]
    [SerializeField] private float shieldDuration = 10f;
    [SerializeField] private EmotionType blockedEmotions = EmotionType.Angry | EmotionType.Sad | EmotionType.Scared;
    [SerializeField] private GameObject shieldVisualPrefab;
    
    protected override void ApplySelfEffect(GameObject user, EmotionSkillController controller)
    {
        // Create a temporary shield component on the user
        EmotionalShieldEffect shieldEffect = user.GetComponent<EmotionalShieldEffect>();
        if (shieldEffect == null)
        {
            shieldEffect = user.AddComponent<EmotionalShieldEffect>();
        }
        
        // Configure and activate the shield
        shieldEffect.Initialize(blockedEmotions, shieldDuration, shieldVisualPrefab, effectColor);
        
        // Call base implementation for standard effects
        base.ApplySelfEffect(user, controller);
    }
}

/// <summary>
/// Component that provides temporary protection against certain emotions
/// </summary>
public class EmotionalShieldEffect : MonoBehaviour
{
    private EmotionType blockedEmotions;
    private float remainingDuration;
    private GameObject shieldVisual;
    
    private EmotionalState emotionalState;
    private EmotionSystem emotionSystem;
    
    /// <summary>
    /// Initialize the shield effect
    /// </summary>
    public void Initialize(EmotionType emotions, float duration, GameObject visualPrefab, Color color)
    {
        blockedEmotions = emotions;
        remainingDuration = duration;
        
        emotionalState = GetComponent<EmotionalState>();
        emotionSystem = GetComponent<EmotionSystem>();
        
        // Create visual effect if provided
        if (visualPrefab != null && shieldVisual == null)
        {
            shieldVisual = Instantiate(visualPrefab, transform.position, Quaternion.identity, transform);
            
            // Set color if possible
            Renderer renderer = shieldVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
        
        // Subscribe to stimulus processing
        if (emotionSystem != null)
        {
            emotionSystem.OnStimulusReceived += FilterStimulus;
        }
    }
    
    private void Update()
    {
        // Update duration
        remainingDuration -= Time.deltaTime;
        
        // Scale shield visual based on remaining time
        if (shieldVisual != null)
        {
            float scale = Mathf.Lerp(0.5f, 1f, remainingDuration / 10f);
            shieldVisual.transform.localScale = new Vector3(scale, scale, scale);
        }
        
        // Remove when expired
        if (remainingDuration <= 0)
        {
            if (emotionSystem != null)
            {
                emotionSystem.OnStimulusReceived -= FilterStimulus;
            }
            
            if (shieldVisual != null)
            {
                Destroy(shieldVisual);
            }
            
            Destroy(this);
        }
    }
    
    /// <summary>
    /// Filters incoming emotional stimuli to block certain emotions
    /// </summary>
    private void FilterStimulus(EmotionalStimulus stimulus, ref bool shouldProcess)
    {
        // Block if the stimulus would trigger a blocked emotion
        if ((stimulus.PrimaryEffect.EmotionToTrigger & blockedEmotions) != 0)
        {
            // Visual feedback for blocked stimulus
            if (shieldVisual != null)
            {
                // Pulse the shield
                shieldVisual.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }
            
            // Block processing
            shouldProcess = false;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up
        if (emotionSystem != null)
        {
            emotionSystem.OnStimulusReceived -= FilterStimulus;
        }
        
        if (shieldVisual != null)
        {
            Destroy(shieldVisual);
        }
    }
}
