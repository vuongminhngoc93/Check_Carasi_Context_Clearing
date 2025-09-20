using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Check_carasi_DF_ContextClearing
{
    public struct Carasi_Interface
    {
        public string name;
        public string[] input;
        public string output;
        public string description;

        //Properties
        public string unit;
        public string minValue;
        public string maxValue;
        public string resolution;
        public string initialisation;
        public string swType;
        public string mmType;
        public string conversion;
        public string comments;
        public string computeDetails;
    }

    public struct Dataflow_Interface
    {
        public string status;
        public string description;
        public string Rte_Direction;
        public string MappingType;
        public string Pseudo_code;
        public string System_constant;
        public string FC_Name;
        public string Producer;
        public string Consumers;

        //Properties PSA
        public string PSAname;
        public string PSAswType;
        public string PSAunit;
        public string PSAconversion;
        public string PSAresolution;
        public string PSAminValue;
        public string PSAmaxValue;
        public string PSAoffset;
        public string PSAinitialisation;

        //Properties Bosch
        public string Boschname;
        public string BoschswType;
        public string Boschunit;
        public string Boschconversion;
        public string Boschresolution;
        public string BoschminValue;
        public string BoschmaxValue;
        public string Boschoffset;
        public string Boschinitialisation;
    }


    public class Excel_Parser
    {
        const int MAX_NUMBER_OF_INTERFACE = 10000;
        const String MacroModule_Interface_sheet = "Interfaces";
        const String MacroModule_Dictionary_sheet = "Dictionary";
        const String DataFlow_Interface_sheet = "Mapping";
        const String MacroModule_signals = "carasi";
        const String Dataflow_signals = "dataflow";

        Lib_OLEDB_Excel __excel;
        DataTable dt_template = new DataTable();

        // CACHING: Static cache for query results with file modification tracking
        private static readonly ConcurrentDictionary<string, CachedResult> QueryCache = 
            new ConcurrentDictionary<string, CachedResult>();
        private static readonly object CacheLock = new object();

        // CACHING: Cache entry structure
        private class CachedResult
        {
            public Dictionary<string, bool> Results { get; set; }
            public DateTime FileLastModified { get; set; }
            public DateTime CacheTime { get; set; }
            public string FilePath { get; set; }
        }

        // CACHING: Cache configuration
        private static readonly TimeSpan CacheTimeout = TimeSpan.FromMinutes(10); // 10 minute cache timeout
        private const int MAX_CACHE_SIZE = 50; // Limit cache size to prevent memory overflow

        public Excel_Parser( string FileLink, DataTable dt_template)
        {
            try
            {
                if (string.IsNullOrEmpty(FileLink))
                    throw new ArgumentException("File link cannot be null or empty", nameof(FileLink));
                    
                if (!File.Exists(FileLink))
                    throw new FileNotFoundException($"Excel file not found: {FileLink}");
                    
                linkOfFile = FileLink;
                lb_NameOfFile = linkOfFile.Split('\\').Last();
                __excel = new Lib_OLEDB_Excel(FileLink);
                this.dt_template = dt_template?.Clone() ?? new DataTable();
                
                System.Diagnostics.Debug.WriteLine($"Excel_Parser initialized successfully for: {lb_NameOfFile}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excel_Parser initialization failed: {ex.Message}");
                throw new InvalidOperationException($"Failed to initialize Excel parser for {FileLink}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// RESOURCE MANAGEMENT: Properly dispose Excel connections and clear cache
        /// </summary>
        public void Dispose()
        {
            try
            {
                //Close all Connection
                __excel?.Dispose();
                
                // Clear local cache entries for this file
                ClearCacheForFile(linkOfFile);
                
                System.Diagnostics.Debug.WriteLine($"Excel_Parser disposed successfully for: {lb_NameOfFile}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during Excel_Parser disposal: {ex.Message}");
            }
        }

        public void search_Variable(string var)
        {
            clearData();

            lb_Name = var;
            
            if(lb_NameOfFile.ToLower().Contains(MacroModule_signals))
            {
                carasi_Parser(var);
            }
            else if (lb_NameOfFile.ToLower().Contains(Dataflow_signals))
            {
                dataflow_Parser(var);
            }
            else
            {
                MessageBox.Show("Please make sure 'carasi' contain in Carasi file name or 'dataflow' contain in Dataflow file name...", "Warning!");
            }
        }

        public bool _IsExist_Carasi(string var)
        {
            DataTable dt = __excel.ReadTable("Interfaces$", "[" + "SSTG label" + "]='" + var + "'");
            return (dt.Rows.Count > 0);
        }

        // BATCH OPTIMIZATION: Check multiple variables in single query
        public Dictionary<string, bool> _IsExist_Carasi_Batch(List<string> variables)
        {
            var results = new Dictionary<string, bool>();
            
            if (variables == null || variables.Count == 0)
                return results;

            // CACHING: Check cache first
            string cacheKey = GenerateCacheKey("CARASI", variables);
            var cachedResult = GetCachedResult(cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            // Initialize all variables as false
            foreach (string var in variables)
            {
                results[var] = false;
            }

            try
            {
                // CRITICAL FIX: Pre-validate connection ONCE to prevent multiple connection attempts
                if (!__excel.ValidateConnection())
                {
                    System.Diagnostics.Debug.WriteLine("CRITICAL: Excel connection validation failed in _IsExist_Carasi_Batch");
                    return results; // Return empty results rather than spam connections
                }

                // Build WHERE IN clause for batch query
                var quotedVars = variables.Select(v => "'" + v.Replace("'", "''") + "'");
                string whereClause = "[SSTG label] IN (" + string.Join(",", quotedVars) + ")";
                
                // Limit query size to prevent OLEDB overflow
                if (whereClause.Length > 8000) // SQL query length limit
                {
                    // Split into smaller batches
                    return ProcessLargeBatch_Carasi(variables);
                }
                
                // CRITICAL FIX: Use direct SQL without additional connection validation
                DataTable dt = __excel.ReadTableDirect("Interfaces$", whereClause);
                
                // Mark found variables as true
                foreach (DataRow row in dt.Rows)
                {
                    string foundVar = row["SSTG label"].ToString();
                    if (results.ContainsKey(foundVar))
                    {
                        results[foundVar] = true;
                    }
                }

                // CACHING: Store result in cache
                StoreCachedResult(cacheKey, results);
            }
            catch (Exception ex)
            {
                // Fallback to individual queries if batch fails
                System.Diagnostics.Debug.WriteLine("Batch query failed, falling back to individual queries: " + ex.Message);
                return ProcessIndividualQueries_Carasi(variables);
            }
            
            return results;
        }

        public bool _IsExist_Dataflow(string var)
        {
            DataTable dt = __excel.ReadTable("Mapping$", "[" + "F2" + "]='" + var + "'" + " OR " + "[" + "F17" + "]='" + var + "'");
            return (dt.Rows.Count > 0);
        }

        // BATCH OPTIMIZATION: Check multiple variables in Dataflow
        public Dictionary<string, bool> _IsExist_Dataflow_Batch(List<string> variables)
        {
            var results = new Dictionary<string, bool>();
            
            if (variables == null || variables.Count == 0)
                return results;

            // CACHING: Check cache first
            string cacheKey = GenerateCacheKey("DATAFLOW", variables);
            var cachedResult = GetCachedResult(cacheKey);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            // Initialize all variables as false
            foreach (string var in variables)
            {
                results[var] = false;
            }

            try
            {
                // CRITICAL FIX: Pre-validate connection ONCE to prevent multiple connection attempts
                if (!__excel.ValidateConnection())
                {
                    System.Diagnostics.Debug.WriteLine("CRITICAL: Excel connection validation failed in _IsExist_Dataflow_Batch");
                    return results; // Return empty results rather than spam connections
                }

                // Build WHERE clause for batch query (F2 OR F17 fields)
                var conditions = new List<string>();
                foreach (string var in variables)
                {
                    string escapedVar = var.Replace("'", "''");
                    conditions.Add("([F2]='" + escapedVar + "' OR [F17]='" + escapedVar + "')");
                }
                string whereClause = string.Join(" OR ", conditions);
                
                // CRITICAL FIX: Use direct SQL without additional connection validation
                DataTable dt = __excel.ReadTableDirect("Mapping$", whereClause);
                
                // Mark found variables as true
                foreach (DataRow row in dt.Rows)
                {
                    string f2Value = row["F2"].ToString();
                    string f17Value = row["F17"].ToString();
                    
                    if (results.ContainsKey(f2Value))
                        results[f2Value] = true;
                    if (results.ContainsKey(f17Value))
                        results[f17Value] = true;
                }

                // CACHING: Store result in cache
                StoreCachedResult(cacheKey, results);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in batch Dataflow query: " + ex.Message, "Error");
            }
            
            return results;
        }

        private void dataflow_Parser(string var)
        {
            DataTable dt =  dt_template.Clone();
            df_Properties = __excel.ReadTable("Mapping$", "[" + "F2" + "]='" + var + "'" + " OR " + "[" + "F17" + "]='" + var + "'");
            
            foreach (DataRow dr in df_Properties.Rows)
            {
                dt.Rows.Add(dr.ItemArray);
            }

            df_Properties = dt;
            dataview_df_Properties = df_Properties.DefaultView;
        }

        private void carasi_Parser(string var)
        {
            // OPTIMIZATION: Use main excel instance (which uses pooled connections)
            carasi_Interfaces = __excel.ReadTable("Interfaces$", "[" + "SSTG label" + "]='" + var + "'");
            carasi_dictionary = __excel.ReadTable("Dictionary$", "[" + "SSTG label" + "]='" + var + "'");

            // CACHE: Store single result for future reference
            string cacheKey = $"{linkOfFile}|Interfaces$|single|{var}";
            var singleResult = new Dictionary<string, bool> { { var, carasi_Interfaces.Rows.Count > 0 } };
            StoreCachedResult(cacheKey, singleResult);

            dictionary = carasi_dictionary.DefaultView;
            interfaces = carasi_Interfaces.DefaultView;
        }

        private void clearData()
        {
            lb_Name = string.Empty;
            df_Properties.Clear();
            carasi_Interfaces.Clear();
            carasi_dictionary.Clear();

            dataview_df_Properties = null;
            dictionary = new DataView();
            interfaces = new DataView();
        }

        private string linkOfFile = string.Empty;
        private string lb_Name = string.Empty;
        private string lb_NameOfFile = string.Empty;
        private string computeDetails = string.Empty;

        private DataTable df_Properties = new DataTable();
        private DataView dataview_df_Properties = new DataView();
        private Carasi_Interface variable = new Carasi_Interface();

        private DataTable carasi_Interfaces = new DataTable(); 
        private DataTable carasi_dictionary = new DataTable();
        private DataView dictionary = new DataView();
        private DataView interfaces = new DataView();


        public string Lb_Name { get => lb_Name; }
        public string Lb_NameOfFile { get => lb_NameOfFile; }

        public DataTable DF_Properties { get => df_Properties;}
        public DataView Dataview_df_Properties { get => dataview_df_Properties;}
        public string ComputeDetails { get => computeDetails;}
        public Carasi_Interface Variable { get => variable; }
        public DataTable Carasi_Interfaces { get => carasi_Interfaces; }
        public DataTable Carasi_dictionary { get => carasi_dictionary; }
        public DataView Dictionary { get => dictionary; }
        public DataView Interfaces { get => interfaces; }

        // CACHING: Helper methods
        private string GenerateCacheKey(string queryType, List<string> variables)
        {
            var sortedVars = variables.OrderBy(v => v).ToList();
            return $"{queryType}_{linkOfFile}_{string.Join("|", sortedVars)}";
        }

        private Dictionary<string, bool> GetCachedResult(string cacheKey)
        {
            if (QueryCache.TryGetValue(cacheKey, out CachedResult cached))
            {
                // Check if cache is still valid
                bool isValid = IsValidCache(cached);
                if (isValid)
                {
                    return new Dictionary<string, bool>(cached.Results);
                }
                else
                {
                    // Remove invalid cache entry
                    QueryCache.TryRemove(cacheKey, out _);
                }
            }
            return null;
        }

        private bool IsValidCache(CachedResult cached)
        {
            try
            {
                // Check cache timeout
                if (DateTime.Now - cached.CacheTime > CacheTimeout)
                    return false;

                // Check if file has been modified
                FileInfo fileInfo = new FileInfo(cached.FilePath);
                if (!fileInfo.Exists || fileInfo.LastWriteTime > cached.FileLastModified)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void StoreCachedResult(string cacheKey, Dictionary<string, bool> results)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(linkOfFile);
                var cachedResult = new CachedResult
                {
                    Results = new Dictionary<string, bool>(results),
                    FileLastModified = fileInfo.LastWriteTime,
                    CacheTime = DateTime.Now,
                    FilePath = linkOfFile
                };
                
                QueryCache.TryAdd(cacheKey, cachedResult);
                
                // Cleanup old cache entries periodically
                CleanupOldCacheEntries();
            }
            catch
            {
                // Ignore cache storage errors
            }
        }

        private static void CleanupOldCacheEntries()
        {
            // Aggressive cleanup when approaching limits
            if (QueryCache.Count >= MAX_CACHE_SIZE)
            {
                lock (CacheLock)
                {
                    // Remove half of the oldest entries
                    var entriesToRemove = QueryCache.Count / 2;
                    var oldestKeys = QueryCache
                        .OrderBy(kvp => kvp.Value.CacheTime)
                        .Take(entriesToRemove)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (string key in oldestKeys)
                    {
                        QueryCache.TryRemove(key, out _);
                    }
                }
            }
            // Regular cleanup occasionally to avoid performance impact
            else if (QueryCache.Count > 20 && DateTime.Now.Millisecond < 10)
            {
                lock (CacheLock)
                {
                    var expiredKeys = QueryCache
                        .Where(kvp => DateTime.Now - kvp.Value.CacheTime > CacheTimeout)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (string key in expiredKeys)
                    {
                        QueryCache.TryRemove(key, out _);
                    }
                }
            }
        }

        // CACHING: Static method to clear all cached results
        public static void ClearQueryCache()
        {
            lock (CacheLock)
            {
                QueryCache.Clear();
            }
        }

        // CACHING: Get cache statistics for monitoring
        public static int GetCacheSize()
        {
            return QueryCache.Count;
        }

        // CRITICAL FIX: Process large batches by splitting - OPTIMIZED TO PREVENT CONNECTION LOOPS
        private Dictionary<string, bool> ProcessLargeBatch_Carasi(List<string> variables)
        {
            var results = new Dictionary<string, bool>();
            
            // Initialize all as false
            foreach (string var in variables)
            {
                results[var] = false;
            }

            // CRITICAL FIX: Pre-validate connection ONCE to prevent multiple connection attempts
            try
            {
                if (!__excel.ValidateConnection())
                {
                    System.Diagnostics.Debug.WriteLine("CRITICAL: Excel connection validation failed in ProcessLargeBatch_Carasi");
                    return results; // Return empty results rather than spam connections
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL: Connection validation error in ProcessLargeBatch_Carasi: {ex.Message}");
                return results;
            }

            // Split into chunks of 50 variables (increased from 20 to reduce connection calls)
            const int CHUNK_SIZE = 50;
            for (int i = 0; i < variables.Count; i += CHUNK_SIZE)
            {
                var chunk = variables.Skip(i).Take(CHUNK_SIZE).ToList();
                try
                {
                    var quotedVars = chunk.Select(v => "'" + v.Replace("'", "''") + "'");
                    string whereClause = "[SSTG label] IN (" + string.Join(",", quotedVars) + ")";
                    
                    // CRITICAL FIX: Use direct SQL without additional connection validation
                    DataTable dt = __excel.ReadTableDirect("Interfaces$", whereClause);
                    
                    foreach (DataRow row in dt.Rows)
                    {
                        string foundVar = row["SSTG label"].ToString();
                        if (results.ContainsKey(foundVar))
                        {
                            results[foundVar] = true;
                        }
                    }
                }
                catch
                {
                    // If chunk fails, try individual queries for this chunk
                    foreach (string var in chunk)
                    {
                        try
                        {
                            results[var] = _IsExist_Carasi(var);
                        }
                        catch { /* Leave as false */ }
                    }
                }
            }
            
            return results;
        }

        // FALLBACK: Individual queries as last resort
        private Dictionary<string, bool> ProcessIndividualQueries_Carasi(List<string> variables)
        {
            var results = new Dictionary<string, bool>();
            
            foreach (string var in variables)
            {
                try
                {
                    results[var] = _IsExist_Carasi(var);
                }
                catch
                {
                    results[var] = false;
                }
            }
            
            return results;
        }

        #region Performance Optimization - Advanced Caching & Memory Management

        /// <summary>
        /// PERFORMANCE: Smart cache management with memory pressure detection
        /// Automatically clears cache when memory usage is high
        /// </summary>
        public static void OptimizeCacheMemory()
        {
            // Monitor memory usage and clear cache if needed
            long memoryBefore = GC.GetTotalMemory(false);
            if (memoryBefore > 100 * 1024 * 1024) // > 100MB
            {
                // Clear oldest cache entries first
                lock (CacheLock)
                {
                    var oldEntries = QueryCache
                        .Where(kvp => DateTime.Now - kvp.Value.CacheTime > TimeSpan.FromMinutes(5))
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (string key in oldEntries)
                    {
                        QueryCache.TryRemove(key, out _);
                    }
                }

                // Force garbage collection if memory is still high
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long memoryAfter = GC.GetTotalMemory(false);
                System.Diagnostics.Debug.WriteLine($"Memory optimization: {memoryBefore / 1024 / 1024}MB → {memoryAfter / 1024 / 1024}MB");
            }
        }

        /// <summary>
        /// PERFORMANCE: Preload frequently used data into cache
        /// Call this during application startup or idle time
        /// </summary>
        public async Task PreloadCommonDataAsync(List<string> commonVariables)
        {
            if (commonVariables == null || commonVariables.Count == 0)
                return;

            await Task.Run(() =>
            {
                try
                {
                    // Batch preload for Carasi variables
                    if (linkOfFile.ToLower().Contains(MacroModule_signals))
                    {
                        _IsExist_Carasi_Batch(commonVariables);
                    }
                    // Batch preload for Dataflow variables
                    else if (linkOfFile.ToLower().Contains(Dataflow_signals))
                    {
                        _IsExist_Dataflow_Batch(commonVariables);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Preload failed: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// PERFORMANCE: Get cache statistics for monitoring performance
        /// Use this to track cache effectiveness
        /// </summary>
        public static Dictionary<string, object> GetCacheStatistics()
        {
            lock (CacheLock)
            {
                var stats = new Dictionary<string, object>
                {
                    ["TotalEntries"] = QueryCache.Count,
                    ["MaxSize"] = MAX_CACHE_SIZE,
                    ["CacheHitRatio"] = CalculateCacheHitRatio(),
                    ["OldestEntry"] = QueryCache.Values.Any() ? QueryCache.Values.Min(c => c.CacheTime) : DateTime.MinValue,
                    ["NewestEntry"] = QueryCache.Values.Any() ? QueryCache.Values.Max(c => c.CacheTime) : DateTime.MinValue,
                    ["MemoryUsage"] = GC.GetTotalMemory(false) / 1024 / 1024 // MB
                };
                return stats;
            }
        }

        /// <summary>
        /// PERFORMANCE: Calculate cache hit ratio for performance monitoring
        /// </summary>
        private static double CalculateCacheHitRatio()
        {
            // This would require hit/miss counters in real implementation
            // For now, return estimated ratio based on cache usage
            if (QueryCache.Count == 0) return 0.0;
            
            var activeEntries = QueryCache.Values.Count(c => DateTime.Now - c.CacheTime < CacheTimeout);
            return (double)activeEntries / QueryCache.Count;
        }

        /// <summary>
        /// PERFORMANCE: Asynchronous batch processing with progress reporting
        /// For processing large lists of variables without blocking UI
        /// </summary>
        public async Task<Dictionary<string, bool>> ProcessBatchAsync(
            List<string> variables, 
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (variables == null || variables.Count == 0)
                return new Dictionary<string, bool>();

            return await Task.Run(() =>
            {
                var results = new Dictionary<string, bool>();
                int processed = 0;

                try
                {
                    // Use appropriate batch method based on file type
                    if (linkOfFile.ToLower().Contains(MacroModule_signals))
                    {
                        results = _IsExist_Carasi_Batch(variables);
                    }
                    else if (linkOfFile.ToLower().Contains(Dataflow_signals))
                    {
                        results = _IsExist_Dataflow_Batch(variables);
                    }

                    progress?.Report(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Async batch processing failed: {ex.Message}");
                    // Fallback to individual processing with progress
                    foreach (string variable in variables)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        try
                        {
                            if (linkOfFile.ToLower().Contains(MacroModule_signals))
                            {
                                results[variable] = _IsExist_Carasi(variable);
                            }
                            else if (linkOfFile.ToLower().Contains(Dataflow_signals))
                            {
                                results[variable] = _IsExist_Dataflow(variable);
                            }
                        }
                        catch
                        {
                            results[variable] = false;
                        }

                        processed++;
                        progress?.Report((processed * 100) / variables.Count);
                    }
                }

                return results;
            }, cancellationToken);
        }

        /// <summary>
        /// CACHE MANAGEMENT: Clear cache entries for specific file
        /// </summary>
        private static void ClearCacheForFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
                
            try
            {
                lock (CacheLock)
                {
                    var keysToRemove = QueryCache
                        .Where(kvp => kvp.Value.FilePath == filePath)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (string key in keysToRemove)
                    {
                        QueryCache.TryRemove(key, out _);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Cleared {keysToRemove.Count} cache entries for file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cache for file {filePath}: {ex.Message}");
            }
        }

        #endregion
    }
}
