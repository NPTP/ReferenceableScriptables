using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.CustomEditors
{
    [CustomEditor(typeof(ScriptableObject), editorForChildClasses: true), CanEditMultipleObjects]
    public class ScriptableObjectEditor : UnityEditor.Editor
    {
        private const string REFERENCEABLE = "Referenceable";

        private static IEnumerable<Type> TypesWithoutReferenceableOption => new[]
        {
            typeof(ReferenceablesTable),
            typeof(ReferenceableScriptableContainer)
        }; 
        
        private ScriptableObject[] scriptableObjects;
        private bool referenceableValue;

        private void OnEnable()
        {
            scriptableObjects = new ScriptableObject[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                scriptableObjects[i] = (ScriptableObject)targets[i];
            }
            
            referenceableValue = scriptableObjects.All(Referenceables.IsValidEntry);
        }
        
        public override void OnInspectorGUI()
        {
            if (!TypesWithoutReferenceableOption.Contains(target.GetType()))
            {
                bool previousReferenceableValue = referenceableValue;
                referenceableValue = EditorGUILayout.Toggle(REFERENCEABLE, referenceableValue);
            
                if (previousReferenceableValue != referenceableValue)
                {
                    foreach (ScriptableObject scriptable in scriptableObjects)
                    {
                        Referenceables.MakeReferenceable(scriptable, referenceableValue);
                    }
                }
            
                referenceableValue = scriptableObjects.All(Referenceables.IsValidEntry);
                EditorInspectorUtility.DrawHorizontalLine();
            }
          
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}