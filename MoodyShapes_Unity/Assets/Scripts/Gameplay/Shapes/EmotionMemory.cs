using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Stores a history of emotional events and provides methods to analyze past emotions.
/// This component helps shapes remember their emotional experiences and can influence future reactions.
/// </summary>
public class EmotionMemory : MonoBehaviour
{
    [SerializeField] private int _maxMemorySize = 20;
    
    [Tooltip("How quickly memories fade (0-1)")]
    [SerializeField, Range(0f, 1f)] private float _memoryDecayRate = 0.05f;
    
    [Tooltip("Remembered emotions with potency below this threshold will be forgotten")]
    [SerializeField, Range(0f, 0.3f)] private float _forgettingThreshold = 0.1f;
    
    private List<EmotionMemoryRecord> _emotionHistory = new List<EmotionMemoryRecord>();
    private Dictionary<GameObject, float> _entityImpressions = new Dictionary<GameObject, float>();
    
    [Serializable]
    public class EmotionMemoryRecord
    {
        public EmotionType emotion;
        public float intensity;
        public EmotionChangeSource source;
        public GameObject sourceEntity;
        public float timestamp;
        public float currentPotency; // How strongly this is remembered (decays over time)
        
        public EmotionMemoryRecord(EmotionChangeEvent changeEvent)
        {
            emotion = changeEvent.newEmotion;
            intensity = changeEvent.newIntensity;
            source = changeEvent.source;
            timestamp = Time.time;
            currentPotency = intensity;
            
            // If this came from a stimulus, try to get the source entity
            if (changeEvent.stimulus != null && changeEvent.stimulus.Source != null)
            {
                sourceEntity = changeEvent.stimulus.Source;
            }
        }
    }
    
    private void Update()
    {
        // Decay memories over time
        if (_memoryDecayRate > 0)
        {
            DecayMemories();
        }
    }
    
    /// <summary>
    /// Records an emotion event to memory
    /// </summary>
    public void RecordEmotionEvent(EmotionChangeEvent changeEvent)
    {
        // Don't record tiny changes or non-changes
        if (!changeEvent.hasChanged || changeEvent.newIntensity < 0.1f)
            return;
            
        // Create a new memory record
        var record = new EmotionMemoryRecord(changeEvent);
        
        // Add to history, maintaining max size
        _emotionHistory.Insert(0, record); // Insert at beginning (most recent first)
        
        if (_emotionHistory.Count > _maxMemorySize)
        {
            _emotionHistory.RemoveAt(_emotionHistory.Count - 1);
        }
        
        // Update impressions of entities
        if (record.sourceEntity != null)
        {
            UpdateEntityImpression(record);
        }
    }
    
    /// <summary>
    /// Decays all memories based on the decay rate
    /// </summary>
    private void DecayMemories()
    {
        for (int i = _emotionHistory.Count - 1; i >= 0; i--)
        {
            var memory = _emotionHistory[i];
            
            // More intense memories decay more slowly
            float decayMultiplier = Mathf.Lerp(1.5f, 0.5f, memory.intensity);
            memory.currentPotency -= _memoryDecayRate * Time.deltaTime * decayMultiplier;
            
            // Forget if below threshold
            if (memory.currentPotency <= _forgettingThreshold)
            {
                _emotionHistory.RemoveAt(i);
            }
            else
            {
                _emotionHistory[i] = memory; // Update the value in the list
            }
        }
    }
    
    /// <summary>
    /// Updates impression of an entity based on a new emotional experience
    /// </summary>
    private void UpdateEntityImpression(EmotionMemoryRecord record)
    {
        if (!_entityImpressions.ContainsKey(record.sourceEntity))
        {
            _entityImpressions[record.sourceEntity] = 0;
        }
        
        // Positive emotions improve impression, negative ones decrease it
        float impressionChange = 0;
        
        if ((record.emotion & (EmotionType.Happy | EmotionType.Joyful | EmotionType.Calm)) != 0)
        {
            impressionChange = record.intensity * 0.2f;
        }
        else if ((record.emotion & (EmotionType.Angry | EmotionType.Scared | EmotionType.Sad)) != 0)
        {
            impressionChange = -record.intensity * 0.2f;
        }
        
        _entityImpressions[record.sourceEntity] += impressionChange;
        _entityImpressions[record.sourceEntity] = Mathf.Clamp(_entityImpressions[record.sourceEntity], -1f, 1f);
    }
    
    /// <summary>
    /// Gets the dominant emotion felt toward an entity
    /// </summary>
    public EmotionType GetDominantEmotionToward(GameObject entity)
    {
        Dictionary<EmotionType, float> emotionScores = new Dictionary<EmotionType, float>();
        
        // Collect all emotions associated with this entity
        foreach (var memory in _emotionHistory)
        {
            if (memory.sourceEntity == entity)
            {
                if (!emotionScores.ContainsKey(memory.emotion))
                {
                    emotionScores[memory.emotion] = 0;
                }
                emotionScores[memory.emotion] += memory.currentPotency;
            }
        }
        
        // Find the dominant emotion
        EmotionType dominant = EmotionType.Neutral;
        float highestScore = 0;
        
        foreach (var pair in emotionScores)
        {
            if (pair.Value > highestScore)
            {
                highestScore = pair.Value;
                dominant = pair.Key;
            }
        }
        
        return dominant;
    }
    
    /// <summary>
    /// Gets the overall impression toward an entity (-1 to 1)
    /// </summary>
    public float GetImpressionOf(GameObject entity)
    {
        if (_entityImpressions.ContainsKey(entity))
        {
            return _entityImpressions[entity];
        }
        return 0; // Neutral if no impression yet
    }
    
    /// <summary>
    /// Gets the most recent memories of a specific emotion
    /// </summary>
    public List<EmotionMemoryRecord> GetMemoriesOfEmotion(EmotionType emotion, int maxCount = 5)
    {
        List<EmotionMemoryRecord> result = new List<EmotionMemoryRecord>();
        
        foreach (var memory in _emotionHistory)
        {
            if ((memory.emotion & emotion) != 0)
            {
                result.Add(memory);
                
                if (result.Count >= maxCount)
                    break;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Checks if an entity has caused a specific emotion recently
    /// </summary>
    public bool HasEntityCausedEmotion(GameObject entity, EmotionType emotion, float timeWindow = 60f)
    {
        float currentTime = Time.time;
        
        foreach (var memory in _emotionHistory)
        {
            if (memory.sourceEntity == entity && 
                (memory.emotion & emotion) != 0 && 
                (currentTime - memory.timestamp) <= timeWindow)
            {
                return true;
            }
        }
        
        return false;
    }
}
