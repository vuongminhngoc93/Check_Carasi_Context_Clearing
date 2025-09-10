using System;
using System.Data;
using System.IO;
using System.Text;

namespace Check_carasi_DF_ContextClearing.Tests
{
    /// <summary>
    /// Helper class for creating and managing test data
    /// </summary>
    public static class TestDataHelper
    {
        public static string TestDataDirectory => Path.Combine(Path.GetTempPath(), "CarasiDFTestData");

        /// <summary>
        /// Creates a test directory with all necessary test files
        /// </summary>
        /// <returns>Path to the created test directory</returns>
        public static string CreateTestDirectory()
        {
            string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testDir);
            return testDir;
        }

        /// <summary>
        /// Creates a mock Excel file with basic structure
        /// </summary>
        /// <param name="filePath">Path where to create the file</param>
        /// <param name="fileType">Type of file (Carasi or Dataflow)</param>
        /// <param name="hasData">Whether to include sample data</param>
        public static void CreateMockExcelFile(string filePath, string fileType = "Carasi", bool hasData = true)
        {
            StringBuilder content = new StringBuilder();
            
            if (fileType.ToLower().Contains("carasi"))
            {
                content.AppendLine("Interface Name,Input,Output,Description,Unit,Min Value,Max Value,Resolution,SW Type,MM Type");
                if (hasData)
                {
                    content.AppendLine("TestInterface_001,Input1;Input2,Output1,Test interface for unit testing,V,0,100,0.1,UINT16,CAN");
                    content.AppendLine("TestInterface_002,Input3,Output2,Another test interface,A,0,50,0.01,SINT16,LIN");
                    content.AppendLine("TestInterface_003,,Output3,Output only interface,°C,-40,150,1,UINT8,ETH");
                }
            }
            else if (fileType.ToLower().Contains("dataflow"))
            {
                content.AppendLine("Status,Description,RTE Direction,Mapping Type,Pseudo Code,System Constant,FC Name,Producer,Consumers");
                if (hasData)
                {
                    content.AppendLine("Active,Test dataflow interface,Input,Direct,TestCode1,CONST1,FC_Test1,Producer1,Consumer1;Consumer2");
                    content.AppendLine("Active,Another dataflow interface,Output,Indirect,TestCode2,CONST2,FC_Test2,Producer2,Consumer3");
                    content.AppendLine("Inactive,Deprecated interface,Bidirectional,None,TestCode3,CONST3,FC_Test3,Producer3,Consumer4");
                }
            }

            File.WriteAllText(filePath, content.ToString());
        }

        /// <summary>
        /// Creates a mock A2L file with basic ASAP2 structure
        /// </summary>
        /// <param name="filePath">Path where to create the A2L file</param>
        /// <param name="includeTestMeasurements">Whether to include test measurements</param>
        public static void CreateMockA2LFile(string filePath, bool includeTestMeasurements = true)
        {
            StringBuilder a2lContent = new StringBuilder();
            
            a2lContent.AppendLine("ASAP2_VERSION 1 60");
            a2lContent.AppendLine("PROJECT TestProject \"Test A2L File for Unit Testing\"");
            a2lContent.AppendLine("BEGIN_PROJECT");
            a2lContent.AppendLine("    MODULE TestModule \"Test Module\"");
            a2lContent.AppendLine("    BEGIN_MODULE");
            
            if (includeTestMeasurements)
            {
                a2lContent.AppendLine("        MEASUREMENT TestInterface_001 \"Test Interface 1\"");
                a2lContent.AppendLine("        UINT16 0x1000 1 0 65535");
                a2lContent.AppendLine("        ");
                a2lContent.AppendLine("        MEASUREMENT TestInterface_002 \"Test Interface 2\"");
                a2lContent.AppendLine("        SINT16 0x1002 1 -32768 32767");
                a2lContent.AppendLine("        ");
                a2lContent.AppendLine("        MEASUREMENT TestInterface_003 \"Test Interface 3\"");
                a2lContent.AppendLine("        UINT8 0x1004 1 0 255");
            }
            
            a2lContent.AppendLine("    END_MODULE");
            a2lContent.AppendLine("END_PROJECT");

            File.WriteAllText(filePath, a2lContent.ToString());
        }

        /// <summary>
        /// Creates a mock ARXML file with basic AUTOSAR structure
        /// </summary>
        /// <param name="filePath">Path where to create the ARXML file</param>
        /// <param name="isExternFile">Whether this is an _Extern.arxml file</param>
        public static void CreateMockARXMLFile(string filePath, bool isExternFile = true)
        {
            StringBuilder arxmlContent = new StringBuilder();
            
            arxmlContent.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            arxmlContent.AppendLine("<AUTOSAR xmlns=\"http://autosar.org/schema/r4.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            arxmlContent.AppendLine("    <AR-PACKAGES>");
            arxmlContent.AppendLine("        <AR-PACKAGE>");
            arxmlContent.AppendLine("            <SHORT-NAME>TestPackage</SHORT-NAME>");
            arxmlContent.AppendLine("            <ELEMENTS>");
            
            if (isExternFile)
            {
                arxmlContent.AppendLine("                <SW-COMPONENT-TYPE>");
                arxmlContent.AppendLine("                    <SHORT-NAME>ExternalComponent</SHORT-NAME>");
                arxmlContent.AppendLine("                    <PORTS>");
                arxmlContent.AppendLine("                        <P-PORT-PROTOTYPE>");
                arxmlContent.AppendLine("                            <SHORT-NAME>TestInterface_001</SHORT-NAME>");
                arxmlContent.AppendLine("                        </P-PORT-PROTOTYPE>");
                arxmlContent.AppendLine("                    </PORTS>");
                arxmlContent.AppendLine("                </SW-COMPONENT-TYPE>");
            }
            else
            {
                arxmlContent.AppendLine("                <SW-COMPONENT-TYPE>");
                arxmlContent.AppendLine("                    <SHORT-NAME>InternalComponent</SHORT-NAME>");
                arxmlContent.AppendLine("                </SW-COMPONENT-TYPE>");
            }
            
            arxmlContent.AppendLine("            </ELEMENTS>");
            arxmlContent.AppendLine("        </AR-PACKAGE>");
            arxmlContent.AppendLine("    </AR-PACKAGES>");
            arxmlContent.AppendLine("</AUTOSAR>");

            File.WriteAllText(filePath, arxmlContent.ToString());
        }

        /// <summary>
        /// Creates a template DataTable similar to the one used in the application
        /// </summary>
        /// <returns>Configured DataTable template</returns>
        public static DataTable CreateTemplateDataTable()
        {
            DataTable template = new DataTable();
            
            template.Columns.Add("Interface Name", typeof(string));
            template.Columns.Add("Input", typeof(string));
            template.Columns.Add("Output", typeof(string));
            template.Columns.Add("Description", typeof(string));
            template.Columns.Add("Unit", typeof(string));
            template.Columns.Add("Min Value", typeof(string));
            template.Columns.Add("Max Value", typeof(string));
            template.Columns.Add("Resolution", typeof(string));
            template.Columns.Add("Initialisation", typeof(string));
            template.Columns.Add("SW Type", typeof(string));
            template.Columns.Add("MM Type", typeof(string));
            template.Columns.Add("Conversion", typeof(string));
            template.Columns.Add("Comments", typeof(string));
            template.Columns.Add("Compute Details", typeof(string));

            return template;
        }

        /// <summary>
        /// Creates a full test environment with all necessary files and directories
        /// </summary>
        /// <param name="baseDirectory">Base directory for test environment</param>
        /// <returns>Test environment information</returns>
        public static TestEnvironment CreateFullTestEnvironment(string baseDirectory = null)
        {
            if (baseDirectory == null)
                baseDirectory = CreateTestDirectory();

            var environment = new TestEnvironment
            {
                BaseDirectory = baseDirectory,
                NewCarasiFile = Path.Combine(baseDirectory, "NewCarasi.xlsx"),
                OldCarasiFile = Path.Combine(baseDirectory, "OldCarasi.xlsx"),
                NewDataflowFile = Path.Combine(baseDirectory, "NewDataflow.xlsx"),
                OldDataflowFile = Path.Combine(baseDirectory, "OldDataflow.xlsx"),
                A2LFile = Path.Combine(baseDirectory, "TestFile.a2l"),
                ARXMLDirectory = Path.Combine(baseDirectory, "arxml"),
                ExternARXMLFile = Path.Combine(baseDirectory, "arxml", "TestModule_Extern.arxml"),
                RegularARXMLFile = Path.Combine(baseDirectory, "arxml", "TestModule.arxml")
            };

            // Create directories
            Directory.CreateDirectory(environment.ARXMLDirectory);

            // Create files
            CreateMockExcelFile(environment.NewCarasiFile, "Carasi", true);
            CreateMockExcelFile(environment.OldCarasiFile, "Carasi", true);
            CreateMockExcelFile(environment.NewDataflowFile, "Dataflow", true);
            CreateMockExcelFile(environment.OldDataflowFile, "Dataflow", true);
            CreateMockA2LFile(environment.A2LFile, true);
            CreateMockARXMLFile(environment.ExternARXMLFile, true);
            CreateMockARXMLFile(environment.RegularARXMLFile, false);

            return environment;
        }

        /// <summary>
        /// Cleans up a test directory and all its contents
        /// </summary>
        /// <param name="directoryPath">Path to directory to clean up</param>
        public static void CleanupTestDirectory(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Verifies that a test file exists and has content
        /// </summary>
        /// <param name="filePath">Path to file to verify</param>
        /// <returns>True if file exists and has content</returns>
        public static bool VerifyTestFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length > 0;
        }

        /// <summary>
        /// Creates a DataRow with sample Carasi interface data
        /// </summary>
        /// <param name="table">DataTable to create row for</param>
        /// <param name="interfaceName">Name of the interface</param>
        /// <returns>Populated DataRow</returns>
        public static DataRow CreateSampleCarasiRow(DataTable table, string interfaceName)
        {
            DataRow row = table.NewRow();
            row["Interface Name"] = interfaceName;
            row["Input"] = "Input1;Input2";
            row["Output"] = "Output1";
            row["Description"] = $"Test interface {interfaceName}";
            row["Unit"] = "V";
            row["Min Value"] = "0";
            row["Max Value"] = "100";
            row["Resolution"] = "0.1";
            row["SW Type"] = "UINT16";
            row["MM Type"] = "CAN";
            return row;
        }

        /// <summary>
        /// Creates a DataRow with sample Dataflow interface data
        /// </summary>
        /// <param name="table">DataTable to create row for</param>
        /// <param name="interfaceName">Name of the interface</param>
        /// <returns>Populated DataRow</returns>
        public static DataRow CreateSampleDataflowRow(DataTable table, string interfaceName)
        {
            DataRow row = table.NewRow();
            row["Status"] = "Active";
            row["Description"] = $"Test dataflow {interfaceName}";
            row["RTE Direction"] = "Input";
            row["Mapping Type"] = "Direct";
            row["FC Name"] = $"FC_{interfaceName}";
            row["Producer"] = $"Producer_{interfaceName}";
            row["Consumers"] = $"Consumer1_{interfaceName};Consumer2_{interfaceName}";
            return row;
        }
    }

    /// <summary>
    /// Container for test environment information
    /// </summary>
    public class TestEnvironment
    {
        public string BaseDirectory { get; set; }
        public string NewCarasiFile { get; set; }
        public string OldCarasiFile { get; set; }
        public string NewDataflowFile { get; set; }
        public string OldDataflowFile { get; set; }
        public string A2LFile { get; set; }
        public string ARXMLDirectory { get; set; }
        public string ExternARXMLFile { get; set; }
        public string RegularARXMLFile { get; set; }

        public void Cleanup()
        {
            TestDataHelper.CleanupTestDirectory(BaseDirectory);
        }
    }
}
