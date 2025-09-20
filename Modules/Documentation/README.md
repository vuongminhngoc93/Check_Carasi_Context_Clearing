# Carasi DF Context Clearing Tool

## Overview
Công cụ kiểm tra và so sánh interface files (Carasi) và dataflow files trong phát triển phần mềm ô tô (automotive software development).

## Features
- ✅ Đọc và phân tích file Excel Carasi interface
- ✅ Đọc và phân tích file Excel Dataflow mapping  
- ✅ Kiểm tra file A2L (ASAM standards)
- ✅ Validation file ARXML (AUTOSAR)
- ✅ So sánh và báo cáo sự khác biệt
- ✅ Giao diện Windows Forms thân thiện

## Technology Stack
- **.NET Framework 4.7.2**
- **C# 7.3**
- **Windows Forms**
- **Excel OLEDB** connectivity
- **ASAM A2L** file support
- **AUTOSAR ARXML** file support

## Project Structure
```
Check_carasi_DF_ContextClearing/
├── Library/                     # Core business logic
│   ├── Excel_Parser.cs         # Excel file processing
│   ├── A2L_Check.cs            # A2L file validation
│   ├── MM_Check.cs             # Macro module validation
│   ├── Lib_OLEDB_Excel.cs      # Excel database connectivity
│   └── Review_IM_change.cs     # Interface change review
├── View/                       # User interface components
│   ├── UC_Carasi.cs           # Carasi interface control
│   ├── UC_ContextClearing.cs  # Context clearing control
│   ├── UC_dataflow.cs         # Dataflow control
│   └── PopUp_ProjectInfo.cs   # Project info popup
├── Tests/                      # Test suite
│   ├── SimpleTestRunner.cs    # Main test executor
│   ├── TestUtilities/         # Test helper classes
│   └── TestData/              # Sample test files
├── Resources/                  # Template files
└── Properties/                 # Assembly information
```

## Test Suite
Comprehensive test coverage với **13/13 tests passing** ✅

### Test Categories:
1. **Excel Parser Tests** - Kiểm tra struct `Carasi_Interface` và `Dataflow_Interface`
2. **Data Helper Tests** - Test utilities và mock data generation
3. **File Utilities Tests** - Validation cơ bản cho file operations

### Running Tests:
```bash
# Build test project
msbuild Tests\Tests.csproj /p:Configuration=Debug

# Run tests
.\Tests\bin\Debug\Check_carasi_DF_ContextClearing.Tests.exe
```

## Usage
1. Khởi động ứng dụng `Check_carasi_DF_ContextClearing.exe`
2. Chọn file Carasi interface (.xlsx)
3. Chọn file Dataflow mapping (.xlsx)
4. Chọn file A2L (optional)
5. Thực hiện so sánh và xem báo cáo

## Development Environment
- **Visual Studio 2019 Enterprise**
- **MSBuild 16.11.6**
- **Windows 10/11**

## Automotive Domain Knowledge
Tool này được thiết kế đặc biệt cho:
- **PSA/Stellantis** interface specifications
- **Bosch** ECU development standards
- **ASAM A2L** measurement and calibration
- **AUTOSAR ARXML** software component interfaces

## Team Information
Công cụ phát triển nội bộ cho team automotive software development.

## License
Internal tool - Not for public distribution

---
*Last updated: September 2025*
