using System;

[Flags]
public enum EmotionType
{
    Neutral = 0,
    Happy = 1 << 0,
    Sad = 1 << 1,
    Angry = 1 << 2,
    Scared = 1 << 3,
    Surprised = 1 << 4,
    Calm = 1 << 5,
    Frustrated = 1 << 6,
    Joyful = 1 << 7,  // More intense than Happy
    Curious = 1 << 8,
    // Composite emotions
    Bittersweet = Happy | Sad,
    Anxious = Scared | Curious
}
