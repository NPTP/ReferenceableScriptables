using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.ReferenceableScriptables.Utilities.Collections
{
    /// <summary>
    /// Based on one of Unity's serializable dictionaries for a customizable/maintainable version.
    /// </summary>
    [Serializable]
    public sealed class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<KeyValueCombo<TKey, TValue>> keyValueCombos = new();

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
                    return keyValueCombos.Count;
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
                    for (int i = 0; i < keyValueCombos.Count; i++)
                    {
                        KeyValueCombo<TKey, TValue> keyValueCombo = keyValueCombos[i];
                        if (keyValueCombo.Key.Equals(key))
                        {
                            return keyValueCombo.Value;
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
                    for (int i = 0; i < keyValueCombos.Count; i++)
                    {
                        KeyValueCombo<TKey, TValue> keyValueCombo = keyValueCombos[i];
                        if (keyValueCombo.Key.Equals(key))
                        {
                            keyValueCombos[i] = new KeyValueCombo<TKey, TValue>(key, value);
                            return;
                        }
                    }

                    keyValueCombos.Add(new KeyValueCombo<TKey, TValue>(key, value));
                    return;
                }
#endif
                
                internalDictionary[key] = value;
            }
        }

        public TKey this[TValue value]
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    for (int i = 0; i < keyValueCombos.Count; i++)
                    {
                        KeyValueCombo<TKey, TValue> keyValueCombo = keyValueCombos[i];
                        if (keyValueCombo.Value.Equals(value))
                        {
                            return keyValueCombo.Key;
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

        public void ChangeKey(TValue value, TKey newKey)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
               
                for (int i = 0; i < keyValueCombos.Count; i++)
                {
                    KeyValueCombo<TKey, TValue> keyValueCombo = keyValueCombos[i];
                    if (keyValueCombo.Value.Equals(value))
                    {
                        keyValueCombos[i] = new KeyValueCombo<TKey, TValue>(newKey, value);
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

        public void AddRange(IDictionary<TKey, TValue> items)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                List<KeyValueCombo<TKey, TValue>> pairs = new();
                foreach (KeyValuePair<TKey, TValue> pair in items)
                {
                    KeyValueCombo<TKey, TValue> combo = new KeyValueCombo<TKey, TValue>(pair.Key, pair.Value);
                    if (!keyValueCombos.Contains(combo))
                    {
                        keyValueCombos.Add(combo);
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
                foreach (KeyValueCombo<TKey, TValue> keyValueCombo in keyValueCombos)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValueCombo.Key, key))
                    {
                        Debug.Log($"Couldn't add {key} because value already exists in dictionary");
                        return;
                    }
                }

                keyValueCombos.Add(new KeyValueCombo<TKey, TValue>(key, value));
                return;
            }
#endif
            
            internalDictionary.Add(key, value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                foreach (KeyValueCombo<TKey, TValue> keyValueCombo in keyValueCombos)
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
                for (int i = 0; i < keyValueCombos.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValueCombos[i].Key, key))
                    {
                        return true;
                    }
                }

                return false;
            }
#endif
            
            return internalDictionary.ContainsKey(key);
        }
        
        public bool ContainsValue(TValue value)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                for (int i = 0; i < keyValueCombos.Count; i++)
                {
                    if (EqualityComparer<TValue>.Default.Equals(keyValueCombos[i].Value, value))
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
                for (int i = 0; i < keyValueCombos.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValueCombos[i].Key, key))
                    {
                        keyValueCombos.RemoveAt(i);
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
                for (int i = 0; i < keyValueCombos.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(keyValueCombos[i].Key, key))
                    {
                        value = keyValueCombos[i].Value;
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
                keyValueCombos.Clear();
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
                return keyValueCombos.GetEnumerator();
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
            foreach (KeyValueCombo<TKey, TValue> keyValuePair in keyValueCombos)
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