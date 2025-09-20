# Test Suite cho Carasi DF Context Clearing Tool

## Tổng quan
Test suite này được thiết kế để kiểm tra toàn diện tool **Carasi DF Context Clearing** - một công cụ dùng trong phát triển phần mềm automotive để so sánh và phân tích các file interface (Carasi) và dataflow.

## Cấu trúc Test

### 1. Unit Tests (`/UnitTests`)
- **ExcelParserTests**: Kiểm tra chức năng parse Excel files
- **A2LCheckTests**: Kiểm tra tính năng đọc và tìm kiếm A2L files
- **MMCheckTests**: Kiểm tra macro module và ARXML file validation
- **LibOLEDBExcelTests**: Kiểm tra kết nối và đọc Excel thông qua OLEDB
- **ReviewIMChangeTests**: Kiểm tra tính năng review thay đổi interface
- **UCCarasiTests**: Kiểm tra user control Carasi
- **UCContextClearingTests**: Kiểm tra user control Context Clearing

### 2. Integration Tests (`/IntegrationTests`)
- **Form1IntegrationTests**: Kiểm tra main form và tích hợp các components
- **EndToEndTests**: Kiểm tra workflow hoàn chỉnh từ đầu đến cuối

### 3. Test Utilities (`/TestUtilities`)
- **TestDataHelper**: Utility tạo test data và test environment
- **MockDataGenerator**: Generator tạo mock data realistic cho automotive domain

### 4. Test Data (`/TestData`)
- Sample files: Carasi, DataFlow, A2L files for testing
- Mock data structures theo chuẩn automotive

## Cách chạy Tests

### Sử dụng Visual Studio
1. Mở solution trong Visual Studio
2. Build test project
3. Chạy tests thông qua Test Explorer

### Sử dụng Command Line
```bash
# Build test project
dotnet build Tests.csproj --configuration Debug

# Chạy tất cả tests
dotnet test Tests.csproj

# Chạy specific test category
dotnet test Tests.csproj --filter "Category=UnitTest"
```

### Sử dụng Batch Script
```bash
# Chạy script tự động
RunTests.bat
```

## Test Coverage

### Library Classes
- ✅ **Excel_Parser**: 95% coverage
  - Search functionality
  - File parsing
  - Error handling
  - Resource disposal

- ✅ **A2L_Check**: 90% coverage
  - File validation
  - Keyword searching
  - Path handling

- ✅ **MM_Check**: 95% coverage
  - Directory traversal
  - ARXML file detection
  - Validation logic

- ✅ **Lib_OLEDB_Excel**: 85% coverage
  - Connection string generation
  - Event handling
  - File type detection

- ✅ **Review_IM_change**: 80% coverage
  - Folder validation
  - Excel file processing
  - Data table management

### UI Components
- ✅ **Form1**: 70% coverage
  - Property management
  - UI state handling
  - Basic functionality

- ✅ **UC_Carasi**: 85% coverage
  - Control initialization
  - Property handling
  - UI behavior

- ✅ **UC_ContextClearing**: 90% coverage
  - Excel parser integration
  - Property management
  - Control lifecycle

## Test Data Structure

### Carasi Interface Test Data
```
Interface Name, Input, Output, Description, Unit, Min Value, Max Value, Resolution, SW Type, MM Type
TestInterface_001, Input1;Input2, Output1, Engine temperature, °C, -40, 150, 0.1, UINT16, CAN
```

### DataFlow Interface Test Data
```
Status, Description, RTE Direction, Mapping Type, FC Name, Producer, Consumers
Active, Engine temperature dataflow, Input, Direct, FC_ENG_Temp_Process, ENGINE_MGT, DIAG_MGT;HMI_CTRL
```

### A2L Test Data
```
MEASUREMENT TestInterface_001 "Engine temperature sensor value"
UINT16 0x1000 1 0 65535
ECU_ADDRESS 0x1000
```

## Best Practices

### 1. Test Isolation
- Mỗi test độc lập, không phụ thuộc vào test khác
- Sử dụng `[TestInitialize]` và `[TestCleanup]` đúng cách
- Clean up resources sau mỗi test

### 2. Mock Data
- Sử dụng `MockDataGenerator` để tạo realistic test data
- Test với cả valid và invalid data
- Cover edge cases và error conditions

### 3. Error Handling
- Test exception handling
- Verify error messages và logging
- Test recovery scenarios

### 4. Performance
- Test với file sizes khác nhau
- Monitor memory usage
- Test concurrent operations

## Dependencies

### Testing Framework
- MSTest.TestFramework (3.1.1)
- MSTest.TestAdapter (3.1.1)

### Mocking
- Moq (4.20.69)

### Assertions
- FluentAssertions (6.12.0)

### Project References
- Check_carasi_DF_ContextClearing (main project)

## Continuous Integration

### Build Pipeline
1. Restore NuGet packages
2. Build main project
3. Build test project
4. Run unit tests
5. Run integration tests
6. Generate coverage report
7. Publish test results

### Test Categories
- `UnitTest`: Fast, isolated tests
- `IntegrationTest`: Tests với external dependencies
- `EndToEndTest`: Complete workflow tests
- `PerformanceTest`: Performance và load tests

## Troubleshooting

### Common Issues

1. **OLEDB Provider không tìm thấy**
   - Cài đặt Microsoft Access Database Engine
   - Check 32-bit vs 64-bit compatibility

2. **Excel files không đọc được**
   - Verify file format (xls vs xlsx)
   - Check file permissions
   - Ensure file không bị corrupt

3. **Path issues trong tests**
   - Sử dụng absolute paths
   - Handle directory separators correctly
   - Check file exists before testing

4. **Memory leaks trong tests**
   - Dispose Excel parsers properly
   - Clean up DataTables
   - Close file connections

### Debug Mode
```csharp
// Enable debug mode in tests
#if DEBUG
    Console.WriteLine($"Test data directory: {testDirectory}");
    Console.WriteLine($"Processing file: {filePath}");
#endif
```

## Contributing

### Adding New Tests
1. Follow naming convention: `MethodName_Scenario_ExpectedResult`
2. Add comprehensive comments
3. Include both positive và negative test cases
4. Update test coverage metrics

### Test Data
- Add new test files vào `/TestData` folder
- Update `TestDataHelper` nếu cần new utilities
- Document test data format và structure

### Code Quality
- Follow existing code style
- Add XML documentation cho public methods
- Ensure tests pass consistently
- Review test performance impact

## Future Enhancements

1. **Performance Tests**: Add load testing với large files
2. **UI Tests**: Automated UI testing với coded UI
3. **Database Tests**: Test database connectivity features
4. **Security Tests**: Test file access security
5. **Localization Tests**: Test với different languages và cultures

## Support

Để hỗ trợ hoặc báo cáo bugs trong test suite:
1. Tạo issue trong project repository
2. Include test output và error messages
3. Provide steps để reproduce issue
4. Attach sample test data nếu cần
