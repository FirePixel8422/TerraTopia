using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct SerializableKeyValueList<TKey, TValue>
{
    [SerializeField] private TKey[] keys;
    [SerializeField] private TValue[] values;

    [SerializeField] private int size;

    public SerializableKeyValueList(int initialCapacity)
    {
        keys = new TKey[initialCapacity];
        values = new TValue[initialCapacity];
        size = keys.Length;
    }

    public void Add(TKey key, TValue value)
    {
        int count = keys.Length;

        if (size == count)
        {
            // Resize the array if it’s full
            Array.Resize(ref keys, count * 2);
            Array.Resize(ref values, count * 2);
        }

        keys[size++] = key;
        values[size++] = value;
    }

    public bool RemoveAtSwapBack(TKey compareKey)
    {
        for (int i = 0; i < size; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(compareKey, keys[i]))
            {
                keys[i] = keys[size - 1];
                values[i] = values[size - 1];

                size--;
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(TKey compareKey, out TValue value)
    {
        for (int i = 0; i < size; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(compareKey, keys[i]))
            {
                value = values[i];
                return true;
            }
        }

        value = default;
        return false;
    }

    public TKey GetKeyById(int id)
    {
        if (id >= 0 && id < size)
        {
            return keys[id];
        }
        else
        {
            throw new ArgumentOutOfRangeException("id", "Invalid index. The index is out of range.");
        }
    }

    public bool ContainsKey(TKey compareKey)
    {
        for (int i = 0; i < size; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(compareKey, keys[i]))
            {
                return true;
            }
        }

        return false;
    }

    public int Count => size;

    // Serializable Key-Value Pair
    [System.Serializable]
    public struct SerializableKeyValuePair<TK, TV>
    {
        [SerializeField]
        public TK Key;
        [SerializeField]
        public TV Value;

        public SerializableKeyValuePair(TK key, TV value)
        {
            Key = key;
            Value = value;
        }
    }
}
