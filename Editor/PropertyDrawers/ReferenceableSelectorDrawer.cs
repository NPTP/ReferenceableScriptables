using System;
using System.Collections.Generic;
using System.Reflection;
using NPTP.ReferenceableScriptables.Utilities.Collections;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Referenceable<>))]
    public class ReferenceableSelectorDrawer : PropertyDrawer
    {
        private bool hasInitialized;
        private bool noItemsFound;
        private string[] guids;
        private string[] paths;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            SerializedProperty guidProperty = property.FindPropertyRelative("guid");

            if (noItemsFound)
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.Popup(position, label.text, 0, new[] { "No Referenceables found." });
                guidProperty.stringValue = string.Empty;
                EditorGUI.EndProperty();
                return;
            }

            int index = Mathf.Max(0, System.Array.IndexOf(guids, guidProperty.stringValue));
            EditorGUI.BeginProperty(position, label, property);
            index = EditorGUI.Popup(position, label.text, index, paths);
            guidProperty.stringValue = guids[index];
            EditorGUI.EndProperty();
        }

        private void Initialize(SerializedProperty property)
        {
            if (!hasInitialized)
            {
                hasInitialized = true;
                var table = ReferenceablesTable.Table;

                if (!TryGetGenericType(property, out Type genericType))
                {
                    return;
                }

                List<string> guidsList = new() { string.Empty };
                List<string> pathsList = new() { string.Empty };

                foreach (KeyValueCombo<string, string> combo in table)
                {
                    var container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>($"Assets/Resources/{combo.Value}.asset");
                    if (container == null || container.Reference == null)
                    {
                        continue;
                    }

                    if (container.Reference.GetType() == genericType)
                    {
                        guidsList.Add(combo.Key);
                        string usefulPath = combo.Value;
                        pathsList.Add(usefulPath
                                          .Remove(usefulPath.LastIndexOf('/') + 1)
                                          .Remove(0, $"Referenceables/{genericType.Name}".Length + 1)
                                      + container.Reference.name);
                    }
                }

                guids = guidsList.ToArray();
                paths = pathsList.ToArray();

                if (guids.Length == 0 || paths.Length == 0)
                {
                    noItemsFound = true;
                }
            }
        }

        private static bool TryGetGenericType(SerializedProperty property, out Type genericType)
        {
            Type type = property.serializedObject.targetObject.GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo info in fieldInfos)
            {
                if (info.Name == property.name)
                {
                    genericType = info.FieldType.GenericTypeArguments[0];
                    return true;
                }
            }

            genericType = null;
            return false;
        }
    }
}