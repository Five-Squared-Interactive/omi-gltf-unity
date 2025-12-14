// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OMI
{
    /// <summary>
    /// Provides batched processing utilities for handling multiple nodes efficiently.
    /// </summary>
    public static class OMIBatchProcessor
    {
        /// <summary>
        /// Default batch size for processing.
        /// </summary>
        public const int DefaultBatchSize = 32;

        /// <summary>
        /// Processes items in batches, yielding control back to Unity between batches.
        /// This prevents frame hitches when processing large hierarchies.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">Items to process.</param>
        /// <param name="processor">Action to process each item.</param>
        /// <param name="batchSize">Number of items per batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task ProcessInBatchesAsync<T>(
            IReadOnlyList<T> items,
            Action<T> processor,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            if (items == null || items.Count == 0)
                return;

            int count = items.Count;
            int processed = 0;

            while (processed < count)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int batchEnd = Math.Min(processed + batchSize, count);
                for (int i = processed; i < batchEnd; i++)
                {
                    processor(items[i]);
                }

                processed = batchEnd;

                // Yield to prevent frame hitches
                if (processed < count)
                {
                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// Processes items in batches with index, yielding control back to Unity between batches.
        /// </summary>
        public static async Task ProcessInBatchesAsync<T>(
            IReadOnlyList<T> items,
            Action<T, int> processor,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            if (items == null || items.Count == 0)
                return;

            int count = items.Count;
            int processed = 0;

            while (processed < count)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int batchEnd = Math.Min(processed + batchSize, count);
                for (int i = processed; i < batchEnd; i++)
                {
                    processor(items[i], i);
                }

                processed = batchEnd;

                if (processed < count)
                {
                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// Processes items in batches with async processor.
        /// </summary>
        public static async Task ProcessInBatchesAsync<T>(
            IReadOnlyList<T> items,
            Func<T, Task> processor,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            if (items == null || items.Count == 0)
                return;

            int count = items.Count;
            int processed = 0;

            while (processed < count)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int batchEnd = Math.Min(processed + batchSize, count);
                for (int i = processed; i < batchEnd; i++)
                {
                    await processor(items[i]);
                }

                processed = batchEnd;

                if (processed < count)
                {
                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// Processes a hierarchy in batches, depth-first.
        /// </summary>
        /// <param name="root">Root transform.</param>
        /// <param name="processor">Action to process each transform with its node index.</param>
        /// <param name="batchSize">Number of nodes per batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task ProcessHierarchyInBatchesAsync(
            Transform root,
            Action<Transform, int> processor,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            var stack = new Stack<Transform>(64);
            stack.Push(root);

            int nodeIndex = 0;
            int batchCount = 0;

            while (stack.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var current = stack.Pop();
                processor(current, nodeIndex++);
                batchCount++;

                // Push children in reverse order
                int childCount = current.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }

                // Yield after batch
                if (batchCount >= batchSize)
                {
                    batchCount = 0;
                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// Processes extension data for all nodes in batches.
        /// </summary>
        /// <typeparam name="TData">Extension data type.</typeparam>
        /// <param name="nodeDataPairs">List of (nodeIndex, data) pairs to process.</param>
        /// <param name="context">Import context.</param>
        /// <param name="processor">Function to process each node's extension data.</param>
        /// <param name="batchSize">Batch size.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task ProcessNodeExtensionsAsync<TData>(
            IReadOnlyList<(int nodeIndex, TData data)> nodeDataPairs,
            OMIImportContext context,
            Func<int, TData, GameObject, Task<bool>> processor,
            int batchSize = DefaultBatchSize,
            CancellationToken cancellationToken = default)
            where TData : class
        {
            if (nodeDataPairs == null || nodeDataPairs.Count == 0)
                return;

            int count = nodeDataPairs.Count;
            int processed = 0;

            while (processed < count)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int batchEnd = Math.Min(processed + batchSize, count);
                for (int i = processed; i < batchEnd; i++)
                {
                    var (nodeIndex, data) = nodeDataPairs[i];
                    var gameObject = context.GetGameObject(nodeIndex);
                    if (gameObject != null && data != null)
                    {
                        await processor(nodeIndex, data, gameObject);
                    }
                }

                processed = batchEnd;

                if (processed < count)
                {
                    await Task.Yield();
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for batch processing.
    /// </summary>
    public static class OMIBatchExtensions
    {
        /// <summary>
        /// Processes a list in batches asynchronously.
        /// </summary>
        public static Task ProcessInBatchesAsync<T>(
            this IReadOnlyList<T> items,
            Action<T> processor,
            int batchSize = OMIBatchProcessor.DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            return OMIBatchProcessor.ProcessInBatchesAsync(items, processor, batchSize, cancellationToken);
        }

        /// <summary>
        /// Processes a list in batches asynchronously with async processor.
        /// </summary>
        public static Task ProcessInBatchesAsync<T>(
            this IReadOnlyList<T> items,
            Func<T, Task> processor,
            int batchSize = OMIBatchProcessor.DefaultBatchSize,
            CancellationToken cancellationToken = default)
        {
            return OMIBatchProcessor.ProcessInBatchesAsync(items, processor, batchSize, cancellationToken);
        }
    }
}
