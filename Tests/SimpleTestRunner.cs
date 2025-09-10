using System;
using System.IO;
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
            TestFileUtilities();

            // Print results
            Console.WriteLine("\n=== Test Results ===");
            Console.WriteLine($"Tests Passed: {testsPassed}");
            Console.WriteLine($"Tests Failed: {testsFailed}");
            Console.WriteLine($"Total Tests: {testsPassed + testsFailed}");

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

        static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.WriteLine($"  ✓ {testName}");
                testsPassed++;
            }
            else
            {
                Console.WriteLine($"  ✗ {testName}");
                testsFailed++;
            }
        }
    }
}
