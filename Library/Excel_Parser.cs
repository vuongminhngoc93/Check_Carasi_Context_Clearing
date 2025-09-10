using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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


    class Excel_Parser
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
            linkOfFile = FileLink;
            lb_NameOfFile = linkOfFile.Split('\\').Last();
            __excel = new Lib_OLEDB_Excel(FileLink);
            this.dt_template = dt_template.Clone();
        }

        public void Dispose()
        {
            //Close all Connection
            __excel.Dispose();
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
                // Build WHERE IN clause for batch query
                var quotedVars = variables.Select(v => "'" + v.Replace("'", "''") + "'");
                string whereClause = "[SSTG label] IN (" + string.Join(",", quotedVars) + ")";
                
                // Limit query size to prevent OLEDB overflow
                if (whereClause.Length > 8000) // SQL query length limit
                {
                    // Split into smaller batches
                    return ProcessLargeBatch_Carasi(variables);
                }
                
                DataTable dt = __excel.ReadTable("Interfaces$", whereClause);
                
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
                // Build WHERE clause for batch query (F2 OR F17 fields)
                var conditions = new List<string>();
                foreach (string var in variables)
                {
                    string escapedVar = var.Replace("'", "''");
                    conditions.Add("([F2]='" + escapedVar + "' OR [F17]='" + escapedVar + "')");
                }
                string whereClause = string.Join(" OR ", conditions);
                
                DataTable dt = __excel.ReadTable("Mapping$", whereClause);
                
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
            carasi_Interfaces = __excel.ReadTable("Interfaces$", "[" + "SSTG label" + "]='" + var + "'");
            carasi_dictionary = __excel.ReadTable("Dictionary$", "[" + "SSTG label" + "]='" + var + "'");

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

        // FALLBACK: Process large batches by splitting
        private Dictionary<string, bool> ProcessLargeBatch_Carasi(List<string> variables)
        {
            var results = new Dictionary<string, bool>();
            
            // Initialize all as false
            foreach (string var in variables)
            {
                results[var] = false;
            }

            // Split into chunks of 20 variables
            const int CHUNK_SIZE = 20;
            for (int i = 0; i < variables.Count; i += CHUNK_SIZE)
            {
                var chunk = variables.Skip(i).Take(CHUNK_SIZE).ToList();
                try
                {
                    var quotedVars = chunk.Select(v => "'" + v.Replace("'", "''") + "'");
                    string whereClause = "[SSTG label] IN (" + string.Join(",", quotedVars) + ")";
                    
                    DataTable dt = __excel.ReadTable("Interfaces$", whereClause);
                    
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
    }
}
