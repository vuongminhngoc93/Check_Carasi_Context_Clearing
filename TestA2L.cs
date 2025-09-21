using System;
using System.IO;
using System.Diagnostics;

namespace Check_carasi_DF_ContextClearing
{
    class TestA2L
    {
        public static void TestA2LParsing()
        {
            string a2lPath = @"d:\5_Automation\Check_carasi_DF_ContextClearing\Input\VC1CP019_V1070C_1.a2l";
            
            Console.WriteLine($"Testing A2L parsing with: {Path.GetFileName(a2lPath)}");
            Console.WriteLine($"File exists: {File.Exists(a2lPath)}");
            
            if (!File.Exists(a2lPath))
            {
                Console.WriteLine("A2L file not found!");
                return;
            }
            
            try
            {
                // Test basic file info
                var fileInfo = new FileInfo(a2lPath);
                Console.WriteLine($"File size: {fileInfo.Length / 1024 / 1024:F2} MB");
                Console.WriteLine($"Lines count: {File.ReadAllLines(a2lPath).Length}");
                
                // Test A2LParserManager
                Console.WriteLine("\n--- Testing A2LParserManager ---");
                var stopwatch = Stopwatch.StartNew();
                
                // Test some common variable names
                string[] testVariables = { 
                    "MDG1C", 
                    "VC1CP019", 
                    "DIM",
                    "Protocol_Layer",
                    "ASAP2_VERSION"
                };
                
                foreach (var variable in testVariables)
                {
                    Console.WriteLine($"\nSearching for: {variable}");
                    var result = A2LParserManager.FindVariable(a2lPath, variable);
                    
                    if (result.Found)
                    {
                        Console.WriteLine($"  Found!");
                        Console.WriteLine($"  In Measurements: {result.FoundInMeasurements}");
                        Console.WriteLine($"  In Characteristics: {result.FoundInCharacteristics}");
                        Console.WriteLine($"  Summary: {result.GetSummary()}");
                    }
                    else
                    {
                        Console.WriteLine("  Not found");
                    }
                }
                
                stopwatch.Stop();
                Console.WriteLine($"\nTotal search time: {stopwatch.ElapsedMilliseconds} ms");
                
                // Test cache performance
                Console.WriteLine("\n--- Testing Cache Performance ---");
                stopwatch.Restart();
                var result2 = A2LParserManager.FindVariable(a2lPath, "MDG1C");
                stopwatch.Stop();
                Console.WriteLine($"Second search (cached): {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"Result: {(result2.Found ? "Found" : "Not Found")}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing A2L: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
