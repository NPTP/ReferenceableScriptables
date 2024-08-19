using System.Collections.Generic;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    public class ReferenceablesTable : ScriptableObject
    {
        private static ReferenceablesTable instance;
        
        // TODO: Automatically create an instance of this in the top of the Resources folder, whenever there is at least one Referenceable
        internal static ReferenceablesTable Instance => instance;

        [SerializeField] private List<GuidToPath> table = new();

        internal void Add(string guid, string path)
        {
            foreach (GuidToPath guidToPath in table)
            {
                if (guidToPath.Guid == guid)
                {
                    return;
                }
            }
            
            table.Add(new GuidToPath(guid, path));
        }

        public static void Remove(string guid) => Instance.RemoveInternal(guid);
        internal void RemoveInternal(string guid)
        {
            int i = 0;
            while (i < table.Count)
            {
                GuidToPath guidToPath = table[i];
                if (guidToPath != null && guidToPath.Guid == guid)
                {
                    table.RemoveAt(i);
                    continue;
                }

                i++;
            }
        }
    }
}