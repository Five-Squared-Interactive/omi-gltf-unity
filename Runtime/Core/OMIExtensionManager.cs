// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Central manager for registering and accessing OMI extension handlers.
    /// Allows custom implementations to be registered for each extension type.
    /// </summary>
    public class OMIExtensionManager
    {
        private readonly Dictionary<string, object> _handlers = new Dictionary<string, object>();
        private readonly Dictionary<Type, object> _handlersByInterface = new Dictionary<Type, object>();
        private readonly List<object> _allHandlers = new List<object>();

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
            
            if (!_allHandlers.Contains(handler))
            {
                _allHandlers.Add(handler);
            }

            // Sort by priority (higher first)
            _allHandlers.Sort((a, b) =>
            {
                var priorityA = GetHandlerPriority(a);
                var priorityB = GetHandlerPriority(b);
                return priorityB.CompareTo(priorityA);
            });

            Debug.Log($"[OMI] Registered handler for extension: {extensionName}");
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
                
                // Find and remove from interface dictionary
                var toRemove = _handlersByInterface.Where(kvp => kvp.Value == handler).Select(kvp => kvp.Key).ToList();
                foreach (var key in toRemove)
                {
                    _handlersByInterface.Remove(key);
                }
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
            
            // Try to find by scanning all handlers
            foreach (var h in _allHandlers)
            {
                if (h is THandler typed)
                {
                    _handlersByInterface[typeof(THandler)] = h;
                    return typed;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Gets all node extension handlers.
        /// </summary>
        public IEnumerable<object> GetNodeHandlers()
        {
            return _allHandlers.Where(h => IsNodeHandler(h));
        }

        /// <summary>
        /// Gets all document extension handlers.
        /// </summary>
        public IEnumerable<object> GetDocumentHandlers()
        {
            return _allHandlers.Where(h => IsDocumentHandler(h));
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

        private static int GetHandlerPriority(object handler)
        {
            var type = handler.GetType();
            var property = type.GetProperty("Priority");
            if (property != null)
            {
                return (int)property.GetValue(handler);
            }
            return 0;
        }

        private static bool IsNodeHandler(object handler)
        {
            var type = handler.GetType();
            return type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IOMINodeExtensionHandler<>));
        }

        private static bool IsDocumentHandler(object handler)
        {
            var type = handler.GetType();
            return type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IOMIDocumentExtensionHandler<>));
        }
    }
}
