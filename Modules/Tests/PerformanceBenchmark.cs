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
    /// Performance Benchmark: OLEDB vs EPPlus for searching multiple variables
    /// Test với 50+ biến để so sánh tốc độ
    /// </summary>
    class PerformanceBenchmark
    {
        public void RunPerformanceTest()
        {
            Console.WriteLine("=== PERFORMANCE BENCHMARK: OLEDB vs EPPlus ===");
            Console.WriteLine("Testing search performance with 50+ variables\n");

            // Generate test variables
            var testVariables = GenerateTestVariables(50);
            Console.WriteLine($"Generated {testVariables.Count} test variables");

            // Test files from Input folder
            string inputFolder = @"..\..\..\..\Input";
            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("ERROR: Input folder not found. Creating mock test...");
                inputFolder = @".\TestData";
            }

            var excelFiles = Directory.GetFiles(inputFolder, "*carasi*.xls*")
                                   .Take(2) // Test with first 2 carasi files
                                   .ToArray();

            if (excelFiles.Length == 0)
            {
                Console.WriteLine("No carasi Excel files found for testing.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            foreach (string filePath in excelFiles)
            {
                Console.WriteLine($"\n=== Testing File: {Path.GetFileName(filePath)} ===");
                
                // Test Current OLEDB Method
                var oledbTime = BenchmarkOLEDBMethod(filePath, testVariables);
                
                // Test EPPlus Method (will implement)
                var epplusTime = BenchmarkEPPlusMethod(filePath, testVariables);
                
                // Results
                Console.WriteLine($"\n📊 RESULTS for {Path.GetFileName(filePath)}:");
                Console.WriteLine($"  OLEDB Method:  {oledbTime:F2} seconds");
                Console.WriteLine($"  EPPlus Method: {epplusTime:F2} seconds");
                
                if (oledbTime > 0 && epplusTime > 0)
                {
                    var improvement = ((oledbTime - epplusTime) / oledbTime) * 100;
                    Console.WriteLine($"  🚀 Performance Improvement: {improvement:F1}%");
                    Console.WriteLine($"  ⚡ Speed Factor: {oledbTime / epplusTime:F1}x faster");
                }
            }

            Console.WriteLine("\n=== BENCHMARK COMPLETE ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static List<string> GenerateTestVariables(int count)
        {
            var variables = new List<string>();
            
            // Generate realistic variable names for automotive
            string[] prefixes = { "VEH_", "ENG_", "TRANS_", "BRAKE_", "STEER_", "SUSP_", "HVAC_", "BODY_" };
            string[] types = { "Speed", "Temp", "Press", "Volt", "Curr", "Pos", "Angle", "Force" };
            string[] suffixes = { "_Req", "_Act", "_Stat", "_Diag", "_Ctrl", "_Mon", "_Lim", "_Cal" };

            Random rand = new Random(42); // Fixed seed for reproducible results
            
            for (int i = 0; i < count; i++)
            {
                string prefix = prefixes[rand.Next(prefixes.Length)];
                string type = types[rand.Next(types.Length)];
                string suffix = suffixes[rand.Next(suffixes.Length)];
                string number = (i + 1).ToString("D3");
                
                variables.Add($"{prefix}{type}{number}{suffix}");
            }

            return variables;
        }

        static double BenchmarkOLEDBMethod(string filePath, List<string> variables)
        {
            Console.WriteLine("🔍 Testing Current OLEDB Method...");
            
            var stopwatch = Stopwatch.StartNew();
            int foundCount = 0;
            
            try
            {
                // Create template DataTable (simplified)
                var template = new DataTable();
                template.Columns.Add("Interface Name", typeof(string));
                template.Columns.Add("Input", typeof(string));
                template.Columns.Add("Output", typeof(string));

                var parser = new Excel_Parser(filePath, template);
                
                foreach (string variable in variables)
                {
                    try
                    {
                        // Test both Carasi and Dataflow existence
                        bool existsCarasi = parser._IsExist_Carasi(variable);
                        bool existsDataflow = parser._IsExist_Dataflow(variable);
                        
                        if (existsCarasi || existsDataflow)
                        {
                            foundCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"    Error searching {variable}: {ex.Message}");
                    }
                }
                
                parser.Dispose(); // Manual disposal
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ OLEDB Method failed: {ex.Message}");
                return -1;
            }
            
            stopwatch.Stop();
            double totalSeconds = stopwatch.Elapsed.TotalSeconds;
            
            Console.WriteLine($"  ✅ OLEDB completed in {totalSeconds:F2}s (found {foundCount} variables)");
            return totalSeconds;
        }

        static double BenchmarkEPPlusMethod(string filePath, List<string> variables)
        {
            Console.WriteLine("🚀 Testing EPPlus Method...");
            
            var stopwatch = Stopwatch.StartNew();
            int foundCount = 0;
            
            try
            {
                using (var parser = new EPPlusExcelParser(filePath))
                {
                    // Use BATCH processing for maximum performance!
                    Dictionary<string, bool> carasiResults = null;
                    Dictionary<string, bool> dataflowResults = null;
                    
                    if (parser.IsCarasiFile)
                    {
                        carasiResults = parser.BatchCheckCarasi(variables);
                        foundCount = carasiResults.Values.Count(v => v);
                    }
                    else if (parser.IsDataflowFile)
                    {
                        dataflowResults = parser.BatchCheckDataflow(variables);
                        foundCount = dataflowResults.Values.Count(v => v);
                    }
                    else
                    {
                        // Test both if file type unclear
                        carasiResults = parser.BatchCheckCarasi(variables);
                        dataflowResults = parser.BatchCheckDataflow(variables);
                        foundCount = carasiResults.Values.Count(v => v) + dataflowResults.Values.Count(v => v);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ EPPlus Method failed: {ex.Message}");
                return -1;
            }
            
            stopwatch.Stop();
            double totalSeconds = stopwatch.Elapsed.TotalSeconds;
            
            Console.WriteLine($"  ✅ EPPlus completed in {totalSeconds:F2}s (found {foundCount} variables)");
            return totalSeconds;
        }
    }

    /// <summary>
    /// EPPlus-based Excel Parser for high performance
    /// Implementation moved to Library\EPPlusExcelParser.cs
    /// </summary>
    public class EPPlusExcelParser : Check_carasi_DF_ContextClearing.EPPlusExcelParser
    {
        public EPPlusExcelParser(string filePath) : base(filePath)
        {
        }
    }
}
