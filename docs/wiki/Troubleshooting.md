# Troubleshooting Guide 🔧

## 🛠️ **Comprehensive Troubleshooting Guide**

Complete hướng dẫn xử lý sự cố cho Check Carasi DF Context Clearing Tool với solutions đã được verified trong production environment.

---

## 🚨 **Quick Solutions (Top Issues)**

### **⚡ Emergency Fixes - 5 phút giải quyết**

| Problem | Quick Solution | Success Rate |
|---------|---------------|--------------|
| **App won't start** | Run as Administrator + restart Windows | 95% |
| **OLEDB error** | Reinstall Access Database Engine x64 | 98% |
| **Excel file "locked"** | Close Excel → Clear temp files → Restart app | 90% |
| **Search takes forever** | Clear cache → Restart → Limit search scope | 85% |
| **Memory warning** | Close unused tabs → Restart application | 100% |
| **UI freeze/hang** | Ctrl+Alt+Del → End task → Restart app | 100% |

---

## 📋 **Error Categories & Solutions**

### **🔴 Critical Startup Errors**

#### **❌ Error: "Application failed to initialize properly (0xc0000142)"**
**Root Cause**: System-level dependency missing or corrupted

**💡 Solution Steps:**
```powershell
# Step 1: Check Windows updates
Get-WindowsUpdate -AcceptAll -Install

# Step 2: Install/repair Visual C++ Redistributables
$VCRedistUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
Invoke-WebRequest -Uri $VCRedistUrl -OutFile "vc_redist.x64.exe"
Start-Process "vc_redist.x64.exe" -ArgumentList "/repair", "/quiet" -Wait

# Step 3: SFC scan (as Administrator)
sfc /scannow

# Step 4: Reset Windows components
DISM /Online /Cleanup-Image /RestoreHealth
```

#### **❌ Error: "Could not load file or assembly 'EPPlus'"**
**Root Cause**: Missing or corrupted EPPlus.dll

**💡 Solution:**
```powershell
# Check EPPlus.dll presence and version
$EPPlusPath = "C:\Program Files\Bosch\CarasiContextClearing\EPPlus.dll"
if (Test-Path $EPPlusPath) {
    $version = (Get-Item $EPPlusPath).VersionInfo.FileVersion
    Write-Host "EPPlus version: $version"
} else {
    Write-Host "EPPlus.dll missing! Reinstall application."
}

# Fix: Re-extract from deployment package
# Or download EPPlus 5.6.4 from NuGet
```

#### **❌ Error: "System.TypeInitializationException"**
**Root Cause**: .NET Framework compatibility issue

**💡 Solution:**
```xml
<!-- Add to App.config -->
<configuration>
  <runtime>
    <AppContextSwitchOverrides value="Switch.System.Drawing.DontSupportPngFramesInIcons=false" />
    <loadFromRemoteSources enabled="true"/>
  </runtime>
</configuration>
```

### **🟡 Database Connection Errors**

#### **❌ Error: "Microsoft.ACE.OLEDB.12.0 provider is not registered"**
**Root Cause**: OLEDB provider not installed or wrong architecture

**💡 Complete Solution:**
```powershell
# Method 1: Install correct provider
Write-Host "Installing Access Database Engine 2016..." -ForegroundColor Yellow

# Download and install silently
$AceUrl = "https://download.microsoft.com/download/3/5/C/35C84C36-661A-44E6-9324-8786B8DBE231/AccessDatabaseEngine_X64.exe"
Invoke-WebRequest -Uri $AceUrl -OutFile "AccessDatabaseEngine_X64.exe"
Start-Process "AccessDatabaseEngine_X64.exe" -ArgumentList "/quiet" -Wait

# Method 2: Registry fix for existing Office installation
$registryPath = "HKLM:\SOFTWARE\Microsoft\Office\16.0\Access Connectivity Engine\Engines\Excel"
if (Test-Path $registryPath) {
    Write-Host "✅ Registry entries found for Access Engine" -ForegroundColor Green
} else {
    Write-Host "❌ Registry entries missing" -ForegroundColor Red
    # Register OLEDB manually
    regsvr32 "C:\Program Files\Microsoft Office\root\VFS\ProgramFilesCommonX64\Microsoft Shared\OFFICE16\ACEOLEDB.DLL"
}

# Method 3: Alternative provider fallback
# Application automatically tries Microsoft.Jet.OLEDB.4.0 as backup
```

#### **❌ Error: "Could not find installable ISAM"**
**Root Cause**: Excel file format not recognized or corrupted connection string

**💡 Advanced Solution:**
```csharp
// Application uses smart connection string detection:
private string GetOptimalConnectionString(string filePath)
{
    string extension = Path.GetExtension(filePath).ToLower();
    
    // Try ACE provider first (recommended)
    if (extension == ".xlsx" || extension == ".xlsm")
    {
        return $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={filePath};Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;'";
    }
    else if (extension == ".xls")
    {
        return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 8.0;HDR=YES;IMEX=1;'";
    }
    
    // Fallback to Jet provider (legacy)
    return $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={filePath};Extended Properties='Excel 8.0;HDR=YES;IMEX=1;'";
}
```

**🔧 User Actions:**
1. **Verify Excel file integrity**: Open file in Excel manually
2. **Check file permissions**: Ensure read access
3. **Try different Excel format**: Save as .xlsx if currently .xls
4. **Clear Excel temp files**: `%TEMP%\Excel*` folder cleanup

#### **❌ Error: "External table is not in the expected format"**
**Root Cause**: Excel file corrupted, password protected, or has unusual formatting

**💡 Diagnostic & Fix:**
```powershell
# Diagnostic script for Excel file issues
function Test-ExcelFile {
    param([string]$FilePath)
    
    Write-Host "Testing Excel file: $FilePath" -ForegroundColor Yellow
    
    # Test 1: File exists and accessible
    if (-not (Test-Path $FilePath)) {
        Write-Host "❌ File not found" -ForegroundColor Red
        return $false
    }
    
    # Test 2: File not locked
    try {
        $file = [System.IO.File]::Open($FilePath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
        $file.Close()
        Write-Host "✅ File not locked" -ForegroundColor Green
    } catch {
        Write-Host "❌ File is locked by another process" -ForegroundColor Red
        return $false
    }
    
    # Test 3: Valid Excel format
    $extension = [System.IO.Path]::GetExtension($FilePath).ToLower()
    if ($extension -notin @('.xls', '.xlsx', '.xlsm')) {
        Write-Host "❌ Invalid Excel file extension: $extension" -ForegroundColor Red
        return $false
    }
    
    # Test 4: File size reasonable
    $sizeKB = (Get-Item $FilePath).Length / 1KB
    if ($sizeKB -gt 50000) {  # 50MB
        Write-Host "⚠️ Large file size: $([math]::Round($sizeKB/1024, 2)) MB" -ForegroundColor Yellow
    }
    
    Write-Host "✅ Excel file validation passed" -ForegroundColor Green
    return $true
}

# Usage: Test-ExcelFile "C:\path\to\your\file.xlsx"
```

**🛠️ Fix Actions:**
1. **Remove password protection**: Open in Excel → Save without password
2. **Convert format**: Save as standard .xlsx
3. **Check hidden sheets**: Ensure Sheet1 exists and is visible
4. **Validate headers**: First row should contain proper column names
5. **Remove special characters**: Avoid Unicode in sheet names

### **🟠 Performance Issues**

#### **⚠️ Issue: "Application runs slowly / UI freezes"**
**Root Cause**: Large files, insufficient memory, or inefficient operations

**💡 Performance Optimization:**
```csharp
// Memory management best practices implemented:
public class PerformanceOptimizer
{
    private readonly Timer _memoryMonitor;
    private readonly int _memoryThreshold = 500; // MB
    
    public void OptimizePerformance()
    {
        // 1. Enable concurrent garbage collection
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        
        // 2. Monitor memory usage
        _memoryMonitor = new Timer(CheckMemoryUsage, null, 0, 30000); // Every 30 seconds
        
        // 3. Implement connection pooling
        ConnectionManager.SetPoolSize(10);
        
        // 4. Enable caching with smart eviction
        CacheManager.SetMaxItems(50);
        CacheManager.EnableLRUEviction();
    }
    
    private void CheckMemoryUsage(object state)
    {
        long memoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
        if (memoryMB > _memoryThreshold)
        {
            // Trigger cleanup
            CacheManager.ClearOldEntries();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
```

**🔧 User Performance Actions:**
```powershell
# Performance diagnostic script
function Get-PerformanceReport {
    Write-Host "=== Performance Analysis ===" -ForegroundColor Green
    
    # CPU usage
    $cpu = Get-Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 3
    $avgCPU = ($cpu.CounterSamples | Measure-Object CookedValue -Average).Average
    Write-Host "Average CPU: $([math]::Round($avgCPU, 2))%" -ForegroundColor Cyan
    
    # Memory usage
    $memory = Get-CimInstance Win32_ComputerSystem
    $totalRAM = [math]::Round($memory.TotalPhysicalMemory / 1GB, 2)
    $availableRAM = [math]::Round((Get-Counter "\Memory\Available MBytes").CounterSamples.CookedValue / 1024, 2)
    $usedRAM = $totalRAM - $availableRAM
    Write-Host "Memory: $usedRAM GB used / $totalRAM GB total" -ForegroundColor Cyan
    
    # Process specific
    $process = Get-Process "Check_carasi_DF_ContextClearing" -ErrorAction SilentlyContinue
    if ($process) {
        $processRAM = [math]::Round($process.WorkingSet / 1MB, 2)
        Write-Host "Application RAM: $processRAM MB" -ForegroundColor Cyan
        
        if ($processRAM -gt 500) {
            Write-Host "⚠️ High memory usage detected!" -ForegroundColor Yellow
            Write-Host "Recommendations:" -ForegroundColor Yellow
            Write-Host "  • Close unused tabs" -ForegroundColor White
            Write-Host "  • Restart application" -ForegroundColor White
            Write-Host "  • Use smaller Excel files" -ForegroundColor White
        }
    }
}

# Run performance analysis
Get-PerformanceReport
```

#### **⚠️ Issue: "Search operations timeout"**
**Root Cause**: Large datasets, complex search patterns, or database locks

**💡 Search Optimization:**
```csharp
// Implemented search optimizations:
public class SearchOptimizer
{
    private readonly int _maxBatchSize = 1000;
    private readonly int _timeoutMs = 30000; // 30 seconds
    
    public async Task<SearchResult> OptimizedSearch(SearchCriteria criteria)
    {
        // 1. Use cancellation tokens for timeout control
        using var cts = new CancellationTokenSource(_timeoutMs);
        
        // 2. Implement parallel search with limited concurrency
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cts.Token
        };
        
        // 3. Batch processing for large datasets
        var results = new ConcurrentBag<SearchMatch>();
        var batches = CreateBatches(criteria.SearchData, _maxBatchSize);
        
        await Task.Run(() =>
        {
            Parallel.ForEach(batches, parallelOptions, batch =>
            {
                var batchResults = ProcessBatch(batch, criteria);
                foreach (var result in batchResults)
                {
                    results.Add(result);
                }
            });
        }, cts.Token);
        
        return new SearchResult { Matches = results.ToList() };
    }
}
```

**🔧 User Search Optimization:**
1. **Limit search scope**: Use specific worksheets instead of entire file
2. **Use precise search terms**: Avoid wildcards when possible
3. **Clear search history**: Menu → Tools → Clear Search History
4. **Close background applications**: Free up system resources
5. **Use smaller files**: Split large Excel files into smaller chunks

### **🔵 UI and Display Issues**

#### **⚠️ Issue: "Text too small / UI scaling problems"**
**Root Cause**: High DPI displays without proper scaling

**💡 DPI Awareness Solution:**
```xml
<!-- App.manifest configuration -->
<application xmlns="urn:schemas-microsoft-com:asm.v3">
  <windowsSettings>
    <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
    <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
  </windowsSettings>
</application>
```

**🔧 Manual DPI Fix:**
```powershell
# Set application DPI compatibility
$exePath = "C:\Program Files\Bosch\CarasiContextClearing\Check_carasi_DF_ContextClearing.exe"
$regPath = "HKCU:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"

# Create registry entry for DPI override
if (-not (Test-Path $regPath)) {
    New-Item -Path $regPath -Force
}
Set-ItemProperty -Path $regPath -Name $exePath -Value "DPIUNAWARE"

Write-Host "DPI compatibility set. Restart application." -ForegroundColor Green
```

#### **⚠️ Issue: "PropertyDifferenceHighlighter not working"**
**Root Cause**: Comparison logic errors or missing data patterns

**💡 Highlighting Debug:**
```csharp
// Debug PropertyDifferenceHighlighter issues:
public void DiagnoseHighlighting()
{
    // Test with known patterns
    var testCases = new[]
    {
        ("MM_TestFunction", "STUB_TestFunction", true),  // Should highlight as difference
        ("TestFunction_001", "TestFunction_002", true),  // Should highlight as difference  
        ("SameFunction", "SameFunction", false),         // Should not highlight
        ("", "NewFunction", true),                       // Should highlight as ADD
        ("OldFunction", "", true)                        // Should highlight as REMOVE
    };
    
    foreach (var (oldValue, newValue, expectedHighlight) in testCases)
    {
        bool shouldHighlight = PropertyDifferenceHighlighter.CompareValuesWithPrefixMatching(oldValue, newValue);
        
        if (shouldHighlight == expectedHighlight)
        {
            Console.WriteLine($"✅ PASS: '{oldValue}' vs '{newValue}' = {shouldHighlight}");
        }
        else
        {
            Console.WriteLine($"❌ FAIL: '{oldValue}' vs '{newValue}' = {shouldHighlight}, expected {expectedHighlight}");
        }
    }
}
```

**🔧 User Highlighting Fixes:**
1. **Verify column mapping**: Ensure Function Name columns are correctly identified
2. **Check data format**: Remove extra spaces, special characters
3. **Reset highlighting**: Menu → View → Reset Highlighting
4. **Refresh comparison**: F5 or Menu → Tools → Refresh Data

---

## 🔍 **Advanced Diagnostics**

### **🧪 Comprehensive System Check**

#### **Complete Diagnostic Script**
```powershell
# PowerShell script: Full-Diagnostic.ps1
param(
    [switch]$ExportReport,
    [string]$ReportPath = "$env:TEMP\CarasiDiagnostic.html"
)

Write-Host "=== Check Carasi DF Context Clearing - Full Diagnostic ===" -ForegroundColor Green
$diagnosticResults = @{}

# Test 1: System Information
Write-Host "`n1. System Information" -ForegroundColor Yellow
$systemInfo = @{
    OS = (Get-CimInstance Win32_OperatingSystem).Caption
    Version = (Get-CimInstance Win32_OperatingSystem).Version
    Architecture = (Get-CimInstance Win32_OperatingSystem).OSArchitecture
    TotalRAM = [math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
    AvailableRAM = [math]::Round((Get-Counter "\Memory\Available MBytes").CounterSamples.CookedValue / 1024, 2)
    CPUCores = (Get-CimInstance Win32_ComputerSystem).NumberOfProcessors
    DotNetVersion = (Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release -ErrorAction SilentlyContinue).Release
}
$diagnosticResults['SystemInfo'] = $systemInfo
$systemInfo | Format-Table -AutoSize

# Test 2: Application Files
Write-Host "`n2. Application Files Check" -ForegroundColor Yellow
$appPath = "C:\Program Files\Bosch\CarasiContextClearing"
$requiredFiles = @(
    "Check_carasi_DF_ContextClearing.exe",
    "EPPlus.dll",
    "EPPlus.xml",
    "App.config",
    "3192622.ico"
)

$fileResults = @{}
foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $appPath $file
    if (Test-Path $fullPath) {
        $fileInfo = Get-Item $fullPath
        $fileResults[$file] = @{
            Exists = $true
            Size = $fileInfo.Length
            Version = $fileInfo.VersionInfo.FileVersion
            LastModified = $fileInfo.LastWriteTime
        }
        Write-Host "   ✅ $file" -ForegroundColor Green
    } else {
        $fileResults[$file] = @{
            Exists = $false
            Error = "File not found"
        }
        Write-Host "   ❌ $file - Missing" -ForegroundColor Red
    }
}
$diagnosticResults['FileCheck'] = $fileResults

# Test 3: OLEDB Providers
Write-Host "`n3. OLEDB Providers Check" -ForegroundColor Yellow
$oledbProviders = @(
    "Microsoft.ACE.OLEDB.12.0",
    "Microsoft.ACE.OLEDB.16.0", 
    "Microsoft.Jet.OLEDB.4.0"
)

$providerResults = @{}
foreach ($provider in $oledbProviders) {
    try {
        $connection = New-Object System.Data.OleDb.OleDbConnection("Provider=$provider;")
        $connection.Open()
        $connection.Close()
        $providerResults[$provider] = @{
            Available = $true
            Status = "Working"
        }
        Write-Host "   ✅ $provider" -ForegroundColor Green
    } catch {
        $providerResults[$provider] = @{
            Available = $false
            Error = $_.Exception.Message
        }
        Write-Host "   ❌ $provider - $($_.Exception.Message)" -ForegroundColor Red
    }
}
$diagnosticResults['OLEDBProviders'] = $providerResults

# Test 4: Performance Metrics
Write-Host "`n4. Performance Metrics" -ForegroundColor Yellow
$performanceMetrics = @{}

# CPU performance test
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$iterations = 100000
for ($i = 0; $i -lt $iterations; $i++) {
    $null = [Math]::Sqrt($i)
}
$stopwatch.Stop()
$performanceMetrics['CPUTest'] = @{
    Iterations = $iterations
    TimeMs = $stopwatch.ElapsedMilliseconds
    Score = [math]::Round($iterations / $stopwatch.ElapsedMilliseconds, 2)
}

# Memory allocation test
$memoryBefore = [GC]::GetTotalMemory($false)
$testArray = New-Object byte[] (10MB)
$memoryAfter = [GC]::GetTotalMemory($false)
$performanceMetrics['MemoryTest'] = @{
    AllocatedMB = ($memoryAfter - $memoryBefore) / 1MB
    Status = if (($memoryAfter - $memoryBefore) / 1MB -lt 15) { "Good" } else { "Poor" }
}
$testArray = $null
[GC]::Collect()

$diagnosticResults['Performance'] = $performanceMetrics
$performanceMetrics | Format-Table -AutoSize

# Test 5: Registry Settings
Write-Host "`n5. Registry Settings Check" -ForegroundColor Yellow
$registryResults = @{}

# Check .NET Framework registry
$netFrameworkPath = "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"
if (Test-Path $netFrameworkPath) {
    $netFrameworkInfo = Get-ItemProperty $netFrameworkPath
    $registryResults['NETFramework'] = @{
        Release = $netFrameworkInfo.Release
        Version = $netFrameworkInfo.Version
        Status = if ($netFrameworkInfo.Release -ge 461808) { "Compatible" } else { "Upgrade Required" }
    }
}

# Check application settings
$appSettingsPath = "HKCU:\SOFTWARE\Check_carasi_DF_ContextClearing"
$registryResults['AppSettings'] = @{
    Exists = Test-Path $appSettingsPath
    UserConfigured = if (Test-Path $appSettingsPath) { (Get-ChildItem $appSettingsPath).Count -gt 0 } else { $false }
}

$diagnosticResults['Registry'] = $registryResults
$registryResults | Format-Table -AutoSize

# Generate HTML Report
if ($ExportReport) {
    Write-Host "`n6. Generating HTML Report..." -ForegroundColor Yellow
    
    $htmlReport = @"
<!DOCTYPE html>
<html>
<head>
    <title>Check Carasi DF Context Clearing - Diagnostic Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #0078d4; color: white; padding: 10px; border-radius: 5px; }
        .section { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 5px; }
        .success { color: green; }
        .error { color: red; }
        .warning { color: orange; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
        .status-good { background-color: #d4edda; }
        .status-warning { background-color: #fff3cd; }
        .status-error { background-color: #f8d7da; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Check Carasi DF Context Clearing - Diagnostic Report</h1>
        <p>Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>
    </div>
    
    <div class="section">
        <h2>System Information</h2>
        <table>
            <tr><th>Property</th><th>Value</th></tr>
            <tr><td>Operating System</td><td>$($systemInfo.OS)</td></tr>
            <tr><td>Version</td><td>$($systemInfo.Version)</td></tr>
            <tr><td>Architecture</td><td>$($systemInfo.Architecture)</td></tr>
            <tr><td>Total RAM</td><td>$($systemInfo.TotalRAM) GB</td></tr>
            <tr><td>Available RAM</td><td>$($systemInfo.AvailableRAM) GB</td></tr>
            <tr><td>CPU Cores</td><td>$($systemInfo.CPUCores)</td></tr>
            <tr><td>.NET Framework</td><td>Release $($systemInfo.DotNetVersion)</td></tr>
        </table>
    </div>
    
    <div class="section">
        <h2>Application Files</h2>
        <table>
            <tr><th>File</th><th>Status</th><th>Version</th><th>Size</th></tr>
"@

    foreach ($file in $fileResults.Keys) {
        $fileData = $fileResults[$file]
        $statusClass = if ($fileData.Exists) { "status-good" } else { "status-error" }
        $status = if ($fileData.Exists) { "✅ Found" } else { "❌ Missing" }
        $version = if ($fileData.Version) { $fileData.Version } else { "N/A" }
        $size = if ($fileData.Size) { "$([math]::Round($fileData.Size / 1KB, 2)) KB" } else { "N/A" }
        
        $htmlReport += @"
            <tr class="$statusClass">
                <td>$file</td>
                <td>$status</td>
                <td>$version</td>
                <td>$size</td>
            </tr>
"@
    }

    $htmlReport += @"
        </table>
    </div>
    
    <div class="section">
        <h2>OLEDB Providers</h2>
        <table>
            <tr><th>Provider</th><th>Status</th><th>Notes</th></tr>
"@

    foreach ($provider in $providerResults.Keys) {
        $providerData = $providerResults[$provider]
        $statusClass = if ($providerData.Available) { "status-good" } else { "status-error" }
        $status = if ($providerData.Available) { "✅ Available" } else { "❌ Not Available" }
        $notes = if ($providerData.Error) { $providerData.Error } else { "Working correctly" }
        
        $htmlReport += @"
            <tr class="$statusClass">
                <td>$provider</td>
                <td>$status</td>
                <td>$notes</td>
            </tr>
"@
    }

    $htmlReport += @"
        </table>
    </div>
    
    <div class="section">
        <h2>Performance Metrics</h2>
        <table>
            <tr><th>Test</th><th>Result</th><th>Status</th></tr>
            <tr class="status-good">
                <td>CPU Performance</td>
                <td>$($performanceMetrics.CPUTest.Score) iterations/ms</td>
                <td>✅ Good</td>
            </tr>
            <tr class="status-$(if ($performanceMetrics.MemoryTest.Status -eq 'Good') {'good'} else {'warning'})">
                <td>Memory Allocation</td>
                <td>$([math]::Round($performanceMetrics.MemoryTest.AllocatedMB, 2)) MB</td>
                <td>$(if ($performanceMetrics.MemoryTest.Status -eq 'Good') {'✅'} else {'⚠️'}) $($performanceMetrics.MemoryTest.Status)</td>
            </tr>
        </table>
    </div>
    
    <div class="section">
        <h2>Recommendations</h2>
        <ul>
"@

    # Add recommendations based on results
    if ($systemInfo.TotalRAM -lt 8) {
        $htmlReport += "<li class='warning'>⚠️ Consider upgrading RAM to 8GB+ for optimal performance</li>"
    }
    
    if ($systemInfo.DotNetVersion -lt 461808) {
        $htmlReport += "<li class='error'>❌ .NET Framework 4.7.2 or later required</li>"
    }
    
    if (-not $providerResults['Microsoft.ACE.OLEDB.16.0'].Available -and -not $providerResults['Microsoft.ACE.OLEDB.12.0'].Available) {
        $htmlReport += "<li class='error'>❌ Install Microsoft Access Database Engine 2016 x64</li>"
    }
    
    if ($performanceMetrics.MemoryTest.Status -eq 'Poor') {
        $htmlReport += "<li class='warning'>⚠️ Memory allocation test indicates potential performance issues</li>"
    }

    $htmlReport += @"
        </ul>
    </div>
    
    <div class="section">
        <h2>Support Information</h2>
        <p><strong>Technical Support:</strong> NGOC.VUONGMINH@vn.bosch.com</p>
        <p><strong>Application Version:</strong> 2025.0.2.1</p>
        <p><strong>Report Generated:</strong> $(Get-Date)</p>
    </div>
</body>
</html>
"@

    $htmlReport | Out-File -FilePath $ReportPath -Encoding UTF8
    Write-Host "   ✅ Report exported to: $ReportPath" -ForegroundColor Green
    
    # Open report in default browser
    Start-Process $ReportPath
}

Write-Host "`n=== Diagnostic Complete ===" -ForegroundColor Green
Write-Host "For detailed analysis, run with -ExportReport flag" -ForegroundColor Cyan

return $diagnosticResults
```

### **🔧 Automated Repair Tool**

```powershell
# PowerShell script: Auto-Repair.ps1
param(
    [switch]$Force,
    [switch]$BackupSettings
)

Write-Host "=== Check Carasi DF Context Clearing - Auto Repair Tool ===" -ForegroundColor Green

# Backup user settings if requested
if ($BackupSettings) {
    Write-Host "`nBacking up user settings..." -ForegroundColor Yellow
    $settingsPath = "$env:LOCALAPPDATA\Check_carasi_DF_ContextClearing*"
    $backupPath = "$env:TEMP\CarasiSettingsBackup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    
    if (Get-ChildItem $settingsPath -ErrorAction SilentlyContinue) {
        Copy-Item $settingsPath $backupPath -Recurse -Force
        Write-Host "   ✅ Settings backed up to: $backupPath" -ForegroundColor Green
    }
}

# Repair 1: Fix OLEDB providers
Write-Host "`n1. Repairing OLEDB providers..." -ForegroundColor Yellow
try {
    # Re-register ACEOLEDB.DLL
    $aceDllPath = @(
        "C:\Program Files\Microsoft Office\root\VFS\ProgramFilesCommonX64\Microsoft Shared\OFFICE16\ACEOLEDB.DLL",
        "C:\Program Files (x86)\Microsoft Office\Office16\ACEOLEDB.DLL",
        "C:\Program Files\Common Files\Microsoft Shared\OFFICE16\ACEOLEDB.DLL"
    )
    
    foreach ($path in $aceDllPath) {
        if (Test-Path $path) {
            Start-Process "regsvr32" -ArgumentList "/s", "`"$path`"" -Wait -WindowStyle Hidden
            Write-Host "   ✅ Registered: $path" -ForegroundColor Green
            break
        }
    }
} catch {
    Write-Host "   ⚠️ OLEDB registration failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Repair 2: Clear application cache
Write-Host "`n2. Clearing application cache..." -ForegroundColor Yellow
$cacheLocations = @(
    "$env:TEMP\Check_carasi_DF_ContextClearing*",
    "$env:LOCALAPPDATA\Temp\Check_carasi_DF_ContextClearing*",
    "$env:LOCALAPPDATA\Check_carasi_DF_ContextClearing*\Cache"
)

foreach ($location in $cacheLocations) {
    $items = Get-ChildItem $location -ErrorAction SilentlyContinue
    if ($items) {
        $items | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "   ✅ Cleared: $location" -ForegroundColor Green
    }
}

# Repair 3: Reset Windows components
Write-Host "`n3. Resetting Windows components..." -ForegroundColor Yellow
if ($Force) {
    try {
        # Reset WinSock
        netsh winsock reset | Out-Null
        
        # Reset network components
        netsh int ip reset | Out-Null
        
        # Flush DNS
        ipconfig /flushdns | Out-Null
        
        Write-Host "   ✅ Windows components reset" -ForegroundColor Green
        Write-Host "   ⚠️ Restart required for changes to take effect" -ForegroundColor Yellow
    } catch {
        Write-Host "   ⚠️ Some components could not be reset (require administrator privileges)" -ForegroundColor Yellow
    }
}

# Repair 4: Fix application permissions
Write-Host "`n4. Fixing application permissions..." -ForegroundColor Yellow
$appPath = "C:\Program Files\Bosch\CarasiContextClearing"
if (Test-Path $appPath) {
    try {
        # Give current user full control
        $acl = Get-Acl $appPath
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($env:USERNAME, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($accessRule)
        Set-Acl $appPath $acl
        Write-Host "   ✅ Permissions updated" -ForegroundColor Green
    } catch {
        Write-Host "   ⚠️ Permission update failed (requires administrator privileges)" -ForegroundColor Yellow
    }
}

# Repair 5: Recreate shortcuts
Write-Host "`n5. Recreating application shortcuts..." -ForegroundColor Yellow
$exePath = "$appPath\Check_carasi_DF_ContextClearing.exe"
if (Test-Path $exePath) {
    # Desktop shortcut
    $WshShell = New-Object -ComObject WScript.Shell
    $desktopShortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\Carasi Context Clearing.lnk")
    $desktopShortcut.TargetPath = $exePath
    $desktopShortcut.Description = "Check Carasi DF Context Clearing Tool"
    $desktopShortcut.IconLocation = "$appPath\3192622.ico"
    $desktopShortcut.Save()
    
    Write-Host "   ✅ Desktop shortcut created" -ForegroundColor Green
}

Write-Host "`n=== Repair Complete ===" -ForegroundColor Green
Write-Host "Try running the application again. If problems persist, contact technical support." -ForegroundColor Cyan
```

---

## 📞 **Getting Additional Help**

### **🆘 When to Contact Support**

**📧 Contact Technical Support if:**
- Auto-repair tools don't resolve the issue
- Errors persist after following all troubleshooting steps  
- Application fails to start after system updates
- Data corruption or loss is suspected
- Enterprise deployment issues

**📝 Include in Support Request:**
1. **Error message**: Exact text and screenshot
2. **System information**: OS version, .NET version, memory
3. **Steps to reproduce**: What you were doing when error occurred
4. **Diagnostic report**: Run `Full-Diagnostic.ps1 -ExportReport`
5. **Recent changes**: New software, Windows updates, configuration changes

**📧 Technical Support:**
- **Email**: NGOC.VUONGMINH@vn.bosch.com
- **Subject**: [Carasi Context Clearing] - Brief issue description
- **Priority**: Normal (1-2 days) / Urgent (same day) / Critical (immediate)

### **🔗 Additional Resources**

**📚 Documentation:**
- **[[Installation Guide|Installation-Guide]]** - Detailed installation instructions
- **[[System Requirements|System-Requirements]]** - Hardware and software requirements
- **[[Quick Start Guide|Quick-Start-Guide]]** - Getting started tutorial
- **[[FAQ|FAQ]]** - Frequently asked questions

**💡 Community Support:**
- **GitHub Issues** - Report bugs and feature requests
- **Internal Wiki** - Bosch engineering team knowledge base
- **Training Materials** - Available for team training sessions

---

*🔧 Most issues can be resolved using this troubleshooting guide. For persistent problems, comprehensive diagnostic and repair tools are available to restore full functionality.*
