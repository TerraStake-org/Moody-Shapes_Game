using UnityEngine;

/// <summary>
/// Connects a shape GameObject with its EmotionProfileSO and handles profile-specific reactions.
/// Serves as a bridge between the emotion system and the specific shape configuration.
/// </summary>
public class EmotionProfileHandler : MonoBehaviour
{
    [SerializeField] private EmotionProfileSO profile;
    
    // Visual references for emotion display
    [SerializeField] private Renderer shapeRenderer;
    [SerializeField] private ParticleSystem emotionParticles;
    [SerializeField] private Transform pupilTransform;
    
    private EmotionalState emotionalState;
    private Material instancedMaterial;
    
    // Property to access the profile externally
    public EmotionProfileSO Profile => profile;
    
    private void Awake()
    {
        // Get the emotional state component
        emotionalState = GetComponent<EmotionalState>();
        
        // Create instance of material to avoid shared material modifications
        if (shapeRenderer != null && profile != null && profile.visuals.baseMaterial != null)
        {
            instancedMaterial = new Material(profile.visuals.baseMaterial);
            shapeRenderer.material = instancedMaterial;
        }
        
        // Initialize the emotional state based on profile
        InitializeEmotionalState();
    }
    
    private void OnEnable()
    {
        if (emotionalState != null)
        {
            // Subscribe to emotion change events
            emotionalState.OnEmotionChanged += HandleEmotionChanged;
            emotionalState.OnEmotionPeaked += HandleEmotionPeaked;
        }
    }
    
    private void OnDisable()
    {
        if (emotionalState != null)
        {
            // Unsubscribe from events
            emotionalState.OnEmotionChanged -= HandleEmotionChanged;
            emotionalState.OnEmotionPeaked -= HandleEmotionPeaked;
        }
    }
    
    private void InitializeEmotionalState()
    {
        if (emotionalState != null && profile != null)
        {
            // Create the personality traits from the profile
            EmotionTraits traits = new EmotionTraits
            {
                DominantTrait = profile.dominantTrait,
                EmotionalStability = 1.0f - profile.emotionalDecayRate, // Invert for clarity
                Expressiveness = profile.sensitivity
            };
            
            // If already using emotionalState, update existing values
            emotionalState.Personality = traits;
            emotionalState.ResetToDefault();
        }
    }
    
    /// <summary>
    /// Handle emotion changes by updating visuals
    /// </summary>
    private void HandleEmotionChanged(EmotionType emotion, float intensity)
    {
        UpdateVisuals(emotion, intensity);
        
        // Check for threshold behaviors
        if (profile != null)
        {
            EmotionThresholdBehavior behavior = profile.GetThresholdBehavior(emotion, intensity);
            if (behavior != null && (!behavior.isOneTimeOnly || !behavior.hasTriggered))
            {
                TriggerEmotionBehavior(behavior);
                
                // Mark as triggered if one-time only
                if (behavior.isOneTimeOnly)
                {
                    behavior.hasTriggered = true;
                }
            }
        }
    }
    
    /// <summary>
    /// Handle emotions that reach peak intensity
    /// </summary>
    private void HandleEmotionPeaked(EmotionType emotion)
    {
        // Play a more dramatic effect for peaked emotions
        if (emotionParticles != null)
        {
            var main = emotionParticles.main;
            main.startSize = 1.5f; // Larger particles for peaks
            emotionParticles.Play();
        }
    }
    
    /// <summary>
    /// Update visual representation based on emotion
    /// </summary>
    private void UpdateVisuals(EmotionType emotion, float intensity)
    {
        if (profile == null || shapeRenderer == null) return;
        
        // Update material color
        if (instancedMaterial != null && profile.visuals.emotionColorOverrides != null)
        {
            Color targetColor = profile.visuals.emotionColorOverrides.Evaluate(intensity);
            instancedMaterial.color = Color.Lerp(
                instancedMaterial.color, 
                targetColor, 
                Time.deltaTime * profile.visuals.colorTransitionSpeed
            );
        }
        
        // Update pupil shape if available
        if (pupilTransform != null)
        {
            // Find matching pupil shape for this emotion
            EmotionPupilShape pupilShape = profile.visuals.pupilShapes.Find(p => (p.emotion & emotion) != 0);
            if (pupilShape != null && pupilShape.shape != null)
            {
                // Assuming we have a sprite renderer on the pupil transform
                SpriteRenderer pupilRenderer = pupilTransform.GetComponent<SpriteRenderer>();
                if (pupilRenderer != null)
                {
                    pupilRenderer.sprite = pupilShape.shape;
                    
                    // Scale pupil based on emotion intensity
                    Vector3 targetScale = Vector3.Lerp(
                        Vector3.one * 0.5f, 
                        new Vector3(pupilShape.scale.x, pupilShape.scale.y, 1f),
                        intensity
                    );
                    pupilTransform.localScale = targetScale;
                }
            }
        }
        
        // Update particles to match emotion if available
        if (emotionParticles != null)
        {
            var main = emotionParticles.main;
            if (profile.visuals.emotionColorOverrides != null)
            {
                main.startColor = profile.visuals.emotionColorOverrides.Evaluate(intensity);
            }
            
            // Only emit particles if intensity is significant
            if (intensity > 0.3f && !emotionParticles.isPlaying)
            {
                main.startSize = intensity;
                emotionParticles.Play();
            }
            else if (intensity <= 0.3f && emotionParticles.isPlaying)
            {
                emotionParticles.Stop();
            }
        }
    }
    
    /// <summary>
    /// Trigger a specific emotional behavior
    /// </summary>
    private void TriggerEmotionBehavior(EmotionThresholdBehavior behavior)
    {
        // Play animation trigger if specified
        if (!string.IsNullOrEmpty(behavior.animationTrigger))
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(behavior.animationTrigger);
            }
        }
        
        // Play particles if specified
        if (behavior.particles != null)
        {
            behavior.particles.Play();
        }
        
        // Play sound effect if specified
        if (behavior.soundEffect != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = behavior.soundEffect;
                audioSource.Play();
            }
        }
    }
    
    /// <summary>
    /// Process a stimulus using the profile's reaction rules
    /// </summary>
    public void ProcessProfileStimulus(EmotionalStimulus stimulus)
    {
        if (profile == null || emotionalState == null) return;
        
        // First let the base emotional state process the stimulus
        emotionalState.ProcessStimulus(stimulus);
        
        // Then check for any special profile-specific reactions
        ReactionRule rule = profile.GetReactionRule(stimulus.Type, emotionalState.CurrentEmotion);
        
        if (rule != null)
        {
            // Handle any special rule processing here, could include
            // delayed reactions, special effects, etc.
            
            if (rule.reactionDelay > 0)
            {
                // Use coroutine for delayed reaction
                StartCoroutine(DelayedReaction(rule));
            }
            else
            {
                // Apply immediate reaction
                ApplyReactionRule(rule);
            }
        }
    }
    
    private System.Collections.IEnumerator DelayedReaction(ReactionRule rule)
    {
        yield return new WaitForSeconds(rule.reactionDelay);
        
        ApplyReactionRule(rule);
        
        // Apply sustained duration if specified
        if (rule.sustainedDuration > 0)
        {
            // This could lock the emotion in place for a duration
            // before allowing normal decay to resume
            yield return new WaitForSeconds(rule.sustainedDuration);
            
            // Resume normal decay
            emotionalState.ResumeDecay();
        }
    }
    
    private void ApplyReactionRule(ReactionRule rule)
    {
        if (rule.overrideCurrentEmotion)
        {
            // Completely replace current emotion
            emotionalState.SetEmotion(
                rule.resultingEmotion, 
                Mathf.Clamp01(rule.intensityChangeAmount),
                true
            );
        }
        else
        {
            // Just modify intensity of current emotion
            emotionalState.ModifyIntensity(rule.intensityChangeAmount);
        }
    }
}
