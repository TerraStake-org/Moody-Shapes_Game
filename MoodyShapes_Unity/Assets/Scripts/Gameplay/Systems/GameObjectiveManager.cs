using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manages win/lose objectives for the emotional shapes scenario:
/// 1. Guide all shapes into a target emotional state within a time limit.
/// 2. Spread the target emotion to a percentage of shapes.
/// 3. Keep hostile relationships above a minimum tension threshold.
/// </summary>
public class GameObjectiveManager : MonoBehaviour
{
    [Header("Time Objective")]
    [Tooltip("Time limit to complete the objectives, in seconds.")]
    [SerializeField] private float timeLimitSeconds = 120f;

    [Header("Emotion Objective")]
    [Tooltip("The emotion that all shapes should reach to achieve harmony.")]
    [SerializeField] private EmotionType targetEmotion = EmotionType.Joy;
    [Range(0f, 1f)]
    [Tooltip("Fraction of shapes that must reach the target emotion.")]
    [SerializeField] private float emotionThreshold = 0.75f;

    [Header("Relationship Objective")]
    [Tooltip("Minimum allowed relationship score across all pairs (hostility threshold).")]
    [SerializeField] private float hostilityThreshold = -0.5f;

    [Header("References")]
    [Tooltip("Reference to the SocialRelationshipManager in the scene.")]
    [SerializeField] private SocialRelationshipManager relationshipManager;

    [Header("Events")]
    public UnityEvent OnObjectivesSucceeded;
    public UnityEvent OnObjectivesFailed;
    public UnityEvent<float> OnTimeRemainingUpdated;

    private List<EmotionSystem> _shapes;
    private float _elapsedTime;
    private bool _ended;

    private void Awake()
    {
        // Collect all EmotionSystem instances
        RefreshShapesList();
        if (relationshipManager == null)
        {
            relationshipManager = FindObjectOfType<SocialRelationshipManager>();
        }
    }

    private void Update()
    {
        if (_ended) return;

        _elapsedTime += Time.deltaTime;
        float remaining = Mathf.Max(0f, timeLimitSeconds - _elapsedTime);
        OnTimeRemainingUpdated?.Invoke(remaining);

        // Check win/lose conditions
        bool harmony = CheckHarmony();
        bool spread = CheckSpread();
        bool hostilityOK = CheckHostility();

        if (harmony && spread && hostilityOK)
        {
            _ended = true;
            OnObjectivesSucceeded?.Invoke();
        }
        else if (_elapsedTime >= timeLimitSeconds || !hostilityOK)
        {
            _ended = true;
            OnObjectivesFailed?.Invoke();
        }
    }

    /// <summary>
    /// Rebuilds the list of tracked shapes in the scene.
    /// </summary>
    public void RefreshShapesList()
    {
        _shapes = new List<EmotionSystem>(FindObjectsOfType<EmotionSystem>());
    }

    private bool CheckHarmony()
    {
        foreach (var shape in _shapes)
        {
            if (shape.CurrentState.CurrentEmotion != targetEmotion)
                return false;
        }
        return true;
    }

    private bool CheckSpread()
    {
        if (_shapes.Count == 0) return false;
        int count = 0;
        foreach (var shape in _shapes)
        {
            if (shape.CurrentState.CurrentEmotion == targetEmotion)
                count++;
        }
        return (float)count / _shapes.Count >= emotionThreshold;
    }

    private bool CheckHostility()
    {
        if (relationshipManager == null) return true;
        var allRels = relationshipManager.GetAllRelationships();
        if (allRels == null || allRels.Count == 0) return true;

        // Find the lowest relationship score
        float minScore = float.MaxValue;
        foreach (var rel in allRels)
        {
            if (rel.relationshipScore < minScore)
                minScore = rel.relationshipScore;
        }
        // All relationships must be above or equal to threshold
        return minScore >= hostilityThreshold;
    }

    /// <summary>
    /// Ratio of shapes with the target emotion (0 to 1)
    /// </summary>
    public float SpreadRatio
    {
        get
        {
            if (_shapes == null || _shapes.Count == 0) return 0f;
            int count = 0;
            foreach (var shape in _shapes)
            {
                if (shape.CurrentState.CurrentEmotion == targetEmotion)
                    count++;
            }
            return (float)count / _shapes.Count;
        }
    }

    /// <summary>
    /// Minimum relationship score across all pairs
    /// </summary>
    public float MinRelationshipScore
    {
        get
        {
            if (relationshipManager == null) return 1f;
            var allRels = relationshipManager.GetAllRelationships();
            if (allRels == null || allRels.Count == 0) return 1f;
            float minScore = 1f;
            foreach (var rel in allRels)
            {
                if (rel.relationshipScore < minScore)
                    minScore = rel.relationshipScore;
            }
            return minScore;
        }
    }

    /// <summary>
    /// Remaining time before time limit
    /// </summary>
    public float TimeRemaining => Mathf.Max(0f, timeLimitSeconds - _elapsedTime);
}
