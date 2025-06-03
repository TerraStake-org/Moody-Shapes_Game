#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AnimationSetupHelper : EditorWindow
{
    [MenuItem("Tools/Setup Emotion Animator")]
    static void Init()
    {
        var window = GetWindow<AnimationSetupHelper>();
        window.titleContent = new GUIContent("Animator Setup");
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Create Emotion Layers"))
        {
            CreateLayers();
        }
    }

    void CreateLayers()
    {
        var controller = Selection.activeObject as AnimatorController;
        if (controller == null)
        {
            Debug.LogError("Please select an Animator Controller");
            return;
        }

        // Base layer
        var baseLayer = controller.layers[0];
        baseLayer.name = "Base";

        // Add emotion layers
        AddEmotionLayer(controller, "Happy");
        AddEmotionLayer(controller, "Sad");
        AddEmotionLayer(controller, "Angry");
       
        // Add blend layer
        var blendLayer = new AnimatorControllerLayer {
            name = "Blend",
            stateMachine = new AnimatorStateMachine(),
            defaultWeight = 0f,
            blendingMode = AnimatorLayerBlendingMode.Override
        };
        controller.AddLayer(blendLayer);

        Debug.Log("Successfully setup animation layers");
    }

    void AddEmotionLayer(AnimatorController controller, string name)
    {
        var layer = new AnimatorControllerLayer {
            name = name,
            stateMachine = new AnimatorStateMachine(),
            defaultWeight = 0f,
            blendingMode = AnimatorLayerBlendingMode.Additive
        };
        controller.AddLayer(layer);
    }
}
#endif
