using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor
{
    [CustomEditor(typeof(ReferenceableScriptable), editorForChildClasses: true)][CanEditMultipleObjects]
    public class ReferenceableScriptableEditor : UnityEditor.Editor
    {
        private ReferenceableScriptable refableScriptable;
        private SerializedProperty referenceable;
        private SerializedProperty guid;

        private bool referenceableValue;

        protected virtual void OnEnable()
        {
            refableScriptable = (ReferenceableScriptable)target;
            referenceable = serializedObject.FindProperty(nameof(referenceable));
            guid = serializedObject.FindProperty(nameof(guid));
            referenceableValue = referenceable.boolValue;
        }
        
        public override void OnInspectorGUI()
        {
            bool previousReferenceableValue = referenceable.boolValue;
            referenceableValue = EditorGUILayout.Toggle(nameof(referenceable).CapitalizeFirst(), referenceable.boolValue);
            if (previousReferenceableValue != referenceableValue)
            {
                if (referenceableValue)
                    AssetCreator.MakeReferenceable(refableScriptable);
                else
                    AssetCreator.RemoveReferenceable(refableScriptable);
            }

            if (referenceableValue)
            {
                EditorGUILayout.PropertyField(guid, new GUIContent("Referenceable Address"));
            }
            
            EditorInspectorUtility.DrawHorizontalLine();
            DrawDefaultInspector();
            referenceable.boolValue = referenceableValue;
            serializedObject.ApplyModifiedProperties();
        }
    }
}