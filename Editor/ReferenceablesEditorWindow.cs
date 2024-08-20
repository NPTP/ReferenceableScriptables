using System;
using System.Collections.Generic;
using NPTP.ReferenceableScriptables.AssetTypes;
using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor
{
    public class ReferenceablesEditorWindow : EditorWindow
    {
        private class ReferenceableToggler
        {
            public ReferenceableScriptable scriptable;
            public bool toggle;

            public ReferenceableToggler(ReferenceableScriptable scriptable, bool toggle)
            {
                this.scriptable = scriptable;
                this.toggle = toggle;
            }
        }
        
        private const string MENU_ITEM_PATH = "Tools/Referenceables Management";

        private ReferenceableScriptable[] scriptables = Array.Empty<ReferenceableScriptable>();
        private Vector2 scrollPosition = Vector2.zero;
        private Dictionary<Type, List<ReferenceableToggler>> typeToReferenceableToggler = new();
        
        [MenuItem(MENU_ITEM_PATH)]
        private static void Init()
        {
            ReferenceablesEditorWindow window = (ReferenceablesEditorWindow)GetWindow(typeof(ReferenceablesEditorWindow));
            window.Show();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            typeToReferenceableToggler.Clear();
            scriptables = AssetDatabaseUtility.GetAssets<ReferenceableScriptable>();

            foreach (ReferenceableScriptable scriptable in scriptables)
            {
                Type type = scriptable.GetType();
                if (!typeToReferenceableToggler.ContainsKey(type))
                {
                    typeToReferenceableToggler.Add(type, new List<ReferenceableToggler>());
                }
                
                typeToReferenceableToggler[type].Add(new ReferenceableToggler(scriptable, ReferenceablesTable.IsValidEntry(scriptable)));
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Referenceables", EditorStyles.whiteLargeLabel);
            EditorInspectorUtility.DrawHorizontalLine();
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh"))
            {
                Refresh();
            }

            if (GUILayout.Button("Make All Referenceable"))
            {
                MakeAllReferenceable(true);
            }

            if (GUILayout.Button("Remove All Referenceable"))
            {
                MakeAllReferenceable(false);
            }

            EditorInspectorUtility.DrawHorizontalLine();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

            foreach (KeyValuePair<Type,List<ReferenceableToggler>> keyValuePair in typeToReferenceableToggler)
            {
                string typeName = keyValuePair.Key.Name;
                List<ReferenceableToggler> list = keyValuePair.Value;
                
                EditorGUILayout.LabelField(typeName, EditorStyles.whiteLargeLabel);
                
                EditorGUILayout.BeginHorizontal();
            
                EditorGUILayout.BeginVertical(GUILayout.Width(250));
                EditorGUILayout.LabelField("Scriptable Object");
                EditorGUILayout.EndVertical();
            
                EditorGUILayout.BeginVertical(GUILayout.Width(20));
                EditorGUILayout.LabelField("Referenceable?");
                EditorGUILayout.EndVertical();
            
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < list.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(list[i].scriptable, typeof(ReferenceableScriptable), true, GUILayout.Width(250));
                
                    bool previousValue = list[i].toggle;
                    list[i].toggle = EditorGUILayout.Toggle(list[i].toggle, GUILayout.Width(100));
                    if (previousValue != list[i].toggle)
                    {
                        MakeReferenceable(scriptables[i], list[i].toggle);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                
                EditorInspectorUtility.DrawHorizontalLine();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void MakeReferenceable(ReferenceableScriptable scriptable, bool referenceable)
        {
            Referenceables.MakeReferenceable(scriptable, referenceable);
        }

        private void MakeAllReferenceable(bool referenceable)
        {
            for (int i = 0; i < scriptables.Length; i++)
            {
                Referenceables.MakeReferenceable(scriptables[i], referenceable);
            }
            
            Refresh();
        }
    }
}