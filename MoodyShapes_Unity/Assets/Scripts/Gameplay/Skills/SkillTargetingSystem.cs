using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component that handles targeting and using skills on game objects
/// </summary>
public class SkillTargetingSystem : MonoBehaviour
{
    [SerializeField] private EmotionSkillUI skillUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask targetableLayers;
    [SerializeField] private float maxTargetingDistance = 20f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject targetingIndicatorPrefab;
    [SerializeField] private Color validTargetColor = Color.green;
    [SerializeField] private Color invalidTargetColor = Color.red;
    
    private GameObject currentTarget;
    private GameObject indicatorInstance;
    
    private void Start()
    {
        // Auto-find references if not set
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (skillUI == null)
            skillUI = FindObjectOfType<EmotionSkillUI>();
    }
    
    private void Update()
    {
        // Check if we have a UI, and if we're over a UI element
        if (skillUI == null || EventSystem.current.IsPointerOverGameObject())
        {
            ClearTargeting();
            return;
        }
        
        // Cast a ray from the mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxTargetingDistance, targetableLayers))
        {
            // Check if the hit object has an EmotionalState component
            if (hit.collider.gameObject.GetComponent<EmotionalState>() != null)
            {
                // Valid target
                SetTarget(hit.collider.gameObject, true);
            }
            else
            {
                // Invalid target
                SetTarget(hit.collider.gameObject, false);
            }
        }
        else
        {
            // No target
            ClearTargeting();
        }
        
        // Check for click to use skill
        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            skillUI.UseSelectedSkillOnTarget(currentTarget);
            ClearTargeting();
        }
    }
    
    /// <summary>
    /// Sets the current target and updates the visual indicator
    /// </summary>
    private void SetTarget(GameObject target, bool isValid)
    {
        currentTarget = isValid ? target : null;
        
        // Create or move the indicator
        if (targetingIndicatorPrefab != null)
        {
            if (indicatorInstance == null)
            {
                indicatorInstance = Instantiate(targetingIndicatorPrefab);
            }
            
            indicatorInstance.transform.position = target.transform.position;
            
            // Set color based on validity
            Renderer renderer = indicatorInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isValid ? validTargetColor : invalidTargetColor;
            }
            
            indicatorInstance.SetActive(true);
        }
    }
    
    /// <summary>
    /// Clears the current target and hides the indicator
    /// </summary>
    private void ClearTargeting()
    {
        currentTarget = null;
        
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(false);
        }
    }
    
    /// <summary>
    /// Cleans up when the object is destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }
    }
}
