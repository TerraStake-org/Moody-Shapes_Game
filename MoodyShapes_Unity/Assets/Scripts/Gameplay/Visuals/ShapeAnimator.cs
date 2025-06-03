using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(EmotionSystem))]
public class ShapeAnimator : MonoBehaviour, IShapeVisuals
{
    [System.Serializable]
    public class EmotionAnimationProfile
    {
        public EmotionType emotion;
        public AnimationClip baseClip;
        public AnimationClip[] intensityVariants;
        public AvatarMask blendMask;
        public float transitionDuration = 0.2f;
    }

    [Header("Core Configuration")]
    [SerializeField] private EmotionAnimationProfile[] _emotionProfiles;
    [SerializeField] private Rig _faceRig;
    [SerializeField] private float _blinkInterval = 3f;
    [SerializeField, Range(0, 1)] private float _eyeFollowIntensity = 0.7f;

    [Header("Advanced Settings")]
    [SerializeField] private bool _enableProceduralMotion = true;
    [SerializeField] private float _breathAmplitude = 0.02f;
    [SerializeField] private float _breathFrequency = 0.5f;
    [SerializeField] private float _excitementShake = 0.1f;

    // Cached components
    private Animator _animator;
    private EmotionSystem _emotionSystem;
    private Transform _eyeTarget;

    // State
    private EmotionType _currentEmotion;
    private float _currentIntensity;
    private int _currentVariantIndex;
    private float _blinkTimer;
    private float _idleTimer;
    private Vector3 _basePosition;

    // Animation hashes
    private static readonly int EmotionHash = Animator.StringToHash("Emotion");
    private static readonly int IntensityHash = Animator.StringToHash("Intensity");
    private static readonly int BlinkHash = Animator.StringToHash("Blink");
    private static readonly int VariantHash = Animator.StringToHash("Variant");

    // Public properties for testing integration
    public bool IsAnimating => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f || _isTransitioning;
    public EmotionType CurrentEmotion => _currentEmotion;
    public float CurrentIntensity => _currentIntensity;
    public bool EnableProceduralMotion { get; set; } = true;

    // State tracking
    private bool _isTransitioning = false;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _emotionSystem = GetComponent<EmotionSystem>();
        _basePosition = transform.localPosition;

        InitializeAnimationController();
        SetupFaceRig();
    }

    void OnEnable()
    {
        _emotionSystem.OnEmotionChanged += HandleEmotionChange;
        StartCoroutine(ProceduralMotionRoutine());
    }

    void OnDisable()
    {
        _emotionSystem.OnEmotionChanged -= HandleEmotionChange;
    }

    void Update()
    {
        HandleIdleBehaviors();
        UpdateEyeTracking();
    }

    #region Initialization
    private void InitializeAnimationController()
    {
        // Create override controller at runtime
        var overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = overrideController;

        // Preload all animation clips
        foreach (var profile in _emotionProfiles)
        {
            overrideController[profile.baseClip.name] = profile.baseClip;
            foreach (var variant in profile.intensityVariants)
            {
                overrideController[variant.name] = variant;
            }
        }
    }

    private void SetupFaceRig()
    {
        if (_faceRig != null)
        {
            _eyeTarget = new GameObject("EyeTarget").transform;
            _eyeTarget.SetParent(transform);
            _eyeTarget.localPosition = Vector3.forward * 2f;

            var aimConstraint = _faceRig.GetComponentInChildren<MultiAimConstraint>();
            if (aimConstraint != null)
            {
                var source = new WeightedTransform(_eyeTarget, 1f);
                aimConstraint.data.sourceObjects = new WeightedTransformArray { source };
            }
        }
    }
    #endregion    #region Emotion Handling
    public void UpdateVisuals(EmotionType newEmotion, float newIntensity)
    {
        if (!_animator.isActiveAndEnabled) return;

        EmotionAnimationProfile profile = GetProfileForEmotion(newEmotion);
        if (profile == null) return;

        // Mark as transitioning
        _isTransitioning = true;
        StartCoroutine(TransitionComplete(profile.transitionDuration));

        // Blend to new emotion layer
        _animator.SetInteger(EmotionHash, (int)newEmotion);
        _animator.SetFloat(IntensityHash, newIntensity);

        // Select intensity variant
        int newVariant = Mathf.FloorToInt(newIntensity * (profile.intensityVariants.Length - 1));
        if (newVariant != _currentVariantIndex)
        {
            _currentVariantIndex = newVariant;
            _animator.SetInteger(VariantHash, _currentVariantIndex);
        }

        // Apply blend mask if available
        if (profile.blendMask != null)
        {
            _animator.SetLayerWeight(1, 1f);
            _animator.SetLayerWeight(2, 0f);
        }

        _currentEmotion = newEmotion;
        _currentIntensity = newIntensity;

        // Trigger immediate reactions for dramatic changes
        if (newIntensity > 0.8f)
        {
            PlayEmotionPeakEffect(newEmotion);
        }
    }

    private void HandleEmotionChange(EmotionChangeEvent change)
    {
        UpdateVisuals(change.newEmotion, change.newIntensity);
    }

    private void PlayEmotionPeakEffect(EmotionType emotion)
    {
        switch (emotion)
        {
            case EmotionType.Happy:
                _animator.SetTrigger("HappyPeak");
                break;
            case EmotionType.Angry:
                _animator.SetTrigger("AngryPeak");
                Shake(0.3f, 0.1f);
                break;
            case EmotionType.Surprised:
                _animator.SetTrigger("SurprisePeak");
                break;
        }
    }
    #endregion    #region Procedural Animations
    private IEnumerator ProceduralMotionRoutine()
    {
        while (enabled)
        {
            if (!EnableProceduralMotion)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // Breathing effect
            float breathOffset = Mathf.Sin(Time.time * _breathFrequency) * _breathAmplitude *
                               (1f + _currentIntensity * 0.5f);
           
            // Excitement shake
            Vector3 excitementOffset = Vector3.zero;
            if (_currentIntensity > 0.7f)
            {
                excitementOffset = new Vector3(
                    Random.Range(-_excitementShake, _excitementShake),
                    Random.Range(-_excitementShake, _excitementShake),
                    0) * _currentIntensity;
            }

            transform.localPosition = _basePosition +
                                    Vector3.up * breathOffset +
                                    excitementOffset;

            yield return null;
        }
    }

    private void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * _currentIntensity;
            float y = Random.Range(-1f, 1f) * magnitude * _currentIntensity;

            transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
    #endregion

    #region Idle Behaviors
    private void HandleIdleBehaviors()
    {
        // Random blinking
        _blinkTimer += Time.deltaTime;
        if (_blinkTimer >= _blinkInterval)
        {
            _animator.SetTrigger(BlinkHash);
            _blinkTimer = 0f;
            _blinkInterval = Random.Range(2f, 5f); // Vary blink frequency
        }

        // Idle variations
        if (_currentIntensity < 0.3f)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > 10f)
            {
                _animator.SetTrigger("IdleVariation");
                _idleTimer = 0f;
            }
        }
    }

    private void UpdateEyeTracking()
    {
        if (_eyeTarget != null)
        {
            // Simple look-at-player behavior
            Vector3 lookTarget = Camera.main.transform.position;
            Vector3 targetDirection = (lookTarget - _eyeTarget.position).normalized;
           
            _eyeTarget.position = Vector3.Lerp(
                _eyeTarget.position,
                transform.position + targetDirection * 2f,
                Time.deltaTime * 5f * _eyeFollowIntensity
            );
        }
    }
    #endregion

    #region Helper Methods
    private EmotionAnimationProfile GetProfileForEmotion(EmotionType emotion)
    {
        foreach (var profile in _emotionProfiles)
        {
            if (profile.emotion == emotion)
            {
                return profile;
            }
        }
        return null;
    }
    #endregion

    #region Editor Debugging
    private void OnDrawGizmosSelected()
    {
        if (_eyeTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_eyeTarget.position, 0.05f);
            Gizmos.DrawLine(transform.position, _eyeTarget.position);
        }
    }
    #endregion

    private IEnumerator TransitionComplete(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isTransitioning = false;
    }
}
