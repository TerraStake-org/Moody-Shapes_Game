using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class MoodMusicManager : MonoBehaviour
{
    [System.Serializable]
    public class MoodMusicLayer
    {
        public EmotionType associatedMood;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float fadeTime = 1f;
        public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }

    [Header("Audio Configuration")]
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private List<MoodMusicLayer> _moodLayers = new List<MoodMusicLayer>();
    [SerializeField] private AudioClip _defaultMusic;
    [SerializeField, Range(0.1f, 5f)] private float _moodCheckInterval = 2f;
    [SerializeField, Range(0f, 1f)] private float _globalMusicVolume = 0.8f;

    [Header("Mood Analysis")]
    [SerializeField, Range(0f, 1f)] private float _dominanceThreshold = 0.4f;
    [SerializeField] private bool _weightByDistance = true;
    [SerializeField] private float _listenerRange = 20f;

    private AudioSource _primarySource;
    private Dictionary<EmotionType, AudioSource> _layerSources = new Dictionary<EmotionType, AudioSource>();
    private Dictionary<EmotionType, float> _targetVolumes = new Dictionary<EmotionType, float>();
    private List<EmotionSystem> _emotionalEntities = new List<EmotionSystem>();
    private Transform _listenerTransform;
    private float _nextMoodCheckTime;

    void Awake()
    {
        _primarySource = GetComponent<AudioSource>();
        _primarySource.outputAudioMixerGroup = _musicMixerGroup;
        _primarySource.loop = true;

        // Create audio sources for each mood layer
        foreach (var layer in _moodLayers)
        {
            if (!_layerSources.ContainsKey(layer.associatedMood))
            {
                var newSource = gameObject.AddComponent<AudioSource>();
                newSource.outputAudioMixerGroup = _musicMixerGroup;
                newSource.clip = layer.clip;
                newSource.loop = true;
                newSource.volume = 0f;
                newSource.Play();
                _layerSources[layer.associatedMood] = newSource;
                _targetVolumes[layer.associatedMood] = 0f;
            }
        }

        _listenerTransform = Camera.main?.transform;
    }

    void OnEnable()
    {
        EmotionSystem.OnAnyEmotionChanged += HandleEmotionChange;
        RefreshEntityList();
    }

    void OnDisable()
    {
        EmotionSystem.OnAnyEmotionChanged -= HandleEmotionChange;
    }

    void Start()
    {
        PlayDefaultMusic();
        _nextMoodCheckTime = Time.time + _moodCheckInterval;
    }    void Update()
    {
        UpdateLayerVolumes();
       
        if (Time.time >= _nextMoodCheckTime)
        {
            AnalyzeMood();
            _nextMoodCheckTime = Time.time + _moodCheckInterval;
            
            if (_enableDebugLogging)
            {
                LogAudioState();
            }
        }
    }

    private void RefreshEntityList()
    {
        _emotionalEntities = FindObjectsOfType<EmotionSystem>()
            .Where(es => es != null && es.enabled)
            .ToList();
    }

    private void HandleEmotionChange(EmotionChangeEvent change)
    {
        // Immediate reactions for dramatic changes
        if (change.newIntensity > 0.8f && change.oldIntensity < 0.5f)
        {
            PlayStinger(change.newEmotion);
        }
    }

    private void AnalyzeMood()
    {
        if (_emotionalEntities.Count == 0)
        {
            SetDominantMood(EmotionType.Neutral);
            return;
        }

        var moodScores = new Dictionary<EmotionType, float>();
        int totalWeight = 0;

        foreach (var entity in _emotionalEntities)
        {
            if (entity == null) continue;

            float weight = 1f;
            if (_weightByDistance && _listenerTransform != null)
            {
                float distance = Vector3.Distance(_listenerTransform.position, entity.transform.position);
                weight = Mathf.Clamp01(1 - (distance / _listenerRange));
            }

            var state = entity.CurrentState;
            if (!moodScores.ContainsKey(state.CurrentEmotion))
            {
                moodScores[state.CurrentEmotion] = 0f;
            }

            moodScores[state.CurrentEmotion] += state.Intensity * weight;
            totalWeight++;
        }

        if (totalWeight == 0)
        {
            SetDominantMood(EmotionType.Neutral);
            return;
        }

        // Normalize and find dominant mood
        EmotionType dominantMood = EmotionType.Neutral;
        float maxScore = 0f;

        foreach (var mood in moodScores)
        {
            float normalizedScore = mood.Value / totalWeight;
            if (normalizedScore > maxScore && normalizedScore >= _dominanceThreshold)
            {
                maxScore = normalizedScore;
                dominantMood = mood.Key;
            }
        }

        SetDominantMood(dominantMood);
        AdjustLayerVolumes(moodScores, totalWeight);
    }

    private void SetDominantMood(EmotionType mood)
    {
        bool shouldPlayDefault = mood == EmotionType.Neutral || !_layerSources.ContainsKey(mood);
       
        if (shouldPlayDefault && _primarySource.clip != _defaultMusic)
        {
            _primarySource.clip = _defaultMusic;
            _primarySource.volume = _globalMusicVolume;
            _primarySource.Play();
        }
        else if (!shouldPlayDefault && _primarySource.isPlaying)
        {
            _primarySource.volume = 0f; // Fade out primary when mood music plays
        }
    }

    private void AdjustLayerVolumes(Dictionary<EmotionType, float> moodScores, int totalWeight)
    {
        foreach (var layer in _moodLayers)
        {
            float targetVolume = 0f;
           
            if (moodScores.TryGetValue(layer.associatedMood, out float score))
            {
                float normalizedScore = score / totalWeight;
                targetVolume = Mathf.Clamp01(normalizedScore / _dominanceThreshold) * layer.volume;
            }

            _targetVolumes[layer.associatedMood] = targetVolume * _globalMusicVolume;
        }
    }

    private void UpdateLayerVolumes()
    {
        foreach (var layer in _moodLayers)
        {
            if (_layerSources.TryGetValue(layer.associatedMood, out AudioSource source))
            {
                float currentVol = source.volume;
                float targetVol = _targetVolumes[layer.associatedMood];
               
                if (!Mathf.Approximately(currentVol, targetVol))
                {
                    float fadeSpeed = 1f / layer.fadeTime;
                    float newVol = Mathf.MoveTowards(currentVol, targetVol,
                        fadeSpeed * Time.deltaTime * layer.fadeCurve.Evaluate(Mathf.Abs(targetVol - currentVol)));
                    source.volume = newVol;
                }
            }
        }
    }    [System.Serializable]
    public class EmotionStinger
    {
        public EmotionType emotion;
        public AudioClip[] stingerClips;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitchRange = 1f;
    }
    
    [Header("Stingers")]
    [SerializeField] private List<EmotionStinger> _emotionStingers = new List<EmotionStinger>();
    [SerializeField] private AudioSource _stingerSource;
    
    private void PlayStinger(EmotionType emotion)
    {
        // If we don't have a dedicated stinger source, create one
        if (_stingerSource == null)
        {
            _stingerSource = gameObject.AddComponent<AudioSource>();
            _stingerSource.outputAudioMixerGroup = _musicMixerGroup;
            _stingerSource.playOnAwake = false;
        }
        
        // Find matching stinger
        EmotionStinger stinger = _emotionStingers.Find(s => (s.emotion & emotion) != 0);
        if (stinger == null || stinger.stingerClips == null || stinger.stingerClips.Length == 0)
        {
            // If no stinger found, try to use the placeholder generator
            AudioPlaceholderGenerator generator = GetComponent<AudioPlaceholderGenerator>();
            if (generator != null)
            {
                AudioClip generatedClip = generator.GetEmotionClip(emotion);
                if (generatedClip != null)
                {
                    _stingerSource.pitch = Random.Range(0.9f, 1.1f);
                    _stingerSource.volume = _globalMusicVolume * 0.7f;
                    _stingerSource.PlayOneShot(generatedClip);
                }
            }
            return;
        }
        
        // Pick a random stinger clip
        AudioClip clip = stinger.stingerClips[Random.Range(0, stinger.stingerClips.Length)];
        if (clip == null) return;
        
        // Set properties and play
        _stingerSource.pitch = Random.Range(2 - stinger.pitchRange, stinger.pitchRange);
        _stingerSource.volume = stinger.volume * _globalMusicVolume;
        _stingerSource.PlayOneShot(clip);
    }

    private void PlayDefaultMusic()
    {
        if (_defaultMusic != null)
        {
            _primarySource.clip = _defaultMusic;
            _primarySource.volume = _globalMusicVolume;
            _primarySource.Play();
        }
    }

    public void SetGlobalVolume(float volume)
    {
        _globalMusicVolume = Mathf.Clamp01(volume);
        _primarySource.volume = _primarySource.clip == _defaultMusic ? _globalMusicVolume : 0f;
    }

    #region Debugging and Testing
    
    [Header("Debugging")]
    [SerializeField] private bool _enableDebugLogging = false;
    
    /// <summary>
    /// Enables or disables detailed debug logging for the audio system.
    /// </summary>
    public bool EnableDebugLogging
    {
        get { return _enableDebugLogging; }
        set { _enableDebugLogging = value; }
    }
    
    /// <summary>
    /// Returns the current dominant mood based on the active music layers.
    /// </summary>
    public EmotionType GetCurrentDominantMood()
    {
        // Find highest volume layer
        EmotionType dominantMood = EmotionType.Neutral;
        float highestVolume = 0.1f; // Threshold to consider a layer active
        
        foreach (var layer in _moodLayers)
        {
            if (_layerSources.TryGetValue(layer.associatedMood, out AudioSource source))
            {
                if (source.volume > highestVolume)
                {
                    highestVolume = source.volume;
                    dominantMood = layer.associatedMood;
                }
            }
        }
        
        return dominantMood;
    }
    
    /// <summary>
    /// Checks if a specific emotion layer is currently active (playing above threshold).
    /// </summary>
    public bool IsLayerActive(EmotionType emotion)
    {
        if (_layerSources.TryGetValue(emotion, out AudioSource source))
        {
            return source.volume > 0.1f; // Consider active if above 10% volume
        }
        
        return false;
    }
    
    /// <summary>
    /// Returns a dictionary with the current volume of each layer.
    /// Useful for visualization and debugging.
    /// </summary>
    public Dictionary<EmotionType, float> GetCurrentLayerVolumes()
    {
        var result = new Dictionary<EmotionType, float>();
        
        foreach (var layer in _moodLayers)
        {
            if (_layerSources.TryGetValue(layer.associatedMood, out AudioSource source))
            {
                result[layer.associatedMood] = source.volume;
            }
            else
            {
                result[layer.associatedMood] = 0f;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Logs the current audio state to the console.
    /// </summary>
    public void LogAudioState()
    {
        if (!_enableDebugLogging) return;
        
        EmotionType dominantMood = GetCurrentDominantMood();
        string activeLayersText = "Active Layers: ";
        
        foreach (var kvp in GetCurrentLayerVolumes())
        {
            if (kvp.Value > 0.05f)
            {
                activeLayersText += $"{kvp.Key}({kvp.Value:F2}) ";
            }
        }
        
        Debug.Log($"[MoodMusicManager] Dominant Mood: {dominantMood}\n{activeLayersText}");
    }
    
    #endregion
}
