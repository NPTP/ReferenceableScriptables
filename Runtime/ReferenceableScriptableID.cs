using System;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    [Serializable]
    public struct ReferenceableScriptableID<T> where T : ScriptableObject
    {
        [SerializeField] private string value;

        public ReferenceableScriptableID(string value)
        {
            this.value = value;
        }
        
        public static ReferenceableScriptableID<T> Invalid => new(string.Empty);
        public bool TryLoad(out T scriptableObject) => ReferenceablesTable.TryLoad(value, out scriptableObject);
    }
}
