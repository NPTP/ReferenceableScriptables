using System;
using System.Collections.Generic;
using System.IO;
using NPTP.ReferenceableScriptables.Attributes;
using NPTP.ReferenceableScriptables.Utilities;
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

#if UNITY_EDITOR
        private void SetDirtySaveAndRefresh()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static string GetContainerAssetPath(string pathInsideResources)
        {
            return $"Assets/Resources/{pathInsideResources}.asset";
        }

        private static string ConvertAssetPathToResourcesPath(string assetPath)
        {
            return assetPath
                .Remove(assetPath.LastIndexOf(".asset", StringComparison.Ordinal))
                .Remove(0, "Assets/Resources/".Length);
        }

        private static string GetContainingFolderFromAssetPath(string assetPath)
        {
            string folder = assetPath;
            folder = folder.Remove(folder.LastIndexOf('/'));
            folder = folder.Remove(0, folder.LastIndexOf('/') + 1);
            return folder;
        }

        public static void Clean() => Instance.CleanNonStatic();
        private void CleanNonStatic()
        {
            bool dirty = false;
            
            dirty |= FixContainerPaths();
            dirty |= DeleteEmptyAndUnreferencedContainers();
            dirty |= DeleteEmptyFolders();
            dirty |= RemoveDeadTableEntries();

            if (dirty)
            {
                SetDirtySaveAndRefresh();
            }

            Debug.Log($"Referenceables Table cleaned.");
        }

        private bool FixContainerPaths()
        {
            bool dirty = false;
            
            List<ScriptableReferenceContainer> containers = GetContainers();
            foreach (ScriptableReferenceContainer container in containers)
            {
                string containingFolder = GetContainingFolderFromAssetPath(AssetDatabase.GetAssetPath(container));

                if (container.Reference != null && containingFolder != container.Reference.GetType().Name)
                {
                    ReferenceableScriptable scriptable = container.Reference;
                    Referenceables.CreatePath(Referenceables.GetAssetsFolderPath(scriptable.GetType()));
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(container), Referenceables.GetAssetsContainerPath(scriptable.GetType(), container.name));
                    dirty = true;
                }
            }

            return dirty;
        }

        internal static bool TryLoad(string guid, out ReferenceableScriptable scriptable) => Instance.TryLoadScriptableNonStatic(guid, out scriptable);
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

        public static bool IsValidEntry(ReferenceableScriptable scriptable) => Instance.IsValidEntryNonStatic(scriptable);
        private bool IsValidEntryNonStatic(ReferenceableScriptable scriptable)
        {
            if (!guidToPathTable.TryGetValue(scriptable.Guid, out string pathInsideResources))
            {
                return false;
            }

            var container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>(GetContainerAssetPath(pathInsideResources));
            if (container == null || container.Reference == null)
            {
                return false;
            }

            return container.Reference == scriptable;
        }

        private static ReferenceablesTable Create()
        {
            ReferenceablesTable table = CreateInstance<ReferenceablesTable>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: "Resources");
            AssetDatabase.CreateAsset(table, $"Assets/Resources/{nameof(ReferenceablesTable)}.asset");
            Instance.SetDirtySaveAndRefresh();
            return table;
        }
        
        private static string GetResourcesContainerPath(Type scriptableType, string containerName)
        {
            return $"Referenceables/{scriptableType.Name}/{containerName}";
        }

        internal static void Add(ReferenceableScriptable scriptable) => Instance.AddNonStatic(scriptable);
        private void AddNonStatic(ReferenceableScriptable scriptable)
        {
            string guid = scriptable.Guid;
            string path = GetResourcesContainerPath(scriptable.GetType(), scriptable.Guid);
            
            guidToPathTable.Add(guid, path);
        }

        internal static void Remove(ReferenceableScriptable rs) => Instance.RemoveNonStatic(rs);
        private void RemoveNonStatic(ReferenceableScriptable scriptable)
        {
            // Search all containers and remove any that refer back to this scriptable. 
            bool dirty = false;
            dirty |= DeleteContainersContaining(scriptable);
            dirty |= guidToPathTable.Remove(scriptable.Guid);
            
            if (dirty)
            {
                SetDirtySaveAndRefresh();
            }
        }

        private List<ScriptableReferenceContainer> GetContainers()
        {
            List<ScriptableReferenceContainer> containers = new();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ScriptableReferenceContainer)}", new[] { "Assets/Resources" });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableReferenceContainer container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>(assetPath);
                if (container != null)
                {
                    containers.Add(container);
                }
            }

            return containers;
        }

        private bool DeleteContainersContaining(ReferenceableScriptable scriptable)
        {
            bool deleted = false;
            
            foreach (ScriptableReferenceContainer container in GetContainers())
            {
                if (container.Reference == scriptable)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(container));
                    deleted = true;
                }
            }

            return deleted;
        }
        
        private bool DeleteEmptyAndUnreferencedContainers()
        {
            bool deleted = false;
            
            foreach (ScriptableReferenceContainer container in GetContainers())
            {
                if (container.Reference == null ||
                    !guidToPathTable.ContainsKey(container.Reference.Guid) ||
                    !guidToPathTable.ContainsValue(ConvertAssetPathToResourcesPath(AssetDatabase.GetAssetPath(container))))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(container));
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
        
        private bool RemoveDeadTableEntries()
        {
            bool dirty = false;
            List<string> keysToRemove = new();
            foreach (KeyValueCombo<string, string> combo in guidToPathTable)
            {
                string containerPath = GetContainerAssetPath(pathInsideResources: combo.Value);
                var container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>(containerPath);
                if (container == null)
                {
                    keysToRemove.Add(combo.Key);
                    dirty = true;
                }
            }

            foreach (string key in keysToRemove)
            {
                guidToPathTable.Remove(key);
            }

            return dirty;
        }
#endif
    }
}