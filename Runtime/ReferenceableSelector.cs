using System;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    [Serializable]
    public class ReferenceableSelector
    {
        [SerializeField] protected string guid;
    }

    [Serializable]
    public class Referenceable<T> : ReferenceableSelector where T : ReferenceableScriptable
    {
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
