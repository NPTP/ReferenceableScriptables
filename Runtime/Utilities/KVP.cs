using System;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Utilities
{
    /// <summary>
    /// Serializable version of a KVP (Key Value Pair) struct.
    /// </summary>
    [Serializable]
    internal struct KVP<TKey, TValue>
    {
        [SerializeField] private TKey key;
        internal TKey Key => key;
        
        [SerializeField] private TValue value;
        internal TValue Value => value;
        
        internal KVP(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}