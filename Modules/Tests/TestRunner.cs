using System;
using System.IO;
using Check_carasi_DF_ContextClearing.Tests;

namespace Check_carasi_DF_ContextClearing
{
    class TestRunner
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== BENCHMARK TEST RUNNER ===");
            Console.WriteLine("Testing OLEDB vs EPPlus performance with 50+ variables...\n");
            
            try
            {
                var benchmark = new SimplePerformanceBenchmark();
                benchmark.RunPerformanceTest();
                
                Console.WriteLine("\n=== BENCHMARK COMPLETED ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
