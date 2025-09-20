using System;
using System.Data;
using System.IO;

namespace Check_carasi_DF_ContextClearing.Tests
{
    class CreateExcelFromCSV
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Creating Excel from CSV ===");
            
            string csvPath = @".\TestData\SampleCarasi.csv";
            string excelPath = @".\TestData\SampleCarasi.xlsx";
            
            if (!File.Exists(csvPath))
            {
                Console.WriteLine("CSV file not found!");
                return;
            }
            
            try
            {
                // Read CSV data
                string[] lines = File.ReadAllLines(csvPath);
                Console.WriteLine("Read " + lines.Length + " lines from CSV");
                
                // Create DataTable
                DataTable dt = new DataTable();
                
                // Add columns from header
                if (lines.Length > 0)
                {
                    string[] headers = lines[0].Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header.Trim());
                    }
                    Console.WriteLine("Created " + dt.Columns.Count + " columns");
                }
                
                // Add rows
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');
                    DataRow row = dt.NewRow();
                    
                    for (int j = 0; j < Math.Min(values.Length, dt.Columns.Count); j++)
                    {
                        row[j] = values[j].Trim();
                    }
                    
                    dt.Rows.Add(row);
                }
                
                Console.WriteLine("Added " + dt.Rows.Count + " data rows");
                
                // For now, just create a simple text-based Excel simulation
                // In a real scenario, you'd use EPPlus or similar library
                
                // Create a dummy Excel file with proper structure
                File.WriteAllText(excelPath + ".tmp", "This would be a real Excel file with binary format");
                
                Console.WriteLine("Excel file creation simulated");
                Console.WriteLine("Note: In production, use EPPlus library for real Excel creation");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
