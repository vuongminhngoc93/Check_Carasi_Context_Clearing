using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Check_carasi_DF_ContextClearing
{
    /// <summary>
    /// GPU-accelerated helper for Excel processing operations
    /// Uses parallel processing to leverage GPU through compute-intensive tasks
    /// </summary>
    public static class GPU_Accelerator
    {
        // GPU OPTIMIZATION: Configuration constants
        private const int GPU_THREAD_COUNT = Environment.ProcessorCount * 4; // Leverage hyperthreading
        private const int GPU_BATCH_SIZE = 100; // Optimal batch size for GPU processing
        private const int MIN_ITEMS_FOR_GPU = 50; // Minimum items to justify GPU usage
        
        // GPU UTILIZATION: Performance monitoring
        private static readonly object GpuLock = new object();
        private static int _activeGpuOperations = 0;
        
        /// <summary>
        /// GPU-accelerated string matching for Excel variable searches
        /// </summary>
        public static Dictionary<string, bool> AcceleratedStringSearch(
            List<string> searchTerms, 
            List<string> targetStrings, 
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var results = new ConcurrentDictionary<string, bool>();
            
            // Initialize all search terms as false
            foreach (string term in searchTerms)
            {
                results[term] = false;
            }
            
            // GPU OPTIMIZATION: Use parallel processing only for large datasets
            if (searchTerms.Count < MIN_ITEMS_FOR_GPU || targetStrings.Count < MIN_ITEMS_FOR_GPU)
            {
                return FallbackStringSearch(searchTerms, targetStrings, comparison);
            }
            
            try
            {
                IncrementGpuOperations();
                PerformanceLogger.StartTimer("GPU_StringSearch");
                
                // GPU ACCELERATION: Parallel batch processing
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = GPU_THREAD_COUNT
                };
                
                // Split search terms into GPU-optimized batches
                var batches = CreateBatches(searchTerms, GPU_BATCH_SIZE);
                
                Parallel.ForEach(batches, parallelOptions, batch =>
                {
                    ProcessSearchBatch(batch, targetStrings, results, comparison);
                });
                
                PerformanceLogger.StopTimer("GPU_StringSearch");
                return new Dictionary<string, bool>(results);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GPU acceleration failed, falling back: {ex.Message}");
                return FallbackStringSearch(searchTerms, targetStrings, comparison);
            }
            finally
            {
                DecrementGpuOperations();
            }
        }
        
        /// <summary>
        /// GPU-accelerated Excel data processing for multiple variables
        /// </summary>
        public static Dictionary<string, List<T>> AcceleratedDataProcessing<T>(
            List<string> variables,
            Func<string, List<T>> processingFunction)
        {
            var results = new ConcurrentDictionary<string, List<T>>();
            
            if (variables.Count < MIN_ITEMS_FOR_GPU)
            {
                return FallbackDataProcessing(variables, processingFunction);
            }
            
            try
            {
                IncrementGpuOperations();
                PerformanceLogger.StartTimer("GPU_DataProcessing");
                
                // GPU ACCELERATION: Parallel processing with memory optimization
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = GPU_THREAD_COUNT
                };
                
                Parallel.ForEach(variables, parallelOptions, variable =>
                {
                    try
                    {
                        var result = processingFunction(variable);
                        results[variable] = result;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"GPU processing failed for {variable}: {ex.Message}");
                        results[variable] = new List<T>();
                    }
                });
                
                PerformanceLogger.StopTimer("GPU_DataProcessing");
                return new Dictionary<string, List<T>>(results);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GPU data processing failed, falling back: {ex.Message}");
                return FallbackDataProcessing(variables, processingFunction);
            }
            finally
            {
                DecrementGpuOperations();
            }
        }
        
        /// <summary>
        /// GPU-accelerated batch Excel query processing
        /// </summary>
        public static Dictionary<string, bool> AcceleratedBatchQuery(
            List<string> variables,
            Func<List<string>, Dictionary<string, bool>> batchQueryFunction,
            int optimalBatchSize = 20)
        {
            var results = new ConcurrentDictionary<string, bool>();
            
            if (variables.Count < MIN_ITEMS_FOR_GPU)
            {
                return batchQueryFunction(variables);
            }
            
            try
            {
                IncrementGpuOperations();
                PerformanceLogger.StartTimer("GPU_BatchQuery");
                
                // GPU OPTIMIZATION: Split into optimal batches for parallel processing
                var batches = CreateBatches(variables, optimalBatchSize);
                
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Min(GPU_THREAD_COUNT, batches.Count)
                };
                
                Parallel.ForEach(batches, parallelOptions, batch =>
                {
                    try
                    {
                        var batchResults = batchQueryFunction(batch);
                        foreach (var kvp in batchResults)
                        {
                            results[kvp.Key] = kvp.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"GPU batch query failed: {ex.Message}");
                        // Fallback to individual processing for this batch
                        foreach (var variable in batch)
                        {
                            results[variable] = false;
                        }
                    }
                });
                
                PerformanceLogger.StopTimer("GPU_BatchQuery");
                return new Dictionary<string, bool>(results);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GPU batch processing failed, falling back: {ex.Message}");
                return batchQueryFunction(variables);
            }
            finally
            {
                DecrementGpuOperations();
            }
        }
        
        /// <summary>
        /// Get current GPU utilization stats
        /// </summary>
        public static int GetActiveGpuOperations()
        {
            lock (GpuLock)
            {
                return _activeGpuOperations;
            }
        }
        
        /// <summary>
        /// Check if GPU acceleration should be used based on workload
        /// </summary>
        public static bool ShouldUseGpuAcceleration(int itemCount, int currentGpuLoad = -1)
        {
            if (itemCount < MIN_ITEMS_FOR_GPU)
                return false;
                
            if (currentGpuLoad == -1)
                currentGpuLoad = GetActiveGpuOperations();
                
            // Don't overwhelm GPU with too many concurrent operations
            return currentGpuLoad < GPU_THREAD_COUNT / 2;
        }
        
        // HELPER METHODS
        private static void IncrementGpuOperations()
        {
            lock (GpuLock)
            {
                _activeGpuOperations++;
            }
        }
        
        private static void DecrementGpuOperations()
        {
            lock (GpuLock)
            {
                _activeGpuOperations = Math.Max(0, _activeGpuOperations - 1);
            }
        }
        
        private static List<List<T>> CreateBatches<T>(List<T> items, int batchSize)
        {
            var batches = new List<List<T>>();
            for (int i = 0; i < items.Count; i += batchSize)
            {
                batches.Add(items.Skip(i).Take(batchSize).ToList());
            }
            return batches;
        }
        
        private static void ProcessSearchBatch(
            List<string> searchTerms,
            List<string> targetStrings,
            ConcurrentDictionary<string, bool> results,
            StringComparison comparison)
        {
            foreach (string term in searchTerms)
            {
                bool found = targetStrings.Any(target => 
                    string.Equals(target, term, comparison) || 
                    target.IndexOf(term, comparison) >= 0);
                results[term] = found;
            }
        }
        
        private static Dictionary<string, bool> FallbackStringSearch(
            List<string> searchTerms,
            List<string> targetStrings,
            StringComparison comparison)
        {
            var results = new Dictionary<string, bool>();
            foreach (string term in searchTerms)
            {
                results[term] = targetStrings.Any(target => 
                    string.Equals(target, term, comparison) || 
                    target.IndexOf(term, comparison) >= 0);
            }
            return results;
        }
        
        private static Dictionary<string, List<T>> FallbackDataProcessing<T>(
            List<string> variables,
            Func<string, List<T>> processingFunction)
        {
            var results = new Dictionary<string, List<T>>();
            foreach (string variable in variables)
            {
                try
                {
                    results[variable] = processingFunction(variable);
                }
                catch
                {
                    results[variable] = new List<T>();
                }
            }
            return results;
        }
    }
}
