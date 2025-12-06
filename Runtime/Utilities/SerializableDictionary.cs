using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Utilities
{
    /// <summary>
    /// Based on one of Unity's serializable dictionaries for a customizable/maintainable version.
    /// </summary>
    [Serializable]
    internal sealed class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<KVP<TKey, TValue>> keyValuePairs = new();

        private Dictionary<TKey, TValue> internalDictionary = new();

        public ICollection<TKey> Keys => internalDictionary.Keys;
        public ICollection<TValue> Values => internalDictionary.Values;

        public int Count
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    return keyValuePairs.Count;
                }
#endif
                return internalDictionary.Count;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    for (int i = 0; i < keyValuePairs.Count; i++)
                    {
                        KVP<TKey, TValue> kvp = keyValuePairs[i];
                        if (kvp.Key.Equals(key))
                        {
                            return kvp.Value;
                        }
                    }

                    throw new KeyNotFoundException();
                }
#endif

                return internalDictionary[key];
            }
            set
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    for (int i = 0; i < keyValuePairs.Count; i++)
                    {
                        KVP<TKey, TValue> kvp = keyValuePairs[i];
                        if (kvp.Key.Equals(key))
                        {
                            keyValuePairs[i] = new KVP<TKey, TValue>(key, value);
                            return;
                        }
                    }

                    keyValuePairs.Add(new KVP<TKey, TValue>(key, value));
                    return;
                }
#endif
                
                internalDictionary[key] = value;
            }
        }

        internal TKey this[TValue value]
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    for (int i = 0; i < keyValuePairs.Count; i++)
                    {
                        KVP<TKey, TValue> kvp = keyValuePairs[i];
                        if (kvp.Value.Equals(value))
                        {
                            return kvp.Key;
                        }
                    }

                    throw new Exception("Value not found");
                }
#endif
                List<TKey> keys = new(internalDictionary.Keys);
                List<TValue> values = new(internalDictionary.Values);
                int index = values.FindIndex(x => x.Equals(value));
                if (index < 0)
                {
                    throw new Exception("Value not found");
                }

                return keys[index];
            }
        }

        internal void ChangeKey(TValue value, TKey newKey)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
               
                for (int i = 0; i < keyValuePairs.Count; i++)
                {
                    KVP<TKey, TValue> kvp = keyValuePairs[i];
                    if (kvp.Value.Equals(value))
                    {
                        keyValuePairs[i] = new KVP<TKey, TValue>(newKey, value);
                        break;
                    }
                }
                
                return;
            }
#endif
            bool removeKeyHasBeenSet = false;
            TKey removeKey = default;
            foreach (KeyValuePair<TKey, TValue> keyValuePair in internalDictionary)
            {
                if (EqualityComparer<TValue>.Default.Equals(keyValuePair.Value, value))
                {
                    removeKey = keyValuePair.Key;
                    removeKeyHasBeenSet = true;
                }
            }

            if (removeKeyHasBeenSet)
            {
                internalDictionary.Remove(removeKey);
                internalDictionary.Add(newKey, value);
            }
        }

        internal void AddRange(IDictionary<TKey, TValue> items)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                foreach (KeyValuePair<TKey, TValue> pair in items)
                {
                    KVP<TKey, TValue> combo = new KVP<TKey, TValue>(pair.Key, pair.Value);
                    if (!keyValuePairs.Contains(combo))
                    {
                        keyValuePairs.Add(combo);
                    }
                }
                
                return;
            }
#endif
            foreach (TKey key in items.Keys)
            {
                internalDictionary.TryAdd(key, items[key]);
            }
        }

        public void Add(TKey key, TValue value)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                foreach (KVP<TKey, TValue> keyValueCombo in keyValuePairs)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValueCombo.Key, key))
                    {
                        Debug.Log($"Couldn't add {key} because value already exists in dictionary");
                        return;
                    }
                }

                keyValuePairs.Add(new KVP<TKey, TValue>(key, value));
                return;
            }
#endif
            
            internalDictionary.Add(key, value);
        }

        internal bool TryAdd(TKey key, TValue value)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                foreach (KVP<TKey, TValue> keyValueCombo in keyValuePairs)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValueCombo.Key, key))
                    {
                        return false;
                    }
                }

                Add(key, value);
                return true;
            }
#endif
            
            return internalDictionary.TryAdd(key, value);
        }

        public bool ContainsKey(TKey key)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                for (int i = 0; i < keyValuePairs.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValuePairs[i].Key, key))
                    {
                        return true;
                    }
                }

                return false;
            }
#endif
            
            return internalDictionary.ContainsKey(key);
        }
        
        internal bool ContainsValue(TValue value)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                for (int i = 0; i < keyValuePairs.Count; i++)
                {
                    if (EqualityComparer<TValue>.Default.Equals(keyValuePairs[i].Value, value))
                    {
                        return true;
                    }
                }

                return false;
            }
#endif
            
            return internalDictionary.ContainsValue(value);
        }

        public bool Remove(TKey key)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                for (int i = 0; i < keyValuePairs.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValuePairs[i].Key, key))
                    {
                        keyValuePairs.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }
#endif
            return internalDictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                for (int i = 0; i < keyValuePairs.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValuePairs[i].Key, key))
                    {
                        value = keyValuePairs[i].Value;
                        return true;
                    }
                }

                value = default;
                return false;
            }
#endif
            return internalDictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                keyValuePairs.Clear();
                return;
            }
#endif
            internalDictionary.Clear();
        }

        public IEnumerator GetEnumerator()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return keyValuePairs.GetEnumerator();
            }
#endif
            return internalDictionary.GetEnumerator();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            internalDictionary.Clear();
            foreach (KVP<TKey, TValue> keyValuePair in keyValuePairs)
            {
                internalDictionary.TryAdd(keyValuePair.Key, keyValuePair.Value);
            }
        }

        #region Explicit IDictionary Implementations

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly =>
            (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) =>
            (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) =>
            (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) =>
            (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
            (internalDictionary as IEnumerable<KeyValuePair<TKey, TValue>>).GetEnumerator();

        #endregion
    }
}