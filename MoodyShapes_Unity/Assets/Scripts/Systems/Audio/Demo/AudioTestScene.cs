using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// A comprehensive test scene for the Moody Shapes audio system.
/// This demo allows for testing both the adaptive music system and shape audio feedback.
/// </summary>
public class AudioTestScene : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private MoodMusicManager _musicManager;
    [SerializeField] private AudioPlaceholderGenerator _audioGenerator;
    [SerializeField] private Transform _testShapesContainer;
    
    [Header("UI References")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Toggle _usePlaceholderAudioToggle;
    [SerializeField] private Button[] _emotionButtons;
    [SerializeField] private TMP_Dropdown _intensityDropdown;
    [SerializeField] private Button _spawnShapeButton;
    [SerializeField] private Button _randomizeButton;
    [SerializeField] private TextMeshProUGUI _statusText;
    
    [Header("Test Configuration")]
    [SerializeField] private GameObject _shapeTestPrefab;
    [SerializeField] private int _maxTestShapes = 10;
    [SerializeField] private float _spawnRadius = 5f;
    
    private List<EmotionSystem> _spawnedShapes = new List<EmotionSystem>();
    private Dictionary<EmotionType, Color> _emotionColors = new Dictionary<EmotionType, Color>()
    {
        { EmotionType.Happy, new Color(1f, 0.9f, 0f) },
        { EmotionType.Sad, new Color(0.3f, 0.3f, 0.8f) },
        { EmotionType.Angry, new Color(0.9f, 0.2f, 0.2f) },
        { EmotionType.Scared, new Color(0.8f, 0.3f, 0.8f) },
        { EmotionType.Surprised, new Color(0.2f, 0.8f, 0.8f) },
        { EmotionType.Calm, new Color(0.2f, 0.8f, 0.4f) },
        { EmotionType.Frustrated, new Color(0.9f, 0.5f, 0.2f) },
        { EmotionType.Joyful, new Color(1f, 0.7f, 0.3f) },
        { EmotionType.Curious, new Color(0.5f, 0.8f, 0.9f) },
        { EmotionType.Neutral, new Color(0.7f, 0.7f, 0.7f) }
    };
    
    private void Start()
    {
        InitializeUI();
        SetupAudioSystem();
        UpdateStatus("Audio test scene ready. Spawn shapes to begin testing.");
    }
    
    private void InitializeUI()
    {
        // Initialize volume slider
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = 0.8f;
            _masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        // Initialize placeholder audio toggle
        if (_usePlaceholderAudioToggle != null)
        {
            _usePlaceholderAudioToggle.isOn = true;
            _usePlaceholderAudioToggle.onValueChanged.AddListener(OnUsePlaceholderToggled);
        }
        
        // Initialize intensity dropdown
        if (_intensityDropdown != null)
        {
            _intensityDropdown.ClearOptions();
            
            List<string> options = new List<string>
            {
                "Low Intensity (0.3)",
                "Medium Intensity (0.6)",
                "High Intensity (0.9)",
                "Peak Intensity (1.0)"
            };
            
            _intensityDropdown.AddOptions(options);
            _intensityDropdown.value = 2; // High by default
        }
        
        // Initialize emotion buttons
        SetupEmotionButtons();
        
        // Initialize spawn button
        if (_spawnShapeButton != null)
        {
            _spawnShapeButton.onClick.AddListener(OnSpawnButtonClicked);
        }
        
        // Initialize randomize button
        if (_randomizeButton != null)
        {
            _randomizeButton.onClick.AddListener(OnRandomizeButtonClicked);
        }
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
            EmotionType.Calm,
            EmotionType.Frustrated,
            EmotionType.Joyful,
            EmotionType.Curious
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
    
    private void SetupAudioSystem()
    {
        // Find audio system components if not assigned
        if (_musicManager == null)
        {
            _musicManager = FindObjectOfType<MoodMusicManager>();
            if (_musicManager == null)
            {
                Debug.LogWarning("No MoodMusicManager found in scene. Creating one.");
                GameObject audioManagerObj = new GameObject("MoodMusicManager");
                _musicManager = audioManagerObj.AddComponent<MoodMusicManager>();
            }
        }
        
        // Ensure we have audio generator for placeholder audio
        if (_audioGenerator == null)
        {
            _audioGenerator = _musicManager.GetComponent<AudioPlaceholderGenerator>();
            if (_audioGenerator == null && _musicManager != null)
            {
                _audioGenerator = _musicManager.gameObject.AddComponent<AudioPlaceholderGenerator>();
            }
        }
        
        // Initialize placeholder audio
        if (_audioGenerator != null && _usePlaceholderAudioToggle != null && _usePlaceholderAudioToggle.isOn)
        {
            _audioGenerator.GenerateAllEmotionSounds();
            UpdateStatus("Generated placeholder audio for testing.");
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
            Debug.Log(message);
        }
    }
    
    #region UI Event Handlers
    
    private void OnVolumeChanged(float value)
    {
        if (_musicManager != null)
        {
            _musicManager.SetGlobalVolume(value);
            UpdateStatus($"Master volume set to {value:F2}");
        }
    }
    
    private void OnUsePlaceholderToggled(bool isOn)
    {
        if (isOn && _audioGenerator != null)
        {
            _audioGenerator.GenerateAllEmotionSounds();
            UpdateStatus("Using generated placeholder audio.");
        }
        else
        {
            UpdateStatus("Using provided audio assets (if available).");
        }
    }
    
    private void OnEmotionButtonClicked(EmotionType emotion)
    {
        float intensity = GetSelectedIntensity();
        
        if (_spawnedShapes.Count == 0)
        {
            // If no shapes, create one with this emotion
            EmotionSystem shape = SpawnShape();
            if (shape != null)
            {
                shape.ForceEmotion(emotion, intensity, 10f);
                UpdateStatus($"Created shape with {emotion} emotion at {intensity:F1} intensity.");
            }
        }
        else
        {
            // Apply to existing shapes
            int count = Mathf.Min(_spawnedShapes.Count, Random.Range(1, 4));
            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, _spawnedShapes.Count);
                if (_spawnedShapes[index] != null)
                {
                    _spawnedShapes[index].ForceEmotion(emotion, intensity, 10f);
                }
            }
            
            UpdateStatus($"Applied {emotion} emotion at {intensity:F1} intensity to {count} shapes.");
        }
        
        // Test direct sound playback
        if (_audioGenerator != null && _usePlaceholderAudioToggle.isOn)
        {
            _audioGenerator.PlayEmotionSound(emotion);
        }
    }
    
    private void OnSpawnButtonClicked()
    {
        EmotionSystem newShape = SpawnShape();
        if (newShape != null)
        {
            UpdateStatus($"Spawned new shape. Total: {_spawnedShapes.Count}");
        }
        else
        {
            UpdateStatus($"Maximum number of test shapes reached ({_maxTestShapes}).");
        }
    }
    
    private void OnRandomizeButtonClicked()
    {
        if (_spawnedShapes.Count == 0)
        {
            UpdateStatus("No shapes to randomize. Spawn some first.");
            return;
        }
        
        // Randomly assign emotions
        EmotionType[] emotions = new EmotionType[]
        {
            EmotionType.Happy,
            EmotionType.Sad,
            EmotionType.Angry,
            EmotionType.Scared,
            EmotionType.Surprised,
            EmotionType.Calm,
            EmotionType.Frustrated,
            EmotionType.Joyful,
            EmotionType.Curious
        };
        
        foreach (var shape in _spawnedShapes)
        {
            if (shape == null) continue;
            
            EmotionType randomEmotion = emotions[Random.Range(0, emotions.Length)];
            float randomIntensity = Random.Range(0.5f, 1.0f);
            
            shape.ForceEmotion(randomEmotion, randomIntensity, 10f);
        }
        
        UpdateStatus($"Randomized emotions for {_spawnedShapes.Count} shapes.");
    }
    
    #endregion
    
    #region Helper Methods
    
    private float GetSelectedIntensity()
    {
        if (_intensityDropdown == null)
            return 0.8f;
            
        switch (_intensityDropdown.value)
        {
            case 0: return 0.3f; // Low
            case 1: return 0.6f; // Medium
            case 2: return 0.9f; // High
            case 3: return 1.0f; // Peak
            default: return 0.8f;
        }
    }
    
    private EmotionSystem SpawnShape()
    {
        if (_spawnedShapes.Count >= _maxTestShapes || _shapeTestPrefab == null)
            return null;
            
        // Calculate random position within spawn radius
        Vector3 randomOffset = Random.insideUnitSphere * _spawnRadius;
        randomOffset.y = 0; // Keep on same plane
        
        Vector3 spawnPosition = _testShapesContainer != null 
            ? _testShapesContainer.position + randomOffset 
            : transform.position + randomOffset;
            
        // Instantiate shape
        GameObject shapeObj = Instantiate(_shapeTestPrefab, spawnPosition, Quaternion.identity);
        if (_testShapesContainer != null)
        {
            shapeObj.transform.SetParent(_testShapesContainer);
        }
        
        EmotionSystem emotionSystem = shapeObj.GetComponent<EmotionSystem>();
        ShapeAudioFeedback audioFeedback = shapeObj.GetComponent<ShapeAudioFeedback>();
        
        if (emotionSystem == null)
        {
            Debug.LogError("Test shape prefab doesn't have an EmotionSystem component!");
            Destroy(shapeObj);
            return null;
        }
        
        // Add audio feedback if not present
        if (audioFeedback == null)
        {
            audioFeedback = shapeObj.AddComponent<ShapeAudioFeedback>();
        }
        
        // Add to list
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
        
        return emotionSystem;
    }
    
    #endregion
}
