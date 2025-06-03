using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

/// <summary>
/// Utility class for loading skill assets from JSON and creating skill instances
/// </summary>
public static class SkillLoader
{
    /// <summary>
    /// Load a skill from a JSON file
    /// </summary>
    public static EmotionSkillSO LoadSkillFromJson(string jsonPath, EmotionSkillSO existingAsset = null)
    {
        try
        {
            // Load JSON text
            TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
            if (jsonFile == null)
            {
                Debug.LogError($"Failed to load skill JSON file at path: {jsonPath}");
                return null;
            }
            
            // Parse the JSON into a SkillData object
            SkillData data = JsonUtility.FromJson<SkillData>(jsonFile.text);
            
            // Create or update the skill asset
            EmotionSkillSO skill = existingAsset;
            if (skill == null)
            {
                // Determine the skill type from JSON
                Type skillType = typeof(EmotionSkillSO);
                if (jsonPath.Contains("CalmingWave"))
                {
                    skillType = typeof(CalmingWaveSkill);
                }
                else if (jsonPath.Contains("JoyBurst"))
                {
                    skillType = typeof(JoyBurstSkill);
                }
                else if (jsonPath.Contains("EmotionalShield"))
                {
                    skillType = typeof(EmotionalShieldSkill);
                }
                
                // Create the appropriate skill asset
                skill = ScriptableObject.CreateInstance(skillType) as EmotionSkillSO;
            }
            
            // Set common properties
            skill.skillName = data.name;
            skill.description = data.description;
            skill.icon = Resources.Load<Sprite>("SkillIcons/" + data.icon);
            
            // Parse enum from string
            skill.requiredEmotion = ParseEmotionType(data.requiredEmotion);
            
            // Set numeric properties
            skill.minimumIntensity = data.minimumIntensity;
            skill.consumesEmotion = data.consumesEmotion;
            skill.emotionConsumptionAmount = data.emotionConsumptionAmount;
            skill.cooldownTime = data.cooldownTime;
            skill.castTime = data.castTime;
            skill.isPassive = data.isPassive;
            
            // Parse effect type from string
            skill.effectType = ParseEffectType(data.effectType);
            
            // Set remaining properties
            skill.effectRadius = data.effectRadius;
            skill.effectDuration = data.effectDuration;
            skill.effectPower = data.effectPower;
            skill.effectColor = ParseColor(data.effectColor);
            
            // Set layer mask from string
            skill.targetLayers = LayerMask.GetMask(data.targetLayers.Split(','));
            
            skill.requiresLineOfSight = data.requiresLineOfSight;
            skill.emotionTransferable = data.emotionTransferable;
            
            return skill;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading skill from JSON: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Load all skills from JSON files in the Resources/Skills folder
    /// </summary>
    public static List<EmotionSkillSO> LoadAllSkills()
    {
        List<EmotionSkillSO> skills = new List<EmotionSkillSO>();
        
        TextAsset[] skillJsonFiles = Resources.LoadAll<TextAsset>("Skills");
        foreach (TextAsset json in skillJsonFiles)
        {
            EmotionSkillSO skill = LoadSkillFromJson("Skills/" + Path.GetFileNameWithoutExtension(json.name));
            if (skill != null)
            {
                skills.Add(skill);
            }
        }
        
        return skills;
    }
    
    /// <summary>
    /// Parse EmotionType from string
    /// </summary>
    private static EmotionType ParseEmotionType(string emotionString)
    {
        // Handle composite emotions
        if (emotionString.Contains("|"))
        {
            EmotionType result = EmotionType.Neutral;
            string[] parts = emotionString.Split('|');
            foreach (string part in parts)
            {
                if (Enum.TryParse<EmotionType>(part.Trim(), out EmotionType emotion))
                {
                    result |= emotion;
                }
            }
            return result;
        }
        
        // Handle single emotion
        if (Enum.TryParse<EmotionType>(emotionString, out EmotionType singleEmotion))
        {
            return singleEmotion;
        }
        
        return EmotionType.Neutral;
    }
    
    /// <summary>
    /// Parse SkillEffectType from string
    /// </summary>
    private static SkillEffectType ParseEffectType(string typeString)
    {
        if (Enum.TryParse<SkillEffectType>(typeString, out SkillEffectType effectType))
        {
            return effectType;
        }
        
        return SkillEffectType.Self;
    }
    
    /// <summary>
    /// Parse Color from hex string
    /// </summary>
    private static Color ParseColor(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            return color;
        }
        
        return Color.white;
    }
    
    /// <summary>
    /// Data structure for parsing skill JSON
    /// </summary>
    [Serializable]
    private class SkillData
    {
        public string name;
        public string description;
        public string icon;
        public string requiredEmotion;
        public float minimumIntensity;
        public bool consumesEmotion;
        public float emotionConsumptionAmount;
        public float cooldownTime;
        public float castTime;
        public bool isPassive;
        public string effectType;
        public float effectRadius;
        public float effectDuration;
        public float effectPower;
        public string effectColor;
        public string targetLayers;
        public bool requiresLineOfSight;
        public bool emotionTransferable;
    }
}
