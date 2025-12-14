// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Generic object pool for reducing allocations during import/export.
    /// Thread-safe for use with async operations.
    /// </summary>
    /// <typeparam name="T">Type of objects to pool.</typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _pool;
        private readonly Action<T> _resetAction;
        private readonly int _maxSize;
        private readonly object _lock = new object();

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="initialCapacity">Initial pool capacity.</param>
        /// <param name="maxSize">Maximum pool size (0 for unlimited).</param>
        /// <param name="resetAction">Optional action to reset objects when returned.</param>
        public ObjectPool(int initialCapacity = 16, int maxSize = 256, Action<T> resetAction = null)
        {
            _pool = new Stack<T>(initialCapacity);
            _maxSize = maxSize;
            _resetAction = resetAction;

            // Pre-warm the pool
            for (int i = 0; i < initialCapacity; i++)
            {
                _pool.Push(new T());
            }
        }

        /// <summary>
        /// Gets an object from the pool or creates a new one.
        /// </summary>
        public T Get()
        {
            lock (_lock)
            {
                return _pool.Count > 0 ? _pool.Pop() : new T();
            }
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;

            _resetAction?.Invoke(item);

            lock (_lock)
            {
                if (_maxSize == 0 || _pool.Count < _maxSize)
                {
                    _pool.Push(item);
                }
            }
        }

        /// <summary>
        /// Clears the pool.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }
    }

    /// <summary>
    /// Pooled list wrapper that returns to pool on dispose.
    /// </summary>
    public struct PooledList<T> : IDisposable
    {
        private List<T> _list;
        private readonly ObjectPool<List<T>> _pool;

        internal PooledList(List<T> list, ObjectPool<List<T>> pool)
        {
            _list = list;
            _pool = pool;
        }

        public List<T> List => _list;
        public int Count => _list.Count;
        public T this[int index] => _list[index];

        public void Add(T item) => _list.Add(item);
        public void AddRange(IEnumerable<T> items) => _list.AddRange(items);
        public void Clear() => _list.Clear();

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        public void Dispose()
        {
            if (_list != null)
            {
                _list.Clear();
                _pool?.Return(_list);
                _list = null;
            }
        }
    }

    /// <summary>
    /// Pooled dictionary wrapper that returns to pool on dispose.
    /// </summary>
    public struct PooledDictionary<TKey, TValue> : IDisposable
    {
        private Dictionary<TKey, TValue> _dict;
        private readonly ObjectPool<Dictionary<TKey, TValue>> _pool;

        internal PooledDictionary(Dictionary<TKey, TValue> dict, ObjectPool<Dictionary<TKey, TValue>> pool)
        {
            _dict = dict;
            _pool = pool;
        }

        public Dictionary<TKey, TValue> Dictionary => _dict;
        public int Count => _dict.Count;
        public TValue this[TKey key]
        {
            get => _dict[key];
            set => _dict[key] = value;
        }

        public void Add(TKey key, TValue value) => _dict.Add(key, value);
        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public void Clear() => _dict.Clear();

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => _dict.GetEnumerator();

        public void Dispose()
        {
            if (_dict != null)
            {
                _dict.Clear();
                _pool?.Return(_dict);
                _dict = null;
            }
        }
    }

    /// <summary>
    /// Central pool manager for OMI operations.
    /// </summary>
    public static class OMIPools
    {
        // List pools
        private static readonly ObjectPool<List<GameObject>> s_GameObjectListPool =
            new ObjectPool<List<GameObject>>(8, 64, list => list.Clear());

        private static readonly ObjectPool<List<Transform>> s_TransformListPool =
            new ObjectPool<List<Transform>>(8, 64, list => list.Clear());

        private static readonly ObjectPool<List<int>> s_IntListPool =
            new ObjectPool<List<int>>(16, 128, list => list.Clear());

        private static readonly ObjectPool<List<string>> s_StringListPool =
            new ObjectPool<List<string>>(8, 64, list => list.Clear());

        private static readonly ObjectPool<List<object>> s_ObjectListPool =
            new ObjectPool<List<object>>(8, 64, list => list.Clear());

        // Dictionary pools
        private static readonly ObjectPool<Dictionary<int, GameObject>> s_NodeMapPool =
            new ObjectPool<Dictionary<int, GameObject>>(4, 32, dict => dict.Clear());

        private static readonly ObjectPool<Dictionary<string, object>> s_ExtensionDataPool =
            new ObjectPool<Dictionary<string, object>>(8, 64, dict => dict.Clear());

        // StringBuilder pool for string operations
        private static readonly ObjectPool<System.Text.StringBuilder> s_StringBuilderPool =
            new ObjectPool<System.Text.StringBuilder>(8, 32, sb => sb.Clear());

        /// <summary>
        /// Gets a pooled list of GameObjects.
        /// </summary>
        public static PooledList<GameObject> GetGameObjectList()
        {
            return new PooledList<GameObject>(s_GameObjectListPool.Get(), s_GameObjectListPool);
        }

        /// <summary>
        /// Gets a pooled list of Transforms.
        /// </summary>
        public static PooledList<Transform> GetTransformList()
        {
            return new PooledList<Transform>(s_TransformListPool.Get(), s_TransformListPool);
        }

        /// <summary>
        /// Gets a pooled list of integers.
        /// </summary>
        public static PooledList<int> GetIntList()
        {
            return new PooledList<int>(s_IntListPool.Get(), s_IntListPool);
        }

        /// <summary>
        /// Gets a pooled list of strings.
        /// </summary>
        public static PooledList<string> GetStringList()
        {
            return new PooledList<string>(s_StringListPool.Get(), s_StringListPool);
        }

        /// <summary>
        /// Gets a pooled list of objects.
        /// </summary>
        public static PooledList<object> GetObjectList()
        {
            return new PooledList<object>(s_ObjectListPool.Get(), s_ObjectListPool);
        }

        /// <summary>
        /// Gets a pooled node-to-GameObject dictionary.
        /// </summary>
        public static PooledDictionary<int, GameObject> GetNodeMap()
        {
            return new PooledDictionary<int, GameObject>(s_NodeMapPool.Get(), s_NodeMapPool);
        }

        /// <summary>
        /// Gets a pooled extension data dictionary.
        /// </summary>
        public static PooledDictionary<string, object> GetExtensionDataMap()
        {
            return new PooledDictionary<string, object>(s_ExtensionDataPool.Get(), s_ExtensionDataPool);
        }

        /// <summary>
        /// Gets a pooled StringBuilder.
        /// </summary>
        public static System.Text.StringBuilder GetStringBuilder()
        {
            return s_StringBuilderPool.Get();
        }

        /// <summary>
        /// Returns a StringBuilder to the pool.
        /// </summary>
        public static void ReturnStringBuilder(System.Text.StringBuilder sb)
        {
            if (sb != null)
            {
                sb.Clear();
                s_StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// Clears all pools. Call on scene unload or when memory pressure is high.
        /// </summary>
        public static void ClearAll()
        {
            s_GameObjectListPool.Clear();
            s_TransformListPool.Clear();
            s_IntListPool.Clear();
            s_StringListPool.Clear();
            s_ObjectListPool.Clear();
            s_NodeMapPool.Clear();
            s_ExtensionDataPool.Clear();
            s_StringBuilderPool.Clear();
        }
    }
}
