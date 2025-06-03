using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EmotionSystem))]
public class Shape_Animator : MonoBehaviour, IShapeVisuals
{
    [Header("Animation Configuration")]
    [SerializeField] private float _blendSmoothing = 5f;
    [SerializeField] private float _triggerCooldown = 0.3f;
   
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem _emotionParticles;
    [SerializeField] private Light _emotionLight;
    [Range(0.1f, 2f)] [SerializeField] private float _lightIntensityMultiplier = 1f;

    // Cached components
    private Animator _animator;
    private EmotionSystem _emotionSystem;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    // Animator hashes
    private static readonly int EmotionTypeHash = Animator.StringToHash("EmotionType");
    private static readonly int EmotionIntensityHash = Animator.StringToHash("EmotionIntensity");
    private static readonly int EmotionBlendHash = Animator.StringToHash("EmotionBlend");
    private static readonly int SurpriseTriggerHash = Animator.StringToHash("SurpriseTrigger");
   
    // State
    private EmotionType _currentEmotion;
    private float _targetIntensity;
    private float _lastTriggerTime;
    private Coroutine _blendCoroutine;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _emotionSystem = GetComponent<EmotionSystem>();
        _renderer = GetComponentInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();
       
        if (_renderer == null)
        {
            Debug.LogWarning("No Renderer found for emotion visuals", this);
        }
    }

    void OnEnable()
    {
        _emotionSystem.OnEmotionChanged += HandleEmotionChange;
        InitializeVisualState();
    }

    void OnDisable()
    {
        _emotionSystem.OnEmotionChanged -= HandleEmotionChange;
    }

    public void UpdateVisuals(EmotionType newEmotion, float newIntensity)
    {
        // This is now called via event instead of direct interface method
    }

    private void InitializeVisualState()
    {
        _currentEmotion = _emotionSystem.CurrentState.CurrentEmotion;
        _targetIntensity = _emotionSystem.CurrentState.Intensity;
       
        UpdateAnimatorImmediate(_currentEmotion, _targetIntensity);
        UpdateMaterialProperties(_currentEmotion, _targetIntensity);
    }

    private void HandleEmotionChange(EmotionChangeEvent change)
    {
        if (_blendCoroutine != null)
        {
            StopCoroutine(_blendCoroutine);
        }
       
        _blendCoroutine = StartCoroutine(BlendToNewEmotion(change.newEmotion, change.newIntensity));

        // Handle immediate triggers
        if (ShouldPlayTrigger(change))
        {
            PlayEmotionTrigger(change.newEmotion);
        }
    }

    private IEnumerator BlendToNewEmotion(EmotionType targetEmotion, float targetIntensity)
    {
        float startTime = Time.time;
        float startIntensity = _animator.GetFloat(EmotionIntensityHash);
        EmotionType startEmotion = _currentEmotion;

        while (Time.time < startTime + (1f/_blendSmoothing))
        {
            float t = (Time.time - startTime) * _blendSmoothing;
            float currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, t);
           
            // Handle emotion blending if different
            if (startEmotion != targetEmotion)
            {
                _animator.SetFloat(EmotionBlendHash, t);
                UpdateMaterialProperties(targetEmotion, currentIntensity);
            }
            else
            {
                UpdateMaterialProperties(targetEmotion, currentIntensity);
            }

            _animator.SetFloat(EmotionIntensityHash, currentIntensity);
            yield return null;
        }

        // Finalize
        _currentEmotion = targetEmotion;
        _targetIntensity = targetIntensity;
        _animator.SetInteger(EmotionTypeHash, (int)targetEmotion);
        _animator.SetFloat(EmotionIntensityHash, targetIntensity);
        _animator.SetFloat(EmotionBlendHash, 1f);
        UpdateMaterialProperties(targetEmotion, targetIntensity);
       
        _blendCoroutine = null;
    }

    private void UpdateAnimatorImmediate(EmotionType emotion, float intensity)
    {
        _animator.SetInteger(EmotionTypeHash, (int)emotion);
        _animator.SetFloat(EmotionIntensityHash, intensity);
        _animator.SetFloat(EmotionBlendHash, 1f);
    }

    private void UpdateMaterialProperties(EmotionType emotion, float intensity)
    {
        if (_renderer == null) return;

        _renderer.GetPropertyBlock(_propBlock);
       
        // Example properties - customize based on your shader
        _propBlock.SetColor("_BaseColor", GetColorForEmotion(emotion));
        _propBlock.SetFloat("_GlowIntensity", intensity);
       
        // Emotion light effect
        if (_emotionLight != null)
        {
            _emotionLight.color = GetColorForEmotion(emotion);
            _emotionLight.intensity = intensity * _lightIntensityMultiplier;
        }

        _renderer.SetPropertyBlock(_propBlock);
    }

    private bool ShouldPlayTrigger(EmotionChangeEvent change)
    {
        // Only play triggers for significant changes
        return (change.oldEmotion != change.newEmotion ||
               Mathf.Abs(change.oldIntensity - change.newIntensity) > 0.3f) &&
               Time.time > _lastTriggerTime + _triggerCooldown;
    }

    private void PlayEmotionTrigger(EmotionType emotion)
    {
        _lastTriggerTime = Time.time;

        switch (emotion)
        {
            case EmotionType.Surprised:
                _animator.SetTrigger(SurpriseTriggerHash);
                PlayParticles(GetColorForEmotion(emotion), 0.5f);
                break;
               
            case EmotionType.Happy when _targetIntensity > 0.7f:
                _animator.SetTrigger("HappyBigTrigger");
                PlayParticles(GetColorForEmotion(emotion), 1f);
                break;
               
            // Add more specific cases
        }
    }

    private void PlayParticles(Color color, float intensity)
    {
        if (_emotionParticles == null) return;
       
        var main = _emotionParticles.main;
        main.startColor = color;
        _emotionParticles.Play();
    }

    private Color GetColorForEmotion(EmotionType emotion)
    {
        // Map emotions to colors - could be driven by EmotionProfileSO
        return emotion switch
        {
            EmotionType.Happy => Color.yellow,
            EmotionType.Sad => Color.blue,
            EmotionType.Angry => Color.red,
            EmotionType.Surprised => Color.white,
            _ => Color.gray
        };
    }
}
