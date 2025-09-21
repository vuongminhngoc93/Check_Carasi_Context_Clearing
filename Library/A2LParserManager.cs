using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// HIGH-PERFORMANCE A2L Parser Manager with caching and batch operations
    /// Similar to ExcelParserManager but for A2L files
    /// Provides cached A2L parsers for fast repeated lookups
    /// </summary>
    public static class A2LParserManager
    {
        // CACHE: Thread-safe A2L parser cache
        private static readonly ConcurrentDictionary<string, A2LParser> _parsers = new ConcurrentDictionary<string, A2LParser>();
        private static readonly ConcurrentDictionary<string, DateTime> _lastModified = new ConcurrentDictionary<string, DateTime>();
        
        // PERFORMANCE: Cache statistics
        public static int CachedParsersCount => _parsers.Count;
        public static IEnumerable<string> CachedFiles => _parsers.Keys;
        
        /// <summary>
        /// PERFORMANCE: Get cached A2L parser or create new one
        /// Automatically handles file modification detection and cache invalidation
        /// </summary>
        public static A2LParser GetParser(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;
                
            string normalizedPath = Path.GetFullPath(filePath);
            DateTime currentModified = File.GetLastWriteTime(normalizedPath);
            
            // CHECK CACHE: Return cached parser if file hasn't changed
            if (_parsers.TryGetValue(normalizedPath, out A2LParser cachedParser) &&
                _lastModified.TryGetValue(normalizedPath, out DateTime cachedModified) &&
                cachedModified >= currentModified &&
                cachedParser.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"A2L CACHE HIT: {Path.GetFileName(filePath)}");
                return cachedParser;
            }
            
            // CREATE NEW: Parse A2L file and cache result
            try
            {
                var parser = new A2LParser(normalizedPath);
                if (parser.IsValid)
                {
                    _parsers[normalizedPath] = parser;
                    _lastModified[normalizedPath] = currentModified;
                    System.Diagnostics.Debug.WriteLine($"A2L CACHE MISS: Parsed {Path.GetFileName(filePath)} in {parser.ParseTime.TotalMilliseconds:F0}ms");
                    System.Diagnostics.Debug.WriteLine($"  Measurements: {parser.TotalMeasurements:N0}, Characteristics: {parser.TotalCharacteristics:N0}");
                    return parser;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"A2L PARSE ERROR: {Path.GetFileName(filePath)} - {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// BATCH OPERATIONS: Search multiple variables across A2L file
        /// </summary>
        public static Dictionary<string, A2LSearchResult> BatchSearchVariables(string filePath, List<string> variables)
        {
            var results = new Dictionary<string, A2LSearchResult>();
            
            if (variables == null || !variables.Any())
                return results;
                
            var parser = GetParser(filePath);
            if (parser == null)
            {
                // Return "not found" results for all variables
                foreach (var variable in variables)
                {
                    results[variable] = new A2LSearchResult { VariableName = variable, Found = false };
                }
                return results;
            }
            
            // PERFORMANCE: Batch search using cached parser
            return parser.FindVariables(variables);
        }
        
        /// <summary>
        /// PATTERN SEARCH: Find variables matching pattern
        /// </summary>
        public static List<string> FindVariablesByPattern(string filePath, string pattern)
        {
            var parser = GetParser(filePath);
            return parser?.FindVariablesByPattern(pattern) ?? new List<string>();
        }
        
        /// <summary>
        /// SEARCH SINGLE: Find single variable with structured result
        /// </summary>
        public static A2LSearchResult FindVariable(string filePath, string variableName)
        {
            var parser = GetParser(filePath);
            return parser?.FindVariable(variableName) ?? new A2LSearchResult 
            { 
                VariableName = variableName, 
                Found = false 
            };
        }
        
        /// <summary>
        /// CACHE MANAGEMENT: Clear specific file from cache
        /// </summary>
        public static void ClearCache(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            
            string normalizedPath = Path.GetFullPath(filePath);
            _parsers.TryRemove(normalizedPath, out _);
            _lastModified.TryRemove(normalizedPath, out _);
            
            System.Diagnostics.Debug.WriteLine($"A2L CACHE CLEARED: {Path.GetFileName(filePath)}");
        }
        
        /// <summary>
        /// CACHE MANAGEMENT: Clear all cached parsers
        /// </summary>
        public static void ClearAllCache()
        {
            int clearedCount = _parsers.Count;
            _parsers.Clear();
            _lastModified.Clear();
            
            System.Diagnostics.Debug.WriteLine($"A2L CACHE: Cleared {clearedCount} cached parsers");
        }
        
        /// <summary>
        /// CACHE MANAGEMENT: Remove stale cache entries
        /// </summary>
        public static void CleanupStaleCache()
        {
            var staleKeys = new List<string>();
            
            foreach (var kvp in _lastModified)
            {
                string filePath = kvp.Key;
                DateTime cachedModified = kvp.Value;
                
                if (!File.Exists(filePath) || File.GetLastWriteTime(filePath) > cachedModified)
                {
                    staleKeys.Add(filePath);
                }
            }
            
            foreach (string staleKey in staleKeys)
            {
                _parsers.TryRemove(staleKey, out _);
                _lastModified.TryRemove(staleKey, out _);
            }
            
            if (staleKeys.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"A2L CACHE: Cleaned up {staleKeys.Count} stale entries");
            }
        }
        
        /// <summary>
        /// DIAGNOSTICS: Get cache statistics
        /// </summary>
        public static string GetCacheStats()
        {
            return $"A2L Cache: {_parsers.Count} parsers cached, " +
                   $"Files: [{string.Join(", ", _parsers.Keys.Select(Path.GetFileName))}]";
        }
    }
}
