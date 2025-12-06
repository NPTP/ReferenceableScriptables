using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor
{
    internal sealed class ReferenceablesManagementEditorWindow : EditorWindow
    {
        private class ReferenceableToggler
        {
            internal readonly ScriptableObject scriptable;
            internal bool toggle;

            internal ReferenceableToggler(ScriptableObject scriptable, bool toggle)
            {
                this.scriptable = scriptable;
                this.toggle = toggle;
            }
        }
        
        private const string MENU_ITEM_PATH = "Tools/Referenceables Management";
        
        private static IEnumerable<Type> ExcludedScriptableObjectTypes => new[] { typeof(ReferenceablesTable), typeof(ReferenceableScriptableContainer) };
        
        private readonly Dictionary<Type, List<ReferenceableToggler>> typeToReferenceableToggler = new();
        private ScriptableObject[] scriptables = Array.Empty<ScriptableObject>();
        private Vector2 scrollPosition = Vector2.zero;
        
        [MenuItem(MENU_ITEM_PATH)]
        private static void Init()
        {
            ReferenceablesManagementEditorWindow window = (ReferenceablesManagementEditorWindow)GetWindow(typeof(ReferenceablesManagementEditorWindow));
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Referenceables Management");
            Refresh();
        }

        private void Refresh()
        {
            typeToReferenceableToggler.Clear();
            scriptables = AssetDatabaseUtility.GetAssetsOfTypeWithExclusions<ScriptableObject>(ExcludedScriptableObjectTypes);

            foreach (ScriptableObject scriptable in scriptables)
            {
                Type type = scriptable.GetType();
                if (!typeToReferenceableToggler.ContainsKey(type))
                {
                    typeToReferenceableToggler.Add(type, new List<ReferenceableToggler>());
                }
                
                typeToReferenceableToggler[type].Add(new ReferenceableToggler(scriptable, Referenceables.IsValidEntry(scriptable)));
            }
        }

        private void OnGUI()
        {
            bool shouldRefresh = false;
            
            EditorGUILayout.LabelField("Referenceables Management", EditorStyles.whiteLargeLabel);
            EditorInspectorUtility.DrawHorizontalLine();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Clean on pre-process build?", GUILayout.ExpandWidth(true));
            Referenceables.CleanOnPreProcessBuild = EditorGUILayout.Toggle(Referenceables.CleanOnPreProcessBuild, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Refresh"))
            {
                Refresh();
            }

            if (GUILayout.Button("Make All Referenceable"))
            {
                MakeAllReferenceable(scriptables, true);
                shouldRefresh = true;
            }

            if (GUILayout.Button("Remove All Referenceable"))
            {
                MakeAllReferenceable(scriptables, false);
                shouldRefresh = true;
            }

            EditorInspectorUtility.DrawHorizontalLine();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

            foreach (KeyValuePair<Type,List<ReferenceableToggler>> keyValuePair in typeToReferenceableToggler)
            {
                string typeName = keyValuePair.Key.Name;
                List<ReferenceableToggler> list = keyValuePair.Value;
                
                EditorGUILayout.LabelField(typeName, EditorStyles.whiteLargeLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"Make All {typeName} Referenceable"))
                {
                    MakeAllReferenceable(list.Select(toggler => toggler.scriptable).ToList(), true);
                    shouldRefresh = true;
                }
                if (GUILayout.Button($"Remove All {typeName} Referenceable"))
                {
                    MakeAllReferenceable(list.Select(toggler => toggler.scriptable).ToList(), false);
                    shouldRefresh = true;
                }
                EditorGUILayout.EndHorizontal();
                
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

                    EditorGUILayout.ObjectField(list[i].scriptable, typeof(ScriptableObject), true, GUILayout.Width(250));
                
                    bool previousValue = list[i].toggle;
                    list[i].toggle = EditorGUILayout.Toggle(list[i].toggle, GUILayout.Width(100));
                    if (previousValue != list[i].toggle)
                    {
                        MakeReferenceable(list[i].scriptable, list[i].toggle);
                        shouldRefresh = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }
                
                EditorInspectorUtility.DrawHorizontalLine();
            }
            
            EditorGUILayout.EndScrollView();

            if (shouldRefresh)
            {
                Refresh();
            }
        }

        private void MakeReferenceable(ScriptableObject scriptable, bool referenceable)
        {
            Referenceables.MakeReferenceable(scriptable, referenceable);
        }

        private void MakeAllReferenceable(IEnumerable<ScriptableObject> collection, bool referenceable)
        {
            foreach (ScriptableObject scriptable in collection)
            {
                Referenceables.MakeReferenceable(scriptable, referenceable);
            }
        }
    }
}