using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles player interactions with shapes and connects them to the emotion system.
/// Makes shapes hoverable and clickable to display and trigger emotions.
/// </summary>
public class ShapeInteractionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Collider2D shapeCollider;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float highlightIntensity = 0.2f;
    
    private ShapeEmotionFeedbackUI feedbackUI;
    private EmotionProfileHandler profileHandler;
    private Renderer shapeRenderer;
    private Material originalMaterial;
    private bool isHovered = false;
    
    private void Start()
    {
        // Find the feedback UI in the scene
        feedbackUI = FindObjectOfType<ShapeEmotionFeedbackUI>();
        
        // Get required components
        profileHandler = GetComponent<EmotionProfileHandler>();
        shapeRenderer = GetComponent<Renderer>();
        
        // Store original material if we have a renderer
        if (shapeRenderer != null)
        {
            originalMaterial = shapeRenderer.material;
        }
    }
    
    /// <summary>
    /// Called when pointer enters this shape
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        
        // Apply highlight material
        if (shapeRenderer != null && highlightMaterial != null)
        {
            shapeRenderer.material = highlightMaterial;
        }
        
        // Notify the feedback UI
        if (feedbackUI != null)
        {
            feedbackUI.OnShapeHover(gameObject);
        }
    }
    
    /// <summary>
    /// Called when pointer exits this shape
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        
        // Restore original material
        if (shapeRenderer != null && originalMaterial != null)
        {
            shapeRenderer.material = originalMaterial;
        }
        
        // Notify the feedback UI
        if (feedbackUI != null)
        {
            feedbackUI.OnShapeExit();
        }
    }
    
    /// <summary>
    /// Called when player clicks on the shape
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Determine what type of interaction based on input
        StimulusType interactionType = DetermineInteractionType(eventData);
        
        // Create appropriate stimulus
        EmotionalStimulus stimulus = CreateStimulusForInteraction(interactionType);
        
        // Process the stimulus if we have a profile handler
        if (profileHandler != null && stimulus != null)
        {
            profileHandler.ProcessProfileStimulus(stimulus);
            
            // Show visual feedback for the interaction
            ShowInteractionFeedback(interactionType);
        }
    }
    
    /// <summary>
    /// Determines what type of interaction the player is attempting
    /// </summary>
    private StimulusType DetermineInteractionType(PointerEventData eventData)
    {
        // Left click is positive gesture
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            return StimulusType.PlayerPositiveGesture;
        }
        // Right click is negative gesture
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            return StimulusType.PlayerNegativeGesture;
        }
        // Middle click could be something else
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            return StimulusType.PlayerPresenceNearby;
        }
        
        return StimulusType.PlayerPresenceNearby;
    }
    
    /// <summary>
    /// Creates an appropriate stimulus based on the interaction type
    /// </summary>
    private EmotionalStimulus CreateStimulusForInteraction(StimulusType interactionType)
    {
        // Create different effects based on interaction type
        StimulusEffect primaryEffect;
        
        switch (interactionType)
        {
            case StimulusType.PlayerPositiveGesture:
                primaryEffect = new StimulusEffect
                {
                    EmotionToTrigger = EmotionType.Happy,
                    IntensityMultiplier = 0.7f,
                    DurationModifier = 1.2f
                };
                break;
                
            case StimulusType.PlayerNegativeGesture:
                primaryEffect = new StimulusEffect
                {
                    EmotionToTrigger = EmotionType.Angry | EmotionType.Scared,
                    IntensityMultiplier = 0.6f,
                    DurationModifier = 1.0f
                };
                break;
                
            default:
                primaryEffect = new StimulusEffect
                {
                    EmotionToTrigger = EmotionType.Curious,
                    IntensityMultiplier = 0.3f,
                    DurationModifier = 0.5f
                };
                break;
        }
        
        // Create the stimulus - use camera as source since it represents the player
        return new EmotionalStimulus(
            interactionType,
            0.8f, // Base potency
            primaryEffect,
            Camera.main.gameObject, // Source (player)
            gameObject, // Target (this shape)
            Input.mousePosition // World position of interaction
        );
    }
    
    /// <summary>
    /// Shows visual feedback for the interaction
    /// </summary>
    private void ShowInteractionFeedback(StimulusType interactionType)
    {
        // Could spawn particles, play sound, etc.
        switch (interactionType)
        {
            case StimulusType.PlayerPositiveGesture:
                // Spawn happy particles
                SpawnParticles(Color.yellow, 10);
                break;
                
            case StimulusType.PlayerNegativeGesture:
                // Spawn angry particles
                SpawnParticles(Color.red, 5);
                break;
                
            default:
                // Spawn neutral particles
                SpawnParticles(Color.white, 3);
                break;
        }
    }
    
    /// <summary>
    /// Helper to spawn particles for interaction feedback
    /// </summary>
    private void SpawnParticles(Color color, int count)
    {
        // Get or create a particle system
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles == null) return;
        
        // Configure and play
        var main = particles.main;
        main.startColor = color;
        main.maxParticles = count;
        
        particles.Play();
    }
}
