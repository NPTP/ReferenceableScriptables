using System;
using System.Collections.Generic;
using System.Reflection;
using NPTP.ReferenceableScriptables.Editor.Utilities;
using NPTP.ReferenceableScriptables.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ReferenceableScriptableID<>))]
    internal sealed class ReferenceableScriptableIDDrawer : PropertyDrawer
    {
        private const string VALUE_PROPERTY_NAME = "value";
        
        private bool hasInitialized;
        private bool noItemsFound;
        private Type scriptableObjectType;
        private string[] guids;
        private string[] paths;
        
        private static string GetNoneFoundMessage(Type type)
        {
            return type == typeof(ScriptableObject)
                ? "No scriptable objects are referenceable."
                : $"No scriptable objects of type <{type.Name}> are referenceable.";
        }

        ~ReferenceableScriptableIDDrawer()
        {
            Referenceables.OnReferenceablesUpdated -= HandleReferenceablesUpdated;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty guidProperty = property.FindPropertyRelative(VALUE_PROPERTY_NAME);
            
            if (Application.isPlaying)
            {
                ShowPlayModeProperty(position, property, label, guidProperty);
                return;
            }

            InitializeEditMode(property);

            if (noItemsFound)
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.Popup(position, label.text, 0, new[] { GetNoneFoundMessage(scriptableObjectType) });
                guidProperty.stringValue = string.Empty;
                EditorGUI.EndProperty();
                return;
            }

            int index = Mathf.Max(0, Array.IndexOf(guids, guidProperty.stringValue));
            EditorGUI.BeginProperty(position, label, property);
            index = EditorGUI.Popup(position, label.text, index, paths);
            guidProperty.stringValue = guids[index];
            EditorGUI.EndProperty();
        }

        // TODO: Non-urgent, but maybe support play mode view & modification from runtime dictionary instead of just showing blocked guid
        private static void ShowPlayModeProperty(Rect position, SerializedProperty property, GUIContent label, SerializedProperty guidProperty)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(disabled: true);
            EditorGUI.PropertyField(position, guidProperty, new GUIContent(property.name.AsInspectorLabel()));
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }

        private void InitializeEditMode(SerializedProperty property)
        {
            if (hasInitialized)
            {
                return;
            }
            
            hasInitialized = true;

            if (!TryGetScriptableObjectType(property, out scriptableObjectType))
            {
                return;
            }
            
            Referenceables.OnReferenceablesUpdated += HandleReferenceablesUpdated;
            
            SerializableDictionary<string, string> table = ReferenceablesTable.GuidToPathTable;

            List<string> guidsList = new() { string.Empty };
            List<string> pathsList = new() { string.Empty };

            foreach (KVP<string, string> guidPathPair in table)
            {
                string guid = guidPathPair.Key;
                string path = guidPathPair.Value;
                    
                var container = AssetDatabase.LoadAssetAtPath<ReferenceableScriptableContainer>($"Assets/Resources/{path}.asset");
                if (container == null || container.Reference == null)
                {
                    continue;
                }
                    
                Type containerReferenceType = container.Reference.GetType();
                if (containerReferenceType == scriptableObjectType || containerReferenceType.IsSubclassOf(scriptableObjectType))
                {
                    guidsList.Add(guid);
                    pathsList.Add(container.Reference.name);
                }
            }

            guids = guidsList.ToArray();
            paths = pathsList.ToArray();

            noItemsFound = guids.Length == 1 || paths.Length == 1;
        }

        private void HandleReferenceablesUpdated()
        {
            Referenceables.OnReferenceablesUpdated -= HandleReferenceablesUpdated;
            hasInitialized = false;
        }

        private static bool TryGetScriptableObjectType(SerializedProperty property, out Type scriptableObjectType)
        {
            Type type = property.serializedObject.targetObject.GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (FieldInfo info in fieldInfos)
            {
                if (info.Name == property.name)
                {
                    scriptableObjectType = info.FieldType.GenericTypeArguments[0];
                    return true;
                }
            }

            scriptableObjectType = null;
            return false;
        }
    }
}