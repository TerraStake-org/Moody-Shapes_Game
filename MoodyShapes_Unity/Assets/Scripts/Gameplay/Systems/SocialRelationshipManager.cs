using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SocialRelationship
{
    public EmotionSystem otherEntity;
    [Range(-1f, 1f)] public float relationshipScore; // -1 to 1 (hostile to friendly)
    public float familiarity; // 0 to 1 (stranger to close friend)
}

public class SocialRelationshipManager : MonoBehaviour
{
    [SerializeField] private float _baseInfluenceMultiplier = 1f;
    [SerializeField] private float _familiarityGrowthRate = 0.05f;
    [SerializeField] private bool _debugMode = false;
   
    private Dictionary<EmotionSystem, SocialRelationship> _relationships =
        new Dictionary<EmotionSystem, SocialRelationship>();
        
    /// <summary>
    /// Sets the base influence multiplier for all relationships.
    /// </summary>
    public void SetBaseInfluenceMultiplier(float value)
    {
        _baseInfluenceMultiplier = Mathf.Max(0.1f, value);
    }
      /// <summary>
    /// Sets the familiarity growth rate for all relationships.
    /// </summary>
    public void SetFamiliarityGrowthRate(float value)
    {
        _familiarityGrowthRate = Mathf.Clamp01(value);
    }
    
    /// <summary>
    /// Gets all relationships for a specific entity.
    /// </summary>
    /// <param name="entity">The entity to get relationships for</param>
    /// <returns>A list of social relationships for the entity</returns>
    public List<SocialRelationship> GetRelationshipsForEntity(EmotionSystem entity)
    {
        if (entity == null)
            return null;
            
        List<SocialRelationship> entityRelationships = new List<SocialRelationship>();
        
        // Look for relationships where this entity is the target
        foreach (var relationship in _relationships)
        {
            if (relationship.Key != entity && relationship.Value.otherEntity == entity)
            {
                entityRelationships.Add(new SocialRelationship
                {
                    otherEntity = relationship.Key,
                    relationshipScore = relationship.Value.relationshipScore,
                    familiarity = relationship.Value.familiarity
                });
            }
        }
        
        // Also include relationships initiated by this entity
        if (_relationships.ContainsKey(entity))
        {
            foreach (var kvp in _relationships)
            {
                if (kvp.Key == entity)
                {
                    entityRelationships.Add(new SocialRelationship
                    {
                        otherEntity = kvp.Value.otherEntity,
                        relationshipScore = kvp.Value.relationshipScore,
                        familiarity = kvp.Value.familiarity
                    });
                }
            }
        }
        
        return entityRelationships;
    }

    public float GetInfluenceModifier(EmotionSystem source, EmotionSystem target)
    {
        // Check if relationship exists
        if (!_relationships.TryGetValue(source, out SocialRelationship relationship))
        {
            // Create new neutral relationship
            relationship = new SocialRelationship {
                otherEntity = source,
                relationshipScore = 0f,
                familiarity = 0.1f
            };
            _relationships[source] = relationship;
        }

        // Calculate modifier (range: 0.5x to 2x based on relationship)
        float modifier = 1f + relationship.relationshipScore;
        return Mathf.Clamp(modifier, 0.5f, 2f) * _baseInfluenceMultiplier;
    }

    public void UpdateRelationship(EmotionSystem source, EmotionType emotionExpressed, float intensity)
    {
        if (!_relationships.ContainsKey(source)) return;

        var relationship = _relationships[source];
       
        // Increase familiarity with each interaction
        relationship.familiarity = Mathf.Clamp01(
            relationship.familiarity + (_familiarityGrowthRate * intensity)
        );

        // Update relationship score based on emotion type
        switch (emotionExpressed)
        {
            case EmotionType.Happy:
            case EmotionType.Joyful:
                relationship.relationshipScore += 0.1f * intensity;
                break;
               
            case EmotionType.Angry:
            case EmotionType.Frustrated:
                relationship.relationshipScore -= 0.15f * intensity;
                break;
               
            case EmotionType.Sad:
                relationship.relationshipScore += 0.05f * intensity;
                break;
        }
       
        relationship.relationshipScore = Mathf.Clamp(relationship.relationshipScore, -1f, 1f);
    }
}
