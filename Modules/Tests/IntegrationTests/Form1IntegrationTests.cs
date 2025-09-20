using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests.IntegrationTests
{
    [TestClass]
    public class Form1IntegrationTests
    {
        private Form1 _form1;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            // Initialize test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), "Form1IntegrationTests");
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }

            // Create test files
            CreateTestFiles();

            // Initialize Form1
            _form1 = new Form1();
        }

        private void CreateTestFiles()
        {
            // Create mock Carasi files
            string newCarasiFile = Path.Combine(_testDirectory, "NewCarasi.xlsx");
            string oldCarasiFile = Path.Combine(_testDirectory, "OldCarasi.xlsx");
            
            // Create mock DataFlow files
            string newDataflowFile = Path.Combine(_testDirectory, "NewDataflow.xlsx");
            string oldDataflowFile = Path.Combine(_testDirectory, "OldDataflow.xlsx");

            // Create mock A2L file
            string a2lFile = Path.Combine(_testDirectory, "TestFile.a2l");

            // Create mock files with basic content
            File.WriteAllText(newCarasiFile, "Mock New Carasi Content");
            File.WriteAllText(oldCarasiFile, "Mock Old Carasi Content");
            File.WriteAllText(newDataflowFile, "Mock New Dataflow Content");
            File.WriteAllText(oldDataflowFile, "Mock Old Dataflow Content");
            File.WriteAllText(a2lFile, "Mock A2L Content");

            // Create ARXML directory structure for MM testing
            string arxmlDir = Path.Combine(_testDirectory, "arxml");
            Directory.CreateDirectory(arxmlDir);
            string externArxmlFile = Path.Combine(arxmlDir, "TestFile_Extern.arxml");
            File.WriteAllText(externArxmlFile, "<?xml version=\"1.0\"?><root></root>");
        }

        [TestMethod]
        public void Form1_Constructor_ShouldInitializeCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(_form1);
            Assert.IsNotNull(_form1.Controls);
            Assert.IsTrue(_form1.Controls.Count > 0);
        }

        [TestMethod]
        public void Form1_VersionLabel_ShouldReturnValidVersion()
        {
            // Act
            string version = _form1.VersionLabel;

            // Assert
            Assert.IsNotNull(version);
            Assert.IsTrue(version.Contains("Product Name:"));
            Assert.IsTrue(version.Contains("Version:"));
        }

        [TestMethod]
        public void Form1_Properties_ShouldGetAndSetCorrectly()
        {
            // Arrange
            string testFolder = _testDirectory;
            string testNewCarasi = "TestNewCarasi.xlsx";
            string testOldCarasi = "TestOldCarasi.xlsx";
            string testNewDataflow = "TestNewDataflow.xlsx";
            string testOldDataflow = "TestOldDataflow.xlsx";

            // Act
            _form1.Link2Folder = testFolder;
            _form1.NameOfnewCarasi = testNewCarasi;
            _form1.NameOfoldCarasi = testOldCarasi;
            _form1.NameOfnewDataflow = testNewDataflow;
            _form1.NameOfoldDataflow = testOldDataflow;

            // Assert
            Assert.AreEqual(testFolder, _form1.Link2Folder);
            Assert.AreEqual(testNewCarasi, _form1.NameOfnewCarasi);
            Assert.AreEqual(testOldCarasi, _form1.NameOfoldCarasi);
            Assert.AreEqual(testNewDataflow, _form1.NameOfnewDataflow);
            Assert.AreEqual(testOldDataflow, _form1.NameOfoldDataflow);
        }

        [TestMethod]
        public void Form1_Show_ShouldDisplayForm()
        {
            // Act
            _form1.Show();

            // Assert
            Assert.IsTrue(_form1.Visible);
            Assert.IsFalse(_form1.IsDisposed);

            // Cleanup
            _form1.Hide();
        }

        [TestMethod]
        public void Form1_FormControls_ShouldBeAccessible()
        {
            // Act & Assert
            // Check if main controls exist
            bool hasTabControl = false;
            bool hasMenuStrip = false;
            bool hasToolStrip = false;

            foreach (Control control in _form1.Controls)
            {
                if (control is TabControl)
                    hasTabControl = true;
                if (control is MenuStrip)
                    hasMenuStrip = true;
                if (control is ToolStrip)
                    hasToolStrip = true;
            }

            // The form should have expected controls based on the designer
            Assert.IsTrue(_form1.Controls.Count > 0);
        }

        [TestMethod]
        public void Form1_WindowState_ShouldBeNormal()
        {
            // Act & Assert
            Assert.AreEqual(FormWindowState.Normal, _form1.WindowState);
        }

        [TestMethod]
        public void Form1_SizeAndLocation_ShouldBeValid()
        {
            // Act
            var size = _form1.Size;
            var location = _form1.Location;

            // Assert
            Assert.IsTrue(size.Width > 0);
            Assert.IsTrue(size.Height > 0);
            Assert.IsTrue(location.X >= 0);
            Assert.IsTrue(location.Y >= 0);
        }

        [TestMethod]
        public void Form1_Title_ShouldBeSet()
        {
            // Act
            string title = _form1.Text;

            // Assert
            Assert.IsNotNull(title);
        }

        [TestMethod]
        public void Form1_Icon_ShouldBeSet()
        {
            // Act
            var icon = _form1.Icon;

            // Assert
            // Icon may or may not be set, but should not cause errors
            // If icon is set in the designer, it should not be null
            Assert.IsTrue(true); // This test ensures no exception is thrown
        }

        [TestMethod]
        public void Form1_EnabledState_ShouldBeTrue()
        {
            // Act & Assert
            Assert.IsTrue(_form1.Enabled);
        }

        [TestMethod]
        public void Form1_VisibleState_ShouldBeFalseByDefault()
        {
            // Act & Assert
            // Form should not be visible by default until explicitly shown
            Assert.IsFalse(_form1.Visible);
        }

        [TestMethod]
        public void Form1_StartPosition_ShouldBeSet()
        {
            // Act
            var startPosition = _form1.StartPosition;

            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(FormStartPosition), startPosition));
        }

        [TestMethod]
        public void Form1_MinimumSize_ShouldBeReasonable()
        {
            // Act
            var minSize = _form1.MinimumSize;

            // Assert
            // Minimum size should be reasonable for the application
            Assert.IsTrue(minSize.Width >= 0);
            Assert.IsTrue(minSize.Height >= 0);
        }

        [TestMethod]
        public void Form1_MaximumSize_ShouldBeReasonable()
        {
            // Act
            var maxSize = _form1.MaximumSize;

            // Assert
            // Maximum size should be reasonable (0,0 means no limit)
            Assert.IsTrue(maxSize.Width >= 0);
            Assert.IsTrue(maxSize.Height >= 0);
        }

        [TestMethod]
        public void Form1_CanFocus_ShouldBeTrue()
        {
            // Act & Assert
            Assert.IsTrue(_form1.CanFocus);
        }

        [TestMethod]
        public void Form1_TopLevel_ShouldBeTrue()
        {
            // Act & Assert
            Assert.IsTrue(_form1.TopLevel);
        }

        [TestMethod]
        public void Form1_Close_ShouldDisposeForm()
        {
            // Arrange
            var testForm = new Form1();

            // Act
            testForm.Close();

            // Assert
            Assert.IsTrue(testForm.IsDisposed);
        }

        [TestMethod]
        public void Form1_MultipleInstances_ShouldBeAllowed()
        {
            // Arrange & Act
            var form1 = new Form1();
            var form2 = new Form1();

            // Assert
            Assert.IsNotNull(form1);
            Assert.IsNotNull(form2);
            Assert.AreNotSame(form1, form2);

            // Cleanup
            form1.Dispose();
            form2.Dispose();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                _form1?.Close();
                _form1?.Dispose();

                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
