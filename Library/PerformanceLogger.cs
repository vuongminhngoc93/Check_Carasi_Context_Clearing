using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// Performance monitoring and logging system for UI optimization
    /// </summary>
    public static class PerformanceLogger
    {
        private static Dictionary<string, Stopwatch> _activeTimers = new Dictionary<string, Stopwatch>();
        private static List<PerformanceMetric> _metrics = new List<PerformanceMetric>();
        private static string _logFilePath;
        private static bool _isEnabled = true;

        static PerformanceLogger()
        {
            string tempPath = Path.GetTempPath();
            _logFilePath = Path.Combine(tempPath, $"UI_Performance_Log_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            InitializeLogFile();
        }

        /// <summary>
        /// Start timing an operation
        /// </summary>
        public static void StartTimer(string operationName, string details = "")
        {
            if (!_isEnabled) return;

            try
            {
                string key = $"{operationName}_{DateTime.Now.Ticks}";
                if (!_activeTimers.ContainsKey(key))
                {
                    var stopwatch = Stopwatch.StartNew();
                    _activeTimers[key] = stopwatch;
                    
                    // Log start event
                    LogEvent("START", operationName, 0, details, GetMemoryUsage(), GetTabCount());
                }
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Stop timing and log the result
        /// </summary>
        public static long StopTimer(string operationName, string details = "")
        {
            if (!_isEnabled) return 0;

            try
            {
                var keysToRemove = new List<string>();
                long elapsedMs = 0;

                foreach (var kvp in _activeTimers)
                {
                    if (kvp.Key.StartsWith(operationName))
                    {
                        kvp.Value.Stop();
                        elapsedMs = kvp.Value.ElapsedMilliseconds;
                        
                        // Create performance metric
                        var metric = new PerformanceMetric
                        {
                            Timestamp = DateTime.Now,
                            OperationName = operationName,
                            ElapsedMs = elapsedMs,
                            Details = details,
                            MemoryUsageMB = GetMemoryUsage(),
                            TabCount = GetTabCount()
                        };
                        
                        _metrics.Add(metric);
                        
                        // Log completion event
                        LogEvent("COMPLETE", operationName, elapsedMs, details, metric.MemoryUsageMB, metric.TabCount);
                        
                        keysToRemove.Add(kvp.Key);
                        break; // Take first matching timer
                    }
                }

                // Cleanup completed timers
                foreach (string key in keysToRemove)
                {
                    _activeTimers.Remove(key);
                }

                return elapsedMs;
            }
            catch 
            { 
                return 0; // Ignore logging errors 
            }
        }

        /// <summary>
        /// Log a single performance measurement
        /// </summary>
        public static void LogDuration(string operationName, long elapsedMs, string details = "")
        {
            if (!_isEnabled) return;

            try
            {
                var metric = new PerformanceMetric
                {
                    Timestamp = DateTime.Now,
                    OperationName = operationName,
                    ElapsedMs = elapsedMs,
                    Details = details,
                    MemoryUsageMB = GetMemoryUsage(),
                    TabCount = GetTabCount()
                };
                
                _metrics.Add(metric);
                LogEvent("DURATION", operationName, elapsedMs, details, metric.MemoryUsageMB, metric.TabCount);
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Get performance summary report
        /// </summary>
        public static string GetPerformanceSummary()
        {
            try
            {
                if (_metrics.Count == 0) return "No performance data available.";

                var sb = new StringBuilder();
                sb.AppendLine("🚀 PERFORMANCE SUMMARY REPORT");
                sb.AppendLine("==================================");
                sb.AppendLine($"📊 Total Operations: {_metrics.Count}");
                sb.AppendLine($"⏱️ Log File: {_logFilePath}");
                sb.AppendLine();

                // Group by operation type
                var grouped = new Dictionary<string, List<PerformanceMetric>>();
                foreach (var metric in _metrics)
                {
                    if (!grouped.ContainsKey(metric.OperationName))
                        grouped[metric.OperationName] = new List<PerformanceMetric>();
                    grouped[metric.OperationName].Add(metric);
                }

                // Performance analysis
                foreach (var group in grouped)
                {
                    var operations = group.Value;
                    var avgTime = operations.Count > 0 ? operations.Sum(m => m.ElapsedMs) / operations.Count : 0;
                    var maxTime = operations.Count > 0 ? operations.Max(m => m.ElapsedMs) : 0;
                    var minTime = operations.Count > 0 ? operations.Min(m => m.ElapsedMs) : 0;

                    sb.AppendLine($"🔧 {group.Key}:");
                    sb.AppendLine($"   ✅ Count: {operations.Count}");
                    sb.AppendLine($"   ⚡ Avg: {avgTime}ms");
                    sb.AppendLine($"   🔥 Max: {maxTime}ms");
                    sb.AppendLine($"   ⚡ Min: {minTime}ms");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch 
            {
                return "Error generating performance summary.";
            }
        }

        /// <summary>
        /// Clear all performance data
        /// </summary>
        public static void ClearMetrics()
        {
            _metrics.Clear();
            _activeTimers.Clear();
        }

        /// <summary>
        /// Enable/disable performance logging
        /// </summary>
        public static void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        /// <summary>
        /// Get current log file path
        /// </summary>
        public static string GetLogFilePath()
        {
            return _logFilePath;
        }

        // Helper methods
        private static void InitializeLogFile()
        {
            try
            {
                string header = "Timestamp,EventType,OperationName,ElapsedMs,Details,MemoryMB,TabCount";
                File.WriteAllText(_logFilePath, header + Environment.NewLine);
            }
            catch { /* Ignore file creation errors */ }
        }

        private static void LogEvent(string eventType, string operationName, long elapsedMs, string details, double memoryMB, int tabCount)
        {
            try
            {
                string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{eventType},{operationName},{elapsedMs},\"{details}\",{memoryMB:F1},{tabCount}";
                File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
            }
            catch { /* Ignore file write errors */ }
        }

        private static double GetMemoryUsage()
        {
            try
            {
                return GC.GetTotalMemory(false) / (1024.0 * 1024.0); // Convert to MB
            }
            catch 
            {
                return 0;
            }
        }

        private static int GetTabCount()
        {
            try
            {
                // Find main form and get tab count
                foreach (Form form in Application.OpenForms)
                {
                    if (form is Form1 mainForm)
                    {
                        var tabControl = form.Controls.Find("tabControl1", true);
                        if (tabControl.Length > 0 && tabControl[0] is TabControl tabs)
                        {
                            return tabs.TabPages.Count;
                        }
                    }
                }
                return 0;
            }
            catch 
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Performance metric data structure
    /// </summary>
    public class PerformanceMetric
    {
        public DateTime Timestamp { get; set; }
        public string OperationName { get; set; }
        public long ElapsedMs { get; set; }
        public string Details { get; set; }
        public double MemoryUsageMB { get; set; }
        public int TabCount { get; set; }
    }

    /// <summary>
    /// Extension methods for easy performance measurement
    /// </summary>
    public static class PerformanceExtensions
    {
        public static long Sum(this IEnumerable<PerformanceMetric> metrics, Func<PerformanceMetric, long> selector)
        {
            long sum = 0;
            foreach (var metric in metrics)
            {
                sum += selector(metric);
            }
            return sum;
        }

        public static long Max(this IEnumerable<PerformanceMetric> metrics, Func<PerformanceMetric, long> selector)
        {
            long max = long.MinValue;
            foreach (var metric in metrics)
            {
                var value = selector(metric);
                if (value > max) max = value;
            }
            return max == long.MinValue ? 0 : max;
        }

        public static long Min(this IEnumerable<PerformanceMetric> metrics, Func<PerformanceMetric, long> selector)
        {
            long min = long.MaxValue;
            foreach (var metric in metrics)
            {
                var value = selector(metric);
                if (value < min) min = value;
            }
            return min == long.MaxValue ? 0 : min;
        }
    }
}
