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
        private static string ResourcesFolder(Type rsType) => $"ScriptableReferenceContainers/{rsType.Name}";
        private static string AssetPath(Type rsType, string guid) => $"Assets/Resources/{ResourcesFolder(rsType)}/{guid}.asset";
        
        internal static void MakeReferenceable(ReferenceableScriptable rs)
        {
            return;
            
            // TODO: Steps.
            // 1. Check if ReferenceablesTable exists. If not, create it.
            // 2. Check that ReferenceablesTables matches rs's information. If so, return early.
            // 3. Create the container, hook it up to rs.
            // 4. Update the ReferenceablesTable for the new rs.
            
            ScriptableReferenceContainer container = rs.EDITOR_ReferenceContainer;
            if (rs.EDITOR_ReferenceContainer != null)
            {
                // TODO: Resources path goes into "database"
                // rs.EDITOR_SetResourcesPath(AssetDatabase.GetAssetPath(container));
                return;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(rs, out string guid, out long _))
            {
                return;
            }

            rs.EDITOR_SetGuid(guid);
            container = ScriptableObject.CreateInstance<ScriptableReferenceContainer>();
            CreatePath($"Assets/Resources/{ResourcesFolder(rs.GetType())}");
            Debug.Log($"Creating new {nameof(ScriptableReferenceContainer)} at {AssetPath(rs.GetType(), rs.Guid)}.", container);
            container.EDITOR_SetScriptableObjectReference(rs);
            rs.EDITOR_ReferenceContainer = container;
            
            // TODO: Resources path goes into "database"
            // rs.EDITOR_SetResourcesPath(AssetPath(rs.GetType(), rs.Guid));
            
            AssetDatabase.CreateAsset(container, AssetPath(rs.GetType(), rs.Guid));
            AssetDatabase.SaveAssets();
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

        internal static void RemoveReferenceable(ReferenceableScriptable rs)
        {
            // Search all containers and remove any that refer back to this rs. 
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ScriptableReferenceContainer)}", new[] { "Assets/Resources" });
            foreach (string guid in guids)
            {
                var container = AssetDatabase.LoadAssetAtPath<ScriptableReferenceContainer>(guid);
                if (container != null && container.Reference == rs)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                }
            }

            // Remove the rs guid entry from the ReferenceablesTable if it exists.
            ReflectionUtility.TryInvokeStaticMethod(typeof(ReferenceablesTable), "Remove", rs.Guid);
        }
    }
}