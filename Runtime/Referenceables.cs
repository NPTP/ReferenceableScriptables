using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPTP.ReferenceableScriptables.AssetTypes;
using NPTP.ReferenceableScriptables.Utilities;
using NPTP.ReferenceableScriptables.Utilities.Collections;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    /// <summary>
    /// Top class for runtime usage of Referenceable Scriptables.
    /// </summary>
    public static class Referenceables
    {
        /// <summary>
        /// Try to load a Referenceable Scriptable at the given guid address.
        /// Note that there is no need to unload what is loaded - the scriptable reference stays on the stack.
        /// </summary>
        public static bool TryLoad<T>(string guid, out T scriptable) where T : ReferenceableScriptable
        {
            scriptable = null;
            
            if (!ReferenceablesTable.TryGetValue(guid, out string pathInsideResources))
            {
                return false;
            }

            ReferenceableScriptable referenceableScriptable = null;
            
            var container = Resources.Load<ScriptableReferenceContainer>(pathInsideResources);
            if (container != null)
            {
                referenceableScriptable = container.Reference;
                Resources.UnloadAsset(container);
            }

            scriptable = referenceableScriptable as T;
            return scriptable != null;
        }
        
        #region Editor
#if UNITY_EDITOR
        internal static string GetAssetsFolderPath(Type scriptableType)
        {
            return $"Assets/Resources/Referenceables/{scriptableType.Name}";
        }

        internal static string GetAssetsContainerPath(Type scriptableType, string containerName)
        {
            return $"Assets/Resources/{GetResourcesContainerPath(scriptableType, containerName)}.asset";
        }

        internal static string GetResourcesContainerPath(Type scriptableType, string containerName)
        {
            return $"Referenceables/{scriptableType.Name}/{containerName}";
        }

        public static void MakeReferenceable(ReferenceableScriptable scriptable)
        {
            if (ReferenceablesTable.IsValidEntry(scriptable))
            {
                return;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(scriptable, out string guid, out long _))
            {
                return;
            }

            Type scriptableType = scriptable.GetType();
            ReflectionUtility.SetSerializedField(scriptable, "guid", guid);
            ScriptableReferenceContainer container = ScriptableObject.CreateInstance<ScriptableReferenceContainer>();
            ReflectionUtility.SetSerializedField(container, "reference", scriptable);
            CreatePath(GetAssetsFolderPath(scriptableType));
            ReflectionUtility.InvokeStaticMethod<ReferenceablesTable>("Add", scriptable);
            AssetDatabase.CreateAsset(container, GetAssetsContainerPath(scriptableType, guid));

            EditorUtility.SetDirty(scriptable);
            EditorUtility.SetDirty(container);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveReferenceable(ReferenceableScriptable scriptable)
        {
            ReflectionUtility.InvokeStaticMethod<ReferenceablesTable>("Remove", scriptable);
        }

        public static void CreatePath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/"))
            {
                Debug.LogError("Path didn't start with 'Assets/'. Can't create folders");
                return;
            }

            List<string> folders = assetPath.Split('/').ToList();
            int count = folders.Count;
            if (count == 1) return;
            for (int i = 0; i < count - 1; i++)
            {
                string concat = folders[0] + '/' + folders[1];
                if (!AssetDatabase.IsValidFolder(concat))
                {
                    AssetDatabase.CreateFolder(parentFolder: folders[0], newFolderName: folders[1]);
                }

                folders[0] = concat;
                folders.RemoveAt(1);
            }
        }


        internal static string GetContainerAssetPath(string pathInsideResources)
        {
            return $"Assets/Resources/{pathInsideResources}.asset";
        }

        internal static string ConvertAssetPathToResourcesPath(string assetPath)
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

        public static void Clean()
        {
            bool dirty = false;
            
            dirty |= FixContainerPaths();
            dirty |= DeleteEmptyAndUnreferencedContainers();
            dirty |= DeleteEmptyFolders();
            dirty |= RemoveDeadTableEntries();

            if (dirty)
            {
                ReferenceablesTable.SetDirtySaveAndRefresh();
            }

            Debug.Log($"Referenceables Table cleaned.");
        }

        private static bool FixContainerPaths()
        {
            bool dirty = false;
            
            List<ScriptableReferenceContainer> containers = GetContainers();
            foreach (ScriptableReferenceContainer container in containers)
            {
                string containingFolder = GetContainingFolderFromAssetPath(AssetDatabase.GetAssetPath(container));

                if (container.Reference != null && containingFolder != container.Reference.GetType().Name)
                {
                    ReferenceableScriptable scriptable = container.Reference;
                    CreatePath(GetAssetsFolderPath(scriptable.GetType()));
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(container), GetAssetsContainerPath(scriptable.GetType(), container.name));
                    dirty = true;
                }
            }

            return dirty;
        }

        private static List<ScriptableReferenceContainer> GetContainers()
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

        internal static bool DeleteContainersContaining(ReferenceableScriptable scriptable)
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
        
        private static bool DeleteEmptyAndUnreferencedContainers()
        {
            bool deleted = false;
            
            foreach (ScriptableReferenceContainer container in GetContainers())
            {
                if (container.Reference == null ||
                    !ReferenceablesTable.ContainsGuid(container.Reference.Guid) ||
                    !ReferenceablesTable.ContainsContainer(container))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(container));
                    deleted = true;
                }
            }

            return deleted;
        }

        private static bool DeleteEmptyFolders()
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
        
        private static bool RemoveDeadTableEntries()
        {
            bool dirty = false;
            List<string> keysToRemove = new();
            foreach (KeyValueCombo<string, string> combo in ReferenceablesTable.Table)
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
                ReferenceablesTable.Remove(key);
            }

            return dirty;
        }
#endif

        #endregion
    }
}