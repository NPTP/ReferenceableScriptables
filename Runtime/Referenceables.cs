using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.ReferenceableScriptables.Utilities;
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
            if (!ReferenceablesTable.TryLoad(guid, out var referenceableScriptable))
            {
                scriptable = null;
                return false;
            }

            scriptable = referenceableScriptable as T;
            return scriptable != null;
        }

        #region Editor

#if UNITY_EDITOR
        public static string GetAssetsFolderPath(Type scriptableType)
        {
            return $"Assets/Resources/Referenceables/{scriptableType.Name}";
        }

        public static string GetAssetsContainerPath(Type scriptableType, string containerName)
        {
            return $"Assets/Resources/{GetResourcesContainerPath(scriptableType, containerName)}.asset";
        }

        private static string GetResourcesContainerPath(Type scriptableType, string containerName)
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
#endif

        #endregion
    }
}