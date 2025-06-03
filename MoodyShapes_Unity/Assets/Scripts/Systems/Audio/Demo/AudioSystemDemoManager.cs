using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Demo manager for showcasing and testing the Adaptive Audio System.
/// Provides UI controls to simulate different emotional states and trigger music changes.
/// </summary>
public class AudioSystemDemoManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MoodMusicManager _audioManager;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private GameObject _shapePrefab;
    
    [Header("UI Elements")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Button[] _emotionButtons;
    [SerializeField] private Slider _intensitySlider;
    [SerializeField] private TextMeshProUGUI _currentMoodText;
    [SerializeField] private TextMeshProUGUI _activeLayersText;
    
    [Header("Demo Configuration")]
    [SerializeField] private int _maxShapes = 10;
    [SerializeField] private float _spawnRadius = 5f;
    [SerializeField] private float _updateInterval = 0.5f;
    
    // Internal state
    private List<EmotionSystem> _spawnedShapes = new List<EmotionSystem>();
    private float _nextUpdateTime;
    private Dictionary<EmotionType, Color> _emotionColors = new Dictionary<EmotionType, Color>()
    {
        { EmotionType.Happy, new Color(1f, 0.9f, 0f) },
        { EmotionType.Sad, new Color(0.3f, 0.3f, 0.8f) },
        { EmotionType.Angry, new Color(0.9f, 0.2f, 0.2f) },
        { EmotionType.Scared, new Color(0.8f, 0.3f, 0.8f) },
        { EmotionType.Surprised, new Color(0.2f, 0.8f, 0.8f) },
        { EmotionType.Calm, new Color(0.2f, 0.8f, 0.4f) },
        { EmotionType.Neutral, new Color(0.7f, 0.7f, 0.7f) }
    };
    
    private void Start()
    {
        InitializeUI();
        SpawnInitialShapes();
        
        if (_audioManager == null)
        {
            _audioManager = FindObjectOfType<MoodMusicManager>();
            if (_audioManager == null)
            {
                Debug.LogError("No MoodMusicManager found. Audio demo will not function properly.");
            }
        }
    }
    
    private void Update()
    {
        if (Time.time >= _nextUpdateTime)
        {
            UpdateUIInfo();
            _nextUpdateTime = Time.time + _updateInterval;
        }
    }
    
    private void InitializeUI()
    {
        // Set up master volume slider
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = 0.8f; // Default value
            _masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        // Set up intensity slider
        if (_intensitySlider != null)
        {
            _intensitySlider.value = 0.8f; // Default value
        }
        
        // Set up emotion buttons
        SetupEmotionButtons();
    }
    
    private void SetupEmotionButtons()
    {
        if (_emotionButtons == null || _emotionButtons.Length == 0)
            return;
            
        EmotionType[] emotions = new EmotionType[]
        {
            EmotionType.Happy,
            EmotionType.Sad, 
            EmotionType.Angry,
            EmotionType.Scared,
            EmotionType.Surprised,
            EmotionType.Calm
        };
        
        for (int i = 0; i < _emotionButtons.Length && i < emotions.Length; i++)
        {
            Button button = _emotionButtons[i];
            EmotionType emotion = emotions[i];
            
            // Set button text
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = emotion.ToString();
            }
            
            // Set button color
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null && _emotionColors.TryGetValue(emotion, out Color color))
            {
                buttonImage.color = color;
            }
            
            // Set button click handler
            int index = i; // Capture for lambda
            button.onClick.AddListener(() => OnEmotionButtonClicked(emotions[index]));
        }
    }
    
    private void SpawnInitialShapes()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnShape();
        }
    }
    
    private void UpdateUIInfo()
    {
        if (_audioManager == null)
            return;
            
        // Update dominant mood text
        if (_currentMoodText != null)
        {
            _currentMoodText.text = "Current Mood: Analyzing...";
            
            // Note: MoodMusicManager doesn't expose current mood directly
            // This would require modification to expose that information
            // For now, we'll analyze our shapes ourselves
            EmotionType dominantMood = AnalyzeDominantMood();
            _currentMoodText.text = $"Current Mood: {dominantMood}";
            
            // Set text color based on emotion
            if (_emotionColors.TryGetValue(dominantMood, out Color color))
            {
                _currentMoodText.color = color;
            }
        }
        
        // Update active layers text
        if (_activeLayersText != null)
        {
            _activeLayersText.text = "Active Layers: \n";
            
            // Note: MoodMusicManager doesn't expose active layers directly
            // This would require modification to expose that information
            _activeLayersText.text += "(Audio analysis in progress)";
        }
    }
    
    private EmotionType AnalyzeDominantMood()
    {
        Dictionary<EmotionType, float> moodScores = new Dictionary<EmotionType, float>();
        
        foreach (var shape in _spawnedShapes)
        {
            if (shape == null) continue;
            
            EmotionType emotion = shape.CurrentState.CurrentEmotion;
            float intensity = shape.CurrentState.Intensity;
            
            if (!moodScores.ContainsKey(emotion))
            {
                moodScores[emotion] = 0;
            }
            
            moodScores[emotion] += intensity;
        }
        
        // Find dominant mood
        EmotionType dominantMood = EmotionType.Neutral;
        float highestScore = 0;
        
        foreach (var mood in moodScores)
        {
            if (mood.Value > highestScore)
            {
                highestScore = mood.Value;
                dominantMood = mood.Key;
            }
        }
        
        return dominantMood;
    }
    
    // UI Button handlers
    
    public void OnSpawnButtonClicked()
    {
        SpawnShape();
    }
    
    public void OnClearButtonClicked()
    {
        foreach (var shape in _spawnedShapes)
        {
            if (shape != null)
            {
                Destroy(shape.gameObject);
            }
        }
        
        _spawnedShapes.Clear();
    }
    
    public void OnRandomizeButtonClicked()
    {
        foreach (var shape in _spawnedShapes)
        {
            if (shape == null) continue;
            
            // Randomly assign emotions
            EmotionType[] emotions = new EmotionType[]
            {
                EmotionType.Happy,
                EmotionType.Sad,
                EmotionType.Angry,
                EmotionType.Scared,
                EmotionType.Surprised,
                EmotionType.Calm
            };
            
            EmotionType randomEmotion = emotions[Random.Range(0, emotions.Length)];
            float randomIntensity = Random.Range(0.6f, 1.0f);
            
            shape.ForceEmotion(randomEmotion, randomIntensity, 10f);
        }
    }
    
    private void OnEmotionButtonClicked(EmotionType emotion)
    {
        float intensity = _intensitySlider != null ? _intensitySlider.value : 0.8f;
        
        // Apply to all selected shapes or a random subset
        int count = Mathf.Min(_spawnedShapes.Count, Random.Range(1, 4));
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, _spawnedShapes.Count);
            if (_spawnedShapes[index] != null)
            {
                _spawnedShapes[index].ForceEmotion(emotion, intensity, 10f);
            }
        }
    }
    
    private void OnVolumeChanged(float value)
    {
        if (_audioManager != null)
        {
            _audioManager.SetGlobalVolume(value);
        }
    }
    
    private EmotionSystem SpawnShape()
    {
        if (_spawnedShapes.Count >= _maxShapes || _shapePrefab == null)
            return null;
            
        // Calculate random position within spawn radius
        Vector3 randomOffset = Random.insideUnitSphere * _spawnRadius;
        randomOffset.y = 0; // Keep on same plane
        
        Vector3 spawnPosition = _spawnPoint != null 
            ? _spawnPoint.position + randomOffset 
            : transform.position + randomOffset;
            
        // Instantiate shape
        GameObject shapeObj = Instantiate(_shapePrefab, spawnPosition, Quaternion.identity);
        EmotionSystem emotionSystem = shapeObj.GetComponent<EmotionSystem>();
        
        if (emotionSystem != null)
        {
            _spawnedShapes.Add(emotionSystem);
            
            // Assign random initial emotion
            EmotionType[] emotions = new EmotionType[]
            {
                EmotionType.Happy,
                EmotionType.Sad,
                EmotionType.Angry,
                EmotionType.Scared,
                EmotionType.Surprised,
                EmotionType.Calm,
                EmotionType.Neutral
            };
            
            EmotionType randomEmotion = emotions[Random.Range(0, emotions.Length)];
            float randomIntensity = Random.Range(0.4f, 0.8f);
            
            emotionSystem.ForceEmotion(randomEmotion, randomIntensity, 5f);
        }
        
        return emotionSystem;
    }
}
