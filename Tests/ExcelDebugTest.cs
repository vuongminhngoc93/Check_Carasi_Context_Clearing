using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace Check_carasi_DF_ContextClearing.Tests
{
    class ExcelDebugTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Excel OLEDB Debug Test ===");
            
            string excelPath = @".\TestData\SampleCarasi.xlsx";
            string fullPath = Path.GetFullPath(excelPath);
            
            Console.WriteLine("Excel file path: " + fullPath);
            Console.WriteLine("File exists: " + File.Exists(fullPath));
            
            if (File.Exists(fullPath))
            {
                Console.WriteLine("File size: " + new FileInfo(fullPath).Length + " bytes");
                
                // Test different connection strings
                string[] connectionStrings = {
                    "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;'",
                    "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 12.0;HDR=YES;'",
                    "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fullPath + ";Extended Properties='Excel 8.0;HDR=YES;'",
                    "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + fullPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;'"
                };
                
                for (int i = 0; i < connectionStrings.Length; i++)
                {
                    Console.WriteLine("\n--- Testing Connection String " + (i + 1) + " ---");
                    Console.WriteLine(connectionStrings[i]);
                    
                    try
                    {
                        using (var connection = new OleDbConnection(connectionStrings[i]))
                        {
                            connection.Open();
                            Console.WriteLine("✓ Connection successful!");
                            
                            // Try to get schema info
                            try
                            {
                                DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                Console.WriteLine("✓ Schema tables retrieved: " + schema.Rows.Count + " tables");
                                
                                foreach (DataRow row in schema.Rows)
                                {
                                    Console.WriteLine("  Table: " + row["TABLE_NAME"]);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("✗ Schema error: " + ex.Message);
                            }
                            
                            connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("✗ Connection failed: " + ex.Message);
                    }
                }
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
