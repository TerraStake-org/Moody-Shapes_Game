using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An empathic link skill that creates passive emotion sharing between shapes
/// </summary>
[CreateAssetMenu(fileName = "EmpathyLink", menuName = "MoodyShapes/Skills/Empathy Link", order = 4)]
public class EmpathyLinkSkill : EmotionSkillSO
{
    [Header("Empathy Link Settings")]
    [SerializeField] private float linkStrength = 0.3f;
    [SerializeField] private float linkCheckInterval = 0.5f;
    [SerializeField] private bool bidirectionalLink = true;
    
    protected override void ApplyAreaEffect(GameObject user, EmotionSkillController controller)
    {
        // Find all entities in effect radius
        Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, effectRadius, targetLayers);
        List<GameObject> linkTargets = new List<GameObject>();
        
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
            
            // Check if target has emotional state
            if (hitCollider.TryGetComponent<EmotionalState>(out _))
            {
                linkTargets.Add(hitCollider.gameObject);
            }
        }
        
        // Create empathy link component
        EmpathyLinkEffect linkEffect = user.GetComponent<EmpathyLinkEffect>();
        if (linkEffect == null)
        {
            linkEffect = user.AddComponent<EmpathyLinkEffect>();
        }
        
        // Initialize link
        linkEffect.Initialize(linkTargets, effectDuration, linkStrength, linkCheckInterval, bidirectionalLink);
        
        // Create visual effects
        foreach (var target in linkTargets)
        {
            if (skillParticles != null)
            {
                // Create a beam between user and target
                Vector3 midpoint = (user.transform.position + target.transform.position) / 2f;
                Vector3 direction = (target.transform.position - user.transform.position).normalized;
                float distance = Vector3.Distance(user.transform.position, target.transform.position);
                
                ParticleSystem particles = Instantiate(skillParticles, midpoint, 
                    Quaternion.LookRotation(direction));
                
                // Scale particles to match distance
                particles.transform.localScale = new Vector3(1, 1, distance / 2f);
                
                var main = particles.main;
                main.startColor = effectColor;
                
                particles.Play();
                
                // Destroy after duration
                Destroy(particles.gameObject, 3f);
            }
        }
    }
}

/// <summary>
/// Component that manages an active empathy link between shapes
/// </summary>
public class EmpathyLinkEffect : MonoBehaviour
{
    private List<GameObject> linkedTargets = new List<GameObject>();
    private float remainingDuration;
    private float linkStrength;
    private float checkInterval;
    private bool bidirectional;
    private float nextCheckTime;
    
    private EmotionalState sourceState;
    private Dictionary<GameObject, LineRenderer> linkVisuals = new Dictionary<GameObject, LineRenderer>();
    
    /// <summary>
    /// Initialize the empathy link
    /// </summary>
    public void Initialize(List<GameObject> targets, float duration, float strength, float interval, bool isBidirectional)
    {
        linkedTargets = targets;
        remainingDuration = duration;
        linkStrength = strength;
        checkInterval = interval;
        bidirectional = isBidirectional;
        
        sourceState = GetComponent<EmotionalState>();
        
        // Create visual links
        foreach (var target in linkedTargets)
        {
            CreateLinkVisual(target);
        }
    }
    
    private void Update()
    {
        // Update duration
        remainingDuration -= Time.deltaTime;
        
        // Check if link should expire
        if (remainingDuration <= 0)
        {
            // Remove all visuals
            foreach (var visual in linkVisuals.Values)
            {
                Destroy(visual.gameObject);
            }
            
            // Destroy component
            Destroy(this);
            return;
        }
        
        // Update link visuals
        foreach (var target in linkedTargets.ToArray())
        {
            if (target == null)
            {
                linkedTargets.Remove(target);
                continue;
            }
            
            // Update line positions
            if (linkVisuals.TryGetValue(target, out LineRenderer line))
            {
                line.SetPosition(0, transform.position);
                line.SetPosition(1, target.transform.position);
                
                // Pulse effect based on emotion intensity
                float pulseWidth = Mathf.Lerp(0.05f, 0.2f, sourceState.Intensity);
                line.startWidth = pulseWidth;
                line.endWidth = pulseWidth;
            }
        }
        
        // Check if it's time to share emotions
        if (Time.time >= nextCheckTime)
        {
            ShareEmotions();
            nextCheckTime = Time.time + checkInterval;
        }
    }
    
    /// <summary>
    /// Create a visual line between source and target
    /// </summary>
    private void CreateLinkVisual(GameObject target)
    {
        // Create new GameObject for the line
        GameObject lineObj = new GameObject("EmpathyLink_Visual");
        lineObj.transform.SetParent(transform);
        
        // Add line renderer
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        
        // Configure line
        line.positionCount = 2;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        
        // Set positions
        line.SetPosition(0, transform.position);
        line.SetPosition(1, target.transform.position);
        
        // Set color based on current emotion
        Color emotionColor = Color.white;
        if (sourceState.HasEmotion(EmotionType.Happy) || sourceState.HasEmotion(EmotionType.Joyful))
            emotionColor = Color.yellow;
        else if (sourceState.HasEmotion(EmotionType.Sad))
            emotionColor = Color.blue;
        else if (sourceState.HasEmotion(EmotionType.Angry))
            emotionColor = Color.red;
        else if (sourceState.HasEmotion(EmotionType.Scared))
            emotionColor = Color.magenta;
        else if (sourceState.HasEmotion(EmotionType.Calm))
            emotionColor = Color.cyan;
        
        line.startColor = emotionColor;
        line.endColor = emotionColor;
        
        // Store reference
        linkVisuals[target] = line;
    }
    
    /// <summary>
    /// Share emotions between linked shapes
    /// </summary>
    private void ShareEmotions()
    {
        if (sourceState == null) return;
        
        foreach (var target in linkedTargets.ToArray())
        {
            if (target == null)
            {
                linkedTargets.Remove(target);
                continue;
            }
            
            // Get target emotional state
            if (target.TryGetComponent<EmotionalState>(out var targetState))
            {
                // Source to target emotion sharing
                if (sourceState.Intensity > targetState.Intensity * 1.2f)
                {
                    // Create a stimulus to share emotion
                    StimulusEffect effect = new StimulusEffect
                    {
                        EmotionToTrigger = sourceState.CurrentEmotion,
                        IntensityMultiplier = linkStrength,
                        DurationModifier = 0.5f
                    };
                    
                    EmotionalStimulus stimulus = new EmotionalStimulus(
                        StimulusType.EmpathicResponse,
                        linkStrength,
                        effect,
                        gameObject,
                        target
                    );
                    
                    // Apply the stimulus
                    if (target.TryGetComponent<EmotionSystem>(out var emotionSystem))
                    {
                        emotionSystem.ProcessStimulus(stimulus);
                    }
                    else
                    {
                        targetState.ProcessStimulus(stimulus);
                    }
                    
                    // Update link visual color
                    if (linkVisuals.TryGetValue(target, out LineRenderer line))
                    {
                        // Get color based on emotion
                        Color emotionColor = GetEmotionColor(sourceState.CurrentEmotion);
                        line.startColor = emotionColor;
                        line.endColor = emotionColor;
                    }
                }
                
                // Target to source emotion sharing (if bidirectional)
                if (bidirectional && targetState.Intensity > sourceState.Intensity * 1.2f)
                {
                    // Create a stimulus to share emotion back
                    StimulusEffect effect = new StimulusEffect
                    {
                        EmotionToTrigger = targetState.CurrentEmotion,
                        IntensityMultiplier = linkStrength * 0.8f, // Slightly weaker feedback
                        DurationModifier = 0.3f
                    };
                    
                    EmotionalStimulus stimulus = new EmotionalStimulus(
                        StimulusType.EmpathicResponse,
                        linkStrength * 0.8f,
                        effect,
                        target,
                        gameObject
                    );
                    
                    // Apply the stimulus
                    if (TryGetComponent<EmotionSystem>(out var sourceSystem))
                    {
                        sourceSystem.ProcessStimulus(stimulus);
                    }
                    else
                    {
                        sourceState.ProcessStimulus(stimulus);
                    }
                    
                    // Update link visual color
                    if (linkVisuals.TryGetValue(target, out LineRenderer line))
                    {
                        // Get color based on emotion and blend with source color
                        Color targetColor = GetEmotionColor(targetState.CurrentEmotion);
                        Color sourceColor = line.startColor;
                        Color blendedColor = Color.Lerp(sourceColor, targetColor, 0.3f);
                        
                        line.startColor = sourceColor;
                        line.endColor = targetColor;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Get color representing an emotion
    /// </summary>
    private Color GetEmotionColor(EmotionType emotion)
    {
        if ((emotion & EmotionType.Happy) != 0 || (emotion & EmotionType.Joyful) != 0)
            return Color.yellow;
        else if ((emotion & EmotionType.Sad) != 0)
            return Color.blue;
        else if ((emotion & EmotionType.Angry) != 0)
            return Color.red;
        else if ((emotion & EmotionType.Scared) != 0)
            return Color.magenta;
        else if ((emotion & EmotionType.Calm) != 0)
            return Color.cyan;
        else if ((emotion & EmotionType.Curious) != 0)
            return Color.green;
        else if ((emotion & EmotionType.Surprised) != 0)
            return Color.white;
        else
            return Color.gray;
    }
    
    private void OnDestroy()
    {
        // Clean up visuals
        foreach (var visual in linkVisuals.Values)
        {
            if (visual != null)
            {
                Destroy(visual.gameObject);
            }
        }
    }
}
