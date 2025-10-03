# FAQ - Frequently Asked Questions ❓

## 🤔 **Frequently Asked Questions**

Comprehensive collection của các câu hỏi thường gặp về Check Carasi DF Context Clearing Tool với answers được verify trong production environment.

---

## 🚀 **Getting Started Questions**

### **❓ Q1: What is Check Carasi DF Context Clearing Tool?**
**💡 A1:** Check Carasi DF Context Clearing Tool là một Windows Forms application chuyên dụng để analyze và compare automotive interface data. Tool này được design để:
- **Compare Carasi files**: Old vs New versions để detect changes  
- **Analyze DataFlow files**: Interface specifications và function mappings
- **Highlight differences**: Intelligent highlighting system với MM_/STUB_ prefix detection
- **Export results**: Generate reports và DD request emails
- **Batch processing**: Handle multiple files simultaneously với optimal performance

**🎯 Primary Users**: Automotive software engineers, system integrators, interface specialists tại Bosch và partners.

### **❓ Q2: Do I need Microsoft Office to use this tool?**
**💡 A2:** **NO**, Microsoft Office is NOT required! Tool sử dụng:
- **OLEDB providers** for Excel file access (lightweight approach)
- **EPPlus library** for advanced Excel operations  
- **Direct file reading** without requiring Excel application

**📦 Required Components:**
- ✅ **Microsoft Access Database Engine** (free download)
- ✅ **.NET Framework 4.7.2+** (usually pre-installed)
- ❌ **Microsoft Excel** (not required, but can coexist)

### **❓ Q3: Can I use this tool on non-Windows systems?**
**💡 A3:** **Currently Windows-only**, nhưng có alternatives:
- **Primary Platform**: Windows 7+ (x64) với .NET Framework
- **Virtual Machines**: Có thể run trên VMware/VirtualBox trên Mac/Linux
- **Remote Desktop**: Access Windows machines remotely
- **Future Plans**: Cross-platform version using .NET Core đang được considered

### **❓ Q4: Is my data secure when using this tool?**
**💡 A4:** **YES, completely secure**:
- **Local Processing**: All data processed locally, không có cloud transmission
- **No Network Requirements**: Tool hoạt động hoàn toàn offline
- **File Access**: Standard Windows file permissions, không có special privileges
- **No Data Collection**: Tool không collect hay transmit user data
- **Source Code**: Available for internal review (Bosch team)

---

## 🔧 **Installation & Setup Questions**

### **❓ Q5: Why do I get "OLEDB provider not registered" error?**
**💡 A5:** Most common installation issue. Solutions by priority:

**🥇 Solution 1: Install Access Database Engine**
```powershell
# Download và install Microsoft Access Database Engine 2016 x64
$url = "https://www.microsoft.com/download/details.aspx?id=54920"
# Install with: AccessDatabaseEngine_X64.exe /quiet
```

**🥈 Solution 2: Registry Fix**
```powershell
# Register OLEDB manually if Office installed
regsvr32 "C:\Program Files\Microsoft Office\root\VFS\ProgramFilesCommonX64\Microsoft Shared\OFFICE16\ACEOLEDB.DLL"
```

**🥉 Solution 3: Architecture Mismatch**
- Ensure both application và OLEDB provider are x64
- Uninstall 32-bit Office components nếu có conflicts

### **❓ Q6: Application won't start - what should I check?**
**💡 A6:** Step-by-step diagnostic:

**1. Check Prerequisites**
```powershell
# Verify .NET Framework 4.7.2+
$release = (Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release).Release
if ($release -ge 461808) { "✅ .NET OK" } else { "❌ Update .NET Framework" }
```

**2. Run as Administrator (temporary test)**
- Right-click → "Run as administrator"
- If works, likely permissions issue

**3. Check Event Viewer**
```powershell
# Check for application errors
Get-WinEvent -LogName Application -MaxEvents 5 | 
Where-Object {$_.LevelDisplayName -eq "Error"}
```

**4. Antivirus Exclusions**
- Add application folder to antivirus exclusions
- Temporarily disable real-time protection for testing

### **❓ Q7: Can I install this tool on a network drive?**
**💡 A7:** **Not recommended**, but possible with caveats:
- **Performance**: Significant slowdown (2-5x longer operations)
- **Reliability**: Network interruptions can cause crashes
- **Dependencies**: OLEDB providers must be installed locally
- **Permissions**: Network execution policies may block application

**🏆 Best Practice**: Install locally, store data files on network drives.

---

## 📊 **Usage & Functionality Questions**

### **❓ Q8: What file formats does the tool support?**
**💡 A8:** Complete support matrix:

| Format | Read | Write | Notes |
|--------|------|-------|-------|
| **Excel (.xlsx)** | ✅ Full | ✅ Export | Preferred format, all features |
| **Excel (.xls)** | ✅ Full | ✅ Export | Legacy format, full compatibility |
| **Excel (.xlsm)** | ✅ Basic | ❌ No | Macros ignored, data only |
| **A2L files** | ✅ Full | ❌ No | ASAM standard automotive files |
| **ARXML files** | ✅ Basic | ❌ No | AUTOSAR XML format |
| **CSV files** | ⚠️ Limited | ✅ Export | Via Excel import only |

### **❓ Q9: How does the PropertyDifferenceHighlighter work?**
**💡 A9:** Intelligent comparison system với advanced pattern matching:

**🎯 Detection Logic:**
```csharp
// Prefix matching for automotive naming conventions
MM_ prefix (Memory Mapped) vs STUB_ prefix detection
Function name changes: TestFunction_001 vs TestFunction_002
ADD cases: Empty cell → New function  
REMOVE cases: Existing function → Empty cell
```

**🎨 Color Scheme:**
- **🔴 Red Background**: Major differences requiring attention
- **🟡 Yellow Background**: Minor changes for review
- **🟢 Green Text**: ADD cases (new functions)
- **🔴 Red Text**: REMOVE cases (deleted functions)
- **⚪ White/Neutral**: No significant changes

**⚙️ Customization:**
- Colors can be adjusted in Settings
- Sensitivity levels: Low, Medium, High
- Pattern matching rules customizable

### **❓ Q10: Why are some search operations slow?**
**💡 A10:** Performance factors và optimization strategies:

**📊 Performance Factors:**
- **File size**: >50MB files require more processing time
- **Data complexity**: Files with many worksheets/complex formulas
- **Search scope**: Whole file vs specific sheets
- **System resources**: Available RAM và CPU cores

**⚡ Optimization Tips:**
```powershell
# 1. Increase available memory
# Close unused applications and browser tabs

# 2. Use targeted searches  
# Search specific worksheets instead of entire file

# 3. Enable caching
# Menu → Tools → Settings → Enable Smart Caching (default: ON)

# 4. Limit concurrent operations
# Process files sequentially for large datasets
```

**🔧 Advanced Settings:**
- **Batch Search Threads**: Default 4, adjust based on CPU cores
- **Memory Warning Threshold**: Default 500MB, increase if you have more RAM
- **Cache Size**: Default 50 files, adjust based on usage patterns

### **❓ Q11: Can I compare files with different structures?**
**💡 A11:** **YES**, tool handles structural differences intelligently:

**✅ Supported Scenarios:**
- Different column orders (auto-mapping by header names)
- Missing columns (highlighted as REMOVE cases)
- Extra columns (highlighted as ADD cases)  
- Different sheet names (manual mapping required)
- Different row counts (automatic alignment)

**🔧 Column Mapping Features:**
- **Automatic Detection**: Headers like "Function Name", "Interface Name"
- **Manual Override**: Right-click → "Map Columns" for custom mapping
- **Fuzzy Matching**: Handles minor header variations ("Func Name" = "Function_Name")
- **Case Insensitive**: "FUNCTION_NAME" matches "function name"

**⚠️ Limitations:**
- Completely different file purposes may not map well
- Binary/image data columns are ignored
- Heavily formatted cells may lose formatting in comparison

---

## 🛠️ **Advanced Features Questions**

### **❓ Q12: How do I generate DD (Data Dictionary) request emails?**
**💡 A12:** Automated email generation workflow:

**📧 Email Generation Process:**
1. **Complete Analysis**: Finish your file comparison
2. **Select Changes**: Highlight specific differences to include
3. **Generate Email**: Menu → Tools → Generate DD Request Email
4. **Review Content**: Automatic population with:
   - Changed functions list
   - Impact analysis  
   - Recommended actions
   - Technical details

**📝 Email Template Customization:**
```
Location: Menu → Tools → Settings → Email Templates
Default CC: team-automotive@bosch.com (configurable)
Signature: Auto-populated from Windows user info
Priority: Normal/High/Critical based on change impact
```

**🔧 Advanced Options:**
- **Batch Email Generation**: For multiple file comparisons
- **Custom Recipients**: Project-specific distribution lists
- **Attachment Options**: Include Excel files, screenshots, reports

### **❓ Q13: Can I automate repetitive tasks?**
**💡 A13:** Several automation features available:

**🤖 Built-in Automation:**
- **Recent Files**: Quick access to frequently used files
- **Search History**: Repeat previous searches with one click
- **Batch Processing**: Select multiple files for simultaneous processing
- **Auto-Save Settings**: Remember window positions, column widths, preferences

**📜 Command Line Support (Limited):**
```powershell
# Basic command line usage
Check_carasi_DF_ContextClearing.exe --file="C:\path\to\file.xlsx" --auto-analyze
```

**🔧 Integration Options:**
- **PowerShell Scripts**: Call application programmatically
- **Batch Files**: Automate file preparation workflows
- **Task Scheduler**: Schedule periodic analysis tasks

### **❓ Q14: How do I backup and restore my settings?**
**💡 A14:** Settings management và migration:

**📂 Settings Location:**
```
User Settings: %LOCALAPPDATA%\Check_carasi_DF_ContextClearing_URL_*
Application Config: C:\Program Files\Bosch\CarasiContextClearing\App.config
Cache Files: %TEMP%\Check_carasi_DF_ContextClearing_*
```

**💾 Backup Process:**
```powershell
# Manual backup script
$settingsPath = "$env:LOCALAPPDATA\Check_carasi_DF_ContextClearing*"
$backupPath = "$env:USERPROFILE\Documents\CarasiBackup_$(Get-Date -Format 'yyyyMMdd')"
Copy-Item $settingsPath $backupPath -Recurse -Force
```

**🔄 Restore Process:**
```powershell
# Stop application first
taskkill /F /IM "Check_carasi_DF_ContextClearing.exe" 2>$null

# Restore from backup
$backupPath = "$env:USERPROFILE\Documents\CarasiBackup_20250115"
$settingsPath = "$env:LOCALAPPDATA\"
Copy-Item "$backupPath\*" $settingsPath -Recurse -Force
```

---

## 🚨 **Troubleshooting Questions**

### **❓ Q15: Application crashes when opening large files - what can I do?**
**💡 A15:** Large file handling strategies:

**📊 File Size Guidelines:**
- **< 20MB**: Optimal performance, no special handling needed
- **20-50MB**: Good performance, may take 5-15 seconds to load
- **50-100MB**: Acceptable performance, consider increasing memory
- **> 100MB**: May require optimization techniques

**⚡ Optimization Techniques:**
```powershell
# 1. Increase virtual memory
# Control Panel → System → Advanced → Performance Settings → Virtual Memory

# 2. Close other applications
# Free up RAM for file processing

# 3. Split large files
# Use Excel to split into smaller worksheets

# 4. Use 64-bit system
# Ensures maximum memory availability
```

**🔧 Emergency Recovery:**
- **Auto-Recovery**: Tool auto-saves progress every 5 minutes
- **Safe Mode**: Hold Shift while starting to disable caching
- **Memory Cleanup**: Menu → Tools → Clear Cache and restart

### **❓ Q16: Search results are incomplete or incorrect - why?**
**💡 A16:** Search accuracy troubleshooting:

**🔍 Common Causes:**
1. **Hidden Characters**: Excel files may contain non-visible characters
2. **Formula Cells**: Search includes formula text, not just displayed values
3. **Merged Cells**: Content in merged cells may not be indexed properly
4. **Data Types**: Numbers stored as text vs actual numbers

**🛠️ Solutions:**
```powershell
# 1. Enable "Include Hidden Content" in search options
# Menu → Search → Advanced Options → Include Hidden Content

# 2. Use exact match mode for precise results
# Search box → Dropdown → "Exact Match"

# 3. Clear search cache
# Menu → Tools → Clear Search History

# 4. Refresh data source
# Press F5 or Menu → View → Refresh
```

**🔧 Advanced Search Tips:**
- **Wildcard Search**: Use * for partial matches ("Test*" finds "TestFunction")
- **Case Sensitivity**: Toggle in search options for precise matching
- **Regular Expressions**: Advanced pattern matching (enable in settings)

### **❓ Q17: UI elements are too small on high-DPI displays - how to fix?**
**💡 A17:** High-DPI display optimization:

**🖥️ Windows DPI Settings:**
```powershell
# Set application DPI awareness
$exePath = "C:\Program Files\Bosch\CarasiContextClearing\Check_carasi_DF_ContextClearing.exe"
$regPath = "HKCU:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"

# Create registry entry for DPI override
New-Item -Path $regPath -Force -ErrorAction SilentlyContinue
Set-ItemProperty -Path $regPath -Name $exePath -Value "HIGHDPIAWARE"
```

**⚙️ Application Settings:**
- **Font Scaling**: Menu → View → Font Size → 110%, 125%, 150%
- **UI Scaling**: Menu → View → Interface Scale → Large, Extra Large
- **Auto-Detect**: Enable "Auto-adjust for display" in Settings

**💡 Alternative Solutions:**
- **Windows Display Settings**: 125% or 150% scaling
- **Compatibility Mode**: Right-click app → Properties → Compatibility → DPI settings
- **External Monitor**: Use standard DPI monitor for detailed work

---

## 🎯 **Best Practices Questions**

### **❓ Q18: What are the recommended workflows for efficient analysis?**
**💡 A18:** Proven workflows optimized cho automotive engineering:

**🎯 Standard Comparison Workflow:**
```
1. File Preparation (5 min)
   ├── Verify file integrity và naming conventions
   ├── Check for password protection or corruption
   └── Organize files in logical folder structure

2. Initial Analysis (10-15 min)
   ├── Load Old và New Carasi files
   ├── Quick scan for major structural changes
   └── Enable PropertyDifferenceHighlighter

3. Detailed Comparison (30-45 min)
   ├── Systematic review of highlighted differences
   ├── Document significant changes với screenshots
   └── Cross-reference with DataFlow files

4. Report Generation (10-15 min)
   ├── Generate summary report
   ├── Prepare DD request email if needed
   └── Export results for stakeholders
```

**🔄 Batch Processing Workflow:**
```
1. Prepare File Lists
2. Use Menu → Tools → Batch Compare
3. Configure output options
4. Review consolidated results
5. Generate master report
```

### **❓ Q19: How should I organize my files for optimal workflow?**
**💡 A19:** File organization best practices:

**📁 Recommended Folder Structure:**
```
Project_YYYY_MM/
├── 01_Original_Files/
│   ├── Old_Carasi/
│   ├── New_Carasi/
│   └── DataFlow/
├── 02_Analysis_Results/
│   ├── Screenshots/
│   ├── Reports/
│   └── DD_Requests/
├── 03_Templates/
│   ├── Email_Templates/
│   └── Report_Templates/
└── 04_Archive/
    └── Previous_Versions/
```

**🏷️ File Naming Conventions:**
```
Carasi Files: YYYYMMDD_ProjectCode_Version_Type.xlsx
- 20250115_P001_v1.2_OldCarasi.xlsx
- 20250115_P001_v1.3_NewCarasi.xlsx

DataFlow Files: YYYYMMDD_System_Component_Version.xlsx
- 20250115_eVCU_ContextClearing_v2.1.xlsx

Reports: YYYYMMDD_Analysis_ProjectCode_vX.X.xlsx
- 20250115_Analysis_P001_v1.0.xlsx
```

### **❓ Q20: What are common mistakes to avoid?**
**💡 A20:** Critical mistakes và prevention strategies:

**❌ Common Mistakes:**

**1. File Version Confusion**
```
Problem: Comparing wrong file versions
Solution: Always verify file timestamps và version numbers
Tool: Use Menu → Tools → File Information to verify details
```

**2. Ignoring Structural Changes**  
```
Problem: Focusing only on data, missing column additions/removals
Solution: Enable "Show Structural Differences" in comparison settings
Tool: Use 4-panel view để see side-by-side structure
```

**3. Incomplete Analysis**
```
Problem: Missing critical differences in hidden worksheets
Solution: Always check all worksheets, including hidden ones
Tool: Menu → View → Show All Worksheets
```

**4. Poor Documentation**
```
Problem: Not documenting findings for future reference
Solution: Use built-in screenshot tool và export detailed reports
Tool: Menu → Tools → Generate Analysis Report
```

**5. Cache Issues**
```
Problem: Outdated cached data showing incorrect results
Solution: Regular cache cleanup, especially after file updates
Tool: Menu → Tools → Clear Cache (weekly recommended)
```

---

## 🔗 **Integration & Advanced Usage**

### **❓ Q21: Can I integrate this tool with other Bosch systems?**
**💡 A21:** Integration capabilities và enterprise features:

**🏢 Bosch Enterprise Integration:**
- **LDAP Authentication**: Domain user integration (configurable)
- **Network Drive Support**: Access shared project folders
- **Email Integration**: Automatic CC to team distribution lists
- **Reporting Systems**: Export compatible formats

**🔧 Technical Integration:**
```powershell
# PowerShell integration example
$app = Start-Process "Check_carasi_DF_ContextClearing.exe" -ArgumentList "--automation-mode" -PassThru
# COM interface planned for future versions
```

**📊 Data Export Options:**
- **Excel Reports**: Standardized templates
- **CSV Format**: For database import
- **JSON/XML**: For system integration  
- **PDF Reports**: For documentation

### **❓ Q22: Are there keyboard shortcuts to speed up work?**
**💡 A22:** Complete keyboard shortcuts reference:

**⌨️ Essential Shortcuts:**
| Shortcut | Action | Context |
|----------|--------|---------|
| **Ctrl+O** | Open File | Global |
| **Ctrl+S** | Save Analysis | Global |
| **Ctrl+F** | Find/Search | Global |
| **F5** | Refresh Data | Global |
| **Ctrl+T** | New Tab | Global |
| **Ctrl+W** | Close Tab | Tab view |
| **Ctrl+Tab** | Switch Tabs | Tab view |
| **Ctrl+1,2,3,4** | Switch Panels | 4-panel view |
| **Ctrl+H** | Toggle Highlighting | Comparison view |
| **Ctrl+E** | Export Results | Analysis complete |
| **Ctrl+Shift+C** | Clear Cache | Performance |
| **F1** | Help | Global |

**🔧 Advanced Shortcuts:**
| Shortcut | Action | Power User Feature |
|----------|--------|--------------------|
| **Ctrl+Shift+O** | Batch Open | Multiple files |
| **Ctrl+Shift+F** | Advanced Search | Cross-file search |
| **Ctrl+Shift+E** | Email Generation | DD requests |
| **Ctrl+Alt+S** | Screenshot Tool | Documentation |
| **Ctrl+Shift+R** | Generate Report | Analysis summary |

---

## 📞 **Support & Resources**

### **❓ Q23: Where can I get additional help and training?**
**💡 A23:** Comprehensive support ecosystem:

**🆘 Technical Support:**
- **Primary Contact**: NGOC.VUONGMINH@vn.bosch.com
- **Team Support**: Bosch Engineering Vietnam
- **Response Time**: 1-2 business days
- **Escalation**: Critical issues same-day response

**📚 Documentation Resources:**
- **[[Quick Start Guide|Quick-Start-Guide]]** - Getting started tutorial
- **[[System Requirements|System-Requirements]]** - Hardware/software specs  
- **[[Installation Guide|Installation-Guide]]** - Detailed setup instructions
- **[[Troubleshooting|Troubleshooting]]** - Problem resolution guide

**🎓 Training Options:**
- **Self-Paced**: Built-in tutorials và help system
- **Team Training**: Available for groups (contact support)
- **Video Tutorials**: Planned for common workflows
- **Best Practices**: Internal knowledge sharing sessions

### **❓ Q24: How do I report bugs or request new features?**
**💡 A24:** Bug reporting và feature request process:

**🐛 Bug Reports:**
```
Email Subject: [BUG] Brief description
Required Information:
1. Steps to reproduce
2. Expected vs actual behavior  
3. System information (OS, RAM, .NET version)
4. Screenshots/error messages
5. Sample files (if applicable)
```

**💡 Feature Requests:**
```
Email Subject: [FEATURE] Brief description  
Include:
1. Business justification
2. Current workaround (if any)
3. Proposed solution
4. Impact on workflow
5. Priority level
```

**🚀 Enhancement Tracking:**
- **GitHub Issues**: Internal tracking system
- **Roadmap Updates**: Quarterly feature planning
- **User Feedback**: Regular surveys và usage analytics
- **Beta Testing**: Early access for power users

### **❓ Q25: Is there a user community or forum?**
**💡 A25:** Community và knowledge sharing:

**👥 Internal Community:**
- **Bosch Engineering Teams**: Cross-team knowledge sharing
- **Best Practices Wiki**: Internal documentation
- **Monthly User Group**: Virtual meetings for power users
- **Expert Network**: Direct access to application developers

**📊 Knowledge Sharing:**
- **Use Case Documentation**: Real-world examples
- **Template Library**: Standardized workflows
- **Tips & Tricks**: Community-contributed optimizations
- **Success Stories**: Project case studies

**🔄 Feedback Loop:**
- **User Surveys**: Quarterly usage và satisfaction
- **Feature Voting**: Community-driven development priorities
- **Beta Programs**: Early access to new features
- **Training Feedback**: Continuous improvement of documentation

---

## 📈 **Future Development**

### **❓ Q26: What new features are planned for future versions?**
**💡 A26:** Roadmap và upcoming enhancements:

**🚀 Version 2025.1.0 (Q2 2025):**
- **Enhanced Search**: Regular expressions và fuzzy matching
- **Performance**: Multi-core optimization for large files
- **UI Improvements**: Modern theme options và customizable layouts
- **Export Options**: PowerBI-compatible data export

**🌟 Version 2025.2.0 (Q3 2025):**
- **API Integration**: RESTful API for system integration
- **Cloud Support**: OneDrive/SharePoint integration
- **Advanced Analytics**: Trend analysis across multiple versions
- **Automated Testing**: Built-in validation workflows

**🔮 Long-term Vision (2025+):**
- **Cross-Platform**: .NET Core version for Linux/Mac
- **Web Interface**: Browser-based version for universal access
- **AI Integration**: Machine learning for pattern detection
- **Real-time Collaboration**: Multi-user simultaneous editing

---

*❓ Didn't find your question? Contact technical support at NGOC.VUONGMINH@vn.bosch.com for personalized assistance!*
