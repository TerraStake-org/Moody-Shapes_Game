using UnityEngine;

/// <summary>
/// Helper component for setting up a shape with all the necessary audio components.
/// This is for demonstration purposes to show how to properly configure a shape
/// with audio feedback.
/// </summary>
[ExecuteInEditMode]
public class EmotionalShapeAudioSetup : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    [SerializeField] private bool _setupOnAwake = false;
    [SerializeField] private bool _usePlaceholderAudio = true;
    
    private void Awake()
    {
        if (_setupOnAwake && Application.isPlaying)
        {
            SetupShapeAudio();
        }
    }
    
    /// <summary>
    /// Sets up all the audio components needed for an emotional shape.
    /// </summary>
    public void SetupShapeAudio()
    {
        // Ensure we have an EmotionSystem
        EmotionSystem emotionSystem = GetComponent<EmotionSystem>();
        if (emotionSystem == null)
        {
            emotionSystem = gameObject.AddComponent<EmotionSystem>();
            Debug.LogWarning($"Added EmotionSystem to {gameObject.name}. You should configure it properly.", this);
        }
        
        // Ensure we have an AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 20.0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            
            if (_sfxMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = _sfxMixerGroup;
            }
        }
        
        // Ensure we have ShapeAudioFeedback
        ShapeAudioFeedback audioFeedback = GetComponent<ShapeAudioFeedback>();
        if (audioFeedback == null)
        {
            audioFeedback = gameObject.AddComponent<ShapeAudioFeedback>();
            audioFeedback.usePlaceholderAudioWhenNeeded = _usePlaceholderAudio;
        }
        
        Debug.Log($"Audio setup complete for {gameObject.name}.", this);
    }
    
    /// <summary>
    /// Unity Editor button to setup the audio components
    /// </summary>
    [ContextMenu("Setup Audio Components")]
    private void EditorSetupAudio()
    {
        SetupShapeAudio();
    }
}
