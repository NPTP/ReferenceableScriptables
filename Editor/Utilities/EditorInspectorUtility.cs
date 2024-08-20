using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    public static class EditorInspectorUtility
    {
        public static void DrawHorizontalLine()
        {
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
        }
        
        public static void ShowScriptInspector<T>(T targetMonoBehaviour) where T : MonoBehaviour
        {
            EditorGUI.BeginDisabledGroup(disabled: true);
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(targetMonoBehaviour), typeof(T), false);
            EditorGUI.EndDisabledGroup();
        }
    }
}