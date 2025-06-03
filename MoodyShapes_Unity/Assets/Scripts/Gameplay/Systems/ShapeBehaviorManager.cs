using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages dynamic shape properties based on player tendencies.
/// </summary>
public class ShapeBehaviorManager : MonoBehaviour
{
    [Header("Shape Settings")]
    [Tooltip("Reference to all shapes in the scene.")]
    private List<EmotionSystem> shapes;

    private void Awake()
    {
        RefreshShapesList();
    }

    /// <summary>
    /// Rebuilds the list of tracked shapes in the scene.
    /// </summary>
    public void RefreshShapesList()
    {
        shapes = new List<EmotionSystem>(FindObjectsOfType<EmotionSystem>());
    }

    /// <summary>
    /// Adjusts shape properties based on player tendencies.
    /// </summary>
    public void AdaptShapeBehavior(EmotionType dominantEmotion)
    {
        foreach (var shape in shapes)
        {
            switch (dominantEmotion)
            {
                case EmotionType.Anger:
                    shape.SetFragility(true); // Example: Make shapes fragile
                    break;
                case EmotionType.Joy:
                    shape.SetCooperation(true); // Example: Make shapes cooperative
                    break;
                default:
                    shape.ResetBehavior(); // Reset to default behavior
                    break;
            }
        }
    }
}
