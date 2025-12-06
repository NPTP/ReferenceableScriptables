using System.Collections.Generic;
using System.Linq;
using NPTP.ReferenceableScriptables.Attributes;
using NPTP.ReferenceableScriptables.Utilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using NPTP.ReferenceableScriptables.Utilities.Editor;
#endif

namespace NPTP.ReferenceableScriptables
{
    internal class ReferenceablesTable : ScriptableObject
    {
        private static string ResourcesPathToThis => nameof(ReferenceablesTable);
        
        [SerializeField][GUIDisabled] private SerializableDictionary<string, string> guidToPathTable = new();
        internal static SerializableDictionary<string, string> GuidToPathTable => Instance.guidToPathTable;

        [SerializeField][GUIDisabled] private SerializableDictionary<string, string[]> nameToGuidsTable = new();
        private static SerializableDictionary<string, string[]> NameToGuidsTable => Instance.nameToGuidsTable;
        
        private static ReferenceablesTable instance;
        private static ReferenceablesTable Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!TryLoad(out instance))
                    {
                        instance = CreateTable();
                    }
                    
                    // Guarantee the table is ready
                    instance.guidToPathTable.OnAfterDeserialize();
                }

                return instance;
            }
        }

        internal static bool Add(string name, string guid, string path)
        {
            if (!GuidToPathTable.TryAdd(guid, path))
            {
                return false;
            }
            
            if (NameToGuidsTable.TryGetValue(name, out var guids))
            {
                guids = guids.WithElementAdded(guid);
            }
            else
            {
                guids = new[] { guid };
            }

            NameToGuidsTable[name] = guids;

            return true;
        }
        
        internal static bool Remove(string name, string guid)
        {
            if (!GuidToPathTable.Remove(guid))
            {
                return false;
            }

            if (!NameToGuidsTable.TryGetValue(name, out var guids))
            {
                return true;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                if (guids[i] == guid)
                {
                    guids = guids.WithRemovedAt(i);
                    break;
                }
            }

            if (guids.Length == 0)
            {
                NameToGuidsTable.Remove(name);
            }
            else
            {
                NameToGuidsTable[name] = guids;
            }

            return true;
        }

        internal static bool ContainsEntry(string guid, string path)
        {
            return GuidToPathTable.ContainsKey(guid) || GuidToPathTable.ContainsValue(path);
        }
        
        internal static bool TryGetResourcesPathByGuid(string guid, out string resourcesPath)
        {
            return GuidToPathTable.TryGetValue(guid, out resourcesPath);
        }
        
        internal static bool TryGetGuidsByName(string name, out string[] guids)
        {
            return NameToGuidsTable.TryGetValue(name, out guids);
        }
        
        /// <summary>
        /// Try to load a Referenceable Scriptable at the given guid address.
        /// Note that there is no need to unload what is loaded - the container is
        /// unloaded and the referenced scriptable object ready for use stays on the stack.
        /// </summary>
        internal static bool TryLoad<T>(string guid, out T scriptable) where T : ScriptableObject
        {
            scriptable = null;
            
            if (!GuidToPathTable.TryGetValue(guid, out string pathInsideResources))
            {
                return false;
            }

            ScriptableObject referenceableScriptable = null;
            
            var container = Resources.Load<ReferenceableScriptableContainer>(pathInsideResources);
            if (container != null)
            {
                referenceableScriptable = container.Reference;
                Resources.UnloadAsset(container);
            }

            scriptable = referenceableScriptable as T;
            return scriptable != null;
        }

        private static bool TryLoad(out ReferenceablesTable table)
        {
#if UNITY_EDITOR
            table = EditorApplication.isPlaying
                ? Resources.Load<ReferenceablesTable>(ResourcesPathToThis)
                : AssetDatabase.LoadAssetAtPath<ReferenceablesTable>(Paths.GetAssetPathFromResourcesPath(ResourcesPathToThis));
#else
            table = Resources.Load<ReferenceablesTable>(ResourcesPathToThis);
#endif
            return table != null;
        }
        
        private static ReferenceablesTable CreateTable()
        {
            ReferenceablesTable table = CreateInstance<ReferenceablesTable>();
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: "Resources");
            }
            
            AssetDatabase.CreateAsset(table, Paths.GetAssetPathFromResourcesPath(ResourcesPathToThis));
            SetDirtySaveAndRefresh(); // TODO: Fix possible recursion?
#endif
            return table;
        }

#if UNITY_EDITOR
        internal static void SetDirtySaveAndRefresh()
        {
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        internal static bool RemoveDeadEntries()
        {
            bool dirty = false;
            List<string> guidsToRemove = new();
            
            foreach (KVP<string, string> guidPathPair in GuidToPathTable)
            {
                string guid = guidPathPair.Key;
                string resourcesPath = guidPathPair.Value;

                string containerPath = Paths.GetAssetPathFromResourcesPath(resourcesPath);
                var container = AssetDatabase.LoadAssetAtPath<ReferenceableScriptableContainer>(containerPath);
                if (container == null)
                {
                    guidsToRemove.Add(guid);
                    dirty = true;
                }
            }

            foreach (string guid in guidsToRemove)
            {
                GuidToPathTable.Remove(guid);
                RemoveGuidFromNameToGuidsTable(guid);
            }

            return dirty;
        }

        private static void RemoveGuidFromNameToGuidsTable(string guid)
        {
            List<string> names = new();
            List<string[]> guids = new();

            foreach (KVP<string, string[]> nameGuidsPair in NameToGuidsTable)
            {
                string name = nameGuidsPair.Key;
                List<string> guidsList = nameGuidsPair.Value.ToList();

                bool changed = false;

                for (int i = 0; i < guidsList.Count;)
                {
                    if (guidsList[i] == guid)
                    {
                        changed = true;
                        guidsList.RemoveAt(i);
                        continue;
                    }

                    i++;
                }

                if (changed)
                {
                    names.Add(name);
                    guids.Add(guidsList.ToArray());
                }
            }

            for (int i = 0; i < names.Count; i++)
            {
                if (guids[i].Length == 0)
                {
                    NameToGuidsTable.Remove(names[i]);
                }
                else
                {
                    NameToGuidsTable[names[i]] = guids[i];
                }
            }
        }
#endif
    }
}