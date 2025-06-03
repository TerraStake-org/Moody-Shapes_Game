using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides specialized audio feedback for a specific shape's emotional states.
/// Works alongside the MoodMusicManager to handle individual shape sound effects.
/// </summary>
[RequireComponent(typeof(EmotionSystem))]
[RequireComponent(typeof(AudioSource))]
public class ShapeAudioFeedback : MonoBehaviour
{
    [System.Serializable]
    public class EmotionSoundEffect
    {
        public EmotionType emotionType;
        public AudioClip[] audioClips;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.8f, 1.2f)] public float randomPitchRange = 1.05f;
        [Range(0f, 1f)] public float minIntensityThreshold = 0.7f;
        public float cooldownTime = 5f;
    }

    [Header("Audio Configuration")]
    [SerializeField] private List<EmotionSoundEffect> _emotionSounds = new List<EmotionSoundEffect>();
    [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
    [SerializeField] private bool _enableRandomVariation = true;
    [SerializeField] private float _emotionTransitionCooldown = 3f;    [Header("Integration")]
    [SerializeField] private bool _notifyGlobalAudioSystem = true;
    [SerializeField] private float _globalAudioInfluenceRadius = 5f;
    [SerializeField] private bool _usePlaceholderAudioWhenNeeded = true;
    
    private EmotionSystem _emotionSystem;
    private AudioSource _audioSource;
    private Dictionary<EmotionType, float> _cooldownTimers = new Dictionary<EmotionType, float>();
    private EmotionType _lastPlayedEmotion = EmotionType.Neutral;
    private float _lastPlayTime;

    private void Awake()
    {
        _emotionSystem = GetComponent<EmotionSystem>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        _emotionSystem.OnEmotionChanged += HandleEmotionChange;
    }

    private void OnDisable()
    {
        _emotionSystem.OnEmotionChanged -= HandleEmotionChange;
    }

    private void Update()
    {
        // Update cooldown timers
        List<EmotionType> expiredCooldowns = new List<EmotionType>();
        
        foreach (var timer in _cooldownTimers)
        {
            _cooldownTimers[timer.Key] -= Time.deltaTime;
            if (_cooldownTimers[timer.Key] <= 0)
            {
                expiredCooldowns.Add(timer.Key);
            }
        }
        
        foreach (var emotion in expiredCooldowns)
        {
            _cooldownTimers.Remove(emotion);
        }
    }

    private void HandleEmotionChange(EmotionChangeEvent change)
    {
        // Skip if same emotion or too soon after last transition
        if (change.newEmotion == _lastPlayedEmotion && Time.time - _lastPlayTime < _emotionTransitionCooldown)
            return;
            
        // Check if intensity is significant
        if (change.newIntensity < 0.4f || change.newIntensity <= change.oldIntensity)
            return;
            
        // Check if this emotion is on cooldown
        if (_cooldownTimers.ContainsKey(change.newEmotion))
            return;
            
        // Try to play sound for this emotion
        PlayEmotionSound(change.newEmotion, change.newIntensity);
    }

    private void PlayEmotionSound(EmotionType emotion, float intensity)
    {        // Find matching sound effect
        EmotionSoundEffect soundEffect = null;
        
        foreach (var effect in _emotionSounds)
        {
            if ((effect.emotionType & emotion) != 0 && intensity >= effect.minIntensityThreshold)
            {
                soundEffect = effect;
                break;
            }
        }
        
        AudioClip clipToPlay = null;
        
        if (soundEffect == null || soundEffect.audioClips.Length == 0)
        {
            // If no sound effect is configured, try to use placeholder audio
            if (_usePlaceholderAudioWhenNeeded)
            {
                // Try to find an AudioPlaceholderGenerator in the scene
                AudioPlaceholderGenerator generator = FindObjectOfType<AudioPlaceholderGenerator>();
                if (generator != null)
                {
                    clipToPlay = generator.GetEmotionClip(emotion);
                    if (clipToPlay == null)
                    {
                        clipToPlay = generator.GenerateEmotionSound(emotion, 
                            GetBaseFrequencyForEmotion(emotion), 
                            GetSecondaryFrequencyForEmotion(emotion));
                    }
                }
            }
            
            if (clipToPlay == null)
                return;
        }
        else
        {
            // Select random clip if multiple are available
            clipToPlay = soundEffect.audioClips[Random.Range(0, soundEffect.audioClips.Length)];
            if (clipToPlay == null)
                return;
        }
        
        // Apply random pitch variation if enabled
        if (_enableRandomVariation)
        {
            float pitchRange = soundEffect != null ? soundEffect.randomPitchRange : 1.05f;
            _audioSource.pitch = Random.Range(2 - pitchRange, pitchRange);
        }
        else
        {
            _audioSource.pitch = 1f;
        }
        
        // Set volume based on intensity and configuration
        float effectVolume = soundEffect != null ? soundEffect.volume : 0.7f;
        float calculatedVolume = effectVolume * _masterVolume * Mathf.Clamp01(intensity);
        _audioSource.volume = calculatedVolume;
        
        // Play the sound
        _audioSource.PlayOneShot(clipToPlay);
        
        // Update state
        _lastPlayedEmotion = emotion;
        _lastPlayTime = Time.time;
        
        float cooldownTime = soundEffect != null ? soundEffect.cooldownTime : 5f;
        _cooldownTimers[emotion] = cooldownTime;
        
        // Notify global audio system if enabled
        if (_notifyGlobalAudioSystem)
        {
            NotifyGlobalAudioSystem(emotion, intensity);
        }
    }
    
    private void NotifyGlobalAudioSystem(EmotionType emotion, float intensity)
    {
        // Create a temporary invisible GameObject that will briefly influence
        // the global audio mood in the vicinity of this shape
        if (intensity >= 0.7f)
        {
            StartCoroutine(CreateTemporaryEmotionalInfluence(emotion, intensity));
        }
    }
    
    private IEnumerator CreateTemporaryEmotionalInfluence(EmotionType emotion, float intensity)
    {
        // Create temporary game object with emotion system
        GameObject tempInfluencer = new GameObject("TempAudioInfluence");
        tempInfluencer.transform.position = transform.position;
        
        // Add emotion system that will be picked up by the MoodMusicManager
        EmotionSystem tempSystem = tempInfluencer.AddComponent<EmotionSystem>();
        
        // Set its emotion (implementation may vary based on your EmotionSystem setup)
        // This assumes ForceEmotion is a public method that sets an emotion for a duration
        tempSystem.ForceEmotion(emotion, intensity * 1.2f, 3f); 
        
        // Make influence radius-based
        SphereCollider influenceCollider = tempInfluencer.AddComponent<SphereCollider>();
        influenceCollider.radius = _globalAudioInfluenceRadius;
        influenceCollider.isTrigger = true;
        
        // Wait for a few seconds for the MoodMusicManager to pick up this emotion
        yield return new WaitForSeconds(3.5f);
        
        // Clean up
        Destroy(tempInfluencer);
    }
    
    /// <summary>
    /// Plays a specific emotion sound immediately, regardless of current emotional state.
    /// Useful for scripted events or reactions.
    /// </summary>
    public void TriggerEmotionSound(EmotionType emotion, float intensity = 1f)
    {
        PlayEmotionSound(emotion, intensity);
    }
    
    /// <summary>
    /// Sets the master volume for all emotion sounds from this shape.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Gets a base frequency for procedurally generating a sound for an emotion.
    /// Used when no audio clips are available.
    /// </summary>
    private float GetBaseFrequencyForEmotion(EmotionType emotion)
    {
        if ((emotion & EmotionType.Happy) != 0 || (emotion & EmotionType.Joyful) != 0)
            return 440.0f; // A4
        if ((emotion & EmotionType.Sad) != 0)
            return 196.0f; // G3
        if ((emotion & EmotionType.Angry) != 0 || (emotion & EmotionType.Frustrated) != 0)
            return 311.1f; // D#4
        if ((emotion & EmotionType.Scared) != 0)
            return 293.7f; // D4
        if ((emotion & EmotionType.Surprised) != 0)
            return 587.3f; // D5
        if ((emotion & EmotionType.Calm) != 0)
            return 261.6f; // C4
        if ((emotion & EmotionType.Curious) != 0)
            return 392.0f; // G4
            
        return 329.6f; // E4 (default)
    }
    
    /// <summary>
    /// Gets a secondary frequency for procedurally generating a sound for an emotion.
    /// Used when no audio clips are available.
    /// </summary>
    private float GetSecondaryFrequencyForEmotion(EmotionType emotion)
    {
        if ((emotion & EmotionType.Happy) != 0 || (emotion & EmotionType.Joyful) != 0)
            return 554.4f; // C#5
        if ((emotion & EmotionType.Sad) != 0)
            return 220.0f; // A3
        if ((emotion & EmotionType.Angry) != 0 || (emotion & EmotionType.Frustrated) != 0)
            return 329.6f; // E4
        if ((emotion & EmotionType.Scared) != 0)
            return 370.0f; // F#4
        if ((emotion & EmotionType.Surprised) != 0)
            return 659.3f; // E5
        if ((emotion & EmotionType.Calm) != 0)
            return 329.6f; // E4
        if ((emotion & EmotionType.Curious) != 0)
            return 440.0f; // A4
            
        return 392.0f; // G4 (default)
    }
}
