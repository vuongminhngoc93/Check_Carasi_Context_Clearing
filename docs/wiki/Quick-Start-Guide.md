# Quick Start Guide 🚀

## 📋 **Getting Started in 5 Minutes**

Hướng dẫn nhanh để bắt đầu sử dụng Check Carasi DF Context Clearing Tool cho automotive interface analysis.

---

## ⚡ **Prerequisites Check**

### 🔧 **System Requirements**
```powershell
# Run this in PowerShell to check your system
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, TotalPhysicalMemory
```

**Minimum Requirements:**
- ✅ **Windows 7+** (Windows 10/11 recommended)
- ✅ **.NET Framework 4.7.2** (usually pre-installed)
- ✅ **Microsoft Access Database Engine** (for Excel processing)
- ✅ **4GB RAM** (8GB+ recommended)
- ✅ **500MB free disk space**

### 🔍 **Quick Dependency Check**
```batch
# Check .NET Framework
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" /v Release

# Check OLEDB providers
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Microsoft.ACE.OLEDB.12.0"
```

---

## 📥 **Installation Steps**

### **Method 1: Simple Installation (Recommended)**

1. **📥 Download** latest release from [GitHub Releases](https://github.com/vuongminhngoc93/Check_Carasi_Context_Clearing/releases)
2. **📦 Extract** `Check_carasi_DF_ContextClearing_v2025.0.2.1.zip`
3. **🎯 Run** `Check_carasi_DF_ContextClearing.exe`

```bash
# If you encounter OLEDB errors, install dependencies:
# 1. Download Access Database Engine 2016 Redistributable (x64)
# 2. Run: AccessDatabaseEngine_X64.exe
# 3. Restart the application
```

### **Method 2: Development Setup**

```bash
git clone https://github.com/vuongminhngoc93/Check_Carasi_Context_Clearing.git
cd Check_carasi_DF_ContextClearing
# Open Check_carasi_DF_ContextClearing.sln in Visual Studio
# Build and run (F5)
```

---

## 🎯 **First Usage Tutorial**

### **Step 1: Launch Application**
```
🚀 Double-click Check_carasi_DF_ContextClearing.exe
✅ Application window opens with modern interface
📊 Status bar shows: Tabs: 1/60, RAM: ~150MB, Cache: 0, Pool: 0
```

### **Step 2: Setup Project Folder**
```
📁 Click "Browse" button next to "Link of Folder"
📂 Select folder containing your Carasi and DataFlow files
✅ Folder path appears in textbox
💾 Path is automatically saved for next session
```

**Recommended Folder Structure:**
```
ProjectFolder/
├── Carasi/
│   ├── 01552_25_03641_v1_0_newCARASI_HEV_J3U_LatAm_P2F2.xlsx
│   └── 01552_21_02627_v31_1_oldCARASI_eVCU_CTEPh2022_W1C9.xlsx
├── DataFlow/
│   ├── swsapsa_newdataflow_LATAM.xls
│   └── swsapsa_olddataflow_evcu_w1c9.xls
└── A2L/
    └── VC1CP019_V1070C_1.a2l
```

### **Step 3: Your First Search**
```
🔍 Type interface name in "Name of Interface" textbox
   Example: "VehicleSpeed_kmh"
⚡ Press Enter or click "Run" button
📊 Progress bar shows search progress
🎯 Results appear in 4-panel comparison view
```

**Example Search Results:**
```
┌─────────────────┬─────────────────┐
│   Old Carasi    │   New Carasi    │
│ VehicleSpeed_kmh│ VehicleSpeed_kmh│
│ Type: uint16    │ Type: uint16    │ ← Same (no highlighting)
│ Range: 0-300    │ Range: 0-350    │ ← Different (highlighted)
├─────────────────┼─────────────────┤
│  Old DataFlow   │  New DataFlow   │
│ Producer: ESP   │ Producer: ESP   │
│ Consumer: HMI   │ Consumer: HMI   │ ← Consumer added (highlighted)
│                 │ Consumer: TCU   │
└─────────────────┴─────────────────┘
```

### **Step 4: Understanding Results**

#### **🎨 Color Coding System**
- **🔴 Light Red Background**: Old value (different from new)
- **🟢 Light Green Background**: New value (different from old)  
- **🟡 Light Yellow Background**: ADD/REMOVE cases (neutral highlighting)
- **⚪ White Background**: Identical values (no differences)

#### **📊 Interface Information Display**
```
Interface Properties:
├── 📝 Description: Functional description
├── 🔢 Type: Data type (uint16, float32, boolean, etc.)
├── 📏 Unit: Physical unit (km/h, °C, bar, etc.)
├── 📈 Range: Min/Max values
├── 🎯 Resolution: Precision/step size
├── 🔧 Conversion: Scaling factor
└── 💡 Hint: Development guidance
```

---

## 🚀 **Advanced Quick Start**

### **🔍 Batch Search (Multiple Variables)**

1. **📝 Create Variable List**
```text
# Create variables.txt file:
VehicleSpeed_kmh
EngineRPM_rpm
CoolantTemp_degC
FuelLevel_percent
```

2. **🔍 Execute Batch Search**
```
📋 Menu: Search → Batch Search List (Ctrl+Shift+F)
📁 Select your variables.txt file
⚡ Watch progress: "Processing 4/4 variables..."
📊 Results appear in dedicated tabs
🎯 60-80% faster than individual searches!
```

### **🎨 Property Highlighting Toggle**

```
🎨 Menu: Tools → Property Highlighting (F8)
✅ Toggle ON: Visual differences highlighted
❌ Toggle OFF: Clean view without highlighting
🔄 Real-time: Changes apply immediately to all tabs
```

### **📧 Professional DD Request Generation**

```
📧 Menu: Tools → DD Request (F5)
📝 Automatic email template generation
👥 Pre-filled distribution list
📅 Project timeline with deadlines
📤 Opens in Outlook for review and sending
```

---

## 🔧 **Performance Tips**

### **⚡ Speed Optimization**

1. **📁 Keep Files Local**
   ```
   ✅ Good: C:\Projects\Carasi\files.xlsx
   ❌ Slow: \\network\share\files.xlsx (network delays)
   ```

2. **🚀 Use Batch Search**
   ```
   ✅ Fast: Batch search 50 variables → 30 seconds
   ❌ Slow: 50 individual searches → 150 seconds
   ```

3. **💾 Leverage Caching**
   ```
   📊 First search: 2-3 seconds (file parsing)
   📊 Cached search: 0.2-0.5 seconds (85-95% faster!)
   ```

### **🛡️ Resource Management**

1. **📊 Monitor Status Bar**
   ```
   Tabs: 25/60     ← Tab count (warning at 50)
   RAM: 450MB      ← Memory usage (warning at 500MB)
   Cache: 5        ← Cached parsers
   Pool: 3/10      ← OLEDB connections
   ```

2. **🧹 Regular Cleanup**
   ```
   🗑️ Close unused tabs (Right-click → Close)
   🔄 Restart app if memory > 800MB
   📁 Keep project folders organized
   ```

---

## 🆘 **Quick Troubleshooting**

### **❌ Common Issues & Solutions**

#### **🔴 "OLEDB provider not found"**
```powershell
# Solution: Install Access Database Engine
Invoke-WebRequest -Uri "https://download.microsoft.com/download/3/5/C/35C84C36-661A-44E6-9324-8786B8DBE231/AccessDatabaseEngine_X64.exe" -OutFile "AccessDatabaseEngine_X64.exe"
.\AccessDatabaseEngine_X64.exe
```

#### **🔴 "File not found" errors**
```
✅ Check: File paths are correct
✅ Check: Files are not locked by Excel
✅ Check: Network drives are accessible
✅ Try: Copy files to local drive
```

#### **🔴 Slow performance**
```
✅ Check: RAM usage in status bar
✅ Try: Close unused tabs
✅ Try: Restart application
✅ Check: Antivirus real-time scanning
```

#### **🔴 Application won't start**
```powershell
# Check .NET Framework
Get-ChildItem 'HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse | 
Get-ItemProperty -Name version -EA 0 | 
Where { $_.PSChildName -Match '^(?!S)\p{L}'} | 
Select PSChildName, version
```

### **🔍 Debug Mode**
```
🛠️ Hold Shift while starting application
📊 Extra debug information in console
📝 Detailed performance logging
🔍 Connection string diagnostics
```

---

## 📚 **Next Steps**

### **📖 Detailed Documentation**
- **[[System Architecture|System-Architecture]]** - Technical details
- **[[Features Overview|Features-Overview]]** - Complete feature list
- **[[Technology Stack|Technology-Stack]]** - Framework documentation
- **[[Installation Guide|Installation-Guide]]** - Detailed setup instructions

### **🎯 Advanced Features**
- **A2L File Integration** - ASAM standard support
- **Multi-Branch Search** - Cross-project analysis
- **Property Highlighting** - Advanced difference visualization
- **Batch Operations** - High-performance bulk processing

### **💬 Support**
- **📧 Email**: NGOC.VUONGMINH@vn.bosch.com
- **📖 Wiki**: Browse documentation sections
- **🐛 Issues**: Report problems on GitHub
- **💡 Features**: Request enhancements

---

## ✅ **Quick Success Checklist**

```
□ Application starts without errors
□ Can browse and select project folder
□ Search returns results in 4-panel view  
□ Status bar shows green resource indicators
□ Property highlighting works (if enabled)
□ Can create and switch between tabs
□ Cache shows improved performance on repeat searches
□ No OLEDB connection errors
```

**🎉 Congratulations! You're ready to use Check Carasi DF Context Clearing Tool for professional automotive interface analysis.**

---

*Need help? Check [[FAQ|FAQ]] or [[Troubleshooting|Troubleshooting]] sections for detailed assistance.*
