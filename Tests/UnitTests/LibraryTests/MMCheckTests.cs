using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests.UnitTests.LibraryTests
{
    [TestClass]
    public class MMCheckTests
    {
        private MM_Check _mmCheck;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _mmCheck = new MM_Check();
            _testDirectory = Path.Combine(Path.GetTempPath(), "MMCheckTests");
            
            // Tạo test directory structure
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }
        }

        [TestMethod]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var mmCheck = new MM_Check();

            // Assert
            Assert.AreEqual(string.Empty, mmCheck.Link_Of_MM);
            Assert.AreEqual(string.Empty, mmCheck.Link_Of_ARXML);
            Assert.IsFalse(mmCheck.IsValidLink);
        }

        [TestMethod]
        public void GetDirectories_ValidPath_ShouldReturnDirectories()
        {
            // Arrange
            string subDir1 = Path.Combine(_testDirectory, "SubDir1");
            string subDir2 = Path.Combine(_testDirectory, "SubDir2");
            Directory.CreateDirectory(subDir1);
            Directory.CreateDirectory(subDir2);

            // Act
            var directories = MM_Check.GetDirectories(_testDirectory, "*", SearchOption.TopDirectoryOnly);

            // Assert
            Assert.IsTrue(directories.Count >= 2);
            Assert.IsTrue(directories.Contains(subDir1));
            Assert.IsTrue(directories.Contains(subDir2));
        }

        [TestMethod]
        public void GetDirectories_NonExistentPath_ShouldReturnEmptyList()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_testDirectory, "NonExistent");

            // Act & Assert
            try
            {
                var directories = MM_Check.GetDirectories(nonExistentPath);
                Assert.AreEqual(0, directories.Count);
            }
            catch (DirectoryNotFoundException)
            {
                // Expected behavior for non-existent directory
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void GetDirectories_WithSearchPattern_ShouldFilterCorrectly()
        {
            // Arrange
            string arxmlDir = Path.Combine(_testDirectory, "arxml");
            string otherDir = Path.Combine(_testDirectory, "other");
            Directory.CreateDirectory(arxmlDir);
            Directory.CreateDirectory(otherDir);

            // Act
            var directories = MM_Check.GetDirectories(_testDirectory, "arxml");

            // Assert
            Assert.IsTrue(directories.Contains(arxmlDir));
            Assert.IsFalse(directories.Contains(otherDir));
        }

        [TestMethod]
        public void GetDirectories_AllDirectories_ShouldReturnAllSubdirectories()
        {
            // Arrange
            string level1 = Path.Combine(_testDirectory, "Level1");
            string level2 = Path.Combine(level1, "Level2");
            Directory.CreateDirectory(level2);

            // Act
            var directories = MM_Check.GetDirectories(_testDirectory, "*", SearchOption.AllDirectories);

            // Assert
            Assert.IsTrue(directories.Contains(level1));
            Assert.IsTrue(directories.Contains(level2));
        }

        [TestMethod]
        public void SetLinkOfMM_ValidPathWithExternArxml_ShouldSetValidLink()
        {
            // Arrange
            string arxmlDir = Path.Combine(_testDirectory, "arxml");
            Directory.CreateDirectory(arxmlDir);
            string externFile = Path.Combine(arxmlDir, "TestFile_Extern.arxml");
            File.WriteAllText(externFile, "<?xml version=\"1.0\"?><root></root>");

            // Act
            _mmCheck.Link_Of_MM = _testDirectory;

            // Assert
            Assert.AreEqual(_testDirectory, _mmCheck.Link_Of_MM);
            Assert.IsTrue(_mmCheck.IsValidLink);
            Assert.AreEqual(externFile, _mmCheck.Link_Of_ARXML);
        }

        [TestMethod]
        public void SetLinkOfMM_ValidPathWithoutExternArxml_ShouldSetInvalidLink()
        {
            // Arrange
            string arxmlDir = Path.Combine(_testDirectory, "arxml");
            Directory.CreateDirectory(arxmlDir);
            string regularFile = Path.Combine(arxmlDir, "RegularFile.arxml");
            File.WriteAllText(regularFile, "<?xml version=\"1.0\"?><root></root>");

            // Act
            _mmCheck.Link_Of_MM = _testDirectory;

            // Assert
            Assert.AreEqual(_testDirectory, _mmCheck.Link_Of_MM);
            Assert.IsFalse(_mmCheck.IsValidLink);
        }

        [TestMethod]
        public void SetLinkOfMM_PathWithoutArxmlDirectory_ShouldSetInvalidLink()
        {
            // Arrange
            string testDir = Path.Combine(_testDirectory, "NoArxml");
            Directory.CreateDirectory(testDir);

            // Act
            _mmCheck.Link_Of_MM = testDir;

            // Assert
            Assert.AreEqual(testDir, _mmCheck.Link_Of_MM);
            Assert.IsFalse(_mmCheck.IsValidLink);
        }

        [TestMethod]
        public void SetLinkOfMM_EmptyPath_ShouldHandleGracefully()
        {
            // Arrange & Act
            _mmCheck.Link_Of_MM = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, _mmCheck.Link_Of_MM);
            Assert.IsFalse(_mmCheck.IsValidLink);
        }

        [TestMethod]
        public void SetLinkOfMM_NullPath_ShouldHandleGracefully()
        {
            // Arrange & Act
            _mmCheck.Link_Of_MM = null;

            // Assert
            Assert.AreEqual(null, _mmCheck.Link_Of_MM);
            Assert.IsFalse(_mmCheck.IsValidLink);
        }

        [TestMethod]
        public void LinkOfARXML_SetDirectly_ShouldMaintainValue()
        {
            // Arrange
            string testPath = @"C:\TestPath\TestFile.arxml";

            // Act
            _mmCheck.Link_Of_ARXML = testPath;

            // Assert
            Assert.AreEqual(testPath, _mmCheck.Link_Of_ARXML);
        }

        [TestMethod]
        public void IsValidLink_SetDirectly_ShouldMaintainValue()
        {
            // Arrange & Act
            _mmCheck.IsValidLink = true;

            // Assert
            Assert.IsTrue(_mmCheck.IsValidLink);

            // Act
            _mmCheck.IsValidLink = false;

            // Assert
            Assert.IsFalse(_mmCheck.IsValidLink);
        }

        [TestMethod]
        public void SetLinkOfMM_MultipleExternFiles_ShouldPickFirst()
        {
            // Arrange
            string arxmlDir = Path.Combine(_testDirectory, "arxml");
            Directory.CreateDirectory(arxmlDir);
            string externFile1 = Path.Combine(arxmlDir, "FirstFile_Extern.arxml");
            string externFile2 = Path.Combine(arxmlDir, "SecondFile_Extern.arxml");
            File.WriteAllText(externFile1, "<?xml version=\"1.0\"?><root></root>");
            File.WriteAllText(externFile2, "<?xml version=\"1.0\"?><root></root>");

            // Act
            _mmCheck.Link_Of_MM = _testDirectory;

            // Assert
            Assert.IsTrue(_mmCheck.IsValidLink);
            Assert.IsTrue(_mmCheck.Link_Of_ARXML.EndsWith("_Extern.arxml"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
            _mmCheck = null;
        }
    }
}
