using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEmotionProfile", menuName = "MoodyShapes/Emotion Profile", order = 1)]
public class EmotionProfileSO : ScriptableObject
{
    [Header("Core Personality Traits")]
    [Tooltip("The baseline emotion this entity tends towards when unaffected.")]
    public EmotionType dominantTrait = EmotionType.Neutral;
   
    [Range(0f, 1f), Tooltip("Default intensity when in dominant state")]
    public float baselineIntensity = 0.3f;

    [Header("Emotional Dynamics")]
    [Range(0.01f, 1f), Tooltip("Base rate at which emotions decay toward dominant trait")]
    public float emotionalDecayRate = 0.1f;
   
    [Range(0.5f, 2.0f), Tooltip("Multiplier for all emotional responses")]
    public float sensitivity = 1.0f;
   
    [Range(0f, 1f), Tooltip("Chance to resist emotions that conflict with dominant trait")]
    public float emotionalResilience = 0.2f;

    [Header("Emotional Reactions")]
    public List<ReactionRule> reactionRules = new List<ReactionRule>();
   
    [Header("Complex Behaviors")]
    public List<EmotionThresholdBehavior> thresholdBehaviors = new List<EmotionThresholdBehavior>();
    public List<EmotionComboBehavior> comboBehaviors = new List<EmotionComboBehavior>();

    [Header("Visual Feedback")]
    public EmotionVisuals visuals;

    public ReactionRule GetReactionRule(StimulusType stimulusType, EmotionType currentEmotion)
    {
        // First check for specific override rules
        var specificRule = reactionRules.Find(rule =>
            rule.stimulus == stimulusType &&
            rule.requiredCurrentEmotion == EmotionType.None);
       
        // Then check for emotion-dependent rules
        var emotionDependentRule = reactionRules.Find(rule =>
            rule.stimulus == stimulusType &&
            (rule.requiredCurrentEmotion & currentEmotion) != 0);
       
        return emotionDependentRule ?? specificRule;
    }

    public EmotionThresholdBehavior GetThresholdBehavior(EmotionType emotion, float intensity)
    {
        return thresholdBehaviors.Find(b =>
            (b.emotion & emotion) != 0 &&
            intensity >= b.minimumIntensity);
    }

    public EmotionComboBehavior GetComboBehavior(EmotionType emotionA, EmotionType emotionB)
    {
        return comboBehaviors.Find(b =>
            (b.emotion1 & emotionA) != 0 && (b.emotion2 & emotionB) != 0 ||
            (b.emotion1 & emotionB) != 0 && (b.emotion2 & emotionA) != 0);
    }
}

[System.Serializable]
public class ReactionRule
{
    public StimulusType stimulus;
   
    [Tooltip("Emotion that must be active for this rule to apply (None = always applies)")]
    public EmotionType requiredCurrentEmotion = EmotionType.None;
   
    public EmotionType resultingEmotion;
   
    [Range(-1f, 1f), Tooltip("Base change (positive or negative)")]
    public float intensityChangeAmount;
   
    [Range(0.1f, 2f), Tooltip("Maximum intensity this reaction can create")]
    public float maxIntensityForThisReaction = 1.0f;
   
    [Tooltip("Should this completely replace current emotion?")]
    public bool overrideCurrentEmotion = true;
   
    [Tooltip("Delay before reaction occurs (seconds)")]
    public float reactionDelay;
   
    [Tooltip("Duration before decay begins (seconds)")]
    public float sustainedDuration;
}

[System.Serializable]
public class EmotionThresholdBehavior
{
    public EmotionType emotion;
    [Range(0.1f, 1f)] public float minimumIntensity;
    public string animationTrigger;
    public ParticleSystem particles;
    public AudioClip soundEffect;
    public bool isOneTimeOnly = false;
    [HideInInspector] public bool hasTriggered = false;
}

[System.Serializable]
public class EmotionComboBehavior
{
    public EmotionType emotion1;
    public EmotionType emotion2;
    public EmotionType resultingEmotion;
    public string specialAnimation;
    public float comboDuration = 3f;
}

[System.Serializable]
public class EmotionVisuals
{
    public Material baseMaterial;
    public Gradient emotionColorOverrides;
    public float colorTransitionSpeed = 2f;
    public List<EmotionPupilShape> pupilShapes = new List<EmotionPupilShape>();
}

[System.Serializable]
public class EmotionPupilShape
{
    public EmotionType emotion;
    public Sprite shape;
    public Vector2 scale = Vector2.one;
}
