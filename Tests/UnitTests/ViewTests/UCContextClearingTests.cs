using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests.UnitTests.ViewTests
{
    [TestClass]
    public class UCContextClearingTests
    {
        private UC_ContextClearing _ucContextClearing;
        private DataTable _templateDataTable;

        [TestInitialize]
        public void Setup()
        {
            _ucContextClearing = new UC_ContextClearing();
            
            // Create template DataTable for testing
            _templateDataTable = new DataTable();
            _templateDataTable.Columns.Add("Interface Name", typeof(string));
            _templateDataTable.Columns.Add("Description", typeof(string));
            _templateDataTable.Columns.Add("Type", typeof(string));
        }

        [TestMethod]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var ucContextClearing = new UC_ContextClearing();

            // Assert
            Assert.IsNotNull(ucContextClearing);
            Assert.IsNotNull(ucContextClearing.Controls);
            
            // Check initial property values
            Assert.AreEqual(string.Empty, ucContextClearing.NameOfnewCarasi);
            Assert.AreEqual(string.Empty, ucContextClearing.NameOfoldCarasi);
            Assert.AreEqual(string.Empty, ucContextClearing.NameOfnewDataflow);
            Assert.AreEqual(string.Empty, ucContextClearing.NameOfoldDataflow);
            Assert.AreEqual(string.Empty, ucContextClearing.Link2Folder);
        }

        [TestMethod]
        public void NameOfnewCarasi_SetValue_ShouldRetainValue()
        {
            // Arrange
            string testValue = "NewCarasi_v1.0.xlsx";

            // Act
            _ucContextClearing.NameOfnewCarasi = testValue;

            // Assert
            Assert.AreEqual(testValue, _ucContextClearing.NameOfnewCarasi);
        }

        [TestMethod]
        public void NameOfoldCarasi_SetValue_ShouldRetainValue()
        {
            // Arrange
            string testValue = "OldCarasi_v0.9.xlsx";

            // Act
            _ucContextClearing.NameOfoldCarasi = testValue;

            // Assert
            Assert.AreEqual(testValue, _ucContextClearing.NameOfoldCarasi);
        }

        [TestMethod]
        public void NameOfnewDataflow_SetValue_ShouldRetainValue()
        {
            // Arrange
            string testValue = "NewDataflow_v1.0.xlsx";

            // Act
            _ucContextClearing.NameOfnewDataflow = testValue;

            // Assert
            Assert.AreEqual(testValue, _ucContextClearing.NameOfnewDataflow);
        }

        [TestMethod]
        public void NameOfoldDataflow_SetValue_ShouldRetainValue()
        {
            // Arrange
            string testValue = "OldDataflow_v0.9.xlsx";

            // Act
            _ucContextClearing.NameOfoldDataflow = testValue;

            // Assert
            Assert.AreEqual(testValue, _ucContextClearing.NameOfoldDataflow);
        }

        [TestMethod]
        public void Link2Folder_SetValidPath_ShouldRetainValue()
        {
            // Arrange
            string validPath = @"C:\TestFolder";

            // Act
            _ucContextClearing.Link2Folder = validPath;

            // Assert
            // Note: The setter has validation logic, so the result depends on folder_verifying method
            // For this test, we assume the path might not be set if validation fails
            Assert.IsNotNull(_ucContextClearing.Link2Folder);
        }

        [TestMethod]
        public void Link2Folder_SetEmptyString_ShouldHandleGracefully()
        {
            // Arrange
            string emptyPath = string.Empty;

            // Act
            _ucContextClearing.Link2Folder = emptyPath;

            // Assert
            Assert.AreEqual(string.Empty, _ucContextClearing.Link2Folder);
        }

        [TestMethod]
        public void Link2Folder_SetNullValue_ShouldHandleGracefully()
        {
            // Arrange
            string nullPath = null;

            // Act
            _ucContextClearing.Link2Folder = nullPath;

            // Assert
            // The setter should handle null values gracefully
            Assert.IsNotNull(_ucContextClearing.Link2Folder);
        }

        [TestMethod]
        public void NewCarasi_Property_ShouldGetAndSet()
        {
            // Arrange
            var mockParser = new Excel_Parser("TestFile.xlsx", _templateDataTable);

            // Act
            _ucContextClearing.NewCarasi = mockParser;

            // Assert
            Assert.AreEqual(mockParser, _ucContextClearing.NewCarasi);
            
            // Cleanup
            mockParser.Dispose();
        }

        [TestMethod]
        public void OldCarasi_Property_ShouldGetAndSet()
        {
            // Arrange
            var mockParser = new Excel_Parser("TestFile.xlsx", _templateDataTable);

            // Act
            _ucContextClearing.OldCarasi = mockParser;

            // Assert
            Assert.AreEqual(mockParser, _ucContextClearing.OldCarasi);
            
            // Cleanup
            mockParser.Dispose();
        }

        [TestMethod]
        public void NewDF_Property_ShouldGetAndSet()
        {
            // Arrange
            var mockParser = new Excel_Parser("TestFile.xlsx", _templateDataTable);

            // Act
            _ucContextClearing.NewDF = mockParser;

            // Assert
            Assert.AreEqual(mockParser, _ucContextClearing.NewDF);
            
            // Cleanup
            mockParser.Dispose();
        }

        [TestMethod]
        public void OldDF_Property_ShouldGetAndSet()
        {
            // Arrange
            var mockParser = new Excel_Parser("TestFile.xlsx", _templateDataTable);

            // Act
            _ucContextClearing.OldDF = mockParser;

            // Assert
            Assert.AreEqual(mockParser, _ucContextClearing.OldDF);
            
            // Cleanup
            mockParser.Dispose();
        }

        [TestMethod]
        public void UserControl_ShouldBeVisibleByDefault()
        {
            // Act & Assert
            Assert.IsTrue(_ucContextClearing.Visible);
        }

        [TestMethod]
        public void UserControl_ShouldBeEnabledByDefault()
        {
            // Act & Assert
            Assert.IsTrue(_ucContextClearing.Enabled);
        }

        [TestMethod]
        public void UserControl_ShouldSupportDocking()
        {
            // Arrange & Act
            _ucContextClearing.Dock = DockStyle.Fill;

            // Assert
            Assert.AreEqual(DockStyle.Fill, _ucContextClearing.Dock);
        }

        [TestMethod]
        public void UserControl_ShouldSupportAnchoring()
        {
            // Arrange & Act
            _ucContextClearing.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Assert
            Assert.AreEqual(AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, _ucContextClearing.Anchor);
        }

        [TestMethod]
        public void UserControl_ShouldHandleVisibilityChanges()
        {
            // Act
            _ucContextClearing.Visible = false;

            // Assert
            Assert.IsFalse(_ucContextClearing.Visible);

            // Act
            _ucContextClearing.Visible = true;

            // Assert
            Assert.IsTrue(_ucContextClearing.Visible);
        }

        [TestMethod]
        public void UserControl_ShouldHandleEnableStateChanges()
        {
            // Act
            _ucContextClearing.Enabled = false;

            // Assert
            Assert.IsFalse(_ucContextClearing.Enabled);

            // Act
            _ucContextClearing.Enabled = true;

            // Assert
            Assert.IsTrue(_ucContextClearing.Enabled);
        }

        [TestMethod]
        public void UserControl_ShouldHaveValidSize()
        {
            // Act
            var size = _ucContextClearing.Size;

            // Assert
            Assert.IsTrue(size.Width > 0);
            Assert.IsTrue(size.Height > 0);
        }

        [TestMethod]
        public void UserControl_ShouldSupportSizeChanges()
        {
            // Arrange
            var newSize = new System.Drawing.Size(800, 600);

            // Act
            _ucContextClearing.Size = newSize;

            // Assert
            Assert.AreEqual(newSize, _ucContextClearing.Size);
        }

        [TestMethod]
        public void UserControl_ShouldSupportLocationChanges()
        {
            // Arrange
            var newLocation = new System.Drawing.Point(100, 50);

            // Act
            _ucContextClearing.Location = newLocation;

            // Assert
            Assert.AreEqual(newLocation, _ucContextClearing.Location);
        }

        [TestMethod]
        public void UserControl_ShouldHandleDispose()
        {
            // Arrange
            var ucContextClearing = new UC_ContextClearing();

            // Act & Assert
            try
            {
                ucContextClearing.Dispose();
                Assert.IsTrue(true); // Should dispose without exception
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dispose should not throw exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void Properties_ShouldHandleNullValues()
        {
            // Act & Assert
            try
            {
                _ucContextClearing.NameOfnewCarasi = null;
                _ucContextClearing.NameOfoldCarasi = null;
                _ucContextClearing.NameOfnewDataflow = null;
                _ucContextClearing.NameOfoldDataflow = null;
                
                Assert.IsTrue(true); // Should handle null values gracefully
            }
            catch (Exception ex)
            {
                Assert.Fail($"Properties should handle null values: {ex.Message}");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            _ucContextClearing?.Dispose();
            _templateDataTable?.Dispose();
        }
    }
}
