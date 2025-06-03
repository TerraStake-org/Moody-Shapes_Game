using UnityEngine;

/// <summary>
/// Initializes and manages screen effects for the game
/// </summary>
public class EmotionScreenEffectsInitializer : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool createPostProcessingIfMissing = true;
    [SerializeField] private bool createScreenShakeIfMissing = true;
    
    private void Awake()
    {
        // Find main camera if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            // Check for post-processing controller
            var postProcessing = FindObjectOfType<EmotionPostProcessingController>();
            if (postProcessing == null && createPostProcessingIfMissing)
            {
                // Create post-processing controller
                GameObject ppObject = new GameObject("Emotion Post Processing Controller");
                ppObject.transform.SetParent(mainCamera.transform);
                
                // Add required components
                var volume = ppObject.AddComponent<UnityEngine.Rendering.Volume>();
                volume.isGlobal = true;
                volume.priority = 100;
                
                // Create a profile if none exists
                if (volume.profile == null)
                {
                    volume.profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
                    
                    // Add required effects to the profile
                    volume.profile.Add<UnityEngine.Rendering.Universal.Vignette>();
                    volume.profile.Add<UnityEngine.Rendering.Universal.ColorAdjustments>();
                    volume.profile.Add<UnityEngine.Rendering.Universal.Bloom>();
                    volume.profile.Add<UnityEngine.Rendering.Universal.DepthOfField>();
                    volume.profile.Add<UnityEngine.Rendering.Universal.FilmGrain>();
                }
                
                // Add controller
                postProcessing = ppObject.AddComponent<EmotionPostProcessingController>();
            }
            
            // Check for screen shake controller
            var screenShake = FindObjectOfType<EmotionScreenShakeController>();
            if (screenShake == null && createScreenShakeIfMissing)
            {
                // Create screen shake controller
                GameObject shakeObject = new GameObject("Emotion Screen Shake Controller");
                shakeObject.transform.SetParent(mainCamera.transform);
                
                // Add required components
                var impulseSource = shakeObject.AddComponent<Cinemachine.CinemachineImpulseSource>();
                impulseSource.m_DefaultVelocity = Vector3.down;
                
                // Add controller
                screenShake = shakeObject.AddComponent<EmotionScreenShakeController>();
            }
        }
        
        // Initialize the static utility class
        EmotionScreenEffects.Initialize();
    }
}
