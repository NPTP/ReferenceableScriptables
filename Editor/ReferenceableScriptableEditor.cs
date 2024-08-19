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

        private bool referenceableValue;

        protected virtual void OnEnable()
        {
            refableScriptable = (ReferenceableScriptable)target;
            referenceable = serializedObject.FindProperty(nameof(referenceable));
            referenceableValue = referenceable.boolValue;
        }
        
        public override void OnInspectorGUI()
        {
            bool previousReferenceableValue = referenceable.boolValue;
            referenceableValue = EditorGUILayout.Toggle(nameof(referenceable).CapitalizeFirst(), referenceable.boolValue);
            if (previousReferenceableValue != referenceableValue)
            {
                if (referenceableValue)
                {
                    AssetCreator.MakeReferenceable(refableScriptable);
                    Debug.Log("Make refable");
                }
                else
                {
                    AssetCreator.RemoveReferenceable(refableScriptable);
                    Debug.Log("Remove refable");
                }
            }
            EditorInspectorUtility.DrawHorizontalLine();
            DrawDefaultInspector();
            referenceable.boolValue = referenceableValue;
            serializedObject.ApplyModifiedProperties();
        }
    }
}