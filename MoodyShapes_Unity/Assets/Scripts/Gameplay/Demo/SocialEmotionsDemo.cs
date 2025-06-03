using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Demonstrates the social emotion systems in action.
/// Creates shapes with different emotions and shows how they interact.
/// </summary>
public class SocialEmotionsDemo : MonoBehaviour
{
    [SerializeField] private EmotionSystemsManager _systemsManager;
    [SerializeField] private SocialRelationshipUI _relationshipUI;
    
    [Header("Demo Setup")]
    [SerializeField] private int _numberOfShapes = 5;
    [SerializeField] private float _spawnRadius = 5f;
    [SerializeField] private EmotionProfileSO[] _emotionProfiles;
    [SerializeField] private float _delayBetweenEmotions = 5f;
    [SerializeField] private float _movementSpeed = 1f;
    
    private List<EmotionSystem> _spawnedShapes = new List<EmotionSystem>();
    private EmotionSystem _selectedShape;
    
    void Start()
    {
        if (_systemsManager == null)
        {
            _systemsManager = FindObjectOfType<EmotionSystemsManager>();
        }
        
        if (_systemsManager == null)
        {
            Debug.LogError("SocialEmotionsDemo requires an EmotionSystemsManager!");
            enabled = false;
            return;
        }
        
        StartCoroutine(SetupDemo());
    }
    
    IEnumerator SetupDemo()
    {
        // Wait for systems to initialize
        yield return new WaitForSeconds(1f);
        
        // Create shapes
        for (int i = 0; i < _numberOfShapes; i++)
        {
            // Calculate position in circle
            float angle = i * (360f / _numberOfShapes) * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * _spawnRadius,
                0.5f,
                Mathf.Sin(angle) * _spawnRadius
            );
            
            // Create shape
            GameObject shapeObj = _systemsManager.CreateEmotionalShape(position);
            shapeObj.name = $"Shape_{i+1}";
            
            // Assign random profile if available
            if (_emotionProfiles != null && _emotionProfiles.Length > 0)
            {
                EmotionProfileSO profile = _emotionProfiles[Random.Range(0, _emotionProfiles.Length)];
                EmotionSystem emotionSystem = shapeObj.GetComponent<EmotionSystem>();
                if (emotionSystem != null && profile != null)
                {
                    emotionSystem.SetEmotionProfile(profile);
                    _spawnedShapes.Add(emotionSystem);
                }
            }
        }
        
        // Select the first shape
        if (_spawnedShapes.Count > 0)
        {
            _selectedShape = _spawnedShapes[0];
            if (_relationshipUI != null)
            {
                _relationshipUI.SetSelectedEntity(_selectedShape);
            }
        }
        
        // Start emotion cycles
        StartCoroutine(CycleEmotions());
        StartCoroutine(MoveShapes());
    }
    
    IEnumerator CycleEmotions()
    {
        while (true)
        {
            // Trigger random emotions on random shapes
            foreach (var shape in _spawnedShapes)
            {
                if (Random.value < 0.3f)
                {
                    // Create a stimulus
                    EmotionalStimulus stimulus = new EmotionalStimulus
                    {
                        EmotionType = GetRandomEmotion(),
                        Intensity = Random.Range(0.3f, 1.0f),
                        Source = GetRandomSource()
                    };
                    
                    // Apply stimulus
                    shape.ProcessStimulus(stimulus);
                    
                    Debug.Log($"{shape.gameObject.name} feels {stimulus.EmotionType} at intensity {stimulus.Intensity}");
                }
            }
            
            yield return new WaitForSeconds(_delayBetweenEmotions);
        }
    }
    
    IEnumerator MoveShapes()
    {
        while (true)
        {
            // Move shapes slightly to demonstrate proximity effects
            foreach (var shape in _spawnedShapes)
            {
                if (Random.value < 0.2f)
                {
                    Vector3 randomDirection = new Vector3(
                        Random.Range(-1f, 1f),
                        0,
                        Random.Range(-1f, 1f)
                    ).normalized;
                    
                    Vector3 newPosition = shape.transform.position + randomDirection * _movementSpeed;
                    
                    // Keep within spawn radius
                    if (Vector3.Distance(Vector3.zero, newPosition) > _spawnRadius * 1.5f)
                    {
                        newPosition = newPosition.normalized * _spawnRadius;
                    }
                    
                    // Apply position
                    shape.transform.position = new Vector3(
                        newPosition.x,
                        0.5f,
                        newPosition.z
                    );
                }
            }
            
            yield return new WaitForSeconds(2f);
        }
    }
    
    EmotionType GetRandomEmotion()
    {
        EmotionType[] emotions = {
            EmotionType.Joy,
            EmotionType.Sadness,
            EmotionType.Anger,
            EmotionType.Fear,
            EmotionType.Surprise,
            EmotionType.Disgust,
            EmotionType.Trust,
            EmotionType.Anticipation
        };
        
        return emotions[Random.Range(0, emotions.Length)];
    }
    
    GameObject GetRandomSource()
    {
        if (_spawnedShapes.Count == 0)
            return null;
            
        return _spawnedShapes[Random.Range(0, _spawnedShapes.Count)].gameObject;
    }
    
    void Update()
    {
        // Check for shape selection via mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                EmotionSystem clickedShape = hit.collider.GetComponent<EmotionSystem>();
                if (clickedShape != null)
                {
                    _selectedShape = clickedShape;
                    if (_relationshipUI != null)
                    {
                        _relationshipUI.SetSelectedEntity(_selectedShape);
                    }
                    
                    Debug.Log($"Selected shape: {clickedShape.gameObject.name}");
                }
            }
        }
    }
}
