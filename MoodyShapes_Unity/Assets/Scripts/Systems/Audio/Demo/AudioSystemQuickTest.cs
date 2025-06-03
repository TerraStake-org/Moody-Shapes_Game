using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/// <summary>
/// Standalone audio system test that can be added to any scene to validate
/// the audio system's functionality.
/// </summary>
public class AudioSystemQuickTest : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private bool _runAutomaticTest = true;
    [SerializeField] private bool _createPlaceholderAudio = true;
    [SerializeField] private EmotionType[] _emotionsToTest;
    
    [Header("References")]
    [SerializeField] private MoodMusicManager _musicManager;
    [SerializeField] private ShapeAudioFeedback _testShapeAudio;
    
    private AudioPlaceholderGenerator _placeholderGenerator;
    
    private void Start()
    {
        // Ensure we have the audio placeholder generator
        if (_createPlaceholderAudio)
        {
            _placeholderGenerator = GetComponent<AudioPlaceholderGenerator>();
            if (_placeholderGenerator == null)
            {
                _placeholderGenerator = gameObject.AddComponent<AudioPlaceholderGenerator>();
                _placeholderGenerator.GenerateAllEmotionSounds();
            }
        }
        
        // If no emotions specified, test a default set
        if (_emotionsToTest == null || _emotionsToTest.Length == 0)
        {
            _emotionsToTest = new EmotionType[] 
            {
                EmotionType.Happy,
                EmotionType.Sad,
                EmotionType.Angry,
                EmotionType.Calm,
                EmotionType.Surprised
            };
        }
        
        // If automatic test is enabled, start the test sequence
        if (_runAutomaticTest)
        {
            StartCoroutine(RunAutomaticTest());
        }
    }
    
    private IEnumerator RunAutomaticTest()
    {
        Debug.Log("Starting automatic audio system test...");
        
        // Wait for everything to initialize
        yield return new WaitForSeconds(1.0f);
        
        // Enable debug logging
        if (_musicManager != null)
        {
            _musicManager.EnableDebugLogging = true;
        }
        
        // Test each emotion
        foreach (EmotionType emotion in _emotionsToTest)
        {
            Debug.Log($"Testing {emotion} emotion...");
            
            // Test shape audio feedback
            if (_testShapeAudio != null)
            {
                _testShapeAudio.TriggerEmotionSound(emotion, 0.9f);
                yield return new WaitForSeconds(1.0f);
            }
            
            // Test system-wide mood change
            if (_musicManager != null)
            {
                // Create multiple shapes with the same emotion to influence the global mood
                for (int i = 0; i < 3; i++)
                {
                    GameObject testObj = new GameObject($"TestEmotion_{emotion}_{i}");
                    testObj.transform.SetParent(transform);
                    
                    // Add emotion system
                    EmotionSystem emotionSystem = testObj.AddComponent<EmotionSystem>();
                    emotionSystem.ForceEmotion(emotion, 0.9f, 10f);
                    
                    // Add audio feedback
                    AudioSource audioSource = testObj.AddComponent<AudioSource>();
                    ShapeAudioFeedback feedback = testObj.AddComponent<ShapeAudioFeedback>();
                    
                    // Position randomly around center
                    testObj.transform.position = transform.position + Random.insideUnitSphere * 3f;
                }
                
                // Wait for mood analysis to pick up the change
                yield return new WaitForSeconds(3.0f);
                
                // Log the result
                EmotionType currentMood = _musicManager.GetCurrentDominantMood();
                Debug.Log($"Dominant mood is now: {currentMood}");
                
                // Clean up test objects
                foreach (Transform child in transform)
                {
                    if (child.name.StartsWith("TestEmotion_"))
                    {
                        Destroy(child.gameObject);
                    }
                }
                
                yield return new WaitForSeconds(1.0f);
            }
        }
        
        Debug.Log("Audio system test complete!");
    }
    
    /// <summary>
    /// Manually test a specific emotion
    /// </summary>
    public void TestEmotion(EmotionType emotion)
    {
        // Play local sound effect
        if (_testShapeAudio != null)
        {
            _testShapeAudio.TriggerEmotionSound(emotion, 0.9f);
        }
        
        // Create temporary object with this emotion
        GameObject testObj = new GameObject($"ManualTest_{emotion}");
        testObj.transform.SetParent(transform);
        
        // Add emotion system
        EmotionSystem emotionSystem = testObj.AddComponent<EmotionSystem>();
        emotionSystem.ForceEmotion(emotion, 0.9f, 10f);
        
        // Position randomly
        testObj.transform.position = transform.position + Random.insideUnitSphere * 3f;
        
        // Destroy after a while
        Destroy(testObj, 10f);
        
        Debug.Log($"Manually testing {emotion} emotion");
    }
}
