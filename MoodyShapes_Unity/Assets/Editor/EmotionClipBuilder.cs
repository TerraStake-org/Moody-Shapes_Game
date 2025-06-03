#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class EmotionClipBuilder : EditorWindow
{
    private EmotionType _emotionType;
    private AnimationClip _baseClip;
    private int _variantCount = 3;

    [MenuItem("Tools/Create Emotion Clips")]
    static void Init()
    {
        var window = GetWindow<EmotionClipBuilder>();
        window.titleContent = new GUIContent("Clip Builder");
        window.Show();
    }

    void OnGUI()
    {
        _emotionType = (EmotionType)EditorGUILayout.EnumPopup("Emotion", _emotionType);
        _baseClip = EditorGUILayout.ObjectField("Base Clip", _baseClip, typeof(AnimationClip), false) as AnimationClip;
        _variantCount = EditorGUILayout.IntSlider("Variants", _variantCount, 1, 5);

        if (GUILayout.Button("Generate Clips"))
        {
            GenerateClips();
        }
    }

    void GenerateClips()
    {
        if (_baseClip == null) return;

        string path = AssetDatabase.GetAssetPath(_baseClip);
        path = path.Replace(".anim", "");

        for (int i = 0; i < _variantCount; i++)
        {
            float intensity = (i + 1f) / _variantCount;
            var newClip = Instantiate(_baseClip);
            newClip.name = $"{_emotionType}_Intensity_{intensity:F1}";
           
            // Modify curves based on intensity
            ModifyClipCurves(newClip, intensity);
           
            AssetDatabase.CreateAsset(newClip, $"{path}_{newClip.name}.anim");
        }

        AssetDatabase.SaveAssets();
    }

    void ModifyClipCurves(AnimationClip clip, float intensity)
    {
        // Example: Scale all curves by intensity
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            for (int i = 0; i < curve.keys.Length; i++)
            {
                curve.keys[i].value *= intensity;
            }
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
    }
}
#endif
