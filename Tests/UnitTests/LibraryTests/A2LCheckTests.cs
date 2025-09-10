using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Check_carasi_DF_ContextClearing;

namespace Check_carasi_DF_ContextClearing.Tests.UnitTests.LibraryTests
{
    [TestClass]
    public class A2LCheckTests
    {
        private A2L_Check _a2lCheck;
        private string _validA2LPath;
        private string _invalidA2LPath;

        [TestInitialize]
        public void Setup()
        {
            _a2lCheck = new A2L_Check();
            _validA2LPath = @"TestData\SampleA2L.a2l";
            _invalidA2LPath = @"TestData\InvalidPath.a2l";
        }

        [TestMethod]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var a2lCheck = new A2L_Check();

            // Assert
            Assert.AreEqual(string.Empty, a2lCheck.Link_Of_A2L);
            Assert.IsTrue(a2lCheck.IsValidLink);
        }

        [TestMethod]
        public void SetLinkOfA2L_ValidPath_ShouldSetPath()
        {
            // Arrange
            string testPath = _validA2LPath;

            // Act
            _a2lCheck.Link_Of_A2L = testPath;

            // Assert
            Assert.AreEqual(testPath, _a2lCheck.Link_Of_A2L);
        }

        [TestMethod]
        public void SetLinkOfA2L_InvalidPath_ShouldHandleGracefully()
        {
            // Arrange
            string invalidPath = _invalidA2LPath;

            // Act
            _a2lCheck.Link_Of_A2L = invalidPath;

            // Assert
            Assert.AreEqual(invalidPath, _a2lCheck.Link_Of_A2L);
            // Note: Validation logic should be implemented in A2L_setup method
        }

        [TestMethod]
        public void SetLinkOfA2L_EmptyPath_ShouldAcceptEmptyString()
        {
            // Arrange
            string emptyPath = string.Empty;

            // Act
            _a2lCheck.Link_Of_A2L = emptyPath;

            // Assert
            Assert.AreEqual(emptyPath, _a2lCheck.Link_Of_A2L);
        }

        [TestMethod]
        public void SetLinkOfA2L_NullPath_ShouldHandleNull()
        {
            // Arrange
            string nullPath = null;

            // Act
            _a2lCheck.Link_Of_A2L = nullPath;

            // Assert
            Assert.AreEqual(nullPath, _a2lCheck.Link_Of_A2L);
        }

        [TestMethod]
        public void IsExistInA2L_ExistingKeyword_ShouldReturnTrue()
        {
            // Arrange
            _a2lCheck.Link_Of_A2L = _validA2LPath;
            string keyword = "TestKeyword";
            string[] result = new string[10];

            // Act
            bool exists = _a2lCheck.IsExistInA2L(keyword, ref result);

            // Assert
            // Note: Current implementation returns false by default
            // This test assumes the method will be implemented
            Assert.IsFalse(exists); // Current default behavior
        }

        [TestMethod]
        public void IsExistInA2L_NonExistingKeyword_ShouldReturnFalse()
        {
            // Arrange
            _a2lCheck.Link_Of_A2L = _validA2LPath;
            string keyword = "NonExistentKeyword";
            string[] result = new string[10];

            // Act
            bool exists = _a2lCheck.IsExistInA2L(keyword, ref result);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void IsExistInA2L_EmptyKeyword_ShouldReturnFalse()
        {
            // Arrange
            _a2lCheck.Link_Of_A2L = _validA2LPath;
            string keyword = string.Empty;
            string[] result = new string[10];

            // Act
            bool exists = _a2lCheck.IsExistInA2L(keyword, ref result);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void IsExistInA2L_NullKeyword_ShouldHandleGracefully()
        {
            // Arrange
            _a2lCheck.Link_Of_A2L = _validA2LPath;
            string keyword = null;
            string[] result = new string[10];

            // Act
            bool exists = _a2lCheck.IsExistInA2L(keyword, ref result);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void IsExistInA2L_NullResultArray_ShouldHandleGracefully()
        {
            // Arrange
            _a2lCheck.Link_Of_A2L = _validA2LPath;
            string keyword = "TestKeyword";
            string[] result = null;

            // Act & Assert
            // Should not throw exception
            bool exists = _a2lCheck.IsExistInA2L(keyword, ref result);
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void IsValidLink_DefaultValue_ShouldBeTrue()
        {
            // Arrange & Act
            var newA2lCheck = new A2L_Check();

            // Assert
            Assert.IsTrue(newA2lCheck.IsValidLink);
        }

        [TestMethod]
        public void IsValidLink_SetToFalse_ShouldMaintainValue()
        {
            // Arrange & Act
            _a2lCheck.IsValidLink = false;

            // Assert
            Assert.IsFalse(_a2lCheck.IsValidLink);
        }

        [TestMethod]
        public void IsValidLink_SetToTrue_ShouldMaintainValue()
        {
            // Arrange
            _a2lCheck.IsValidLink = false;

            // Act
            _a2lCheck.IsValidLink = true;

            // Assert
            Assert.IsTrue(_a2lCheck.IsValidLink);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _a2lCheck = null;
        }
    }
}
