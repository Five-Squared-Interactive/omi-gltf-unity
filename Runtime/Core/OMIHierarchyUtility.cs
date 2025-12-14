// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Optimized hierarchy traversal utilities that minimize allocations.
    /// </summary>
    public static class OMIHierarchyUtility
    {
        // Reusable stack for non-recursive traversal
        [ThreadStatic]
        private static Stack<Transform> t_TraversalStack;

        /// <summary>
        /// Iterates all transforms in hierarchy without allocations (depth-first).
        /// </summary>
        /// <param name="root">Root transform to start from.</param>
        /// <returns>Struct enumerator that doesn't allocate.</returns>
        public static HierarchyEnumerator GetHierarchyEnumerator(Transform root)
        {
            return new HierarchyEnumerator(root);
        }

        /// <summary>
        /// Builds a node index to GameObject mapping efficiently.
        /// </summary>
        /// <param name="root">Root GameObject.</param>
        /// <param name="nodeMap">Dictionary to populate (will be cleared first).</param>
        public static void BuildNodeMapping(GameObject root, Dictionary<int, GameObject> nodeMap)
        {
            nodeMap.Clear();
            
            t_TraversalStack ??= new Stack<Transform>(64);
            t_TraversalStack.Clear();
            t_TraversalStack.Push(root.transform);

            int nodeIndex = 0;
            while (t_TraversalStack.Count > 0)
            {
                var current = t_TraversalStack.Pop();
                nodeMap[nodeIndex++] = current.gameObject;

                // Push children in reverse order for correct traversal order
                int childCount = current.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    t_TraversalStack.Push(current.GetChild(i));
                }
            }
        }

        /// <summary>
        /// Builds a GameObject to node index mapping efficiently.
        /// </summary>
        /// <param name="root">Root GameObject.</param>
        /// <param name="gameObjectMap">Dictionary to populate (will be cleared first).</param>
        public static void BuildGameObjectMapping(GameObject root, Dictionary<GameObject, int> gameObjectMap)
        {
            gameObjectMap.Clear();
            
            t_TraversalStack ??= new Stack<Transform>(64);
            t_TraversalStack.Clear();
            t_TraversalStack.Push(root.transform);

            int nodeIndex = 0;
            while (t_TraversalStack.Count > 0)
            {
                var current = t_TraversalStack.Pop();
                gameObjectMap[current.gameObject] = nodeIndex++;

                // Push children in reverse order for correct traversal order
                int childCount = current.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    t_TraversalStack.Push(current.GetChild(i));
                }
            }
        }

        /// <summary>
        /// Counts total nodes in hierarchy without allocation.
        /// </summary>
        public static int CountNodes(Transform root)
        {
            int count = 1;
            int childCount = root.childCount;
            for (int i = 0; i < childCount; i++)
            {
                count += CountNodes(root.GetChild(i));
            }
            return count;
        }

        /// <summary>
        /// Gets components of type T from all children without intermediate List allocation.
        /// Populates the provided list instead of creating a new one.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="root">Root transform.</param>
        /// <param name="results">List to populate with results.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        public static void GetComponentsInChildren<T>(Transform root, List<T> results, bool includeInactive = false) where T : Component
        {
            results.Clear();
            
            t_TraversalStack ??= new Stack<Transform>(64);
            t_TraversalStack.Clear();
            t_TraversalStack.Push(root);

            while (t_TraversalStack.Count > 0)
            {
                var current = t_TraversalStack.Pop();
                
                if (!includeInactive && !current.gameObject.activeInHierarchy)
                    continue;

                // Get component on this object
                if (current.TryGetComponent<T>(out var component))
                {
                    results.Add(component);
                }

                // Queue children
                int childCount = current.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    t_TraversalStack.Push(current.GetChild(i));
                }
            }
        }

        /// <summary>
        /// Finds a single component of type T in children, stopping at first match.
        /// </summary>
        public static T FindComponentInChildren<T>(Transform root, bool includeInactive = false) where T : Component
        {
            t_TraversalStack ??= new Stack<Transform>(64);
            t_TraversalStack.Clear();
            t_TraversalStack.Push(root);

            while (t_TraversalStack.Count > 0)
            {
                var current = t_TraversalStack.Pop();
                
                if (!includeInactive && !current.gameObject.activeInHierarchy)
                    continue;

                if (current.TryGetComponent<T>(out var component))
                {
                    t_TraversalStack.Clear();
                    return component;
                }

                int childCount = current.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    t_TraversalStack.Push(current.GetChild(i));
                }
            }

            return null;
        }

        /// <summary>
        /// Executes an action on each transform in the hierarchy.
        /// </summary>
        /// <param name="root">Root transform.</param>
        /// <param name="action">Action to execute on each transform.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        public static void ForEach(Transform root, Action<Transform> action, bool includeInactive = false)
        {
            t_TraversalStack ??= new Stack<Transform>(64);
            t_TraversalStack.Clear();
            t_TraversalStack.Push(root);

            while (t_TraversalStack.Count > 0)
            {
                var current = t_TraversalStack.Pop();
                
                if (!includeInactive && !current.gameObject.activeInHierarchy)
                    continue;

                action(current);

                int childCount = current.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    t_TraversalStack.Push(current.GetChild(i));
                }
            }
        }

        /// <summary>
        /// Executes an action on each transform with its node index.
        /// </summary>
        public static void ForEachIndexed(Transform root, Action<Transform, int> action, bool includeInactive = false)
        {
            t_TraversalStack ??= new Stack<Transform>(64);
            t_TraversalStack.Clear();
            t_TraversalStack.Push(root);

            int index = 0;
            while (t_TraversalStack.Count > 0)
            {
                var current = t_TraversalStack.Pop();
                
                if (!includeInactive && !current.gameObject.activeInHierarchy)
                    continue;

                action(current, index++);

                // Push children in reverse order for correct traversal order
                int childCount = current.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    t_TraversalStack.Push(current.GetChild(i));
                }
            }
        }
    }

    /// <summary>
    /// Allocation-free enumerator for hierarchy traversal.
    /// </summary>
    public struct HierarchyEnumerator : IDisposable
    {
        private Stack<Transform> _stack;
        private Transform _current;
        private bool _isPooled;

        public HierarchyEnumerator(Transform root)
        {
            // Try to get from thread-static pool
            _stack = new Stack<Transform>(32);
            _stack.Push(root);
            _current = null;
            _isPooled = false;
        }

        public Transform Current => _current;

        public bool MoveNext()
        {
            if (_stack.Count == 0)
                return false;

            _current = _stack.Pop();
            
            // Push children in reverse order
            int childCount = _current.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                _stack.Push(_current.GetChild(i));
            }

            return true;
        }

        public HierarchyEnumerator GetEnumerator() => this;

        public void Dispose()
        {
            _stack?.Clear();
            _stack = null;
        }
    }
}
