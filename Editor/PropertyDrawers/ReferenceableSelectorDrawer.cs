using NPTP.ReferenceableScriptables.Utilities.Collections;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ReferenceableSelector), useForChildren: true)]
    public class ReferenceableSelectorDrawer : PropertyDrawer
    {
        private bool hasInitialized;
        private bool noItemsFound;
        private string[] guids;
        private string[] paths;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!hasInitialized)
            {
                hasInitialized = true;
                var table = ReferenceablesTable.Table;
                if (table.Count == 0)
                {
                    noItemsFound = true;
                }
                else
                {
                    noItemsFound = false;
                    guids = new string[table.Count + 1];
                    guids[0] = string.Empty;
                    paths = new string[table.Count + 1];
                    paths[0] = string.Empty;
                    int i = 1;
                    foreach (KeyValueCombo<string, string> combo in table)
                    {
                        guids[i] = combo.Key;
                        string usefulPath = combo.Value.Remove(0, "Referenceables/".Length);
                        
                        var container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>($"Assets/Resources/{combo.Value}.asset");
                        if (container != null)
                        {
                            usefulPath = usefulPath.Remove(usefulPath.LastIndexOf('/') + 1) + container.ReferenceName;
                        }

                        paths[i] = usefulPath;
                        
                        i++;
                    }
                }
            }
            
            SerializedProperty guidProperty = property.FindPropertyRelative("guid");

            if (noItemsFound)
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.Popup(position, label.text, 0, new []{"No Referenceables found."});
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
    }
}