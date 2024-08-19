using System.Collections.Generic;
using System.Linq;
using NPTP.ReferenceableScriptables.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.ReferenceableScriptables
{
    public abstract class ReferenceableScriptable : ScriptableObject
    {
    }

    public abstract class ReferenceableScriptable<T> : ReferenceableScriptable where T : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] [GUIDisabled] private ScriptableReferenceContainer referenceContainer;
#endif
        
        [SerializeField] [GUIDisabled] private string thisGuid;

        [SerializeField] [GUIDisabled] private string resourcesPath;
        public string ResourcesPath => resourcesPath;
        
        private string ResourcesFolder => $"ScriptableReferenceContainers/{typeof(T).Name}";
        private string AssetPath => $"Assets/Resources/{ResourcesFolder}/{thisGuid}.asset";

        public static bool TryLoad<TScriptable>(string guid, out TScriptable referenceableScriptable)
            where TScriptable : ReferenceableScriptable<TScriptable>
        {
            var container = Resources.Load<ScriptableReferenceContainer>(GetResourcesPath<TScriptable>(guid));
            Debug.Log($"Trying to load {GetResourcesPath<TScriptable>(guid)}");
            if (container == null || container.Reference is not TScriptable tScriptable)
            {
                referenceableScriptable = null;
                return false;
            }

            referenceableScriptable = tScriptable;
            container.Loads++;
            return true;
        }

        public static void Unload(ReferenceableScriptable<T> referenceableScriptable)
        {
            if (referenceableScriptable == null ||
                !ScriptableReferenceContainer.IsContainerLoaded(referenceableScriptable.thisGuid))
            {
                return;
            }


            var container = ScriptableReferenceContainer.GetByGUID(referenceableScriptable.thisGuid);
            if (container != null)
            {
                container.Loads--;
            }
        }

        private static string GetResourcesPath<T>(string guid)
        {
            return $"ScriptableReferenceContainers/{typeof(T).Name}/{guid}";
        }

        private void OnDestroy()
        {
            Unload(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (referenceContainer != null)
            {
                resourcesPath = AssetDatabase.GetAssetPath(referenceContainer);
                return;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out string guid, out long _))
            {
                return;
            }

            thisGuid = guid;
            ScriptableReferenceContainer container = CreateInstance<ScriptableReferenceContainer>();
            CreatePath($"Assets/Resources/{ResourcesFolder}");
            Debug.Log($"Creating new {nameof(ScriptableReferenceContainer)} at {AssetPath}.", container);
            container.EDITOR_SetScriptableObjectReference(this);
            referenceContainer = container;
            resourcesPath = AssetPath;
            AssetDatabase.CreateAsset(container, AssetPath);
            AssetDatabase.SaveAssets();
        }

        private void CreatePath(string fullPath)
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
#endif
    }
}