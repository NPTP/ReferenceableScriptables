using NPTP.ReferenceableScriptables.Attributes;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(GUIDisabledAttribute))]
    public class GUIDisabledAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(disabled: true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }
}