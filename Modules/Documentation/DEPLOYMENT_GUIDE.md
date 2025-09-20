# Check Carasi DF Context Clearing Tool - Deployment Guide

## 📦 **System Requirements**

### **Operating System:**
- Windows 10 (64-bit) hoặc Windows 11
- Windows Server 2019/2022 (nếu chạy trên server)

### **Framework Requirements:**
- **.NET Framework 4.7.2** (REQUIRED)
  - Download: https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472
  - Phần lớn máy Windows 10 đã có sẵn
  - Kiểm tra: `Control Panel > Programs > Programs and Features`

### **Microsoft Office Components:**
- **Microsoft Office 2016, 2019, 2021, hoặc Microsoft 365**
  - Cần Excel để đọc/ghi .xls/.xlsx files
  - Cần Outlook để gửi email (tùy chọn)
  - **Office Interop Libraries** sẽ được embed trong .exe

### **OLEDB Drivers:**
- **Microsoft Access Database Engine Redistributable**
  - Download: https://www.microsoft.com/en-us/download/details.aspx?id=54920
  - Choose: `AccessDatabaseEngine_X64.exe` (64-bit)
  - Cần thiết để đọc Excel files qua OLEDB

### **Visual C++ Redistributable:**
- **Microsoft Visual C++ 2015-2022 Redistributable (x64)**
  - Download: https://aka.ms/vs/17/release/vc_redist.x64.exe
  - Cần cho các COM components

---

## 🗂️ **Distribution Package Contents**

```
Check_Carasi_DF_ContextClearing_v1.1.x/
├── 📄 Check_carasi_DF_ContextClearing.exe        # Main executable
├── 📄 Check_carasi_DF_ContextClearing.exe.config # Configuration file
├── 📄 README.md                                  # This file
├── 📄 SYSTEM_REQUIREMENTS.txt                    # Detailed requirements
├── 📁 Dependencies/                              # Required installers
│   ├── 📄 NDP472-KB4054530-x86-x64-AllOS-ENU.exe    # .NET Framework 4.7.2
│   ├── 📄 AccessDatabaseEngine_X64.exe               # OLEDB Engine
│   └── 📄 vc_redist.x64.exe                          # Visual C++ Redistributable
└── 📁 Documentation/
    ├── 📄 User_Manual.pdf                        # User guide
    └── 📄 Performance_Guide.md                   # Optimization tips
```

---

## 🚀 **Installation Instructions**

### **Step 1: Check Prerequisites**
```powershell
# Check .NET Framework version
Get-ChildItem 'HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse | Get-ItemProperty -Name version -EA 0 | Where { $_.PSChildName -Match '^(?!S)\p{L}'} | Select PSChildName, version
```

### **Step 2: Install Missing Dependencies**
1. **Install .NET Framework 4.7.2** (if not present):
   ```
   Run: Dependencies\NDP472-KB4054530-x86-x64-AllOS-ENU.exe
   ```

2. **Install Access Database Engine** (for OLEDB Excel support):
   ```
   Run: Dependencies\AccessDatabaseEngine_X64.exe
   ```

3. **Install Visual C++ Redistributable**:
   ```
   Run: Dependencies\vc_redist.x64.exe
   ```

### **Step 3: Deploy Application**
1. Copy toàn bộ folder đến vị trí mong muốn (VD: `C:\Tools\Check_Carasi\`)
2. Create shortcut to `Check_carasi_DF_ContextClearing.exe` on Desktop
3. Right-click shortcut → Properties → **Run as Administrator** (nếu cần)

---

## ⚡ **Performance Optimization Features**

### **Option 6 Hybrid Approach:**
- ✅ **Batch Queries**: Process multiple variables simultaneously
- ✅ **Connection Pooling**: Reuse OLEDB connections (max 10 concurrent)
- ✅ **Intelligent Caching**: Cache query results with TTL (10 minutes)
- ✅ **Real-time Monitoring**: Status indicators on menubar

### **Resource Management:**
- ✅ **Tab Limits**: Max 60 tabs to prevent crashes
- ✅ **Memory Protection**: Auto cleanup at 50+ tabs
- ✅ **Progressive Warnings**: 58 tabs (warning) → 60 tabs (hard stop)

---

## 🎯 **Usage Guidelines**

### **File Structure Requirements:**
Folder phải chứa 4 files:
- `*newcarasi*.xls(x)`
- `*oldcarasi*.xls(x)`  
- `*newdataflow*.xls(x)`
- `*olddataflow*.xls(x)`

### **Performance Tips:**
- **Single Search**: 1-2 seconds per variable
- **Batch Search**: 10x faster cho 50+ variables
- **Cache Hit**: Instant results cho repeated searches
- **Optimal Range**: 30-50 tabs for best performance

### **Status Indicators:**
- 🟢 **Green**: Normal operation
- 🟠 **Orange**: Warning (approaching limits)
- 🔴 **Red**: High usage (close tabs recommended)

---

## 🔧 **Troubleshooting**

### **Common Issues:**

#### **"Could not load file or assembly" error:**
```
Solution: Install .NET Framework 4.7.2
Download: https://dotnet.microsoft.com/download/dotnet-framework/net472
```

#### **"The Microsoft Access database engine could not find the object" error:**
```
Solution: Install Access Database Engine 2016 Redistributable
Download: https://www.microsoft.com/download/details.aspx?id=54920
```

#### **"System.Runtime.InteropServices.COMException" error:**
```
Solution: 
1. Install Microsoft Office (any version 2016+)
2. Or install Visual C++ Redistributable
3. Run as Administrator
```

#### **Application crashes at 60+ tabs:**
```
Solution: This is by design - Windows Forms TabControl limit
Workaround: Close unused tabs before reaching limit
```

### **Performance Issues:**
- Close unused tabs regularly
- Clear cache via menu if memory issues
- Restart application after processing 100+ variables

---

## 🛡️ **Security Notes**

- Application requires Excel file read permissions
- Outlook integration requires COM access (for email features)
- Temporary files stored in `%TEMP%` directory
- No network connectivity required (except for email)
- All processing done locally

---

## 📞 **Support Information**

**Developer:** Vuong Minh Ngoc  
**Version:** 1.1.x with Option 6 Hybrid Optimization  
**Build Date:** September 2025  
**Repository:** https://github.com/vuongminhngoc93/Check_Carasi_Context_Clearing

**Performance Improvements:**
- 10x faster search for large datasets
- Professional UI with real-time monitoring
- Intelligent resource management
- Stable operation up to 60 tabs

---

## 📝 **Changelog**

### **v1.1.x - Option 6 Hybrid Optimization**
- ✅ Implemented batch queries + connection pooling + caching
- ✅ Added professional status bar with resource monitoring
- ✅ Fixed batch operation warning systems
- ✅ 10x performance improvement for 50+ variables
- ✅ Enhanced stability and resource protection

### **Previous Versions**
- v1.0.x: Basic functionality with individual searches
