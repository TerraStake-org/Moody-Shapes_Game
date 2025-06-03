using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// Custom property drawer for the ReadOnly attribute.
/// Makes properties with this attribute non-editable in the Inspector.
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Save the original GUI enabled state
        bool originalGUIState = GUI.enabled;
        
        // Disable GUI controls
        GUI.enabled = false;
        
        // Draw the property as usual, but with GUI disabled
        EditorGUI.PropertyField(position, property, label, true);
        
        // Restore the original GUI enabled state
        GUI.enabled = originalGUIState;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Use the default property height
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
