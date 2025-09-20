using System;
using System.IO;
using Check_carasi_DF_ContextClearing;

class OLEDBTestFix
{
    static void Main()
    {
        Console.WriteLine("=== OLEDB Provider Fix Test ===");
        
        string inputFolder = @"Input";
        if (!Directory.Exists(inputFolder))
        {
            Console.WriteLine("ERROR: Input folder not found!");
            return;
        }
        
        string[] excelFiles = Directory.GetFiles(inputFolder, "*.xls*");
        Console.WriteLine($"Found {excelFiles.Length} Excel files to test");
        
        int successCount = 0;
        int errorCount = 0;
        
        foreach (string file in excelFiles)
        {
            Console.WriteLine($"\nTesting: {Path.GetFileName(file)}");
            
            try
            {
                using (var lib = new Lib_OLEDB_Excel())
                {
                    lib.FilePath = file;
                    Console.WriteLine($"  Connection String: {lib.ConnectionString}");
                    
                    // Test connection
                    var connection = lib.Connection;
                    Console.WriteLine("  Connection: SUCCESS");
                    
                    // Test schema reading
                    var schema = lib.GetSchema();
                    Console.WriteLine($"  Schema: {schema.Rows.Count} tables found");
                    
                    successCount++;
                    Console.WriteLine("  ✓ PASSED");
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                Console.WriteLine($"  ✗ FAILED: {ex.Message}");
            }
        }
        
        Console.WriteLine($"\n=== Results ===");
        Console.WriteLine($"Success: {successCount}");
        Console.WriteLine($"Errors: {errorCount}");
        Console.WriteLine($"Success Rate: {(successCount * 100.0 / excelFiles.Length):F1}%");
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
