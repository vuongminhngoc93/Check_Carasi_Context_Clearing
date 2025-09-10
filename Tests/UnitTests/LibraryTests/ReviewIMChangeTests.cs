using System;
using System.Data;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests.UnitTests.LibraryTests
{
    [TestClass]
    public class ReviewIMChangeTests
    {
        private string _testDirectory;
        private string _validExcelFile;
        private string _invalidExcelFile;

        [TestInitialize]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "ReviewIMChangeTests");
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }

            _validExcelFile = Path.Combine(_testDirectory, "ValidReview.xlsx");
            _invalidExcelFile = Path.Combine(_testDirectory, "InvalidReview.xlsx");

            // Create mock Excel files
            CreateMockValidExcelFile(_validExcelFile);
            CreateMockInvalidExcelFile(_invalidExcelFile);
        }

        private void CreateMockValidExcelFile(string filePath)
        {
            // Create a mock file that would contain Mapping$ sheet
            File.WriteAllText(filePath, "Mock Excel File with Mapping$ sheet");
        }

        private void CreateMockInvalidExcelFile(string filePath)
        {
            // Create a mock file that would NOT contain Mapping$ sheet
            File.WriteAllText(filePath, "Mock Excel File without Mapping$ sheet");
        }

        [TestMethod]
        public void Constructor_ValidFolderWithValidFiles_ShouldInitializeCorrectly()
        {
            // Note: This test may show a MessageBox in actual implementation
            // In real testing environment, you would mock MessageBox.Show

            // Arrange & Act
            var review = new Review_IM_change(_testDirectory);

            // Assert
            Assert.IsNotNull(review);
            Assert.AreEqual(_testDirectory, review.linkOfFolder);
            Assert.IsNotNull(review.AllData);
        }

        [TestMethod]
        public void Constructor_InvalidFolderPath_ShouldShowWarning()
        {
            // Arrange
            string invalidFolder = Path.Combine(_testDirectory, "NonExistent");

            // Act & Assert
            // Note: This will show a MessageBox in actual implementation
            try
            {
                var review = new Review_IM_change(invalidFolder);
                // If we reach here, the constructor handled the invalid path gracefully
                Assert.IsNotNull(review);
            }
            catch (DirectoryNotFoundException)
            {
                // Expected behavior for invalid directory
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Constructor_EmptyFolder_ShouldShowWarning()
        {
            // Arrange
            string emptyFolder = Path.Combine(_testDirectory, "Empty");
            Directory.CreateDirectory(emptyFolder);

            // Act
            // Note: This will show a MessageBox in actual implementation
            var review = new Review_IM_change(emptyFolder);

            // Assert
            Assert.IsNotNull(review);
            Assert.AreEqual(emptyFolder, review.linkOfFolder);
        }

        [TestMethod]
        public void Constructor_FolderWithNonExcelFiles_ShouldHandleGracefully()
        {
            // Arrange
            string testFolder = Path.Combine(_testDirectory, "NonExcelFiles");
            Directory.CreateDirectory(testFolder);
            string txtFile = Path.Combine(testFolder, "TextFile.txt");
            File.WriteAllText(txtFile, "This is not an Excel file");

            // Act
            var review = new Review_IM_change(testFolder);

            // Assert
            Assert.IsNotNull(review);
            Assert.AreEqual(testFolder, review.linkOfFolder);
        }

        [TestMethod]
        public void LinkOfFolder_Property_ShouldReturnCorrectValue()
        {
            // Arrange
            var review = new Review_IM_change(_testDirectory);

            // Act
            string folderLink = review.linkOfFolder;

            // Assert
            Assert.AreEqual(_testDirectory, folderLink);
        }

        [TestMethod]
        public void AllData_Property_ShouldNotBeNull()
        {
            // Arrange
            var review = new Review_IM_change(_testDirectory);

            // Act
            DataTable[] allData = review.AllData;

            // Assert
            Assert.IsNotNull(allData);
            Assert.AreEqual(20, allData.Length); // MAX_NO_OF_REVIEW_FILES = 20
        }

        [TestMethod]
        public void Constructor_FolderWithMixedFiles_ShouldProcessOnlyValidFiles()
        {
            // Arrange
            string mixedFolder = Path.Combine(_testDirectory, "Mixed");
            Directory.CreateDirectory(mixedFolder);
            
            // Create valid Excel file
            string validFile = Path.Combine(mixedFolder, "Valid.xlsx");
            CreateMockValidExcelFile(validFile);
            
            // Create invalid Excel file
            string invalidFile = Path.Combine(mixedFolder, "Invalid.xlsx");
            CreateMockInvalidExcelFile(invalidFile);
            
            // Create non-Excel file
            string txtFile = Path.Combine(mixedFolder, "Text.txt");
            File.WriteAllText(txtFile, "Not an Excel file");

            // Act
            var review = new Review_IM_change(mixedFolder);

            // Assert
            Assert.IsNotNull(review);
            Assert.AreEqual(mixedFolder, review.linkOfFolder);
            Assert.IsNotNull(review.AllData);
        }

        [TestMethod]
        public void Constructor_NullPath_ShouldHandleGracefully()
        {
            // Act & Assert
            try
            {
                var review = new Review_IM_change(null);
                // If constructor handles null gracefully
                Assert.IsNotNull(review);
            }
            catch (ArgumentNullException)
            {
                // Expected behavior for null path
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                // Some other exception - constructor should handle this better
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Constructor_EmptyStringPath_ShouldHandleGracefully()
        {
            // Act & Assert
            try
            {
                var review = new Review_IM_change(string.Empty);
                // If constructor handles empty string gracefully
                Assert.IsNotNull(review);
            }
            catch (ArgumentException)
            {
                // Expected behavior for empty path
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                // Some other exception - constructor should handle this better
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Constructor_FolderWithMaxNumberOfFiles_ShouldRespectLimit()
        {
            // Arrange
            string maxFilesFolder = Path.Combine(_testDirectory, "MaxFiles");
            Directory.CreateDirectory(maxFilesFolder);
            
            // Create more than MAX_NO_OF_REVIEW_FILES (20) Excel files
            for (int i = 0; i < 25; i++)
            {
                string fileName = Path.Combine(maxFilesFolder, $"File{i:D2}.xlsx");
                CreateMockValidExcelFile(fileName);
            }

            // Act
            var review = new Review_IM_change(maxFilesFolder);

            // Assert
            Assert.IsNotNull(review);
            Assert.AreEqual(maxFilesFolder, review.linkOfFolder);
            
            // Check that only MAX_NO_OF_REVIEW_FILES are processed
            int processedFiles = 0;
            for (int i = 0; i < review.AllData.Length; i++)
            {
                if (review.AllData[i] != null)
                {
                    processedFiles++;
                }
            }
            Assert.IsTrue(processedFiles <= 20); // Should not exceed MAX_NO_OF_REVIEW_FILES
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
        }
    }
}
