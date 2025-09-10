using System;
using System.Data;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests.UnitTests.LibraryTests
{
    [TestClass]
    public class LibOLEDBExcelTests
    {
        private string _testFilePath;
        private string _xlsFilePath;
        private string _xlsxFilePath;

        [TestInitialize]
        public void Setup()
        {
            string testDir = Path.Combine(Path.GetTempPath(), "LibOLEDBExcelTests");
            if (!Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }

            _xlsFilePath = Path.Combine(testDir, "TestFile.xls");
            _xlsxFilePath = Path.Combine(testDir, "TestFile.xlsx");
            
            // Create simple test Excel files (mock files)
            CreateMockExcelFile(_xlsFilePath);
            CreateMockExcelFile(_xlsxFilePath);
        }

        private void CreateMockExcelFile(string filePath)
        {
            // Create a minimal file that can be recognized by the constructor
            // In real scenario, you would create actual Excel files
            File.WriteAllText(filePath, "Mock Excel File Content");
        }

        [TestMethod]
        public void Constructor_ValidXlsPath_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var excelLib = new Lib_OLEDB_Excel(_xlsFilePath);

            // Assert
            Assert.IsNotNull(excelLib);
            Assert.IsNotNull(excelLib.ConnectionString);
        }

        [TestMethod]
        public void Constructor_ValidXlsxPath_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);

            // Assert
            Assert.IsNotNull(excelLib);
            Assert.IsNotNull(excelLib.ConnectionString);
        }

        [TestMethod]
        public void ConnectionString_XlsFile_ShouldContainJet()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsFilePath);

            // Act
            string connectionString = excelLib.ConnectionString;

            // Assert
            Assert.IsTrue(connectionString.Contains("Jet"));
            Assert.IsTrue(connectionString.Contains("4.0"));
            Assert.IsTrue(connectionString.Contains("8.0"));
        }

        [TestMethod]
        public void ConnectionString_XlsxFile_ShouldContainAce()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);

            // Act
            string connectionString = excelLib.ConnectionString;

            // Assert
            Assert.IsTrue(connectionString.Contains("ACE"));
            Assert.IsTrue(connectionString.Contains("12.0"));
        }

        [TestMethod]
        public void ConnectionString_EmptyPath_ShouldReturnEmptyString()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(string.Empty);

            // Act
            string connectionString = excelLib.ConnectionString;

            // Assert
            Assert.AreEqual(string.Empty, connectionString);
        }

        [TestMethod]
        public void ConnectionString_NullPath_ShouldReturnEmptyString()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(null);

            // Act
            string connectionString = excelLib.ConnectionString;

            // Assert
            Assert.AreEqual(string.Empty, connectionString);
        }

        [TestMethod]
        public void ConnectionString_FileWithoutExtension_ShouldHandleGracefully()
        {
            // Arrange
            string fileWithoutExt = Path.Combine(Path.GetTempPath(), "FileWithoutExtension");
            File.WriteAllText(fileWithoutExt, "content");
            var excelLib = new Lib_OLEDB_Excel(fileWithoutExt);

            // Act
            string connectionString = excelLib.ConnectionString;

            // Assert
            // Should handle gracefully and not throw exception
            Assert.IsNotNull(connectionString);
            
            // Cleanup
            File.Delete(fileWithoutExt);
        }

        [TestMethod]
        public void ConnectionString_TmpFile_ShouldUseXlsxFormat()
        {
            // Arrange
            string tmpFile = Path.Combine(Path.GetTempPath(), "TestFile.tmp");
            File.WriteAllText(tmpFile, "content");
            var excelLib = new Lib_OLEDB_Excel(tmpFile);

            // Act
            string connectionString = excelLib.ConnectionString;

            // Assert
            Assert.IsTrue(connectionString.Contains("ACE"));
            Assert.IsTrue(connectionString.Contains("12.0"));
            
            // Cleanup
            File.Delete(tmpFile);
        }

        [TestMethod]
        public void ReadProgress_EventSubscription_ShouldWork()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);
            bool eventFired = false;
            float receivedPercentage = 0;

            // Act
            excelLib.ReadProgress += (percentage) => 
            {
                eventFired = true;
                receivedPercentage = percentage;
            };

            excelLib.onReadProgress(50.0f);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(50.0f, receivedPercentage);
        }

        [TestMethod]
        public void WriteProgress_EventSubscription_ShouldWork()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);
            bool eventFired = false;
            float receivedPercentage = 0;

            // Act
            excelLib.WriteProgress += (percentage) => 
            {
                eventFired = true;
                receivedPercentage = percentage;
            };

            excelLib.onWriteProgress(75.0f);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(75.0f, receivedPercentage);
        }

        [TestMethod]
        public void ConnectionStringChanged_EventSubscription_ShouldWork()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);
            bool eventFired = false;

            // Act
            excelLib.ConnectionStringChanged += (sender, args) => 
            {
                eventFired = true;
            };

            excelLib.onConnectionStringChanged();

            // Assert
            Assert.IsTrue(eventFired);
        }

        [TestMethod]
        public void ReadProgress_EventUnsubscription_ShouldWork()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);
            bool eventFired = false;
            
            Lib_OLEDB_Excel.ProgressWork handler = (percentage) => 
            {
                eventFired = true;
            };

            excelLib.ReadProgress += handler;
            excelLib.ReadProgress -= handler;

            // Act
            excelLib.onReadProgress(50.0f);

            // Assert
            Assert.IsFalse(eventFired);
        }

        [TestMethod]
        public void WriteProgress_EventUnsubscription_ShouldWork()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);
            bool eventFired = false;
            
            Lib_OLEDB_Excel.ProgressWork handler = (percentage) => 
            {
                eventFired = true;
            };

            excelLib.WriteProgress += handler;
            excelLib.WriteProgress -= handler;

            // Act
            excelLib.onWriteProgress(75.0f);

            // Assert
            Assert.IsFalse(eventFired);
        }

        [TestMethod]
        public void OnReadProgress_NullEvent_ShouldNotThrow()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);

            // Act & Assert
            try
            {
                excelLib.onReadProgress(50.0f);
                Assert.IsTrue(true); // No exception thrown
            }
            catch
            {
                Assert.Fail("Should not throw exception when no event handlers are subscribed");
            }
        }

        [TestMethod]
        public void OnWriteProgress_NullEvent_ShouldNotThrow()
        {
            // Arrange
            var excelLib = new Lib_OLEDB_Excel(_xlsxFilePath);

            // Act & Assert
            try
            {
                excelLib.onWriteProgress(75.0f);
                Assert.IsTrue(true); // No exception thrown
            }
            catch
            {
                Assert.Fail("Should not throw exception when no event handlers are subscribed");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (File.Exists(_xlsFilePath))
                    File.Delete(_xlsFilePath);
                if (File.Exists(_xlsxFilePath))
                    File.Delete(_xlsxFilePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
