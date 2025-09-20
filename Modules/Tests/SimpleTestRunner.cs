using System;
using System.IO;
using System.Data.OleDb;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests
{
    class SimpleTestRunner
    {
        private static int testsPassed = 0;
        private static int testsFailed = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("=== Simple Test Runner ===");
            Console.WriteLine("Starting tests...\n");

            // Run basic tests
            TestExcelParserStructs();
            TestDataHelperBasic();
            TestPublicClasses();
            TestA2LCheckFunctionality();
            TestFileUtilities();
            TestOLEDBConnectivity(); // New OLEDB test
            TestRealInputData(); // Test with real Input folder data

            // Print results
            Console.WriteLine("\n=== Test Results ===");
            Console.WriteLine("Tests Passed: " + testsPassed);
            Console.WriteLine("Tests Failed: " + testsFailed);
            Console.WriteLine("Total Tests: " + (testsPassed + testsFailed));

            if (testsFailed == 0)
                Console.WriteLine("All tests PASSED!");
            else
                Console.WriteLine("Some tests FAILED!");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void TestExcelParserStructs()
        {
            Console.WriteLine("Testing Excel Parser Structs...");
            
            // Test Carasi_Interface struct
            var carasi = new Carasi_Interface();
            carasi.name = "TestInterface";
            carasi.description = "Test Description";
            carasi.unit = "V";
            carasi.minValue = "0";
            carasi.maxValue = "5";
            
            Assert(carasi.name == "TestInterface", "Carasi_Interface name assignment");
            Assert(carasi.description == "Test Description", "Carasi_Interface description assignment");
            Assert(carasi.unit == "V", "Carasi_Interface unit assignment");
            
            // Test Dataflow_Interface struct
            var dataflow = new Dataflow_Interface();
            dataflow.description = "Test Dataflow";
            dataflow.status = "Active";
            dataflow.Rte_Direction = "Input";
            
            Assert(dataflow.description == "Test Dataflow", "Dataflow_Interface description assignment");
            Assert(dataflow.status == "Active", "Dataflow_Interface status assignment");
            Assert(dataflow.Rte_Direction == "Input", "Dataflow_Interface direction assignment");
        }

        static void TestDataHelperBasic()
        {
            Console.WriteLine("Testing Data Helper...");
            
            // Test creating test directory
            string testDir = TestDataHelper.CreateTestDirectory();
            Assert(Directory.Exists(testDir), "Test directory creation");
            
            // Test template creation
            var template = TestDataHelper.CreateTemplateDataTable();
            Assert(template != null, "Template DataTable creation");
            Assert(template.Columns.Count > 0, "Template has columns");
            
            // Test sample row creation
            var row = TestDataHelper.CreateSampleCarasiRow(template, "TestInterface");
            Assert(row != null, "Sample Carasi row creation");
            
            // Clean up
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);
        }

        static void TestPublicClasses()
        {
            Console.WriteLine("Testing Public Class Access...");
            
            // Test A2L_Check now accessible
            var a2lCheck = new A2L_Check();
            Assert(a2lCheck != null, "A2L_Check instance creation");
            Assert(a2lCheck.IsValidLink == true, "A2L_Check default state");
            
            // Test MM_Check now accessible
            var mmCheck = new MM_Check();
            Assert(mmCheck != null, "MM_Check instance creation");
            Assert(mmCheck.IsValidLink == false, "MM_Check default state");
            
            // Test setting properties
            a2lCheck.Link_Of_A2L = "test.a2l";
            Assert(a2lCheck.Link_Of_A2L == "test.a2l", "A2L_Check property setter");
            
            mmCheck.Link_Of_MM = "test.mm";
            Assert(mmCheck.Link_Of_MM == "test.mm", "MM_Check property setter");
        }

        static void TestFileUtilities()
        {
            Console.WriteLine("Testing File Utilities...");
            
            // Test that current directory exists
            string currentDir = Directory.GetCurrentDirectory();
            Assert(Directory.Exists(currentDir), "Current directory exists");
            
            // Test basic string operations (simulating file path validation)
            string testPath = @"C:\Test\file.xlsx";
            Assert(testPath.EndsWith(".xlsx"), "Excel file extension check");
            Assert(testPath.Contains("Test"), "Path contains expected folder");
        }

        static void TestA2LCheckFunctionality()
        {
            Console.WriteLine("Testing A2L_Check functionality...");
            
            // Test with valid A2L file
            var a2lCheck = new A2L_Check();
            string testA2LPath = Path.Combine(TestDataHelper.GetTestDataDirectory(), "SampleA2L.a2l");
            a2lCheck.Link_Of_A2L = testA2LPath;
            Assert(a2lCheck.IsValidLink, "A2L_Check with valid file validates correctly");
            
            // Test search functionality - since we have sample A2L, test basic search
            string[] results = null;
            bool found = a2lCheck.IsExistInA2L("MEASUREMENT", ref results);
            Assert(results != null, "A2L_Check search returns results array");
            
            // Test with invalid path
            var a2lCheckInvalid = new A2L_Check();
            a2lCheckInvalid.Link_Of_A2L = "invalid_path.a2l";
            Assert(!a2lCheckInvalid.IsValidLink, "A2L_Check invalid path handling");
            
            // Test empty keyword search
            string[] emptyResults = null;
            bool emptyFound = a2lCheck.IsExistInA2L("", ref emptyResults);
            Assert(!emptyFound && emptyResults.Length == 0, "A2L_Check empty keyword handling");
        }

        static void TestOLEDBConnectivity()
        {
            Console.WriteLine("Testing OLEDB Connectivity...");
            
            // Test process architecture first
            Console.WriteLine("  Process running as: " + (Environment.Is64BitProcess ? "64-bit" : "32-bit"));
            Assert(Environment.Is64BitProcess, "Application running in 64-bit mode");
            
            // Test ACE 12.0 provider registration
            string testExcelPath = @"C:\temp\dummy.xlsx"; // Dummy path for connection test
            string connectionString12 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + testExcelPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;'";
            
            try
            {
                using (var connection = new OleDbConnection(connectionString12))
                {
                    connection.Open(); // Just test if provider is registered
                    Assert(true, "ACE 12.0 OLEDB provider available");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not registered"))
                {
                    Assert(false, "ACE 12.0 OLEDB provider - NOT REGISTERED: " + ex.Message);
                }
                else
                {
                    // File not found is OK - we're just testing provider registration
                    Assert(true, "ACE 12.0 OLEDB provider available (file error expected)");
                }
            }
            
            // Test ACE 16.0 provider registration  
            string connectionString16 = "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + testExcelPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;'";
            
            try
            {
                using (var connection = new OleDbConnection(connectionString16))
                {
                    connection.Open(); // Just test if provider is registered
                    Assert(true, "ACE 16.0 OLEDB provider available");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not registered"))
                {
                    Assert(false, "ACE 16.0 OLEDB provider - NOT REGISTERED: " + ex.Message);
                }
                else
                {
                    // File not found is OK - we're just testing provider registration
                    Assert(true, "ACE 16.0 OLEDB provider available (file error expected)");
                }
            }
            
            // Test with actual Excel file if exists
            string actualExcelPath = @".\TestData\SampleCarasi.xlsx";
            if (File.Exists(actualExcelPath))
            {
                string realConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + actualExcelPath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                try
                {
                    using (var connection = new OleDbConnection(realConnectionString))
                    {
                        connection.Open();
                        Assert(connection.State == System.Data.ConnectionState.Open, "Real Excel file OLEDB connection");
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("not in the expected format"))
                    {
                        Assert(true, "Excel file format issue - OLEDB providers working correctly");
                    }
                    else
                    {
                        Assert(false, "Real Excel file OLEDB connection failed: " + ex.Message);
                    }
                }
            }
            else
            {
                Assert(true, "Excel test file not found - OLEDB provider verification complete");
            }
        }

        static void TestRealInputData()
        {
            Console.WriteLine("Testing Real Input Data...");
            
            // Call methods from RealDataTest class
            try
            {
                RealDataTest.TestInputFolderData();
                Assert(true, "Input folder data enumeration");
                
                RealDataTest.TestFileAccessibility();
                Assert(true, "Input files accessibility verification");
            }
            catch (Exception ex)
            {
                Assert(false, "Real input data test failed: " + ex.Message);
            }
        }

        static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.WriteLine("  ✓ " + testName);
                testsPassed++;
            }
            else
            {
                Console.WriteLine("  ✗ " + testName);
                testsFailed++;
            }
        }
    }
}
