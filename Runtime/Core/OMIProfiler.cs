// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OMI
{
    /// <summary>
    /// Performance profiling and diagnostics utilities for OMI operations.
    /// </summary>
    public static class OMIProfiler
    {
        /// <summary>
        /// Whether profiling is enabled. Set to false in release builds.
        /// </summary>
        public static bool Enabled { get; set; } = 
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            true;
#else
            false;
#endif

        /// <summary>
        /// Whether to log individual timings.
        /// </summary>
        public static bool LogTimings { get; set; } = false;

        // Timing storage
        private static readonly Dictionary<string, ProfileData> s_Profiles = new Dictionary<string, ProfileData>(32);
        private static readonly object s_Lock = new object();

        /// <summary>
        /// Starts a profiling scope. Use with 'using' statement.
        /// </summary>
        /// <param name="name">Name of the operation being profiled.</param>
        /// <returns>A disposable scope that stops timing when disposed.</returns>
        public static ProfileScope BeginScope(string name)
        {
            return new ProfileScope(name);
        }

        /// <summary>
        /// Records a timing for an operation.
        /// </summary>
        public static void RecordTiming(string name, double milliseconds)
        {
            if (!Enabled) return;

            lock (s_Lock)
            {
                if (!s_Profiles.TryGetValue(name, out var profile))
                {
                    profile = new ProfileData(name);
                    s_Profiles[name] = profile;
                }
                profile.AddSample(milliseconds);
            }

            if (LogTimings)
            {
                Debug.Log($"[OMI Profiler] {name}: {milliseconds:F2}ms");
            }
        }

        /// <summary>
        /// Gets profiling statistics for an operation.
        /// </summary>
        public static ProfileStats GetStats(string name)
        {
            lock (s_Lock)
            {
                if (s_Profiles.TryGetValue(name, out var profile))
                {
                    return profile.GetStats();
                }
            }
            return default;
        }

        /// <summary>
        /// Gets all profiling statistics.
        /// </summary>
        public static Dictionary<string, ProfileStats> GetAllStats()
        {
            var result = new Dictionary<string, ProfileStats>();
            lock (s_Lock)
            {
                foreach (var kvp in s_Profiles)
                {
                    result[kvp.Key] = kvp.Value.GetStats();
                }
            }
            return result;
        }

        /// <summary>
        /// Logs a summary of all profiling data.
        /// </summary>
        public static void LogSummary()
        {
            if (!Enabled) return;

            var stats = GetAllStats();
            if (stats.Count == 0)
            {
                Debug.Log("[OMI Profiler] No profiling data collected.");
                return;
            }

            var sb = OMIPools.GetStringBuilder();
            sb.AppendLine("[OMI Profiler] Summary:");
            sb.AppendLine("Operation                          | Count | Total (ms) | Avg (ms) | Min (ms) | Max (ms)");
            sb.AppendLine("-----------------------------------|-------|------------|----------|----------|----------");

            foreach (var kvp in stats)
            {
                var s = kvp.Value;
                sb.AppendLine($"{kvp.Key,-35}| {s.Count,5} | {s.TotalMs,10:F2} | {s.AverageMs,8:F2} | {s.MinMs,8:F2} | {s.MaxMs,8:F2}");
            }

            Debug.Log(sb.ToString());
            OMIPools.ReturnStringBuilder(sb);
        }

        /// <summary>
        /// Clears all profiling data.
        /// </summary>
        public static void Clear()
        {
            lock (s_Lock)
            {
                s_Profiles.Clear();
            }
        }

        /// <summary>
        /// Profile data collector.
        /// </summary>
        private class ProfileData
        {
            public string Name { get; }
            private double _totalMs;
            private double _minMs = double.MaxValue;
            private double _maxMs;
            private int _count;

            public ProfileData(string name)
            {
                Name = name;
            }

            public void AddSample(double ms)
            {
                _totalMs += ms;
                _count++;
                if (ms < _minMs) _minMs = ms;
                if (ms > _maxMs) _maxMs = ms;
            }

            public ProfileStats GetStats()
            {
                return new ProfileStats
                {
                    Count = _count,
                    TotalMs = _totalMs,
                    AverageMs = _count > 0 ? _totalMs / _count : 0,
                    MinMs = _count > 0 ? _minMs : 0,
                    MaxMs = _maxMs
                };
            }
        }
    }

    /// <summary>
    /// Profiling statistics for an operation.
    /// </summary>
    public struct ProfileStats
    {
        public int Count;
        public double TotalMs;
        public double AverageMs;
        public double MinMs;
        public double MaxMs;
    }

    /// <summary>
    /// Disposable profiling scope.
    /// </summary>
    public struct ProfileScope : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        public ProfileScope(string name)
        {
            _name = name;
            _disposed = false;
            
            if (OMIProfiler.Enabled)
            {
                _stopwatch = Stopwatch.StartNew();
            }
            else
            {
                _stopwatch = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_stopwatch != null)
            {
                _stopwatch.Stop();
                OMIProfiler.RecordTiming(_name, _stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }

    /// <summary>
    /// Import/Export statistics collector.
    /// </summary>
    public class OMIOperationStats
    {
        public int NodesProcessed { get; set; }
        public int ExtensionsProcessed { get; set; }
        public int ComponentsCreated { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public double TotalTimeMs { get; set; }

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public void Start()
        {
            _stopwatch.Restart();
        }

        public void Stop()
        {
            _stopwatch.Stop();
            TotalTimeMs = _stopwatch.Elapsed.TotalMilliseconds;
        }

        public void Reset()
        {
            NodesProcessed = 0;
            ExtensionsProcessed = 0;
            ComponentsCreated = 0;
            Errors = 0;
            Warnings = 0;
            TotalTimeMs = 0;
            _stopwatch.Reset();
        }

        public override string ToString()
        {
            return $"OMI Operation: {NodesProcessed} nodes, {ExtensionsProcessed} extensions, " +
                   $"{ComponentsCreated} components in {TotalTimeMs:F2}ms ({Errors} errors, {Warnings} warnings)";
        }
    }
}
