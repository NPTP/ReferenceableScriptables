#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Utilities.Editor
{
    internal static class Paths
    {
        private const string ASSETS_FOLDER = "Assets/";
        private const string ASSETS_RESOURCES_FOLDER = ASSETS_FOLDER + "Resources/";
        private const string REFERENCEABLES_FOLDER = "Referenceables/";
        private const string ASSET_EXTENSION = ".asset";

        internal static string GetAssetsFolderPath(Type scriptableType)
        {
            return $"{ASSETS_RESOURCES_FOLDER}{REFERENCEABLES_FOLDER}{scriptableType.Name}";
        }

        internal static string GetAssetsContainerPath(Type scriptableType, string containerName)
        {
            return $"{ASSETS_RESOURCES_FOLDER}{GetResourcesContainerPath(scriptableType, containerName)}{ASSET_EXTENSION}";
        }

        internal static string GetResourcesContainerPath(Type scriptableType, string containerName)
        {
            return $"{REFERENCEABLES_FOLDER}{scriptableType.Name}/{containerName}";
        }
        
        internal static void CreatePath(string assetPath)
        {
            if (!assetPath.StartsWith(ASSETS_FOLDER))
            {
                Debug.LogError($"Path didn't start with '{ASSETS_FOLDER}'. Can't create folders");
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

        internal static string GetAssetPathFromResourcesPath(string resourcesPath)
        {
            return $"{ASSETS_RESOURCES_FOLDER}{resourcesPath}{ASSET_EXTENSION}";
        }

        internal static string GetContainingFolderFromAssetPath(string assetPath)
        {
            string folder = assetPath;
            folder = folder.Remove(folder.LastIndexOf('/'));
            folder = folder.Remove(0, folder.LastIndexOf('/') + 1);
            return folder;
        }
        
        internal static string GetReferenceablesTablePathValue(ScriptableObject scriptableObject)
        {
            return ConvertAssetPathToResourcesPath(AssetDatabase.GetAssetPath(scriptableObject));
        }
        
        private static string ConvertAssetPathToResourcesPath(string assetPath)
        {
            return assetPath
                .Remove(assetPath.LastIndexOf(ASSET_EXTENSION, StringComparison.Ordinal))
                .Remove(0, ASSETS_RESOURCES_FOLDER.Length);
        }
    }
}
#endif
