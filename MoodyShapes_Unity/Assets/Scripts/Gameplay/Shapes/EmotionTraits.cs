using UnityEngine;

[System.Serializable]
public struct EmotionTraits
{
    public EmotionType DominantTrait;
    public EmotionType VulnerableTrait;  // Emotion this entity is susceptible to
    public EmotionType ResistantTrait;  // Emotion this entity resists
    [Range(0, 1)] public float EmotionalStability;  // How quickly emotions decay
    [Range(0, 1)] public float Expressiveness;  // How strongly emotions are shown
}
