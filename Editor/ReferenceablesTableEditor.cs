using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor
{
    [CustomEditor(typeof(ReferenceablesTable))]
    public class ReferenceablesTableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorInspectorUtility.DrawHorizontalLine();
            if (GUILayout.Button("Clean Referenceables"))
            {
                ReferenceablesTable.Clean();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}