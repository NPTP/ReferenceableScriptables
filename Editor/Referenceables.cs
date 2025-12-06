using System;
using System.Collections.Generic;
using System.IO;
using NPTP.ReferenceableScriptables.Editor.Utilities;
using NPTP.ReferenceableScriptables.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor
{
    internal static class Referenceables
    {
        private const string CLEAN_ON_PREPROCESS_BUILD_EDITOR_PREFS_KEY = nameof(ReferenceableScriptables) + "_" + nameof(CleanOnPreProcessBuild);

        private static event Action onReferenceablesUpdated;
        internal static event Action OnReferenceablesUpdated
        {
            add
            {
                onReferenceablesUpdated -= value;
                onReferenceablesUpdated += value;
            }
            remove => onReferenceablesUpdated -= value;
        }
        
        internal static bool CleanOnPreProcessBuild
        {
            get => EditorPrefs.GetBool(CLEAN_ON_PREPROCESS_BUILD_EDITOR_PREFS_KEY, true);
            set => EditorPrefs.SetBool(CLEAN_ON_PREPROCESS_BUILD_EDITOR_PREFS_KEY, value);
        }
        
        internal static bool IsValidEntry(ScriptableObject scriptable)
        {
            if (!ReferenceablesTable.TryGetResourcesPathByGuid(scriptable.GetAssetGuid(), out string resourcesPath))
            {
                return false;
            }

            var container = AssetDatabase.LoadAssetAtPath<ReferenceableScriptableContainer>(Paths.GetAssetPathFromResourcesPath(resourcesPath));
            if (container == null || container.Reference == null)
            {
                return false;
            }

            return container.Reference == scriptable;
        }

        internal static void MakeReferenceable(ScriptableObject scriptable, bool referenceable)
        {
            if (referenceable) AddReferenceable(scriptable);
            else RemoveReferenceable(scriptable);
        }

        internal static void Clean()
        {
            bool dirty = false;
            
            dirty |= FixContainerPaths();
            dirty |= DeleteEmptyAndUnreferencedContainers();
            dirty |= DeleteEmptyFolders();
            dirty |= ReferenceablesTable.RemoveDeadEntries();

            if (dirty)
            {
                ReferenceablesTable.SetDirtySaveAndRefresh();
                onReferenceablesUpdated?.Invoke();
            }

            Debug.Log($"Referenceables Table cleaned.");
        }

        #region Add
        
        private static void AddReferenceable(ScriptableObject scriptable)
        {
            if (IsValidEntry(scriptable))
            {
                return;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(scriptable, out string guid, out long _))
            {
                return;
            }

            Type scriptableType = scriptable.GetType();
            ReferenceableScriptableContainer scriptableContainer = ScriptableObject.CreateInstance<ReferenceableScriptableContainer>();
            ReflectionUtility.SetSerializedField(scriptableContainer, "reference", scriptable);
            Paths.CreatePath(Paths.GetAssetsFolderPath(scriptableType));
            
            AddToTable(scriptable);
            
            AssetDatabase.CreateAsset(scriptableContainer, Paths.GetAssetsContainerPath(scriptableType, guid));

            EditorUtility.SetDirty(scriptable);
            EditorUtility.SetDirty(scriptableContainer);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            onReferenceablesUpdated?.Invoke();
        }
        
        private static void AddToTable(ScriptableObject scriptable)
        {
            string name = scriptable.name;
            string guid = scriptable.GetAssetGuid();
            string path = Paths.GetResourcesContainerPath(scriptable.GetType(), scriptable.GetAssetGuid());

            if (ReferenceablesTable.Add(name, guid, path))
            {
                ReferenceablesTable.SetDirtySaveAndRefresh();
            }
        }

        #endregion

        #region Remove

        private static void RemoveReferenceable(ScriptableObject scriptable)
        {
            RemoveFromTable(scriptable);
            
            if (DeleteEmptyFolders())
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            onReferenceablesUpdated?.Invoke();
        }

        private static void RemoveFromTable(ScriptableObject scriptable)
        {
            string name = scriptable.name;
            string guid = scriptable.GetAssetGuid();
            
            bool dirty = false;
            
            dirty |= DeleteAllContainersContaining(scriptable);
            dirty |= ReferenceablesTable.Remove(name, guid);
            
            if (dirty)
            {
                ReferenceablesTable.SetDirtySaveAndRefresh();
            }
        }
        
        #endregion

        private static bool FixContainerPaths()
        {
            bool dirty = false;
            
            List<ReferenceableScriptableContainer> containers = GetContainers();
            foreach (ReferenceableScriptableContainer container in containers)
            {
                string containingFolder = Paths.GetContainingFolderFromAssetPath(AssetDatabase.GetAssetPath(container));

                if (container.Reference != null && containingFolder != container.Reference.GetType().Name)
                {
                    ScriptableObject scriptable = container.Reference;
                    Paths.CreatePath(Paths.GetAssetsFolderPath(scriptable.GetType()));
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(container), Paths.GetAssetsContainerPath(scriptable.GetType(), container.name));
                    dirty = true;
                }
            }

            return dirty;
        }

        private static bool DeleteAllContainersContaining(ScriptableObject scriptable)
        {
            bool deleted = false;
            
            foreach (ReferenceableScriptableContainer container in GetContainers())
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
            
            foreach (ReferenceableScriptableContainer container in GetContainers())
            {
                if (container.Reference == null || !ReferenceablesTable.ContainsEntry(container.Reference.GetAssetGuid(), Paths.GetReferenceablesTablePathValue(container)))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(container));
                    deleted = true;
                }
            }

            return deleted;
        }
        
        private static List<ReferenceableScriptableContainer> GetContainers()
        {
            List<ReferenceableScriptableContainer> containers = new();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ReferenceableScriptableContainer)}", new[] { "Assets/Resources" });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ReferenceableScriptableContainer scriptableContainer = AssetDatabase.LoadAssetAtPath<ReferenceableScriptableContainer>(assetPath);
                if (scriptableContainer != null)
                {
                    containers.Add(scriptableContainer);
                }
            }

            return containers;
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
    }
}