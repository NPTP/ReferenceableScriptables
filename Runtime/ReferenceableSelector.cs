using System;
using NPTP.ReferenceableScriptables.AssetTypes;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    [Serializable]
    public class Referenceable<T> where T : ReferenceableScriptable
    {
        [SerializeField] protected string guid;
        
        public bool TryLoad(out T scriptable)
        {
            if (string.IsNullOrEmpty(guid))
            {
                scriptable = default;
                return false;
            }

            return Referenceables.TryLoad(guid, out scriptable);
        }
    }
}
