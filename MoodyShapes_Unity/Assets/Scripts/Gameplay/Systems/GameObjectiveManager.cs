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

    [Header("Replayability Settings")]
    [Tooltip("Enable hidden endings based on player choices.")]
    [SerializeField] private bool enableHiddenEndings = true;

    [Tooltip("Enable evolving rulesets every few levels.")]
    [SerializeField] private bool enableEvolvingRulesets = true;

    private List<EmotionSystem> _shapes;
    private float _elapsedTime;
    private bool _ended;
    private Dictionary<EmotionType, int> _emotionUsage;
    private List<string> _shapeLetters;
    private int _currentLevel;
    private bool _hiddenEndingTriggered;

    private void Awake()
    {
        // Collect all EmotionSystem instances
        RefreshShapesList();
        if (relationshipManager == null)
        {
            relationshipManager = FindObjectOfType<SocialRelationshipManager>();
        }

        _emotionUsage = new Dictionary<EmotionType, int>();
        foreach (EmotionType emotion in System.Enum.GetValues(typeof(EmotionType)))
        {
            _emotionUsage[emotion] = 0;
        }

        _shapeLetters = new List<string>();
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

        CheckHiddenEnding();
        ApplyEvolvingRuleset();
        ApplyEnvironmentalChanges();
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

    private void TrackEmotionUsage(EmotionType emotion)
    {
        if (_emotionUsage.ContainsKey(emotion))
        {
            _emotionUsage[emotion]++;
        }
    }

    private EmotionType GetDominantEmotion()
    {
        EmotionType dominantEmotion = EmotionType.Neutral;
        int maxUsage = 0;

        foreach (var entry in _emotionUsage)
        {
            if (entry.Value > maxUsage)
            {
                maxUsage = entry.Value;
                dominantEmotion = entry.Key;
            }
        }

        return dominantEmotion;
    }

    private void ApplyEnvironmentalChanges()
    {
        EmotionType dominantEmotion = GetDominantEmotion();

        switch (dominantEmotion)
        {
            case EmotionType.Anger:
                // Apply darker level visuals
                Debug.Log("Environment adapting to Anger: Darker visuals.");
                break;
            case EmotionType.Joy:
                // Apply brighter level visuals
                Debug.Log("Environment adapting to Joy: Brighter visuals.");
                break;
            // Add cases for other emotions as needed
        }
    }

    private void GenerateShapeLetters()
    {
        _shapeLetters.Clear();
        foreach (var shape in _shapes)
        {
            string letter = $"Dear Player,\n\nI am feeling {shape.CurrentState.CurrentEmotion}.\n";

            if (shape.CurrentState.CurrentEmotion == EmotionType.Anger)
            {
                letter += "Why did you make me angry? Please help me calm down.\n";
            }
            else if (shape.CurrentState.CurrentEmotion == EmotionType.Joy)
            {
                letter += "Thank you for making me happy! I feel so light and free.\n";
            }
            else if (shape.CurrentState.CurrentEmotion == EmotionType.Sadness)
            {
                letter += "I am feeling down. Can you bring some joy into my life?\n";
            }

            letter += "Sincerely,\nYour Shape\n";
            _shapeLetters.Add(letter);
        }
    }

    public List<string> GetShapeLetters()
    {
        GenerateShapeLetters();
        return _shapeLetters;
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

    private void CheckHiddenEnding()
    {
        if (!enableHiddenEndings || _hiddenEndingTriggered) return;

        if (SpreadRatio >= 0.9f && MinRelationshipScore >= 0.8f)
        {
            _hiddenEndingTriggered = true;
            Debug.Log("Hidden Ending Unlocked: Perfect Harmony!");
            OnObjectivesSucceeded?.Invoke();
        }
    }

    private void ApplyEvolvingRuleset()
    {
        if (!enableEvolvingRulesets) return;

        if (_currentLevel % 5 == 0)
        {
            Debug.Log("Evolving Ruleset Applied: New emotional laws activated.");
            emotionThreshold += 0.05f; // Example: Increase difficulty
            hostilityThreshold += 0.1f; // Example: Adjust hostility tolerance
        }
    }
}
