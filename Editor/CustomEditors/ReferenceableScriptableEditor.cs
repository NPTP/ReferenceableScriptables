using System.Linq;
using NPTP.ReferenceableScriptables.AssetTypes;
using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;

namespace NPTP.ReferenceableScriptables.Editor.CustomEditors
{
    [CustomEditor(typeof(ReferenceableScriptable), editorForChildClasses: true), CanEditMultipleObjects]
    public class ReferenceableScriptableEditor : UnityEditor.Editor
    {
        private ReferenceableScriptable[] scriptables;
        private SerializedProperty guid;

        private bool referenceableValue;

        protected virtual void OnEnable()
        {
            scriptables = new ReferenceableScriptable[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                scriptables[i] = (ReferenceableScriptable)targets[i];
            }
            
            guid = serializedObject.FindProperty(nameof(guid));
            referenceableValue = scriptables.All(ReferenceablesTable.IsValidEntry);
        }
        
        public override void OnInspectorGUI()
        {
            bool previousReferenceableValue = referenceableValue;
            referenceableValue = EditorGUILayout.Toggle("Referenceable", referenceableValue);
            
            if (previousReferenceableValue != referenceableValue)
            {
                foreach (ReferenceableScriptable scriptable in scriptables)
                    Referenceables.MakeReferenceable(scriptable, referenceableValue);
            }

            if (referenceableValue)
            {
                EditorGUILayout.PropertyField(guid);
            }
            
            referenceableValue = scriptables.All(ReferenceablesTable.IsValidEntry);

            EditorInspectorUtility.DrawHorizontalLine();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}