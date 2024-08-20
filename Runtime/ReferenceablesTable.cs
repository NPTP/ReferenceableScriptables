using NPTP.ReferenceableScriptables.AssetTypes;
using NPTP.ReferenceableScriptables.Attributes;
using NPTP.ReferenceableScriptables.Utilities.Collections;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    public class ReferenceablesTable : ScriptableObject
    {
        private static string AssetPathToThis => $"Assets/Resources/{nameof(ReferenceablesTable)}.asset";

        private static ReferenceablesTable instance;
        private static ReferenceablesTable Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!Exists(out instance))
                        instance = CreateTable();
                }

                return instance;
            }
        }

        [SerializeField][GUIDisabled] private SerializableDictionary<string, string> guidToPathTable = new();
        public static SerializableDictionary<string, string> Table => Instance.guidToPathTable;

        private static bool Exists(out ReferenceablesTable table)
        {
#if UNITY_EDITOR
            table = AssetDatabase.LoadAssetAtPath<ReferenceablesTable>(AssetPathToThis);
#else
            table = Resources.Load<ReferenceablesTable>(AssetPathToThis);
#endif
            return table != null;
        }
        
        private static ReferenceablesTable CreateTable()
        {
            ReferenceablesTable table = ScriptableObject.CreateInstance<ReferenceablesTable>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: "Resources");
            AssetDatabase.CreateAsset(table, $"Assets/Resources/{nameof(ReferenceablesTable)}.asset");
            SetDirtySaveAndRefresh();
            return table;
        }
        
        public static bool TryGetValue(string guid, out string pathInsideResources)
        {
            return Instance.guidToPathTable.TryGetValue(guid, out pathInsideResources);
        }

        public static bool Remove(string guid)
        {
            return Instance.guidToPathTable.Remove(guid);
        }

        public static bool ContainsGuid(string guid) => Instance.guidToPathTable.ContainsKey(guid);

        public static bool ContainsContainer(ScriptableReferenceContainer container)
        {
            return Instance.guidToPathTable.ContainsValue(Referenceables.ConvertAssetPathToResourcesPath(AssetDatabase.GetAssetPath(container)));
        }

#if UNITY_EDITOR
        internal static void SetDirtySaveAndRefresh()
        {
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static bool IsValidEntry(ReferenceableScriptable scriptable) => Instance.IsValidEntryNonStatic(scriptable);
        private bool IsValidEntryNonStatic(ReferenceableScriptable scriptable)
        {
            if (!guidToPathTable.TryGetValue(scriptable.Guid, out string pathInsideResources))
            {
                return false;
            }

            var container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>(Referenceables.GetContainerAssetPath(pathInsideResources));
            if (container == null || container.Reference == null)
            {
                return false;
            }

            return container.Reference == scriptable;
        }
        
        internal static void Add(ReferenceableScriptable scriptable) => Instance.AddNonStatic(scriptable);
        private void AddNonStatic(ReferenceableScriptable scriptable)
        {
            string guid = scriptable.Guid;
            string path = Referenceables.GetResourcesContainerPath(scriptable.GetType(), scriptable.Guid);
            
            guidToPathTable.Add(guid, path);
        }

        internal static void Remove(ReferenceableScriptable rs) => Instance.RemoveNonStatic(rs);
        private void RemoveNonStatic(ReferenceableScriptable scriptable)
        {
            // Search all containers and remove any that refer back to this scriptable. 
            bool dirty = false;
            dirty |= Referenceables.DeleteContainersContaining(scriptable);
            dirty |= guidToPathTable.Remove(scriptable.Guid);
            
            if (dirty)
            {
                SetDirtySaveAndRefresh();
            }
        }
#endif
    }
}