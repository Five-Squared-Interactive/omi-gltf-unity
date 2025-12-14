// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Caches component lookups to avoid repeated GetComponent calls.
    /// Scoped to a single import/export operation.
    /// </summary>
    public class OMIComponentCache : IDisposable
    {
        private readonly Dictionary<(GameObject, Type), Component> _cache = new Dictionary<(GameObject, Type), Component>(64);
        private readonly Dictionary<GameObject, Dictionary<Type, Component>> _multiTypeCache = new Dictionary<GameObject, Dictionary<Type, Component>>(32);
        private bool _disposed;

        /// <summary>
        /// Gets a component, using cached value if available.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="gameObject">GameObject to get component from.</param>
        /// <returns>The component, or null if not found.</returns>
        public T GetComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null) return null;

            var key = (gameObject, typeof(T));
            if (_cache.TryGetValue(key, out var cached))
            {
                return cached as T;
            }

            var component = gameObject.GetComponent<T>();
            _cache[key] = component;
            return component;
        }

        /// <summary>
        /// Tries to get a component, using cached value if available.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="gameObject">GameObject to get component from.</param>
        /// <param name="component">The component if found.</param>
        /// <returns>True if component was found.</returns>
        public bool TryGetComponent<T>(GameObject gameObject, out T component) where T : Component
        {
            component = GetComponent<T>(gameObject);
            return component != null;
        }

        /// <summary>
        /// Gets or adds a component.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="gameObject">GameObject to get/add component on.</param>
        /// <returns>The existing or new component.</returns>
        public T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null) return null;

            var key = (gameObject, typeof(T));
            if (_cache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached as T;
            }

            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            
            _cache[key] = component;
            return component;
        }

        /// <summary>
        /// Checks if a GameObject has a component (cached).
        /// </summary>
        public bool HasComponent<T>(GameObject gameObject) where T : Component
        {
            return GetComponent<T>(gameObject) != null;
        }

        /// <summary>
        /// Pre-caches multiple component types for a GameObject.
        /// Useful when you know you'll need several components from the same object.
        /// </summary>
        public void PreCache(GameObject gameObject, params Type[] componentTypes)
        {
            if (gameObject == null) return;

            foreach (var type in componentTypes)
            {
                var key = (gameObject, type);
                if (!_cache.ContainsKey(key))
                {
                    _cache[key] = gameObject.GetComponent(type);
                }
            }
        }

        /// <summary>
        /// Invalidates the cache for a specific GameObject.
        /// Call after adding/removing components.
        /// </summary>
        public void Invalidate(GameObject gameObject)
        {
            if (gameObject == null) return;

            // Remove all cached components for this GameObject
            var keysToRemove = new List<(GameObject, Type)>();
            foreach (var key in _cache.Keys)
            {
                if (key.Item1 == gameObject)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            _multiTypeCache.Remove(gameObject);
        }

        /// <summary>
        /// Clears the entire cache.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _multiTypeCache.Clear();
        }

        /// <summary>
        /// Gets the number of cached entries.
        /// </summary>
        public int CacheSize => _cache.Count;

        public void Dispose()
        {
            if (!_disposed)
            {
                Clear();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Extension methods for component caching.
    /// </summary>
    public static class OMIComponentCacheExtensions
    {
        /// <summary>
        /// Gets a component using the provided cache.
        /// </summary>
        public static T GetComponentCached<T>(this GameObject gameObject, OMIComponentCache cache) where T : Component
        {
            return cache?.GetComponent<T>(gameObject) ?? gameObject.GetComponent<T>();
        }

        /// <summary>
        /// Gets or adds a component using the provided cache.
        /// </summary>
        public static T GetOrAddComponentCached<T>(this GameObject gameObject, OMIComponentCache cache) where T : Component
        {
            return cache?.GetOrAddComponent<T>(gameObject) ?? GetOrAddComponentDirect<T>(gameObject);
        }

        private static T GetOrAddComponentDirect<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
