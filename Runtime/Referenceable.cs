using System;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    [Serializable]
    public class Referenceable<T> where T : ScriptableObject
    {
        [SerializeField] protected string guid;
        
        public bool TryLoad(out T scriptable)
        {
            if (string.IsNullOrEmpty(guid))
            {
                scriptable = default;
                return false;
            }

            return ReferenceablesTable.TryLoad(guid, out scriptable);
        }
    }
}
