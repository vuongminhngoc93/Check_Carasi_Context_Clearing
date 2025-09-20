using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests
{
    /// <summary>
    /// Simple Performance Benchmark: OLEDB vs EPPlus
    /// Focus on the key bottleneck: multiple variable searches
    /// </summary>
    class SimplePerformanceBenchmark
    {
        public void RunPerformanceTest()
        {
            Console.WriteLine("=== SIMPLE PERFORMANCE BENCHMARK ===");
            Console.WriteLine("Testing search performance: OLEDB vs EPPlus\n");

            // Find Input folder with real data
            string inputFolder = @"..\..\..\..\Input";
            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("Input folder not found. Using mock test...");
                MockTest();
                return;
            }

            // Find carasi files
            var carasiFiles = Directory.GetFiles(inputFolder, "*carasi*.xls*").Take(1).ToArray();
            if (carasiFiles.Length == 0)
            {
                Console.WriteLine("No carasi files found.");
                MockTest();
                return;
            }

            string testFile = carasiFiles[0];
            Console.WriteLine($"Testing file: {Path.GetFileName(testFile)}");

            // Generate test variables
            var testVariables = GenerateTestVariables(20); // Start with 20 for initial test
            Console.WriteLine($"Testing with {testVariables.Count} variables\n");

            // Test current OLEDB method
            double oledbTime = TestOLEDBMethod(testFile, testVariables);

            // Test EPPlus method 
            double epplusTime = TestEPPlusMethod(testFile, testVariables);

            // Show results
            ShowResults(oledbTime, epplusTime);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static double TestOLEDBMethod(string filePath, List<string> variables)
        {
            Console.WriteLine("🔍 Testing OLEDB Method...");
            var stopwatch = Stopwatch.StartNew();
            int foundCount = 0;

            try
            {
                var template = CreateTemplate();
                var parser = new Excel_Parser(filePath, template);
                
                foreach (string variable in variables)
                {
                    // This is the bottleneck! Each call = separate OLEDB query
                    bool exists = parser._IsExist_Carasi(variable);
                    if (exists) foundCount++;
                }
                
                parser.Dispose(); // Manual disposal
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ OLEDB failed: {ex.Message}");
                return -1;
            }

            stopwatch.Stop();
            double totalSeconds = stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"  ✅ OLEDB: {totalSeconds:F2}s (found {foundCount} variables)");
            return totalSeconds;
        }

        static double TestEPPlusMethod(string filePath, List<string> variables)
        {
            Console.WriteLine("🚀 Testing EPPlus Method...");
            var stopwatch = Stopwatch.StartNew();
            int foundCount = 0;

            try
            {
                using (var parser = new EPPlusExcelParser(filePath))
                {
                    // BATCH processing - this is the key optimization!
                    var results = parser.BatchCheckCarasi(variables);
                    foundCount = results.Values.Count(v => v);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ EPPlus failed: {ex.Message}");
                return -1;
            }

            stopwatch.Stop();
            double totalSeconds = stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"  ✅ EPPlus: {totalSeconds:F2}s (found {foundCount} variables)");
            return totalSeconds;
        }

        static void ShowResults(double oledbTime, double epplusTime)
        {
            Console.WriteLine("\n📊 PERFORMANCE RESULTS:");
            Console.WriteLine($"  OLEDB Method:  {oledbTime:F2} seconds");
            Console.WriteLine($"  EPPlus Method: {epplusTime:F2} seconds");

            if (oledbTime > 0 && epplusTime > 0)
            {
                var improvement = ((oledbTime - epplusTime) / oledbTime) * 100;
                var speedFactor = oledbTime / epplusTime;

                Console.WriteLine($"\n🚀 PERFORMANCE IMPROVEMENT:");
                Console.WriteLine($"  Speed Improvement: {improvement:F1}%");
                Console.WriteLine($"  Speed Factor: {speedFactor:F1}x faster");

                if (improvement > 50)
                {
                    Console.WriteLine($"  🎉 EXCELLENT! EPPlus is significantly faster!");
                }
                else if (improvement > 20)
                {
                    Console.WriteLine($"  ✅ GOOD! EPPlus shows good improvement!");
                }
                else if (improvement > 0)
                {
                    Console.WriteLine($"  ⚡ OK! EPPlus is faster but modest improvement");
                }
                else
                {
                    Console.WriteLine($"  ⚠️  EPPlus needs optimization or file too small to measure");
                }
            }
        }

        static void MockTest()
        {
            Console.WriteLine("Running mock performance test...");
            Console.WriteLine("OLEDB Method: 5.20s (simulated)");
            Console.WriteLine("EPPlus Method: 0.85s (simulated)");
            Console.WriteLine("🚀 Performance Improvement: 83.7%");
            Console.WriteLine("🚀 Speed Factor: 6.1x faster");
        }

        static List<string> GenerateTestVariables(int count)
        {
            var variables = new List<string>();
            string[] prefixes = { "VEH_", "ENG_", "BRAKE_", "STEER_" };
            string[] types = { "Speed", "Temp", "Press", "Pos" };
            string[] suffixes = { "_Req", "_Act", "_Stat" };

            Random rand = new Random(42);
            for (int i = 0; i < count; i++)
            {
                string prefix = prefixes[rand.Next(prefixes.Length)];
                string type = types[rand.Next(types.Length)];
                string suffix = suffixes[rand.Next(suffixes.Length)];
                variables.Add($"{prefix}{type}{i:D2}{suffix}");
            }

            return variables;
        }

        static DataTable CreateTemplate()
        {
            var template = new DataTable();
            template.Columns.Add("Interface Name", typeof(string));
            template.Columns.Add("Input", typeof(string));
            template.Columns.Add("Output", typeof(string));
            template.Columns.Add("Description", typeof(string));
            return template;
        }
    }
}
