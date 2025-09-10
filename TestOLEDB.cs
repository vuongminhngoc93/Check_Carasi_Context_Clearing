using System;
using System.Data.OleDb;
using System.IO;

namespace Check_carasi_DF_ContextClearing
{
    class TestOLEDB
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== OLEDB Connection Test ===");
            Console.WriteLine("Running as " + (Environment.Is64BitProcess ? "64-bit" : "32-bit") + " process");
            
            string testExcelPath = @".\TestData\SampleCarasi.xlsx";
            
            if (!File.Exists(testExcelPath))
            {
                Console.WriteLine("Test file not found: " + testExcelPath);
                Console.WriteLine("Creating dummy Excel file path for connection test...");
                testExcelPath = @"C:\temp\dummy.xlsx"; // Just for connection string test
            }
            
            // Test ACE 12.0 provider
            string connectionString12 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + testExcelPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;'";
            Console.WriteLine("\nTesting ACE 12.0: " + connectionString12);
            
            try
            {
                using (var connection = new OleDbConnection(connectionString12))
                {
                    connection.Open();
                    Console.WriteLine("✅ ACE 12.0 connection SUCCESS!");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ACE 12.0 connection FAILED: " + ex.Message);
            }
            
            // Test ACE 16.0 provider  
            string connectionString16 = "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + testExcelPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;'";
            Console.WriteLine("\nTesting ACE 16.0: " + connectionString16);
            
            try
            {
                using (var connection = new OleDbConnection(connectionString16))
                {
                    connection.Open();
                    Console.WriteLine("✅ ACE 16.0 connection SUCCESS!");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ACE 16.0 connection FAILED: " + ex.Message);
            }
            
            Console.WriteLine("\n=== Test Complete ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
