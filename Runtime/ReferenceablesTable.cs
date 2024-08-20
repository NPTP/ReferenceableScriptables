using System.IO;
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
                        instance = Create();
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

        private void OnValidate()
        {
            // Delete any junk containers containing null.
            DeleteContainersContaining(null);
            
            // TODO: cleanup whole table
        }

        internal static bool TryLoad(string guid, out ReferenceableScriptable scriptable) =>
            Instance.TryLoadScriptableNonStatic(guid, out scriptable);
        private bool TryLoadScriptableNonStatic(string guid, out ReferenceableScriptable scriptable)
        {
            scriptable = null;
            
            if (!guidToPathTable.TryGetValue(guid, out string pathInsideResources))
            {
                return false;
            }

            var container = Resources.Load<ScriptableReferenceContainer>(pathInsideResources);
            if (container != null)
            {
                scriptable = container.Reference;
                Resources.UnloadAsset(container);
            }

            return scriptable != null;
        }

        public static bool IsEntryValid(ReferenceableScriptable scriptable) => Instance.IsEntryValidNonStatic(scriptable);
        private bool IsEntryValidNonStatic(ReferenceableScriptable scriptable)
        {
            // Is it in the table?
            if (!guidToPathTable.TryGetValue(scriptable.Guid, out string pathInsideResources))
            {
                return false;
            }
            
#if UNITY_EDITOR
            // Is the container in the right folder?
            if (!pathInsideResources.Remove(0, "Referenceables".Length + 1).StartsWith(scriptable.GetType().Name))
            {
                return false;
            }
            
            // Does the container contain the correct reference?
            var container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>($"Assets/Resources/{pathInsideResources}.asset");
            if (container == null || container.Reference != scriptable)
            {
                return false;
            }
#endif

            return true;
        }

        private static ReferenceablesTable Create()
        {
            ReferenceablesTable table = CreateInstance<ReferenceablesTable>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: "Resources");
            AssetDatabase.CreateAsset(table, $"Assets/Resources/{nameof(ReferenceablesTable)}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return table;
        }

        internal static void Add(string guid, string pathInsideResources) => Instance.AddNonStatic(guid, pathInsideResources);
        private void AddNonStatic(string guid, string path)
        {
            guidToPathTable.Add(guid, path);
        }

        internal static void Remove(ReferenceableScriptable rs) => Instance.RemoveNonStatic(rs);
        private void RemoveNonStatic(ReferenceableScriptable scriptable)
        {
            // Search all containers and remove any that refer back to this scriptable. 
            bool dirty = false;
            dirty |= DeleteContainersContaining(scriptable);
            dirty |= guidToPathTable.Remove(scriptable.Guid);
            dirty |= DeleteEmptyFolders();
            
            if (dirty)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private bool DeleteContainersContaining(ReferenceableScriptable scriptable)
        {
            bool deleted = false;
            
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ScriptableReferenceContainer)}", new[] { "Assets/Resources" });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableReferenceContainer container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>(assetPath);
                if (container != null && container.Reference == scriptable)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    deleted = true;
                }
            }

            return deleted;
        }

        private bool DeleteEmptyFolders()
        {
            bool deleted = false;
            
            char sep = Path.DirectorySeparatorChar;
            string topFolder = $"{Application.dataPath}{sep}Resources{sep}Referenceables";
            if (!Directory.Exists(topFolder))
            {
                return false;
            }
            
            deleteEmptyFoldersRecursive(topFolder);
            
            return deleted;

            void deleteEmptyFoldersRecursive(string folder)
            {
                string[] directories = Directory.GetDirectories(folder);
                foreach (string directory in directories)
                {
                    string[] files = Directory.GetFiles(directory);
                    string[] subDirs = Directory.GetDirectories(directory);
                    if (files.Length == 0 && subDirs.Length == 0)
                    {
                        Directory.Delete(directory);
                        File.Delete($"{directory}.meta");
                        deleted = true;
                    }
                    else
                    {
                        deleteEmptyFoldersRecursive(directory);
                    }
                }
            }
        }
    }
}