using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechCosmos.FactionForge.Runtime
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get => dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
            set => dictionary[key] = value;
        }

        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
        public void Remove(TKey key) => dictionary.Remove(key);
        public int Count => dictionary.Count;

        // 实现枚举器
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var pair in dictionary)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            for (int i = 0; i < keys.Count; i++)
            {
                if (i < values.Count && !dictionary.ContainsKey(keys[i]))
                {
                    dictionary[keys[i]] = values[i];
                }
            }
        }

        public void Clear()
        {
            dictionary.Clear();
            keys.Clear();
            values.Clear();
        }

        // 添加Keys和Values属性以便于访问
        public Dictionary<TKey, TValue>.KeyCollection Keys => dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => dictionary.Values;
    }
}
