using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;

namespace NPTP.ReferenceableScriptables.Editor
{
    [CustomEditor(typeof(ReferenceableScriptable), editorForChildClasses: true)]
    public class ReferenceableScriptableEditor : UnityEditor.Editor
    {
        private ReferenceableScriptable scriptable;
        private SerializedProperty guid;

        private bool referenceableValue;

        protected virtual void OnEnable()
        {
            scriptable = (ReferenceableScriptable)target;
            guid = serializedObject.FindProperty(nameof(guid));
            referenceableValue = ReferenceablesTable.IsEntryValid(scriptable);
        }
        
        public override void OnInspectorGUI()
        {
            bool previousReferenceableValue = referenceableValue;
            referenceableValue = EditorGUILayout.Toggle("Referenceable", referenceableValue);
            
            if (previousReferenceableValue != referenceableValue)
            {
                if (referenceableValue)
                    AssetCreator.MakeReferenceable(scriptable);
                else
                    AssetCreator.RemoveReferenceable(scriptable);
            }

            if (referenceableValue)
            {
                EditorGUILayout.PropertyField(guid);
            }
            
            referenceableValue = ReferenceablesTable.IsEntryValid(scriptable);

            EditorInspectorUtility.DrawHorizontalLine();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}