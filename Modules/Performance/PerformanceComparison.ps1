# ===============================================
# PERFORMANCE COMPARISON ANALYSIS SCRIPT
# ===============================================
# Phân tích và so sánh performance giữa các phiên bản
# Author: Performance Analysis Module
# Date: 2025-09-11

param(
    [string]$LogsPath = "d:\5_Automation\Check_carasi_DF_ContextClearing\Modules\Logs",
    [switch]$DetailedReport
)

Write-Host "🔍 PERFORMANCE COMPARISON ANALYSIS" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Function to analyze a single CSV log file
function Analyze-PerformanceLog {
    param(
        [string]$FilePath,
        [string]$VersionName
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-Warning "File not found: $FilePath"
        return $null
    }
    
    $content = Get-Content $FilePath
    if ($content.Count -lt 2) {
        Write-Warning "Empty or invalid CSV: $FilePath"
        return $null
    }
    
    Write-Host "`n📊 Analyzing: $VersionName" -ForegroundColor Yellow
    
    # Tab Creation Performance
    $tabCreations = $content | Select-String "COMPLETE,Create_New_Tab" 
    $tabTimes = $tabCreations | ForEach-Object { 
        $parts = ($_ -split ",")
        if ($parts.Count -ge 4) { [int]$parts[3] }
    } | Where-Object { $_ -gt 0 }
    
    # Variable Check Performance  
    $variableChecks = $content | Select-String "COMPLETE,Variable_Check"
    $searchTimes = $variableChecks | ForEach-Object {
        $parts = ($_ -split ",")
        if ($parts.Count -ge 4) { [int]$parts[3] }
    } | Where-Object { $_ -gt 0 }
    
    # Memory Analysis
    $memoryEntries = $content | Select-String "MemoryMB" | Where-Object { $_ -notmatch "^Timestamp" }
    $memoryValues = $memoryEntries | ForEach-Object {
        $parts = ($_ -split ",")
        if ($parts.Count -ge 6) { [double]$parts[5] }
    } | Where-Object { $_ -gt 0 }
    
    # Tab Count Analysis
    $tabCounts = $memoryEntries | ForEach-Object {
        $parts = ($_ -split ",")
        if ($parts.Count -ge 7) { [int]$parts[6] }
    } | Where-Object { $_ -gt 0 }
    
    $result = @{
        Version = $VersionName
        File = (Split-Path $FilePath -Leaf)
        TabCreations = @{
            Count = $tabTimes.Count
            Average = if ($tabTimes.Count -gt 0) { [math]::Round(($tabTimes | Measure-Object -Average).Average, 1) } else { 0 }
            Min = if ($tabTimes.Count -gt 0) { ($tabTimes | Measure-Object -Minimum).Minimum } else { 0 }
            Max = if ($tabTimes.Count -gt 0) { ($tabTimes | Measure-Object -Maximum).Maximum } else { 0 }
        }
        SearchOperations = @{
            Count = $searchTimes.Count
            Average = if ($searchTimes.Count -gt 0) { [math]::Round(($searchTimes | Measure-Object -Average).Average, 1) } else { 0 }
            Min = if ($searchTimes.Count -gt 0) { ($searchTimes | Measure-Object -Minimum).Minimum } else { 0 }
            Max = if ($searchTimes.Count -gt 0) { ($searchTimes | Measure-Object -Maximum).Maximum } else { 0 }
        }
        Memory = @{
            Start = if ($memoryValues.Count -gt 0) { [math]::Round($memoryValues[0], 1) } else { 0 }
            End = if ($memoryValues.Count -gt 0) { [math]::Round($memoryValues[-1], 1) } else { 0 }
            Peak = if ($memoryValues.Count -gt 0) { [math]::Round(($memoryValues | Measure-Object -Maximum).Maximum, 1) } else { 0 }
        }
        MaxTabs = if ($tabCounts.Count -gt 0) { ($tabCounts | Measure-Object -Maximum).Maximum } else { 0 }
        TotalLogEntries = $content.Count - 1
    }
    
    return $result
}

# Get all performance log files
$logFiles = @(
    @{ Path = "$LogsPath\PerformanceAnalysis_BASELINE.csv"; Name = "BASELINE" }
    @{ Path = "$LogsPath\PerformanceAnalysis_CONNECTIONPOOL.csv"; Name = "CONNECTION_POOL" }
    @{ Path = "$LogsPath\PerformanceAnalysis_CONNECTIONPOOL_FIX.csv"; Name = "POOL_FIXED" }
    @{ Path = "$LogsPath\PerformanceAnalysis_OPTIMIZED.csv"; Name = "OPTIMIZED" }
    @{ Path = "$LogsPath\PerformanceAnalysis_OPTIMIZED_V2.csv"; Name = "OPTIMIZED_V2" }
    @{ Path = "$LogsPath\PerformanceAnalysis_FINETUNED.csv"; Name = "FINE_TUNED" }
    @{ Path = "$LogsPath\PerformanceAnalysis_BATCHSEARCH_FIXED.csv"; Name = "BATCH_SEARCH_FIXED" }
)

$results = @()
foreach ($logFile in $logFiles) {
    $analysis = Analyze-PerformanceLog -FilePath $logFile.Path -VersionName $logFile.Name
    if ($analysis) {
        $results += $analysis
    }
}

# Display Comparison Table
Write-Host "`n📈 PERFORMANCE COMPARISON TABLE" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green

$table = $results | ForEach-Object {
    [PSCustomObject]@{
        Version = $_.Version
        'Tab Count' = $_.MaxTabs
        'Tab Avg (ms)' = $_.TabCreations.Average
        'Tab Max (ms)' = $_.TabCreations.Max
        'Search Avg (ms)' = $_.SearchOperations.Average
        'Search Max (ms)' = $_.SearchOperations.Max
        'Memory Start (MB)' = $_.Memory.Start
        'Memory End (MB)' = $_.Memory.End
        'Memory Peak (MB)' = $_.Memory.Peak
        'Log Entries' = $_.TotalLogEntries
    }
}

$table | Format-Table -AutoSize

# Performance Trends Analysis
Write-Host "`n🔥 PERFORMANCE INSIGHTS" -ForegroundColor Magenta
Write-Host "======================" -ForegroundColor Magenta

$latestVersion = $results | Where-Object { $_.Version -eq "BATCH_SEARCH_FIXED" }
$baseline = $results | Where-Object { $_.Version -eq "BASELINE" }

if ($latestVersion -and $baseline) {
    Write-Host "`n✅ BATCH_SEARCH_FIXED vs BASELINE:" -ForegroundColor Cyan
    
    $tabImprovementPct = if ($baseline.TabCreations.Average -gt 0) {
        [math]::Round((($baseline.TabCreations.Average - $latestVersion.TabCreations.Average) / $baseline.TabCreations.Average) * 100, 1)
    } else { 0 }
    
    $searchImprovementPct = if ($baseline.SearchOperations.Average -gt 0) {
        [math]::Round((($baseline.SearchOperations.Average - $latestVersion.SearchOperations.Average) / $baseline.SearchOperations.Average) * 100, 1)
    } else { 0 }
    
    Write-Host "  🏷️  Tab Creation: $($latestVersion.TabCreations.Average)ms avg (vs $($baseline.TabCreations.Average)ms) = $tabImprovementPct% change"
    Write-Host "  🔍 Search Time: $($latestVersion.SearchOperations.Average)ms avg (vs $($baseline.SearchOperations.Average)ms) = $searchImprovementPct% change"
    Write-Host "  💾 Memory Usage: $($latestVersion.Memory.End)MB final (vs $($baseline.Memory.End)MB)"
    Write-Host "  📊 Tab Capacity: $($latestVersion.MaxTabs) tabs (vs $($baseline.MaxTabs) tabs)"
}

# Best Performing Versions
Write-Host "`n🏆 CHAMPION METRICS:" -ForegroundColor Green
$fastestTabCreation = $results | Sort-Object { $_.TabCreations.Average } | Where-Object { $_.TabCreations.Average -gt 0 } | Select-Object -First 1
$fastestSearch = $results | Sort-Object { $_.SearchOperations.Average } | Where-Object { $_.SearchOperations.Average -gt 0 } | Select-Object -First 1
$mostTabs = $results | Sort-Object { $_.MaxTabs } -Descending | Select-Object -First 1
$lowestMemory = $results | Sort-Object { $_.Memory.End } | Where-Object { $_.Memory.End -gt 0 } | Select-Object -First 1

Write-Host "  ⚡ Fastest Tab Creation: $($fastestTabCreation.Version) ($($fastestTabCreation.TabCreations.Average)ms avg)"
Write-Host "  🔍 Fastest Search: $($fastestSearch.Version) ($($fastestSearch.SearchOperations.Average)ms avg)"
Write-Host "  📊 Most Tabs Handled: $($mostTabs.Version) ($($mostTabs.MaxTabs) tabs)"
    Write-Host "  💾 Lowest Memory: $($lowestMemory.Version) ($($lowestMemory.Memory.End)MB)"

# Save detailed report
if ($DetailedReport) {
    $reportPath = "$LogsPath\Performance_Comparison_Report_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
    $results | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`n💾 Detailed report saved: $reportPath" -ForegroundColor Blue
}

Write-Host "`n✅ Performance analysis completed!" -ForegroundColor Green