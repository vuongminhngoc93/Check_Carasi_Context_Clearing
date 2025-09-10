using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests.UnitTests.LibraryTests
{
    [TestClass]
    public class ExcelParserTests
    {
        private DataTable _templateDataTable;
        private string _testFilePath;

        [TestInitialize]
        public void Setup()
        {
            // Tạo template DataTable giống như trong ứng dụng thực
            _templateDataTable = new DataTable();
            _templateDataTable.Columns.Add("Interface Name", typeof(string));
            _templateDataTable.Columns.Add("Input", typeof(string));
            _templateDataTable.Columns.Add("Output", typeof(string));
            _templateDataTable.Columns.Add("Description", typeof(string));
            _templateDataTable.Columns.Add("Unit", typeof(string));
            _templateDataTable.Columns.Add("Min Value", typeof(string));
            _templateDataTable.Columns.Add("Max Value", typeof(string));
            _templateDataTable.Columns.Add("Resolution", typeof(string));
            _templateDataTable.Columns.Add("SW Type", typeof(string));
            _templateDataTable.Columns.Add("MM Type", typeof(string));

            _testFilePath = @"TestData\SampleCarasi.xlsx";
        }

        [TestMethod]
        public void Constructor_ValidFilePath_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var parser = new Excel_Parser(_testFilePath, _templateDataTable);

            // Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual("SampleCarasi.xlsx", parser.Lb_NameOfFile);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullFilePath_ShouldThrowException()
        {
            // Arrange & Act & Assert
            var parser = new Excel_Parser(null, _templateDataTable);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullTemplate_ShouldThrowException()
        {
            // Arrange & Act & Assert
            var parser = new Excel_Parser(_testFilePath, null);
        }

        [TestMethod]
        public void SearchVariable_ValidCarasiInterface_ShouldReturnCorrectData()
        {
            // Arrange
            var parser = new Excel_Parser("TestData\\TestCarasi.xlsx", _templateDataTable);
            string testInterface = "TestInterface_001";

            // Act
            parser.search_Variable(testInterface);

            // Assert
            Assert.IsNotNull(parser.CarasiInterface);
            Assert.AreEqual(testInterface, parser.Lb_Name);
            Assert.IsTrue(parser.IsFound);
        }

        [TestMethod]
        public void SearchVariable_ValidDataflowInterface_ShouldReturnCorrectData()
        {
            // Arrange
            var parser = new Excel_Parser("TestData\\TestDataflow.xlsx", _templateDataTable);
            string testInterface = "TestDataflow_001";

            // Act
            parser.search_Variable(testInterface);

            // Assert
            Assert.IsNotNull(parser.DataflowInterface);
            Assert.AreEqual(testInterface, parser.Lb_Name);
            Assert.IsTrue(parser.IsFound);
        }

        [TestMethod]
        public void SearchVariable_NonExistentInterface_ShouldReturnNotFound()
        {
            // Arrange
            var parser = new Excel_Parser(_testFilePath, _templateDataTable);
            string nonExistentInterface = "NonExistent_Interface_999";

            // Act
            parser.search_Variable(nonExistentInterface);

            // Assert
            Assert.IsFalse(parser.IsFound);
        }

        [TestMethod]
        public void SearchVariable_EmptyInterfaceName_ShouldHandleGracefully()
        {
            // Arrange
            var parser = new Excel_Parser(_testFilePath, _templateDataTable);

            // Act
            parser.search_Variable(string.Empty);

            // Assert
            Assert.IsFalse(parser.IsFound);
        }

        [TestMethod]
        public void IsExistCarasi_ExistingInterface_ShouldReturnTrue()
        {
            // Arrange
            var parser = new Excel_Parser("TestData\\TestCarasi.xlsx", _templateDataTable);

            // Act
            bool result = parser._IsExist_Carasi("TestInterface_001");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsExistCarasi_NonExistingInterface_ShouldReturnFalse()
        {
            // Arrange
            var parser = new Excel_Parser("TestData\\TestCarasi.xlsx", _templateDataTable);

            // Act
            bool result = parser._IsExist_Carasi("NonExistent_Interface");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ClearData_ShouldResetAllProperties()
        {
            // Arrange
            var parser = new Excel_Parser(_testFilePath, _templateDataTable);
            parser.search_Variable("TestInterface");

            // Act
            parser.clearData();

            // Assert
            Assert.AreEqual(string.Empty, parser.Lb_Name);
            Assert.IsFalse(parser.IsFound);
        }

        [TestMethod]
        public void Dispose_ShouldCleanupResources()
        {
            // Arrange
            var parser = new Excel_Parser(_testFilePath, _templateDataTable);

            // Act & Assert (no exception should be thrown)
            parser.Dispose();
            
            // Verify object is disposed properly
            Assert.IsTrue(true); // If we reach here without exception, disposal worked
        }

        [TestCleanup]
        public void Cleanup()
        {
            _templateDataTable?.Dispose();
        }
    }
}
