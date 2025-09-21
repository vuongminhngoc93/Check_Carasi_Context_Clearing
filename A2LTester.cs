using System;
using System.IO;
using System.Diagnostics;

namespace Check_carasi_DF_ContextClearing
{
    public class A2LTester
    {
        public static void QuickTest()
        {
            try
            {
                string a2lPath = @"d:\5_Automation\Check_carasi_DF_ContextClearing\Input\VC1CP019_V1070C_1.a2l";
                
                Console.WriteLine("=== A2L Integration Test ===");
                Console.WriteLine($"Testing file: {Path.GetFileName(a2lPath)}");
                Console.WriteLine($"File exists: {File.Exists(a2lPath)}");
                
                if (!File.Exists(a2lPath))
                {
                    Console.WriteLine("ERROR: A2L file not found!");
                    return;
                }
                
                var fileInfo = new FileInfo(a2lPath);
                Console.WriteLine($"File size: {fileInfo.Length / (1024 * 1024):F1} MB");
                
                // Test A2LParserManager
                Console.WriteLine("\n--- Testing A2LParserManager.FindVariable() ---");
                
                var stopwatch = Stopwatch.StartNew();
                var result1 = A2LParserManager.FindVariable(a2lPath, "MDG1C");
                stopwatch.Stop();
                
                Console.WriteLine($"Search for 'MDG1C': {(result1.Found ? "FOUND" : "NOT FOUND")}");
                Console.WriteLine($"Search time: {stopwatch.ElapsedMilliseconds} ms");
                
                if (result1.Found)
                {
                    Console.WriteLine($"Found in Measurements: {result1.FoundInMeasurements}");
                    Console.WriteLine($"Found in Characteristics: {result1.FoundInCharacteristics}");
                }
                
                // Test cache performance
                Console.WriteLine("\n--- Testing Cache Performance ---");
                stopwatch.Restart();
                var result2 = A2LParserManager.FindVariable(a2lPath, "MDG1C");
                stopwatch.Stop();
                Console.WriteLine($"Second search time (cached): {stopwatch.ElapsedMilliseconds} ms");
                
                // Test another variable
                Console.WriteLine("\n--- Testing Another Variable ---");
                var result3 = A2LParserManager.FindVariable(a2lPath, "VC1CP019");
                Console.WriteLine($"Search for 'VC1CP019': {(result3.Found ? "FOUND" : "NOT FOUND")}");
                
                Console.WriteLine("\n=== Test Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
