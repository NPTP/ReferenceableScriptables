using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    internal static class EditorInspectorUtility
    {
        internal static void DrawHorizontalLine()
        {
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
        }
    }
}