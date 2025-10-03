# Installation Guide 📦

## 📋 **Complete Installation Guide**

Hướng dẫn chi tiết cài đặt Check Carasi DF Context Clearing Tool cho production environment và development setup.

---

## 🔧 **System Requirements**

### **💻 Hardware Requirements**

| Component | Minimum | Recommended | Enterprise |
|-----------|---------|-------------|------------|
| **CPU** | Dual-core 2.0GHz | Quad-core 2.5GHz+ | 8-core 3.0GHz+ |
| **RAM** | 4GB | 8GB | 16GB+ |
| **Storage** | 500MB free | 2GB free | 10GB+ SSD |
| **Display** | 1024x768 | 1920x1080 | Dual monitors |
| **Network** | Optional | Ethernet/WiFi | High-speed LAN |

### **🖥️ Operating System Compatibility**

| OS Version | Status | Notes |
|------------|--------|-------|
| **Windows 11** | ✅ Fully Supported | Recommended |
| **Windows 10** | ✅ Fully Supported | All builds |
| **Windows 8.1** | ✅ Supported | Update required |
| **Windows 7 SP1** | ⚠️ Limited Support | .NET 4.7.2 may need manual install |
| **Windows Server 2019/2022** | ✅ Supported | Enterprise environments |
| **Windows Server 2016** | ✅ Supported | With latest updates |

### **🔗 Dependencies Matrix**

| Dependency | Version | Status | Download Link |
|------------|---------|--------|---------------|
| **.NET Framework** | 4.7.2+ | ✅ Required | [Microsoft Download](https://dotnet.microsoft.com/download/dotnet-framework/net472) |
| **Access Database Engine** | 2016 x64 | ✅ Required | [Microsoft Download](https://www.microsoft.com/download/details.aspx?id=54920) |
| **Visual C++ Redistributable** | 2015-2022 x64 | ⚠️ Optional | [Microsoft Download](https://aka.ms/vs/17/release/vc_redist.x64.exe) |
| **Microsoft Office** | 2016+ | 🔵 Optional | For advanced Excel features |
| **Microsoft Outlook** | 2016+ | 🔵 Optional | For DD request emails |

---

## 📥 **Installation Methods**

### **Method 1: Standalone Installation (Recommended)**

#### **🎯 Step-by-Step Installation**

1. **📥 Download Release Package**
   ```powershell
   # Download latest release
   $LatestRelease = "https://github.com/vuongminhngoc93/Check_Carasi_Context_Clearing/releases/latest"
   # Download Check_carasi_DF_ContextClearing_v2025.0.2.1.zip
   ```

2. **🛡️ Security Verification**
   ```powershell
   # Check file integrity (if hash provided)
   Get-FileHash "Check_carasi_DF_ContextClearing_v2025.0.2.1.zip" -Algorithm SHA256
   ```

3. **📦 Extract Application**
   ```powershell
   # Extract to Program Files
   Expand-Archive -Path "Check_carasi_DF_ContextClearing_v2025.0.2.1.zip" -DestinationPath "C:\Program Files\Bosch\CarasiContextClearing"
   ```

4. **🔧 Dependency Installation**
   ```batch
   # Run as Administrator
   # Install .NET Framework 4.7.2 (if not present)
   NDP472-KB4054530-x86-x64-AllOS-ENU.exe /quiet
   
   # Install Access Database Engine 2016 x64
   AccessDatabaseEngine_X64.exe /quiet
   
   # Optional: Visual C++ Redistributable
   vc_redist.x64.exe /quiet /norestart
   ```

5. **✅ Verification**
   ```powershell
   # Test application startup
   cd "C:\Program Files\Bosch\CarasiContextClearing"
   .\Check_carasi_DF_ContextClearing.exe --version
   ```

#### **🔗 Desktop Shortcut Creation**
```powershell
# Create desktop shortcut
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$Home\Desktop\Carasi Context Clearing.lnk")
$Shortcut.TargetPath = "C:\Program Files\Bosch\CarasiContextClearing\Check_carasi_DF_ContextClearing.exe"
$Shortcut.Description = "Check Carasi DF Context Clearing Tool v2025.0.2.1"
$Shortcut.IconLocation = "C:\Program Files\Bosch\CarasiContextClearing\3192622.ico"
$Shortcut.Save()
```

### **Method 2: Development Installation**

#### **🛠️ Development Environment Setup**

1. **📥 Clone Repository**
   ```bash
   git clone https://github.com/vuongminhngoc93/Check_Carasi_Context_Clearing.git
   cd Check_carasi_DF_ContextClearing
   ```

2. **🔧 Development Dependencies**
   ```powershell
   # Visual Studio 2019/2022 with .NET Framework development workload
   # Install Visual Studio Community (free) or Professional
   
   # Required components:
   # - .NET Framework 4.7.2 targeting pack
   # - Windows Forms designer
   # - MSTest testing framework
   # - Git for Windows
   ```

3. **📦 NuGet Package Restore**
   ```powershell
   # Restore packages (if any)
   nuget restore Check_carasi_DF_ContextClearing.sln
   ```

4. **🔨 Build & Test**
   ```bash
   # Build solution
   msbuild Check_carasi_DF_ContextClearing.sln /p:Configuration=Release /p:Platform=x64
   
   # Run tests
   cd Tests
   .\SimpleTestRunner.exe
   ```

---

## 🏢 **Enterprise Deployment**

### **📋 Group Policy Deployment**

#### **MSI Package Creation**
```xml
<!-- Create deployment manifest -->
<Package>
    <Identity Name="Check_carasi_DF_ContextClearing" 
              Version="2025.0.2.1" 
              Publisher="Bosch Engineering" />
    <Properties>
        <DisplayName>Check Carasi DF Context Clearing Tool</DisplayName>
        <Description>Automotive interface analysis and comparison tool</Description>
        <Logo>3192622.ico</Logo>
    </Properties>
    <Dependencies>
        <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" />
        <PackageDependency Name="Microsoft.NETFramework" MinVersion="4.7.2" />
        <PackageDependency Name="Microsoft.ACE.OLEDB" MinVersion="16.0" />
    </Dependencies>
</Package>
```

#### **Silent Installation Script**
```batch
@echo off
REM Enterprise silent installation script
REM Run as Administrator

echo Installing Check Carasi DF Context Clearing Tool...

REM Create installation directory
mkdir "C:\Program Files\Bosch\CarasiContextClearing" 2>nul

REM Copy application files
xcopy /E /I /Y "\\deployment-server\software\CarasiContextClearing\*" "C:\Program Files\Bosch\CarasiContextClearing\"

REM Install dependencies silently
echo Installing .NET Framework 4.7.2...
"\\deployment-server\dependencies\NDP472-KB4054530-x86-x64-AllOS-ENU.exe" /quiet /norestart

echo Installing Access Database Engine...
"\\deployment-server\dependencies\AccessDatabaseEngine_X64.exe" /quiet

REM Create Start Menu shortcuts
mkdir "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Bosch Tools" 2>nul
powershell -Command "& {$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%ProgramData%\Microsoft\Windows\Start Menu\Programs\Bosch Tools\Carasi Context Clearing.lnk'); $Shortcut.TargetPath = 'C:\Program Files\Bosch\CarasiContextClearing\Check_carasi_DF_ContextClearing.exe'; $Shortcut.Description = 'Check Carasi DF Context Clearing Tool'; $Shortcut.Save()}"

REM Register application
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CarasiContextClearing" /v "DisplayName" /t REG_SZ /d "Check Carasi DF Context Clearing Tool" /f
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CarasiContextClearing" /v "DisplayVersion" /t REG_SZ /d "2025.0.2.1" /f
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CarasiContextClearing" /v "Publisher" /t REG_SZ /d "Bosch Engineering" /f
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CarasiContextClearing" /v "InstallLocation" /t REG_SZ /d "C:\Program Files\Bosch\CarasiContextClearing\" /f

echo Installation completed successfully!
echo Application can be found in Start Menu under Bosch Tools
pause
```

### **🔧 Configuration Management**

#### **Centralized Configuration**
```xml
<!-- App.config for enterprise deployment -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <appSettings>
        <!-- Enterprise settings -->
        <add key="DefaultProjectPath" value="\\shared-server\projects\automotive\" />
        <add key="TemplateFilePath" value="\\shared-server\templates\carasi_template.xlsx" />
        <add key="LoggingLevel" value="Info" />
        <add key="MaxCacheSize" value="50" />
        <add key="ConnectionPoolSize" value="10" />
        <add key="EnableTelemetry" value="true" />
        
        <!-- Email settings for DD requests -->
        <add key="EmailDomain" value="@bosch.com" />
        <add key="DefaultCCList" value="team-automotive@bosch.com" />
        
        <!-- Performance settings -->
        <add key="BatchSearchThreads" value="4" />
        <add key="MemoryWarningThreshold" value="500" />
        <add key="MaxTabLimit" value="60" />
    </appSettings>
    
    <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
    </startup>
    
    <runtime>
        <gcServer enabled="true" />
        <gcConcurrent enabled="true" />
    </runtime>
</configuration>
```

---

## 🧪 **Installation Verification**

### **✅ Comprehensive Testing Suite**

#### **Dependency Verification Script**
```powershell
# PowerShell script: Verify-Installation.ps1

Write-Host "=== Check Carasi DF Context Clearing - Installation Verification ===" -ForegroundColor Green

# Test 1: Check .NET Framework
Write-Host "`n1. Checking .NET Framework..." -ForegroundColor Yellow
try {
    $netVersion = Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release -ErrorAction Stop
    if ($netVersion.Release -ge 461808) {
        Write-Host "   ✅ .NET Framework 4.7.2+ detected (Release: $($netVersion.Release))" -ForegroundColor Green
    } else {
        Write-Host "   ❌ .NET Framework 4.7.2+ required" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ .NET Framework not found" -ForegroundColor Red
}

# Test 2: Check OLEDB Providers
Write-Host "`n2. Checking OLEDB Providers..." -ForegroundColor Yellow
$oledbProviders = @("Microsoft.ACE.OLEDB.12.0", "Microsoft.ACE.OLEDB.16.0")
foreach ($provider in $oledbProviders) {
    try {
        $null = Get-ItemProperty "HKLM:SOFTWARE\Classes\$provider" -ErrorAction Stop
        Write-Host "   ✅ $provider found" -ForegroundColor Green
    } catch {
        Write-Host "   ⚠️ $provider not found" -ForegroundColor Yellow
    }
}

# Test 3: Check Application Files
Write-Host "`n3. Checking Application Files..." -ForegroundColor Yellow
$appPath = "C:\Program Files\Bosch\CarasiContextClearing\Check_carasi_DF_ContextClearing.exe"
if (Test-Path $appPath) {
    Write-Host "   ✅ Application executable found" -ForegroundColor Green
    
    # Check version
    $version = (Get-Item $appPath).VersionInfo.ProductVersion
    Write-Host "   📋 Version: $version" -ForegroundColor Cyan
} else {
    Write-Host "   ❌ Application executable not found at $appPath" -ForegroundColor Red
}

# Test 4: Test Application Startup
Write-Host "`n4. Testing Application Startup..." -ForegroundColor Yellow
try {
    $process = Start-Process $appPath -ArgumentList "--version" -PassThru -WindowStyle Hidden -ErrorAction Stop
    Start-Sleep -Seconds 3
    if (!$process.HasExited) {
        $process.Kill()
        Write-Host "   ✅ Application starts successfully" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️ Application exited quickly (normal for --version)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Application failed to start: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Check System Resources
Write-Host "`n5. Checking System Resources..." -ForegroundColor Yellow
$memory = Get-CimInstance -ClassName Win32_ComputerSystem
$memoryGB = [math]::Round($memory.TotalPhysicalMemory / 1GB, 2)
Write-Host "   📊 Total RAM: $memoryGB GB" -ForegroundColor Cyan

if ($memoryGB -ge 8) {
    Write-Host "   ✅ Sufficient memory for optimal performance" -ForegroundColor Green
} elseif ($memoryGB -ge 4) {
    Write-Host "   ⚠️ Minimum memory requirements met" -ForegroundColor Yellow
} else {
    Write-Host "   ❌ Insufficient memory (4GB minimum required)" -ForegroundColor Red
}

Write-Host "`n=== Verification Complete ===" -ForegroundColor Green
```

#### **Functional Testing**
```powershell
# Functional test script
$testExcelFile = "C:\temp\test_carasi.xlsx"

# Create test Excel file
Write-Host "Creating test Excel file..." -ForegroundColor Yellow
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$workbook = $excel.Workbooks.Add()
$worksheet = $workbook.Worksheets.Item(1)
$worksheet.Name = "Sheet1"

# Add test data
$worksheet.Cells.Item(1,1) = "Interface Name"
$worksheet.Cells.Item(1,2) = "Function Name"  
$worksheet.Cells.Item(1,3) = "Direction"
$worksheet.Cells.Item(2,1) = "TestInterface_001"
$worksheet.Cells.Item(2,2) = "TestFunction_001"
$worksheet.Cells.Item(2,3) = "Input"

$workbook.SaveAs($testExcelFile)
$workbook.Close()
$excel.Quit()

# Test OLEDB connection
Write-Host "Testing OLEDB connection..." -ForegroundColor Yellow
try {
    $connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$testExcelFile;Extended Properties='Excel 12.0;HDR=YES;'"
    $connection = New-Object System.Data.OleDb.OleDbConnection($connectionString)
    $connection.Open()
    $connection.Close()
    Write-Host "   ✅ OLEDB connection successful" -ForegroundColor Green
} catch {
    Write-Host "   ❌ OLEDB connection failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Cleanup
Remove-Item $testExcelFile -ErrorAction SilentlyContinue
```

---

## 🚫 **Uninstallation**

### **🗑️ Complete Removal**

#### **Automated Uninstall Script**
```batch
@echo off
REM Uninstall Check Carasi DF Context Clearing Tool

echo Uninstalling Check Carasi DF Context Clearing Tool...

REM Stop application if running
taskkill /F /IM "Check_carasi_DF_ContextClearing.exe" 2>nul

REM Remove application files
rmdir /S /Q "C:\Program Files\Bosch\CarasiContextClearing" 2>nul

REM Remove Start Menu shortcuts
del "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Bosch Tools\Carasi Context Clearing.lnk" 2>nul
rmdir "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Bosch Tools" 2>nul

REM Remove Desktop shortcuts
del "%PUBLIC%\Desktop\Carasi Context Clearing.lnk" 2>nul
del "%USERPROFILE%\Desktop\Carasi Context Clearing.lnk" 2>nul

REM Remove registry entries
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CarasiContextClearing" /f 2>nul

REM Clean user settings (optional)
REM rmdir /S /Q "%LOCALAPPDATA%\Check_carasi_DF_ContextClearing" 2>nul

echo Uninstallation completed!
pause
```

### **🔧 Selective Removal**
```powershell
# Keep user settings and cache for reinstallation
Remove-Item "C:\Program Files\Bosch\CarasiContextClearing" -Recurse -Force
# User settings remain in: $env:LOCALAPPDATA\Check_carasi_DF_ContextClearing_URL_*
```

---

## 🔧 **Troubleshooting Installation Issues**

### **❌ Common Installation Problems**

#### **🔴 "OLEDB provider not registered"**
```powershell
# Solution 1: Manual OLEDB registration
regsvr32 "C:\Program Files\Microsoft Office\root\VFS\ProgramFilesCommonX64\Microsoft Shared\OFFICE16\ACEOLEDB.DLL"

# Solution 2: Reinstall Access Database Engine
# Download from: https://www.microsoft.com/download/details.aspx?id=54920
# Install with: AccessDatabaseEngine_X64.exe /quiet
```

#### **🔴 ".NET Framework version error"**
```powershell
# Check installed versions
Get-ChildItem 'HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse |
Get-ItemProperty -Name version -EA 0 |
Where { $_.PSChildName -Match '^(?!S)\p{L}'} |
Select PSChildName, version

# Install .NET Framework 4.7.2
# Download from: https://dotnet.microsoft.com/download/dotnet-framework/net472
```

#### **🔴 "Access denied" during installation**
```batch
REM Run Command Prompt as Administrator
REM Or use PowerShell with elevated privileges:
Start-Process PowerShell -Verb RunAs
```

#### **🔴 Application crashes on startup**
```powershell
# Check Event Viewer for details
Get-WinEvent -LogName Application -MaxEvents 10 | 
Where-Object {$_.ProviderName -like "*Check_carasi*" -or $_.LevelDisplayName -eq "Error"}

# Common solutions:
# 1. Install Visual C++ Redistributable
# 2. Update Windows to latest version  
# 3. Check antivirus exclusions
# 4. Run as Administrator (test only)
```

### **🔍 Advanced Diagnostics**

#### **System Compatibility Check**
```powershell
# Comprehensive system check
$SystemInfo = @{
    OS = (Get-CimInstance Win32_OperatingSystem).Caption
    Architecture = (Get-CimInstance Win32_OperatingSystem).OSArchitecture
    Memory = [math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
    DotNet = (Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release -ErrorAction SilentlyContinue).Release
    PowerShell = $PSVersionTable.PSVersion.ToString()
}

Write-Host "=== System Compatibility Report ===" -ForegroundColor Green
$SystemInfo | Format-Table -AutoSize
```

---

## 📞 **Installation Support**

### **🆘 Getting Help**

**📧 Technical Support:**
- **Email**: NGOC.VUONGMINH@vn.bosch.com
- **Team**: Bosch Engineering Vietnam
- **Response Time**: 1-2 business days

**📚 Self-Help Resources:**
- **[[Troubleshooting|Troubleshooting]]** - Common issues and solutions
- **[[FAQ|FAQ]]** - Frequently asked questions
- **[[System Requirements|System-Requirements]]** - Detailed requirements
- **GitHub Issues** - Report bugs and request features

**🔧 Enterprise Support:**
- **Group Deployment**: Contact IT team for Group Policy setup
- **Custom Configuration**: Request enterprise configuration templates
- **Training Sessions**: Available for teams and departments

---

*Installation complete! Ready to start using Check Carasi DF Context Clearing Tool for professional automotive interface analysis.*
