using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Integration test for the Hair Physics system, demonstrating how it interacts with
/// the EmotionPhysics component and responds to emotions.
/// </summary>
public class HairPhysicsIntegrationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private GameObject _testCharacterPrefab;
    [SerializeField] private EmotionProfileSO _testEmotionProfile;
    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private Material _hairMaterial;
    [SerializeField] private Texture2D _hairFlowMap;
    
    [Header("UI References")]
    [SerializeField] private Button _happyButton;
    [SerializeField] private Button _angryButton;
    [SerializeField] private Button _sadButton;
    [SerializeField] private Button _surprisedButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Slider _intensitySlider;
    [SerializeField] private Toggle _physicsEnabledToggle;
    [SerializeField] private Toggle _windEnabledToggle;
    [SerializeField] private Slider _stiffnessSlider;
    [SerializeField] private Text _statusText;
    
    // Test character references
    private GameObject _testCharacter;
    private EmotionSystem _emotionSystem;
    private EmotionPhysics _emotionPhysics;
    private HairPhysicsController _hairPhysics;
    
    // Test data
    private float _lastFrameRate;
    private int _frameCount;
    private float _frameRateUpdateTime;
    private Dictionary<EmotionType, Color> _emotionColors;

    void Start()
    {
        InitializeEmotionColors();
        InitializeUI();
        SpawnTestCharacter();
    }
    
    void Update()
    {
        UpdateFrameRate();
        UpdateStatusDisplay();
    }
    
    #region Initialization
    
    void InitializeEmotionColors()
    {
        _emotionColors = new Dictionary<EmotionType, Color>
        {
            { EmotionType.Happy, Color.yellow },
            { EmotionType.Angry, Color.red },
            { EmotionType.Sad, Color.blue },
            { EmotionType.Surprised, Color.white },
            { EmotionType.Calm, new Color(0.8f, 0.9f, 1f) }
        };
    }
    
    void InitializeUI()
    {
        if (_happyButton) _happyButton.onClick.AddListener(() => TriggerEmotion(EmotionType.Happy));
        if (_angryButton) _angryButton.onClick.AddListener(() => TriggerEmotion(EmotionType.Angry));
        if (_sadButton) _sadButton.onClick.AddListener(() => TriggerEmotion(EmotionType.Sad));
        if (_surprisedButton) _surprisedButton.onClick.AddListener(() => TriggerEmotion(EmotionType.Surprised));
        if (_resetButton) _resetButton.onClick.AddListener(ResetCharacter);
        
        if (_physicsEnabledToggle) _physicsEnabledToggle.onValueChanged.AddListener(SetPhysicsEnabled);
        if (_windEnabledToggle) _windEnabledToggle.onValueChanged.AddListener(SetWindEnabled);
        if (_stiffnessSlider) _stiffnessSlider.onValueChanged.AddListener(SetStiffness);
        
        // Set default values
        if (_intensitySlider) _intensitySlider.value = 0.8f;
        if (_physicsEnabledToggle) _physicsEnabledToggle.isOn = true;
        if (_windEnabledToggle) _windEnabledToggle.isOn = true;
        if (_stiffnessSlider) _stiffnessSlider.value = 1f;
    }
    
    void SpawnTestCharacter()
    {
        Vector3 position = _spawnPosition ? _spawnPosition.position : Vector3.zero;
        
        if (_testCharacterPrefab != null)
        {
            _testCharacter = Instantiate(_testCharacterPrefab, position, Quaternion.identity);
            _testCharacter.name = "Hair Physics Test Character";
            
            // Get components
            _emotionSystem = _testCharacter.GetComponent<EmotionSystem>();
            _emotionPhysics = _testCharacter.GetComponent<EmotionPhysics>();
            _hairPhysics = _testCharacter.GetComponent<HairPhysicsController>();
            
            // Set emotion profile if provided
            if (_emotionSystem != null && _testEmotionProfile != null)
            {
                _emotionSystem.SetEmotionProfile(_testEmotionProfile);
            }
            
            // Setup physics
            if (_hairPhysics != null)
            {
                SetupHairMaterial();
            }
            else
            {
                Debug.LogError("HairPhysicsController component not found on test character!");
            }
        }
        else
        {
            Debug.LogError("Test character prefab not assigned!");
        }
    }
    
    void SetupHairMaterial()
    {
        if (_hairMaterial == null || _hairPhysics == null) return;
        
        // Set hair material on all strands
        var strands = GetHairStrands();
        if (strands != null && strands.Length > 0)
        {
            foreach (var strand in strands)
            {
                if (strand.hairMaterial == null)
                {
                    strand.hairMaterial = new Material(_hairMaterial);
                    
                    // Set flow map if provided
                    if (_hairFlowMap != null)
                    {
                        strand.hairMaterial.SetTexture("_FlowMap", _hairFlowMap);
                    }
                }
            }
        }
    }
    
    HairPhysicsController.HairStrand[] GetHairStrands()
    {
        if (_hairPhysics == null) return null;
        
        // Use reflection to get the private _hairStrands field
        var field = typeof(HairPhysicsController).GetField("_hairStrands", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return field.GetValue(_hairPhysics) as HairPhysicsController.HairStrand[];
        }
        
        return null;
    }
    
    #endregion
    
    #region Emotion Control
    
    void TriggerEmotion(EmotionType emotion)
    {
        if (_emotionSystem == null) return;
        
        float intensity = _intensitySlider ? _intensitySlider.value : 0.8f;
        _emotionSystem.ForceEmotion(emotion, intensity, 5f);
        
        if (_statusText)
        {
            _statusText.text = $"Triggered {emotion} (Intensity: {intensity:F2})";
            if (_emotionColors.TryGetValue(emotion, out Color color))
            {
                _statusText.color = color;
            }
        }
        
        Debug.Log($"Triggered {emotion} emotion with intensity {intensity}");
    }
    
    void ResetCharacter()
    {
        if (_emotionSystem == null) return;
        
        _emotionSystem.ForceEmotion(EmotionType.Calm, 0.3f, 3f);
        
        if (_statusText)
        {
            _statusText.text = "Reset to Calm State";
            _statusText.color = _emotionColors[EmotionType.Calm];
        }
    }
    
    #endregion
    
    #region Physics Control
    
    void SetPhysicsEnabled(bool enabled)
    {
        if (_hairPhysics != null)
        {
            _hairPhysics.SetHairPhysicsEnabled(enabled);
            
            if (_statusText)
            {
                _statusText.text = $"Hair Physics {(enabled ? "Enabled" : "Disabled")}";
            }
        }
        
        if (_emotionPhysics != null)
        {
            // Use reflection to set EnableAccessories property
            _emotionPhysics.EnableAccessories = enabled;
        }
    }
    
    void SetWindEnabled(bool enabled)
    {
        if (_emotionPhysics != null)
        {
            // Use reflection to set _enableWind field
            var field = typeof(EmotionPhysics).GetField("_enableWind", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(_emotionPhysics, enabled);
                
                if (_statusText)
                {
                    _statusText.text = $"Wind {(enabled ? "Enabled" : "Disabled")}";
                }
            }
        }
    }
    
    void SetStiffness(float stiffness)
    {
        if (_hairPhysics == null) return;
        
        // Use reflection to set _globalStiffness field
        var field = typeof(HairPhysicsController).GetField("_globalStiffness", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(_hairPhysics, stiffness);
            
            if (_statusText)
            {
                _statusText.text = $"Hair Stiffness: {stiffness:F2}";
            }
        }
    }
    
    #endregion
    
    #region Status Monitoring
    
    void UpdateFrameRate()
    {
        _frameCount++;
        
        if (Time.unscaledTime > _frameRateUpdateTime + 0.5f)
        {
            _lastFrameRate = _frameCount / (Time.unscaledTime - _frameRateUpdateTime);
            _frameRateUpdateTime = Time.unscaledTime;
            _frameCount = 0;
        }
    }
    
    void UpdateStatusDisplay()
    {
        if (_statusText == null || _emotionSystem == null) return;
        
        // Update status text periodically with system state
        if (Time.frameCount % 30 == 0)
        {
            string emotion = _emotionSystem.CurrentState.CurrentEmotion.ToString();
            float intensity = _emotionSystem.CurrentState.Intensity;
            
            string status = $"Current State: {emotion} ({intensity:F2})\n";
            status += $"FPS: {_lastFrameRate:F1}\n";
            
            if (_hairPhysics != null)
            {
                status += $"Hair Physics: {(_hairPhysics.enabled ? "Active" : "Inactive")}";
            }
            
            _statusText.text = status;
            
            // Update color based on emotion
            if (_emotionColors.TryGetValue(_emotionSystem.CurrentState.CurrentEmotion, out Color color))
            {
                _statusText.color = color;
            }
        }
    }
    
    #endregion
}