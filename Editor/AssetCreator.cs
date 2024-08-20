using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.ReferenceableScriptables.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor
{
    public static class AssetCreator
    {
        private static string GetAssetsFolderPath(Type scriptableType)
        {
            return $"Assets/Resources/Referenceables/{scriptableType.Name}";
        }
        
        private static string GetAssetsContainerPath(Type scriptableType, string containerName)
        {
            return $"Assets/Resources/{GetResourcesContainerPath(scriptableType, containerName)}.asset";
        }

        private static string GetResourcesContainerPath(Type scriptableType, string containerName)
        {
            return $"Referenceables/{scriptableType.Name}/{containerName}";
        }
        
        internal static void MakeReferenceable(ReferenceableScriptable scriptable)
        {
            if (ReferenceablesTable.IsEntryValid(scriptable))
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
            ReflectionUtility.InvokeStaticMethod<ReferenceablesTable>("Add", guid, GetResourcesContainerPath(scriptableType, guid));
            AssetDatabase.CreateAsset(container, GetAssetsContainerPath(scriptableType, guid));
            AssetDatabase.SaveAssets();
        }
        
        internal static void RemoveReferenceable(ReferenceableScriptable scriptable)
        {
            ReflectionUtility.InvokeStaticMethod<ReferenceablesTable>("Remove", scriptable);
        }

        private static void CreatePath(string fullPath)
        {
            if (!fullPath.StartsWith("Assets/"))
            {
                Debug.LogError("Path didn't start with 'Assets/'. Can't create folders");
                return;
            }

            List<string> folders = fullPath.Split('/').ToList();
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
    }
}