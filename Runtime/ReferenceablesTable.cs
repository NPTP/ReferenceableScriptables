using NPTP.ReferenceableScriptables.Attributes;
using NPTP.ReferenceableScriptables.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    internal class ReferenceablesTable : ScriptableObject
    {
        private static string AssetPathToThis => $"Assets/Resources/{nameof(ReferenceablesTable)}.asset";
        private static string ResourcesPathToThis => nameof(ReferenceablesTable);
        
        private static ReferenceablesTable instance;
        private static ReferenceablesTable Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!TryLoad(out instance))
                    {
                        instance = CreateTable();
                    }
                    
                    // Guarantee the table is ready
                    instance.guidToPathTable.OnAfterDeserialize();
                }

                return instance;
            }
        }

        [SerializeField][GUIDisabled] private SerializableDictionary<string, string> guidToPathTable = new();
        internal static SerializableDictionary<string, string> Table => Instance.guidToPathTable;
        
        /// <summary>
        /// Try to load a Referenceable Scriptable at the given guid address.
        /// Note that there is no need to unload what is loaded - the scriptable reference stays on the stack.
        /// </summary>
        internal static bool TryLoad<T>(string guid, out T scriptable) where T : ScriptableObject
        {
            scriptable = null;
            
            if (!Table.TryGetValue(guid, out string pathInsideResources))
            {
                return false;
            }

            ScriptableObject referenceableScriptable = null;
            
            var container = Resources.Load<ScriptableReferenceContainer>(pathInsideResources);
            if (container != null)
            {
                referenceableScriptable = container.Reference;
                Resources.UnloadAsset(container);
            }

            scriptable = referenceableScriptable as T;
            return scriptable != null;
        }

        private static bool TryLoad(out ReferenceablesTable table)
        {
#if UNITY_EDITOR
            table = EditorApplication.isPlaying
                ? Resources.Load<ReferenceablesTable>(ResourcesPathToThis)
                : AssetDatabase.LoadAssetAtPath<ReferenceablesTable>(AssetPathToThis);
#else
            table = Resources.Load<ReferenceablesTable>(ResourcesPathToThis);
#endif
            return table != null;
        }
        
        private static ReferenceablesTable CreateTable()
        {
            ReferenceablesTable table = CreateInstance<ReferenceablesTable>();
            
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: "Resources");
            }
            
            AssetDatabase.CreateAsset(table, AssetPathToThis);
            SetDirtySaveAndRefresh();
#endif
            
            return table;
        }

#if UNITY_EDITOR
        internal static void SetDirtySaveAndRefresh()
        {
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}