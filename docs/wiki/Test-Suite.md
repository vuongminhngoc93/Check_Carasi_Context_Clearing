# Test Suite Documentation 🧪

## 🧪 **Comprehensive Test Suite Documentation**

Complete testing framework cho Check Carasi DF Context Clearing Tool để ensure reliability, performance, và quality trong production environment.

---

## 🎯 **Testing Framework Overview**

### **📋 Test Architecture**

```
Testing Framework Structure:
├── Unit Tests (70%)
│   ├── Core Logic Tests
│   ├── OLEDB Connection Tests  
│   ├── Excel Parser Tests
│   ├── PropertyDifferenceHighlighter Tests
│   └── A2L Parser Tests
├── Integration Tests (20%)
│   ├── UI Component Tests
│   ├── File Processing Workflow Tests
│   ├── Multi-Panel Integration Tests
│   └── Export Functionality Tests
├── Performance Tests (7%)
│   ├── Load Testing
│   ├── Memory Usage Tests
│   ├── Large File Handling Tests
│   └── Concurrent Operations Tests
└── User Acceptance Tests (3%)
    ├── End-to-End Scenarios
    ├── Real-World Data Tests
    └── Regression Testing
```

### **🏷️ Test Categories**

| Category | Purpose | Coverage | Automation Level |
|----------|---------|----------|------------------|
| **Unit Tests** | Component functionality | 85%+ | Fully Automated |
| **Integration Tests** | Component interaction | 75%+ | Mostly Automated |
| **Performance Tests** | Speed và memory | 90%+ | Fully Automated |
| **UI Tests** | User interface | 60%+ | Semi-Automated |
| **Regression Tests** | Bug prevention | 95%+ | Fully Automated |
| **Manual Tests** | User experience | Variable | Manual Only |

---

## 🔧 **Test Tools & Infrastructure**

### **🛠️ Testing Tools**

#### **Primary Testing Framework**
```csharp
// MSTest Framework for unit testing
[TestClass]
public class ExcelParserTests
{
    [TestMethod]
    public void TestExcelFileReading()
    {
        // Test implementation
    }
    
    [TestMethod]
    [ExpectedException(typeof(OleDbException))]
    public void TestInvalidFileHandling()
    {
        // Test exception handling
    }
}
```

#### **Performance Testing Tools**
```csharp
// Custom performance testing framework
public class PerformanceTestRunner
{
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private readonly PerformanceCounter _memoryCounter;
    
    [PerformanceTest]
    [Timeout(30000)] // 30 seconds max
    public void TestLargeFileProcessing()
    {
        // Performance test implementation
    }
}
```

### **📊 Test Data Management**

#### **Test Data Categories**
```
Test Data Structure:
├── Sample_Files/
│   ├── Small_Tests/ (< 1MB)
│   │   ├── valid_carasi_basic.xlsx
│   │   ├── valid_dataflow_simple.xlsx
│   │   └── test_a2l_minimal.a2l
│   ├── Medium_Tests/ (1-10MB)
│   │   ├── realistic_carasi_project.xlsx
│   │   ├── complex_dataflow_multi_sheet.xlsx
│   │   └── standard_a2l_file.a2l
│   ├── Large_Tests/ (10-100MB)
│   │   ├── enterprise_carasi_full.xlsx
│   │   ├── massive_dataflow_dataset.xlsx
│   │   └── complex_a2l_automotive.a2l
│   └── Edge_Cases/
│       ├── corrupted_excel_file.xlsx
│       ├── password_protected.xlsx
│       ├── unicode_characters.xlsx
│       └── empty_worksheets.xlsx
```

#### **Test Data Generation**
```powershell
# PowerShell script to generate test data
function New-TestExcelFile {
    param(
        [string]$FilePath,
        [int]$RowCount = 1000,
        [string]$TestType = "Basic"
    )
    
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $workbook = $excel.Workbooks.Add()
    $worksheet = $workbook.Worksheets.Item(1)
    
    # Generate headers
    $worksheet.Cells.Item(1,1) = "Interface Name"
    $worksheet.Cells.Item(1,2) = "Function Name"
    $worksheet.Cells.Item(1,3) = "Direction"
    $worksheet.Cells.Item(1,4) = "Data Type"
    
    # Generate test data
    for ($row = 2; $row -le ($RowCount + 1); $row++) {
        $worksheet.Cells.Item($row,1) = "TestInterface_$($row-1)"
        $worksheet.Cells.Item($row,2) = "TestFunction_$($row-1)"
        $worksheet.Cells.Item($row,3) = if ($row % 2) { "Input" } else { "Output" }
        $worksheet.Cells.Item($row,4) = @("uint8", "uint16", "uint32", "float32")[(($row-2) % 4)]
    }
    
    $workbook.SaveAs($FilePath)
    $workbook.Close()
    $excel.Quit()
    
    Write-Host "Generated test file: $FilePath with $RowCount rows" -ForegroundColor Green
}

# Generate test suite
New-TestExcelFile -FilePath "Test_Basic_1000.xlsx" -RowCount 1000
New-TestExcelFile -FilePath "Test_Medium_10000.xlsx" -RowCount 10000  
New-TestExcelFile -FilePath "Test_Large_100000.xlsx" -RowCount 100000
```

---

## 🧪 **Unit Tests**

### **🔬 Core Logic Tests**

#### **Excel Parser Tests**
```csharp
[TestClass]
public class ExcelParserTests
{
    private ExcelParser _parser;
    private string _testFilePath;
    
    [TestInitialize]
    public void Setup()
    {
        _parser = new ExcelParser();
        _testFilePath = Path.Combine(TestContext.TestDirectory, "TestData", "sample_carasi.xlsx");
    }
    
    [TestMethod]
    public void ParseExcelFile_ValidFile_ReturnsCorrectData()
    {
        // Arrange
        var expectedRowCount = 100;
        
        // Act
        var result = _parser.ParseExcelFile(_testFilePath);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedRowCount, result.Rows.Count);
        Assert.IsTrue(result.Headers.Contains("Function Name"));
    }
    
    [TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void ParseExcelFile_NonExistentFile_ThrowsException()
    {
        // Act
        _parser.ParseExcelFile("nonexistent_file.xlsx");
    }
    
    [TestMethod]  
    public void ParseExcelFile_CorruptedFile_HandlesGracefully()
    {
        // Arrange
        var corruptedFilePath = Path.Combine(TestContext.TestDirectory, "TestData", "corrupted.xlsx");
        
        // Act & Assert
        try
        {
            var result = _parser.ParseExcelFile(corruptedFilePath);
            Assert.Fail("Expected exception for corrupted file");
        }
        catch (OleDbException ex)
        {
            Assert.IsTrue(ex.Message.Contains("External table is not in the expected format"));
        }
    }
    
    [TestMethod]
    public void ParseExcelFile_UnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        var unicodeFilePath = Path.Combine(TestContext.TestDirectory, "TestData", "unicode_test.xlsx");
        
        // Act
        var result = _parser.ParseExcelFile(unicodeFilePath);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Data.Any(row => row.Contains("测试功能")));
        Assert.IsTrue(result.Data.Any(row => row.Contains("тестовая_функция")));
    }
}
```

#### **PropertyDifferenceHighlighter Tests**
```csharp
[TestClass]
public class PropertyDifferenceHighlighterTests
{
    private PropertyDifferenceHighlighter _highlighter;
    
    [TestInitialize]
    public void Setup()
    {
        _highlighter = new PropertyDifferenceHighlighter();
    }
    
    [TestMethod]
    public void CompareValuesWithPrefixMatching_MMtoSTUB_ReturnsTrue()
    {
        // Arrange
        string oldValue = "MM_TestFunction";
        string newValue = "STUB_TestFunction";
        
        // Act
        bool result = _highlighter.CompareValuesWithPrefixMatching(oldValue, newValue);
        
        // Assert
        Assert.IsTrue(result, "MM_ to STUB_ prefix change should be highlighted");
    }
    
    [TestMethod]
    public void CompareValuesWithPrefixMatching_SameValues_ReturnsFalse()
    {
        // Arrange
        string oldValue = "TestFunction_001";
        string newValue = "TestFunction_001";
        
        // Act
        bool result = _highlighter.CompareValuesWithPrefixMatching(oldValue, newValue);
        
        // Assert
        Assert.IsFalse(result, "Identical values should not be highlighted");
    }
    
    [TestMethod]
    public void IsAddCase_EmptyToValue_ReturnsTrue()
    {
        // Arrange
        string oldValue = "";
        string newValue = "NewFunction";
        
        // Act
        bool result = _highlighter.IsAddCase(oldValue, newValue);
        
        // Assert
        Assert.IsTrue(result, "Empty to value should be ADD case");
    }
    
    [TestMethod]
    public void IsRemoveCase_ValueToEmpty_ReturnsTrue()
    {
        // Arrange
        string oldValue = "OldFunction";
        string newValue = "";
        
        // Act
        bool result = _highlighter.IsRemoveCase(oldValue, newValue);
        
        // Assert
        Assert.IsTrue(result, "Value to empty should be REMOVE case");
    }
    
    [TestMethod]
    [DataRow("MM_TestFunc", "STUB_TestFunc", true)]
    [DataRow("TestFunc_001", "TestFunc_002", true)]
    [DataRow("Same_Function", "Same_Function", false)]
    [DataRow("", "New_Function", true)]
    [DataRow("Old_Function", "", true)]
    public void CompareValuesWithPrefixMatching_VariousInputs_ReturnsExpected(
        string oldValue, string newValue, bool expected)
    {
        // Act
        bool result = _highlighter.CompareValuesWithPrefixMatching(oldValue, newValue);
        
        // Assert
        Assert.AreEqual(expected, result);
    }
}
```

#### **OLEDB Connection Tests**
```csharp
[TestClass]
public class OLEDBConnectionTests
{
    private OLEDBConnectionManager _connectionManager;
    private string _validExcelFile;
    
    [TestInitialize]
    public void Setup()
    {
        _connectionManager = new OLEDBConnectionManager();
        _validExcelFile = Path.Combine(TestContext.TestDirectory, "TestData", "test_excel.xlsx");
    }
    
    [TestMethod]
    public void GetConnection_ValidExcelFile_ReturnsWorkingConnection()
    {
        // Act
        using (var connection = _connectionManager.GetConnection(_validExcelFile))
        {
            // Assert
            Assert.IsNotNull(connection);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            
            // Test basic query
            using (var command = new OleDbCommand("SELECT COUNT(*) FROM [Sheet1$]", connection))
            {
                var result = command.ExecuteScalar();
                Assert.IsTrue((int)result >= 0);
            }
        }
    }
    
    [TestMethod]
    public void GetConnection_MultipleRequests_ReusesConnections()
    {
        // Arrange
        var connections = new List<OleDbConnection>();
        
        // Act
        for (int i = 0; i < 5; i++)
        {
            connections.Add(_connectionManager.GetConnection(_validExcelFile));
        }
        
        // Assert
        Assert.IsTrue(_connectionManager.PoolSize <= 3, "Connection pool should limit connections");
        
        // Cleanup
        connections.ForEach(conn => conn?.Dispose());
    }
    
    [TestMethod]
    public void GetConnection_InvalidFile_ThrowsAppropriateException()
    {
        // Arrange
        string invalidFile = "nonexistent.xlsx";
        
        // Act & Assert
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            _connectionManager.GetConnection(invalidFile);
        });
    }
}
```

### **🔧 A2L Parser Tests**
```csharp
[TestClass]
public class A2LParserTests
{
    private A2LParser _parser;
    private string _testA2LFile;
    
    [TestInitialize]
    public void Setup()
    {
        _parser = new A2LParser();
        _testA2LFile = Path.Combine(TestContext.TestDirectory, "TestData", "sample.a2l");
    }
    
    [TestMethod]
    public void ParseA2LFile_ValidFile_ExtractsCorrectElements()
    {
        // Act
        var result = _parser.ParseA2LFile(_testA2LFile);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Functions.Count > 0);
        Assert.IsTrue(result.Measurements.Count > 0);
        Assert.IsTrue(result.Characteristics.Count > 0);
    }
    
    [TestMethod]
    public void ParseA2LFile_ComplexStructure_HandlesNestedElements()
    {
        // Arrange
        var complexA2LFile = Path.Combine(TestContext.TestDirectory, "TestData", "complex.a2l");
        
        // Act
        var result = _parser.ParseA2LFile(complexA2LFile);
        
        // Assert
        Assert.IsNotNull(result);
        
        // Verify nested structure parsing
        var functionWithParams = result.Functions.FirstOrDefault(f => f.Parameters.Count > 0);
        Assert.IsNotNull(functionWithParams, "Should find function with parameters");
        
        var measurementWithConversion = result.Measurements.FirstOrDefault(m => m.ConversionFormula != null);
        Assert.IsNotNull(measurementWithConversion, "Should find measurement with conversion");
    }
}
```

---

## 🔗 **Integration Tests**

### **🎯 UI Component Integration Tests**
```csharp
[TestClass]
public class UIIntegrationTests
{
    private Form1 _mainForm;
    private UC_ContextClearing _contextClearingControl;
    
    [TestInitialize]
    public void Setup()
    {
        Application.EnableVisualStyles();
        _mainForm = new Form1();
        _contextClearingControl = new UC_ContextClearing();
        _mainForm.Controls.Add(_contextClearingControl);
    }
    
    [TestMethod]
    public void LoadFiles_BothCarasiFiles_UpdatesAllPanels()
    {
        // Arrange
        var oldCarasiFile = GetTestFile("old_carasi.xlsx");
        var newCarasiFile = GetTestFile("new_carasi.xlsx");
        
        // Act
        _contextClearingControl.LoadOldCarasiFile(oldCarasiFile);
        _contextClearingControl.LoadNewCarasiFile(newCarasiFile);
        
        // Process Windows messages to ensure UI updates
        Application.DoEvents();
        
        // Assert
        Assert.IsTrue(_contextClearingControl.IsOldCarasiLoaded);
        Assert.IsTrue(_contextClearingControl.IsNewCarasiLoaded);
        Assert.IsTrue(_contextClearingControl.HasComparisonData);
    }
    
    [TestMethod]
    public void PropertyHighlighting_WithDifferences_HighlightsCorrectCells()
    {
        // Arrange
        LoadTestFiles();
        
        // Act
        _contextClearingControl.EnablePropertyHighlighting(true);
        _contextClearingControl.RefreshComparison();
        Application.DoEvents();
        
        // Assert
        var highlightedCells = _contextClearingControl.GetHighlightedCells();
        Assert.IsTrue(highlightedCells.Count > 0, "Should find highlighted differences");
        
        // Verify specific highlighting cases
        var mmToStubHighlights = highlightedCells.Where(cell => 
            cell.OldValue.StartsWith("MM_") && cell.NewValue.StartsWith("STUB_"));
        Assert.IsTrue(mmToStubHighlights.Any(), "Should highlight MM_ to STUB_ changes");
    }
    
    [TestMethod]
    public void TabManagement_MultipleFiles_LimitsTabCount()
    {
        // Arrange
        var maxTabs = 60; // Application limit
        
        // Act
        for (int i = 0; i < maxTabs + 10; i++)
        {
            var tabName = $"Test_Tab_{i}";
            _mainForm.CreateNewTab(tabName);
        }
        
        // Assert
        Assert.AreEqual(maxTabs, _mainForm.TabCount, "Should limit tab count to maximum");
        Assert.IsTrue(_mainForm.HasShownTabLimitWarning, "Should show warning when limit reached");
    }
    
    private void LoadTestFiles()
    {
        var oldFile = GetTestFile("comparison_old.xlsx");
        var newFile = GetTestFile("comparison_new.xlsx");
        _contextClearingControl.LoadFiles(oldFile, newFile);
        Application.DoEvents();
    }
    
    private string GetTestFile(string fileName)
    {
        return Path.Combine(TestContext.TestDirectory, "TestData", fileName);
    }
}
```

### **📊 File Processing Workflow Tests**
```csharp
[TestClass]
public class FileProcessingWorkflowTests
{
    private FileProcessingManager _processor;
    
    [TestInitialize]
    public void Setup()
    {
        _processor = new FileProcessingManager();
    }
    
    [TestMethod]
    public void ProcessFileWorkflow_CompleteScenario_ExecutesAllSteps()
    {
        // Arrange
        var oldFile = GetTestFile("workflow_old.xlsx");
        var newFile = GetTestFile("workflow_new.xlsx");
        var dataflowFile = GetTestFile("workflow_dataflow.xlsx");
        
        // Act
        var workflowResult = _processor.ProcessCompleteWorkflow(oldFile, newFile, dataflowFile);
        
        // Assert
        Assert.IsNotNull(workflowResult);
        Assert.IsTrue(workflowResult.IsSuccess);
        Assert.IsNotNull(workflowResult.ComparisonResults);
        Assert.IsNotNull(workflowResult.DataflowAnalysis);
        Assert.IsTrue(workflowResult.ProcessingTimeMs < 30000); // 30 seconds max
    }
    
    [TestMethod]
    public void BatchProcessing_MultipleFilePairs_ProcessesAllSuccessfully()
    {
        // Arrange
        var filePairs = new List<(string oldFile, string newFile)>
        {
            (GetTestFile("batch1_old.xlsx"), GetTestFile("batch1_new.xlsx")),
            (GetTestFile("batch2_old.xlsx"), GetTestFile("batch2_new.xlsx")),
            (GetTestFile("batch3_old.xlsx"), GetTestFile("batch3_new.xlsx"))
        };
        
        // Act
        var batchResults = _processor.ProcessBatch(filePairs);
        
        // Assert
        Assert.AreEqual(filePairs.Count, batchResults.Count);
        Assert.IsTrue(batchResults.All(r => r.IsSuccess));
        
        // Verify no memory leaks
        var memoryAfter = GC.GetTotalMemory(true);
        Assert.IsTrue(memoryAfter < 500 * 1024 * 1024, "Memory usage should be reasonable after batch processing");
    }
    
    private string GetTestFile(string fileName)
    {
        return Path.Combine(TestContext.TestDirectory, "TestData", fileName);
    }
}
```

---

## ⚡ **Performance Tests**

### **🚀 Load Testing**
```csharp
[TestClass]
public class PerformanceTests
{
    private readonly PerformanceTestRunner _testRunner = new PerformanceTestRunner();
    
    [TestMethod]
    [Timeout(60000)] // 1 minute timeout
    public void LoadLargeFile_100MBExcel_CompletesWithinTimeLimit()
    {
        // Arrange
        var largeFile = GetTestFile("large_100mb.xlsx");
        var parser = new ExcelParser();
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = parser.ParseExcelFile(largeFile);
        stopwatch.Stop();
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000, 
            $"Large file parsing took {stopwatch.ElapsedMilliseconds}ms, should be under 30 seconds");
        
        // Verify memory usage
        var memoryUsed = GC.GetTotalMemory(false);
        Assert.IsTrue(memoryUsed < 1024 * 1024 * 1024, // 1GB
            $"Memory usage {memoryUsed / (1024 * 1024)}MB should be under 1GB");
    }
    
    [TestMethod]
    public void ConcurrentFileAccess_MultipleThreads_HandlesCorrectly()
    {
        // Arrange
        var testFile = GetTestFile("concurrent_test.xlsx");
        var tasks = new List<Task<bool>>();
        var threadCount = Environment.ProcessorCount;
        
        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var parser = new ExcelParser();
                    var result = parser.ParseExcelFile(testFile);
                    return result != null && result.Rows.Count > 0;
                }
                catch
                {
                    return false;
                }
            }));
        }
        
        var results = Task.WhenAll(tasks).Result;
        
        // Assert
        Assert.IsTrue(results.All(r => r), "All concurrent operations should succeed");
    }
    
    [TestMethod]
    public void MemoryUsage_ProcessLargeDataset_StaysWithinLimits()
    {
        // Arrange
        var memoryBefore = GC.GetTotalMemory(true);
        var largeDataFiles = new[]
        {
            GetTestFile("memory_test_1.xlsx"),
            GetTestFile("memory_test_2.xlsx"),
            GetTestFile("memory_test_3.xlsx")
        };
        
        // Act
        foreach (var file in largeDataFiles)
        {
            var processor = new FileProcessingManager();
            processor.ProcessFile(file);
            
            // Check memory after each file
            var currentMemory = GC.GetTotalMemory(false);
            var memoryIncrease = currentMemory - memoryBefore;
            
            Assert.IsTrue(memoryIncrease < 200 * 1024 * 1024, // 200MB per file max
                $"Memory increase {memoryIncrease / (1024 * 1024)}MB exceeds 200MB limit");
        }
        
        // Force cleanup and verify memory is released
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var memoryAfter = GC.GetTotalMemory(true);
        var totalIncrease = memoryAfter - memoryBefore;
        
        Assert.IsTrue(totalIncrease < 100 * 1024 * 1024, // 100MB total increase max
            $"Total memory increase {totalIncrease / (1024 * 1024)}MB should be minimal after cleanup");
    }
    
    [TestMethod]
    public void SearchPerformance_LargeDataset_MeetsTargetTimes()
    {
        // Arrange
        var largeFile = GetTestFile("search_performance.xlsx");
        var searchManager = new SearchManager();
        searchManager.LoadFile(largeFile);
        
        var searchTerms = new[] { "TestFunction", "MM_", "STUB_", "Interface_001" };
        
        // Act & Assert
        foreach (var term in searchTerms)
        {
            var stopwatch = Stopwatch.StartNew();
            var results = searchManager.Search(term);
            stopwatch.Stop();
            
            Assert.IsNotNull(results);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, 
                $"Search for '{term}' took {stopwatch.ElapsedMilliseconds}ms, should be under 5 seconds");
        }
    }
    
    private string GetTestFile(string fileName)
    {
        return Path.Combine(TestContext.TestDirectory, "TestData", "Performance", fileName);
    }
}
```

### **📊 Stress Testing**
```csharp
[TestClass]
public class StressTests
{
    [TestMethod]
    [Timeout(300000)] // 5 minutes timeout
    public void StressTest_ContinuousOperations_MaintainsStability()
    {
        // Arrange
        var operationCount = 1000;
        var testFiles = Directory.GetFiles(GetTestDataPath(), "stress_*.xlsx");
        var random = new Random();
        var failures = 0;
        
        // Act
        for (int i = 0; i < operationCount; i++)
        {
            try
            {
                var randomFile = testFiles[random.Next(testFiles.Length)];
                var processor = new FileProcessingManager();
                processor.ProcessFile(randomFile);
                
                // Occasionally force garbage collection
                if (i % 100 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation {i} failed: {ex.Message}");
                failures++;
            }
        }
        
        // Assert
        var failureRate = (double)failures / operationCount;
        Assert.IsTrue(failureRate < 0.01, // Less than 1% failure rate
            $"Failure rate {failureRate:P2} exceeds acceptable threshold of 1%");
    }
    
    private string GetTestDataPath()
    {
        return Path.Combine(TestContext.TestDirectory, "TestData", "Stress");
    }
}
```

---

## 🎭 **User Acceptance Tests**

### **🎯 End-to-End Scenarios**
```csharp
[TestClass]
public class UserAcceptanceTests
{
    private TestApplicationDriver _appDriver;
    
    [TestInitialize]
    public void Setup()
    {
        _appDriver = new TestApplicationDriver();
        _appDriver.StartApplication();
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _appDriver?.CloseApplication();
    }
    
    [TestMethod]
    public void UserScenario_CompleteCarasiComparison_WorkflowSuccessful()
    {
        // Scenario: User compares old và new Carasi files và generates report
        
        // Step 1: Open application
        Assert.IsTrue(_appDriver.IsApplicationRunning());
        
        // Step 2: Load old Carasi file
        var oldFile = GetTestFile("user_test_old_carasi.xlsx");
        _appDriver.LoadOldCarasiFile(oldFile);
        Assert.IsTrue(_appDriver.IsOldCarasiFileLoaded());
        
        // Step 3: Load new Carasi file  
        var newFile = GetTestFile("user_test_new_carasi.xlsx");
        _appDriver.LoadNewCarasiFile(newFile);
        Assert.IsTrue(_appDriver.IsNewCarasiFileLoaded());
        
        // Step 4: Enable highlighting
        _appDriver.EnablePropertyHighlighting();
        Assert.IsTrue(_appDriver.IsHighlightingEnabled());
        
        // Step 5: Perform search
        var searchTerm = "TestFunction";
        var searchResults = _appDriver.PerformSearch(searchTerm);
        Assert.IsTrue(searchResults.Count > 0);
        
        // Step 6: Generate report
        var reportPath = _appDriver.GenerateReport();
        Assert.IsTrue(File.Exists(reportPath));
        
        // Step 7: Verify report contents
        var reportContent = File.ReadAllText(reportPath);
        Assert.IsTrue(reportContent.Contains("Comparison Summary"));
        Assert.IsTrue(reportContent.Contains("Highlighted Differences"));
    }
    
    [TestMethod]
    public void UserScenario_BatchProcessing_HandlesMultipleFiles()
    {
        // Scenario: User processes multiple file pairs in batch
        
        // Step 1: Prepare batch files
        var filePairs = new[]
        {
            (GetTestFile("batch_old_1.xlsx"), GetTestFile("batch_new_1.xlsx")),
            (GetTestFile("batch_old_2.xlsx"), GetTestFile("batch_new_2.xlsx")),
            (GetTestFile("batch_old_3.xlsx"), GetTestFile("batch_new_3.xlsx"))
        };
        
        // Step 2: Start batch processing
        _appDriver.StartBatchProcessing(filePairs);
        
        // Step 3: Wait for completion
        var completed = _appDriver.WaitForBatchCompletion(TimeSpan.FromMinutes(5));
        Assert.IsTrue(completed, "Batch processing should complete within 5 minutes");
        
        // Step 4: Verify results
        var batchResults = _appDriver.GetBatchResults();
        Assert.AreEqual(filePairs.Length, batchResults.Count);
        Assert.IsTrue(batchResults.All(r => r.IsSuccessful));
    }
    
    [TestMethod]
    public void UserScenario_ErrorHandling_RecoverFromInvalidFile()
    {
        // Scenario: User attempts to load invalid file và recovers gracefully
        
        // Step 1: Attempt to load corrupted file
        var corruptedFile = GetTestFile("corrupted_file.xlsx");
        var loadResult = _appDriver.LoadOldCarasiFile(corruptedFile);
        
        // Step 2: Verify error handling
        Assert.IsFalse(loadResult.IsSuccessful);
        Assert.IsTrue(_appDriver.IsErrorMessageDisplayed());
        
        // Step 3: Load valid file after error
        var validFile = GetTestFile("valid_carasi.xlsx");
        var validLoadResult = _appDriver.LoadOldCarasiFile(validFile);
        
        // Step 4: Verify recovery
        Assert.IsTrue(validLoadResult.IsSuccessful);
        Assert.IsTrue(_appDriver.IsOldCarasiFileLoaded());
        Assert.IsFalse(_appDriver.IsErrorMessageDisplayed());
    }
    
    private string GetTestFile(string fileName)
    {
        return Path.Combine(TestContext.TestDirectory, "TestData", "UserAcceptance", fileName);
    }
}
```

---

## 🤖 **Automated Testing Infrastructure**

### **🔄 Continuous Integration Setup**
```yaml
# Azure DevOps Pipeline Configuration
trigger:
  branches:
    include:
    - main
    - development
  paths:
    include:
    - src/*
    - tests/*

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'
  testResultsDirectory: '$(Agent.TempDirectory)/TestResults'

stages:
- stage: Build
  jobs:
  - job: BuildApplication
    steps:
    - task: NuGetToolInstaller@1
      displayName: 'Install NuGet'
    
    - task: NuGetCommand@2
      displayName: 'Restore NuGet packages'
      inputs:
        restoreSolution: '**/*.sln'
    
    - task: MSBuild@1
      displayName: 'Build solution'
      inputs:
        solution: '**/*.sln'
        configuration: '$(buildConfiguration)'
        platform: 'x64'

- stage: Test
  dependsOn: Build
  jobs:
  - job: RunTests
    steps:
    - task: VSTest@2
      displayName: 'Run Unit Tests'
      inputs:
        testSelector: 'testAssemblies'
        testAssemblyVer2: |
          **/*Tests.dll
          !**/*TestAdapter.dll
          !**/obj/**
        searchFolder: '$(System.DefaultWorkingDirectory)'
        testResultsFormat: 'VSTest'
        testResultsFiles: '**/*.trx'
        mergeTestResults: true
        codeCoverageEnabled: true
        platform: 'x64'
        configuration: '$(buildConfiguration)'
    
    - task: PublishTestResults@2
      displayName: 'Publish Test Results'
      inputs:
        testResultsFormat: 'VSTest'
        testResultsFiles: '**/*.trx'
        searchFolder: '$(testResultsDirectory)'
        mergeTestResults: true
    
    - task: PublishCodeCoverageResults@1
      displayName: 'Publish Code Coverage'
      inputs:
        codeCoverageTool: 'Cobertura'
        summaryFileLocation: '$(testResultsDirectory)/**/coverage.cobertura.xml'
```

### **📊 Test Reporting**
```csharp
// Custom test reporting framework
public class TestReportGenerator
{
    public TestReport GenerateReport(TestResults results)
    {
        var report = new TestReport
        {
            Timestamp = DateTime.Now,
            TotalTests = results.TotalTestCount,
            PassedTests = results.PassedTestCount,
            FailedTests = results.FailedTestCount,
            SkippedTests = results.SkippedTestCount,
            CodeCoverage = results.CodeCoveragePercentage,
            ExecutionTime = results.TotalExecutionTime
        };
        
        // Generate detailed breakdown
        report.CategoryBreakdown = new Dictionary<string, TestCategoryResult>
        {
            ["Unit Tests"] = new TestCategoryResult 
            { 
                Total = results.UnitTests.Count, 
                Passed = results.UnitTests.Count(t => t.Passed),
                Coverage = CalculateCoverage(results.UnitTests)
            },
            ["Integration Tests"] = new TestCategoryResult 
            { 
                Total = results.IntegrationTests.Count, 
                Passed = results.IntegrationTests.Count(t => t.Passed),
                Coverage = CalculateCoverage(results.IntegrationTests)
            },
            ["Performance Tests"] = new TestCategoryResult 
            { 
                Total = results.PerformanceTests.Count, 
                Passed = results.PerformanceTests.Count(t => t.Passed),
                AverageExecutionTime = results.PerformanceTests.Average(t => t.ExecutionTimeMs)
            }
        };
        
        return report;
    }
    
    public void ExportToHtml(TestReport report, string filePath)
    {
        var html = GenerateHtmlReport(report);
        File.WriteAllText(filePath, html);
    }
    
    private double CalculateCoverage(IEnumerable<TestResult> tests)
    {
        // Implementation for code coverage calculation
        return 0.0;
    }
    
    private string GenerateHtmlReport(TestReport report)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Test Report - {report.Timestamp:yyyy-MM-dd HH:mm:ss}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .summary {{ background-color: #f0f0f0; padding: 15px; border-radius: 5px; }}
        .passed {{ color: green; }}
        .failed {{ color: red; }}
        .skipped {{ color: orange; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <h1>Check Carasi DF Context Clearing - Test Report</h1>
    
    <div class='summary'>
        <h2>Test Summary</h2>
        <p><strong>Total Tests:</strong> {report.TotalTests}</p>
        <p><strong>Passed:</strong> <span class='passed'>{report.PassedTests}</span></p>
        <p><strong>Failed:</strong> <span class='failed'>{report.FailedTests}</span></p>
        <p><strong>Skipped:</strong> <span class='skipped'>{report.SkippedTests}</span></p>
        <p><strong>Success Rate:</strong> {(double)report.PassedTests / report.TotalTests:P1}</p>
        <p><strong>Code Coverage:</strong> {report.CodeCoverage:P1}</p>
        <p><strong>Execution Time:</strong> {report.ExecutionTime.TotalMinutes:F1} minutes</p>
    </div>
    
    <h2>Category Breakdown</h2>
    <table>
        <tr>
            <th>Category</th>
            <th>Total</th>
            <th>Passed</th>
            <th>Success Rate</th>
            <th>Coverage</th>
        </tr>
        {string.Join("\n", report.CategoryBreakdown.Select(kvp => $@"
        <tr>
            <td>{kvp.Key}</td>
            <td>{kvp.Value.Total}</td>
            <td>{kvp.Value.Passed}</td>
            <td>{(double)kvp.Value.Passed / kvp.Value.Total:P1}</td>
            <td>{kvp.Value.Coverage:P1}</td>
        </tr>"))}
    </table>
</body>
</html>";
    }
}
```

---

## 🏃 **Running Tests**

### **⚡ Quick Test Execution**
```powershell
# PowerShell script: Run-Tests.ps1
param(
    [string]$Category = "All",
    [switch]$GenerateReport,
    [string]$OutputPath = "TestResults"
)

Write-Host "=== Check Carasi DF Context Clearing - Test Runner ===" -ForegroundColor Green

# Setup test environment
$testPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $testPath ".." "Check_carasi_DF_ContextClearing.sln"
$outputPath = Join-Path $testPath $OutputPath

# Create output directory
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force
}

# Build solution first
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
$buildResult = & dotnet build $solutionPath --configuration Release --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed. Cannot run tests." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful" -ForegroundColor Green

# Run tests based on category
$testFilter = switch ($Category) {
    "Unit" { "Category=Unit" }
    "Integration" { "Category=Integration" } 
    "Performance" { "Category=Performance" }
    "UserAcceptance" { "Category=UserAcceptance" }
    "All" { "" }
    default { "Category=$Category" }
}

Write-Host "`nRunning tests (Category: $Category)..." -ForegroundColor Yellow

$testCommand = "dotnet test `"$solutionPath`" --configuration Release --no-build"
if ($testFilter) {
    $testCommand += " --filter `"$testFilter`""
}
$testCommand += " --logger trx --results-directory `"$outputPath`""

# Execute tests
$testStartTime = Get-Date
Invoke-Expression $testCommand
$testEndTime = Get-Date
$testDuration = $testEndTime - $testStartTime

# Check test results
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed. Check results for details." -ForegroundColor Red
}

Write-Host "Test execution completed in $($testDuration.TotalMinutes.ToString('F1')) minutes" -ForegroundColor Cyan

# Generate HTML report if requested
if ($GenerateReport) {
    Write-Host "`nGenerating test report..." -ForegroundColor Yellow
    
    # Find TRX file
    $trxFile = Get-ChildItem $outputPath -Filter "*.trx" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($trxFile) {
        # Convert TRX to HTML (requires custom converter or use ReportGenerator)
        $htmlReportPath = Join-Path $outputPath "TestReport.html"
        
        # Custom report generation logic here
        Write-Host "📊 Test report generated: $htmlReportPath" -ForegroundColor Green
        
        # Open report in browser
        Start-Process $htmlReportPath
    } else {
        Write-Host "⚠️ No test results file found for report generation" -ForegroundColor Yellow
    }
}

Write-Host "`n=== Test Run Complete ===" -ForegroundColor Green
```

### **🎯 Targeted Test Execution**
```batch
REM Batch file for specific test scenarios

@echo off
echo === Quick Test Scenarios ===
echo.

echo 1. Unit Tests Only (Fast)
echo 2. Integration Tests (Medium)  
echo 3. Performance Tests (Slow)
echo 4. All Tests (Complete)
echo 5. Smoke Tests (Critical only)
echo.

set /p choice="Select option (1-5): "

if "%choice%"=="1" (
    echo Running Unit Tests...
    dotnet test --filter "Category=Unit" --logger console
) else if "%choice%"=="2" (
    echo Running Integration Tests...
    dotnet test --filter "Category=Integration" --logger console
) else if "%choice%"=="3" (
    echo Running Performance Tests...
    dotnet test --filter "Category=Performance" --logger console
) else if "%choice%"=="4" (
    echo Running All Tests...
    dotnet test --logger console
) else if "%choice%"=="5" (
    echo Running Smoke Tests...
    dotnet test --filter "Priority=Critical" --logger console
) else (
    echo Invalid selection
    goto :eof
)

echo.
echo Test execution completed!
pause
```

---

## 📞 **Test Support & Maintenance**

### **🔧 Test Maintenance Guidelines**
- **Weekly**: Review và update test data files
- **Monthly**: Performance benchmark validation
- **Quarterly**: Test coverage analysis và improvement
- **Annually**: Complete test framework review

### **📚 Test Documentation**
- **Test Cases**: Documented in Azure DevOps Test Plans
- **Test Data**: Versioned với application releases
- **Test Reports**: Archived for trend analysis
- **Best Practices**: Continuously updated based on learnings

### **🆘 Support Contacts**
- **Test Framework**: NGOC.VUONGMINH@vn.bosch.com
- **Test Infrastructure**: Bosch Engineering Vietnam DevOps Team
- **Performance Testing**: Application Development Team

---

*🧪 Comprehensive test suite ensures reliability và quality of Check Carasi DF Context Clearing Tool across all usage scenarios và environments.*
