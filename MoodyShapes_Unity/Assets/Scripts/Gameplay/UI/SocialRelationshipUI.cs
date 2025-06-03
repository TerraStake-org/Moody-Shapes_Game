using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays UI visualization for social relationships between shapes.
/// Requires a SocialRelationshipManager to function.
/// </summary>
public class SocialRelationshipUI : MonoBehaviour
{
    [System.Serializable]
    public class RelationshipDisplay
    {
        public EmotionSystem entity;
        public GameObject displayObject;
        public Slider relationshipSlider;
        public Image familiarityFill;
        public TextMeshProUGUI nameText;
    }
    
    [Header("References")]
    [SerializeField] private SocialRelationshipManager _relationshipManager;
    [SerializeField] private GameObject _relationshipDisplayPrefab;
    [SerializeField] private Transform _displayContainer;
    [SerializeField] private EmotionSystem _selectedEntity;
    
    [Header("Settings")]
    [SerializeField] private float _updateInterval = 0.5f;
    [SerializeField] private bool _autoFindManager = true;
    [SerializeField] private Color _positiveColor = Color.green;
    [SerializeField] private Color _neutralColor = Color.yellow;
    [SerializeField] private Color _negativeColor = Color.red;
    
    private List<RelationshipDisplay> _displays = new List<RelationshipDisplay>();
    private float _nextUpdateTime;
    
    private void Start()
    {
        if (_autoFindManager && _relationshipManager == null)
        {
            _relationshipManager = FindObjectOfType<SocialRelationshipManager>();
        }
        
        if (_relationshipManager == null)
        {
            Debug.LogError("SocialRelationshipUI requires a SocialRelationshipManager!");
            enabled = false;
            return;
        }
        
        // Create initial displays if we have a selected entity
        if (_selectedEntity != null)
        {
            RefreshDisplays();
        }
    }
    
    private void Update()
    {
        if (Time.time >= _nextUpdateTime)
        {
            UpdateDisplays();
            _nextUpdateTime = Time.time + _updateInterval;
        }
    }
    
    /// <summary>
    /// Sets the selected entity whose relationships will be displayed.
    /// </summary>
    public void SetSelectedEntity(EmotionSystem entity)
    {
        _selectedEntity = entity;
        RefreshDisplays();
    }
    
    /// <summary>
    /// Rebuilds all relationship displays from scratch.
    /// </summary>
    public void RefreshDisplays()
    {
        // Clear existing displays
        foreach (var display in _displays)
        {
            if (display.displayObject != null)
            {
                Destroy(display.displayObject);
            }
        }
        _displays.Clear();
        
        if (_selectedEntity == null || _relationshipManager == null || _relationshipDisplayPrefab == null)
        {
            return;
        }
        
        // Get relationships for the selected entity
        var relationships = _relationshipManager.GetRelationshipsForEntity(_selectedEntity);
        if (relationships == null || relationships.Count == 0)
        {
            return;
        }
        
        // Create displays for each relationship
        foreach (var relationship in relationships)
        {
            if (relationship.otherEntity == null)
                continue;
                
            CreateRelationshipDisplay(relationship);
        }
    }
    
    /// <summary>
    /// Updates all relationship displays with current values.
    /// </summary>
    private void UpdateDisplays()
    {
        if (_selectedEntity == null || _relationshipManager == null)
        {
            return;
        }
        
        // Get updated relationships
        var relationships = _relationshipManager.GetRelationshipsForEntity(_selectedEntity);
        if (relationships == null)
        {
            return;
        }
        
        // Update existing displays
        foreach (var display in _displays)
        {
            var relationship = relationships.Find(r => r.otherEntity == display.entity);
            if (relationship != null)
            {
                UpdateDisplay(display, relationship);
            }
        }
        
        // Check for new relationships
        foreach (var relationship in relationships)
        {
            if (_displays.Find(d => d.entity == relationship.otherEntity) == null)
            {
                CreateRelationshipDisplay(relationship);
            }
        }
    }
    
    /// <summary>
    /// Creates a UI display for a relationship.
    /// </summary>
    private void CreateRelationshipDisplay(SocialRelationship relationship)
    {
        if (_relationshipDisplayPrefab == null || _displayContainer == null)
            return;
            
        // Instantiate the display prefab
        GameObject displayObj = Instantiate(_relationshipDisplayPrefab, _displayContainer);
        
        // Find UI elements
        Slider slider = displayObj.GetComponentInChildren<Slider>();
        Image familiarityFill = null;
        Transform familiarityObj = displayObj.transform.Find("FamiliarityBar");
        if (familiarityObj != null)
        {
            familiarityFill = familiarityObj.GetComponent<Image>();
        }
        
        TextMeshProUGUI nameText = displayObj.GetComponentInChildren<TextMeshProUGUI>();
        
        // Create display entry
        RelationshipDisplay display = new RelationshipDisplay
        {
            entity = relationship.otherEntity,
            displayObject = displayObj,
            relationshipSlider = slider,
            familiarityFill = familiarityFill,
            nameText = nameText
        };
        
        // Add to list
        _displays.Add(display);
        
        // Update with current values
        UpdateDisplay(display, relationship);
    }
    
    /// <summary>
    /// Updates a relationship display with current values.
    /// </summary>
    private void UpdateDisplay(RelationshipDisplay display, SocialRelationship relationship)
    {
        if (display == null || relationship == null)
            return;
            
        // Update slider value (-1 to 1)
        if (display.relationshipSlider != null)
        {
            display.relationshipSlider.value = (relationship.relationshipScore + 1f) / 2f; // Convert -1..1 to 0..1
            
            // Update slider color based on relationship score
            Image fillImage = display.relationshipSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (relationship.relationshipScore > 0.2f)
                    fillImage.color = Color.Lerp(_neutralColor, _positiveColor, relationship.relationshipScore * 0.8f);
                else if (relationship.relationshipScore < -0.2f)
                    fillImage.color = Color.Lerp(_neutralColor, _negativeColor, -relationship.relationshipScore * 0.8f);
                else
                    fillImage.color = _neutralColor;
            }
        }
        
        // Update familiarity fill (0 to 1)
        if (display.familiarityFill != null)
        {
            display.familiarityFill.fillAmount = relationship.familiarity;
        }
        
        // Update name text
        if (display.nameText != null)
        {
            display.nameText.text = relationship.otherEntity.gameObject.name;
        }
    }
}
