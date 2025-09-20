using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// PERFORMANCE OPTIMIZATION: Static manager for Excel_Parser instances
    /// Provides cached, reusable parsers to reduce initialization overhead
    /// Target: Reduce search time from 1.5s to <1s through instance pooling
    /// </summary>
    public static class ExcelParserManager
    {
        // CACHING: Static instance pool with file path keys
        private static readonly ConcurrentDictionary<string, Excel_Parser> ParserPool = 
            new ConcurrentDictionary<string, Excel_Parser>();
        private static readonly ConcurrentDictionary<string, DateTime> LastAccessTime = 
            new ConcurrentDictionary<string, DateTime>();
        private static readonly object PoolLock = new object();
        
        // CACHING: Pool configuration
        private const int MAX_POOL_SIZE = 8; // Limit concurrent parsers
        private static readonly TimeSpan IDLE_TIMEOUT = TimeSpan.FromMinutes(10); // Keep parsers alive longer
        
        // CACHING: Cleanup timer
        private static System.Threading.Timer CleanupTimer;
        
        static ExcelParserManager()
        {
            // Initialize cleanup timer to run every 5 minutes
            CleanupTimer = new System.Threading.Timer(CleanupIdleParsers, null, 
                TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// PERFORMANCE: Get or create Excel_Parser with caching
        /// Returns cached instance if available, creates new one if needed
        /// </summary>
        public static Excel_Parser GetParser(string filePath, DataTable template)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new ArgumentException($"Invalid file path: {filePath}");
                
            // Normalize file path for consistent caching
            string normalizedPath = Path.GetFullPath(filePath).ToLowerInvariant();
            
            // Update access time immediately
            LastAccessTime.AddOrUpdate(normalizedPath, DateTime.Now, (key, oldValue) => DateTime.Now);
            
            // Try to get from cache first
            if (ParserPool.TryGetValue(normalizedPath, out Excel_Parser cachedParser))
            {
                // Validate cached parser is still valid
                if (IsParserValid(cachedParser, filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Using cached Excel_Parser for: {Path.GetFileName(filePath)}");
                    return cachedParser;
                }
                else
                {
                    // Remove invalid parser
                    RemoveParser(normalizedPath);
                }
            }
            
            // Create new parser if not cached or invalid
            lock (PoolLock)
            {
                // Double-check after lock
                if (ParserPool.TryGetValue(normalizedPath, out cachedParser) && 
                    IsParserValid(cachedParser, filePath))
                {
                    return cachedParser;
                }
                
                // Check pool size and cleanup if needed
                if (ParserPool.Count >= MAX_POOL_SIZE)
                {
                    CleanupOldestParser();
                }
                
                // Create new parser
                try
                {
                    var newParser = new Excel_Parser(filePath, template);
                    ParserPool.TryAdd(normalizedPath, newParser);
                    System.Diagnostics.Debug.WriteLine($"Created new cached Excel_Parser for: {Path.GetFileName(filePath)}");
                    return newParser;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create Excel_Parser for {filePath}: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// PERFORMANCE: Batch processing with parser reuse
        /// Processes multiple variables efficiently using cached parsers
        /// </summary>
        public static Dictionary<string, bool> BatchCheckCarasi(string filePath, List<string> variables, DataTable template)
        {
            var parser = GetParser(filePath, template);
            return parser._IsExist_Carasi_Batch(variables);
        }

        /// <summary>
        /// PERFORMANCE: Batch processing for Dataflow with parser reuse
        /// </summary>
        public static Dictionary<string, bool> BatchCheckDataflow(string filePath, List<string> variables, DataTable template)
        {
            var parser = GetParser(filePath, template);
            return parser._IsExist_Dataflow_Batch(variables);
        }

        /// <summary>
        /// CACHE MANAGEMENT: Validate parser is still usable
        /// </summary>
        private static bool IsParserValid(Excel_Parser parser, string filePath)
        {
            try
            {
                // Check if parser object is valid
                if (parser == null)
                    return false;
                    
                // Check if file still exists
                if (!File.Exists(filePath))
                    return false;
                    
                // Check if parser's internal file path matches
                if (parser.Lb_NameOfFile != Path.GetFileName(filePath))
                    return false;
                    
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// CACHE MANAGEMENT: Remove specific parser from pool
        /// </summary>
        private static void RemoveParser(string normalizedPath)
        {
            try
            {
                if (ParserPool.TryRemove(normalizedPath, out Excel_Parser parser))
                {
                    parser?.Dispose();
                    LastAccessTime.TryRemove(normalizedPath, out _);
                    System.Diagnostics.Debug.WriteLine($"Removed parser from cache: {normalizedPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing parser: {ex.Message}");
            }
        }

        /// <summary>
        /// CACHE MANAGEMENT: Remove oldest parser to make room
        /// </summary>
        private static void CleanupOldestParser()
        {
            try
            {
                var oldestEntry = LastAccessTime
                    .OrderBy(kvp => kvp.Value)
                    .FirstOrDefault();
                    
                if (!string.IsNullOrEmpty(oldestEntry.Key))
                {
                    RemoveParser(oldestEntry.Key);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during oldest parser cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// CACHE MANAGEMENT: Timer callback to cleanup idle parsers
        /// </summary>
        private static void CleanupIdleParsers(object state)
        {
            try
            {
                var cutoffTime = DateTime.Now - IDLE_TIMEOUT;
                var idleKeys = LastAccessTime
                    .Where(kvp => kvp.Value < cutoffTime)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                foreach (string key in idleKeys)
                {
                    RemoveParser(key);
                }
                
                if (idleKeys.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Cleaned up {idleKeys.Count} idle Excel_Parser instances");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during idle parser cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// CACHE MANAGEMENT: Clear all cached parsers
        /// </summary>
        public static void ClearAll()
        {
            lock (PoolLock)
            {
                try
                {
                    var allKeys = ParserPool.Keys.ToList();
                    foreach (string key in allKeys)
                    {
                        RemoveParser(key);
                    }
                    
                    // Clear Excel_Parser static cache as well
                    Excel_Parser.ClearQueryCache();
                    
                    System.Diagnostics.Debug.WriteLine("Cleared all cached Excel_Parser instances");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error clearing parser cache: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// MONITORING: Get cache statistics for performance monitoring
        /// </summary>
        public static Dictionary<string, object> GetCacheStatistics()
        {
            return new Dictionary<string, object>
            {
                ["ActiveParsers"] = ParserPool.Count,
                ["MaxPoolSize"] = MAX_POOL_SIZE,
                ["IdleTimeout"] = IDLE_TIMEOUT.ToString(),
                ["CachedFiles"] = ParserPool.Keys.Select(k => Path.GetFileName(k)).ToList(),
                ["LastAccess"] = LastAccessTime.Values.Any() ? LastAccessTime.Values.Max() : DateTime.MinValue,
                ["ExcelParserCacheSize"] = Excel_Parser.GetCacheSize()
            };
        }

        /// <summary>
        /// PERFORMANCE: Preload commonly used files during application startup
        /// </summary>
        public static void PreloadFiles(List<string> filePaths, DataTable template)
        {
            if (filePaths == null || filePaths.Count == 0)
                return;
                
            System.Threading.Tasks.Task.Run(() =>
            {
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            GetParser(filePath, template);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to preload {filePath}: {ex.Message}");
                    }
                }
            });
        }
    }
}
