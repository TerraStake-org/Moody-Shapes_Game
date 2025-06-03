using UnityEngine;

public enum StimulusType
{
    // Player-driven
    PlayerPositiveGesture,    // e.g., "Petting" or "Praising" action
    PlayerNegativeGesture,    // e.g., "Hitting" or "Scolding" action
    PlayerGiftPositive,
    PlayerGiftNegative,
    PlayerPresenceNearby,

    // NPC-driven
    NPCPositiveInteraction,
    NPCNegativeInteraction,
    NPCThreateningDisplay,

    // Environmental
    PleasantEnvironment,
    UnpleasantEnvironment,
    SuddenNoise,
    GoalAchieved,
    GoalFrustrated,
    ResourceFound,
    ResourceLost,
    ObservedJoy,
    ObservedDistress,

    // Special
    MemoryRecallPositive,
    MemoryRecallNegative,
    EmpathicResponse,       // Mirroring others' emotions
    AnticipationPositive,
    AnticipationNegative
}

[System.Serializable]
public struct StimulusEffect
{
    public EmotionType EmotionToTrigger;
    [Range(0, 1)] public float IntensityMultiplier;
    public float DurationModifier; // Can extend or reduce effect duration
}

public class EmotionalStimulus
{
    public StimulusType Type { get; }
    public float BasePotency { get; }
    public GameObject Source { get; }
    public GameObject Target { get; }
    public Vector3 WorldPosition { get; }
    public float Timestamp { get; }
   
    // Stimulus-specific effects
    public StimulusEffect PrimaryEffect { get; }
    public StimulusEffect[] SecondaryEffects { get; }

    public EmotionalStimulus(
        StimulusType type,
        float basePotency,
        StimulusEffect primaryEffect,
        GameObject source = null,
        GameObject target = null,
        Vector3? worldPosition = null,
        StimulusEffect[] secondaryEffects = null)
    {
        Type = type;
        BasePotency = Mathf.Clamp01(basePotency);
        Source = source;
        Target = target;
        WorldPosition = worldPosition ?? (source != null ? source.transform.position : Vector3.zero);
        Timestamp = Time.time;
        PrimaryEffect = primaryEffect;
        SecondaryEffects = secondaryEffects ?? new StimulusEffect[0];
    }

    public float GetEffectivePotency(EmotionTraits receiverTraits)
    {
        float potency = BasePotency;
       
        // Apply personality modifiers
        if (Source != null && Source.TryGetComponent<EmotionalState>(out var sourceState))
        {
            // More expressive sources create stronger stimuli
            potency *= Mathf.Lerp(0.5f, 1.5f, sourceState.Personality.Expressiveness);
        }

        // Distance attenuation (if position matters)
        if (Type == StimulusType.SuddenNoise || Type == StimulusType.PlayerPresenceNearby)
        {
            float distance = Vector3.Distance(WorldPosition, Target.transform.position);
            potency *= 1f - Mathf.Clamp01(distance / 10f); // Full strength at 0m, fades by 10m
        }

        return Mathf.Clamp01(potency);
    }

    public bool ShouldAffect(GameObject potentialTarget)
    {
        // Simple implementation - expand with filters as needed
        return Target == null || Target == potentialTarget;
    }
}
