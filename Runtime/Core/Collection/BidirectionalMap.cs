using System.Collections;
using System.Collections.Generic;

namespace Hypnos.Core.Collection
{
    public class BidirectionalMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> keyMap;
        private readonly Dictionary<TValue, TKey> valueMap;

        public int Count => keyMap.Count;

        public TValue this[TKey key]
        {
            get => keyMap[key];
            set => keyMap[key] = value;
        }

        public TKey this[TValue key]
        {
            get => valueMap[key];
            set => valueMap[key] = value;
        }

        public BidirectionalMap()
        {
            keyMap = new Dictionary<TKey, TValue>();
            valueMap = new Dictionary<TValue, TKey>();
        }

        public BidirectionalMap(int capacity)
        {
            keyMap = new Dictionary<TKey, TValue>(capacity);
            valueMap = new Dictionary<TValue, TKey>(capacity);
        }

        public void Clear()
        {
            keyMap.Clear();
            valueMap.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return keyMap.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return keyMap.GetEnumerator();
        }

        public bool ContainsKey(TKey key)
        {
            return keyMap.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return valueMap.ContainsKey(value);
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return valueMap.TryGetValue(value, out key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return keyMap.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value)
        {
            keyMap.Add(key, value);
            valueMap.Add(value, key);
        }

        public bool Remove(TKey key)
        {
            if (keyMap.TryGetValue(key, out TValue value))
            {
                keyMap.Remove(key);
                valueMap.Remove(value);
                return true;
            }
            return false;
        }

        public bool Remove(TValue value)
        {
            if (valueMap.TryGetValue(value, out TKey key))
            {
                keyMap.Remove(key);
                valueMap.Remove(value);
                return true;
            }
            return false;
        }
    }
}