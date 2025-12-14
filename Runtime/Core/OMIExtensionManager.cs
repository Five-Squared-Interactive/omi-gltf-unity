// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Central manager for registering and accessing OMI extension handlers.
    /// Allows custom implementations to be registered for each extension type.
    /// </summary>
    public class OMIExtensionManager
    {
        private readonly Dictionary<string, object> _handlers = new Dictionary<string, object>(16);
        private readonly Dictionary<Type, object> _handlersByInterface = new Dictionary<Type, object>(16);
        private readonly List<object> _allHandlers = new List<object>(16);
        
        // Cached handler lists to avoid allocation on each call
        private List<object> _cachedNodeHandlers;
        private List<object> _cachedDocumentHandlers;
        private bool _handlerCacheDirty = true;

        /// <summary>
        /// Gets all registered handler extension names.
        /// </summary>
        public IEnumerable<string> RegisteredExtensions => _handlers.Keys;

        /// <summary>
        /// Gets all registered handlers.
        /// </summary>
        public IReadOnlyList<object> AllHandlers => _allHandlers;

        /// <summary>
        /// Registers a handler for a specific extension.
        /// </summary>
        /// <typeparam name="TData">The extension data type.</typeparam>
        /// <param name="handler">The handler instance.</param>
        public void RegisterHandler<TData>(IOMIExtensionHandler<TData> handler) where TData : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var extensionName = handler.ExtensionName;
            if (string.IsNullOrEmpty(extensionName))
                throw new ArgumentException("Handler must have a valid ExtensionName", nameof(handler));

            _handlers[extensionName] = handler;
            _handlersByInterface[typeof(IOMIExtensionHandler<TData>)] = handler;
            
            // Check if handler already exists using direct iteration (no LINQ)
            bool exists = false;
            for (int i = 0; i < _allHandlers.Count; i++)
            {
                if (ReferenceEquals(_allHandlers[i], handler))
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                _allHandlers.Add(handler);
            }

            // Sort by priority (higher first) - in-place sort
            _allHandlers.Sort(CompareHandlerPriority);
            
            // Mark caches as dirty
            _handlerCacheDirty = true;

            if (Application.isEditor)
            {
                Debug.Log($"[OMI] Registered handler for extension: {extensionName}");
            }
        }

        /// <summary>
        /// Unregisters a handler for a specific extension.
        /// </summary>
        /// <param name="extensionName">The extension name to unregister.</param>
        public void UnregisterHandler(string extensionName)
        {
            if (_handlers.TryGetValue(extensionName, out var handler))
            {
                _handlers.Remove(extensionName);
                _allHandlers.Remove(handler);
                
                // Find and remove from interface dictionary without LINQ
                var keysToRemove = new List<Type>(4);
                foreach (var kvp in _handlersByInterface)
                {
                    if (ReferenceEquals(kvp.Value, handler))
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    _handlersByInterface.Remove(keysToRemove[i]);
                }
                
                _handlerCacheDirty = true;
            }
        }

        /// <summary>
        /// Gets a handler for a specific extension name.
        /// </summary>
        /// <typeparam name="TData">The extension data type.</typeparam>
        /// <param name="extensionName">The extension name.</param>
        /// <returns>The handler, or null if not registered.</returns>
        public IOMIExtensionHandler<TData> GetHandler<TData>(string extensionName) where TData : class
        {
            if (_handlers.TryGetValue(extensionName, out var handler) && handler is IOMIExtensionHandler<TData> typedHandler)
            {
                return typedHandler;
            }
            return null;
        }

        /// <summary>
        /// Gets a handler by its interface type.
        /// </summary>
        /// <typeparam name="THandler">The handler interface type.</typeparam>
        /// <returns>The handler, or null if not registered.</returns>
        public THandler GetHandler<THandler>() where THandler : class
        {
            if (_handlersByInterface.TryGetValue(typeof(THandler), out var handler))
            {
                return handler as THandler;
            }
            
            // Try to find by scanning all handlers (no LINQ)
            for (int i = 0; i < _allHandlers.Count; i++)
            {
                if (_allHandlers[i] is THandler typed)
                {
                    _handlersByInterface[typeof(THandler)] = _allHandlers[i];
                    return typed;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Gets all node extension handlers (cached).
        /// </summary>
        public IReadOnlyList<object> GetNodeHandlers()
        {
            RebuildCachesIfNeeded();
            return _cachedNodeHandlers;
        }

        /// <summary>
        /// Gets all document extension handlers (cached).
        /// </summary>
        public IReadOnlyList<object> GetDocumentHandlers()
        {
            RebuildCachesIfNeeded();
            return _cachedDocumentHandlers;
        }

        /// <summary>
        /// Checks if a handler for the given extension is registered.
        /// </summary>
        public bool HasHandler(string extensionName)
        {
            return _handlers.ContainsKey(extensionName);
        }

        /// <summary>
        /// Clears all registered handlers.
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
            _handlersByInterface.Clear();
            _allHandlers.Clear();
            _cachedNodeHandlers?.Clear();
            _cachedDocumentHandlers?.Clear();
            _handlerCacheDirty = true;
        }

        /// <summary>
        /// Creates a new manager with default Unity handlers registered.
        /// </summary>
        public static OMIExtensionManager CreateWithDefaults()
        {
            var manager = new OMIExtensionManager();
            OMIDefaultHandlers.RegisterAll(manager);
            return manager;
        }

        private void RebuildCachesIfNeeded()
        {
            if (!_handlerCacheDirty)
                return;

            _cachedNodeHandlers ??= new List<object>(8);
            _cachedDocumentHandlers ??= new List<object>(8);
            
            _cachedNodeHandlers.Clear();
            _cachedDocumentHandlers.Clear();

            for (int i = 0; i < _allHandlers.Count; i++)
            {
                var handler = _allHandlers[i];
                if (IsNodeHandler(handler))
                {
                    _cachedNodeHandlers.Add(handler);
                }
                if (IsDocumentHandler(handler))
                {
                    _cachedDocumentHandlers.Add(handler);
                }
            }

            _handlerCacheDirty = false;
        }

        private static int CompareHandlerPriority(object a, object b)
        {
            return GetHandlerPriority(b).CompareTo(GetHandlerPriority(a));
        }

        // Cache for handler priority lookups
        private static readonly Dictionary<Type, int> s_PriorityCache = new Dictionary<Type, int>(32);

        private static int GetHandlerPriority(object handler)
        {
            var type = handler.GetType();
            
            if (s_PriorityCache.TryGetValue(type, out var cachedPriority))
            {
                return cachedPriority;
            }
            
            var property = type.GetProperty("Priority");
            int priority = 0;
            if (property != null)
            {
                priority = (int)property.GetValue(handler);
            }
            
            s_PriorityCache[type] = priority;
            return priority;
        }

        // Cache for handler type checks
        private static readonly Dictionary<Type, bool> s_NodeHandlerCache = new Dictionary<Type, bool>(32);
        private static readonly Dictionary<Type, bool> s_DocumentHandlerCache = new Dictionary<Type, bool>(32);
        private static readonly Type s_NodeHandlerType = typeof(IOMINodeExtensionHandler<>);
        private static readonly Type s_DocumentHandlerType = typeof(IOMIDocumentExtensionHandler<>);

        private static bool IsNodeHandler(object handler)
        {
            var type = handler.GetType();
            
            if (s_NodeHandlerCache.TryGetValue(type, out var cached))
            {
                return cached;
            }
            
            var interfaces = type.GetInterfaces();
            bool isNodeHandler = false;
            for (int i = 0; i < interfaces.Length; i++)
            {
                var iface = interfaces[i];
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == s_NodeHandlerType)
                {
                    isNodeHandler = true;
                    break;
                }
            }
            
            s_NodeHandlerCache[type] = isNodeHandler;
            return isNodeHandler;
        }

        private static bool IsDocumentHandler(object handler)
        {
            var type = handler.GetType();
            
            if (s_DocumentHandlerCache.TryGetValue(type, out var cached))
            {
                return cached;
            }
            
            var interfaces = type.GetInterfaces();
            bool isDocHandler = false;
            for (int i = 0; i < interfaces.Length; i++)
            {
                var iface = interfaces[i];
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == s_DocumentHandlerType)
                {
                    isDocHandler = true;
                    break;
                }
            }
            
            s_DocumentHandlerCache[type] = isDocHandler;
            return isDocHandler;
        }
    }
}

