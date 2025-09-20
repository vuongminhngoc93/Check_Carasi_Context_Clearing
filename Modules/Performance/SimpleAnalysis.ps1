# Performance Analysis Script - Fixed Version
param(
    [string]$LogsPath = "d:\5_Automation\Check_carasi_DF_ContextClearing\Modules\Logs"
)

Write-Host "Performance Analysis Starting..." -ForegroundColor Cyan

function Analyze-PerformanceLog {
    param([string]$FilePath, [string]$VersionName)
    
    if (-not (Test-Path $FilePath)) {
        Write-Warning "File not found: $FilePath"
        return $null
    }
    
    $content = Get-Content $FilePath
    if ($content.Count -lt 2) {
        return $null
    }
    
    Write-Host "Analyzing: $VersionName" -ForegroundColor Yellow
    
    # Tab Creation Performance
    $tabCreations = $content | Select-String "COMPLETE,Create_New_Tab" 
    $tabTimes = $tabCreations | ForEach-Object { 
        $parts = ($_ -split ",")
        if ($parts.Count -ge 4) { [int]$parts[3] }
    } | Where-Object { $_ -gt 0 }
    
    # Search Performance  
    $variableChecks = $content | Select-String "COMPLETE,Variable_Check"
    $searchTimes = $variableChecks | ForEach-Object {
        $parts = ($_ -split ",")
        if ($parts.Count -ge 4) { [int]$parts[3] }
    } | Where-Object { $_ -gt 0 }
    
    # Memory Analysis - skip header line
    $dataLines = $content | Where-Object { $_ -notmatch "^Timestamp" -and $_ -match "," }
    $memoryValues = $dataLines | ForEach-Object {
        $parts = ($_ -split ",")
        if ($parts.Count -ge 6 -and $parts[5] -match "^\d+\.?\d*$") { [double]$parts[5] }
    } | Where-Object { $_ -gt 0 }
    
    $tabCounts = $dataLines | ForEach-Object {
        $parts = ($_ -split ",")
        if ($parts.Count -ge 7 -and $parts[6] -match "^\d+$") { [int]$parts[6] }
    } | Where-Object { $_ -gt 0 }
    
    return @{
        Version = $VersionName
        TabCount = $tabTimes.Count
        TabAvg = if ($tabTimes.Count -gt 0) { [math]::Round(($tabTimes | Measure-Object -Average).Average, 1) } else { 0 }
        TabMax = if ($tabTimes.Count -gt 0) { ($tabTimes | Measure-Object -Maximum).Maximum } else { 0 }
        SearchCount = $searchTimes.Count
        SearchAvg = if ($searchTimes.Count -gt 0) { [math]::Round(($searchTimes | Measure-Object -Average).Average, 1) } else { 0 }
        SearchMax = if ($searchTimes.Count -gt 0) { ($searchTimes | Measure-Object -Maximum).Maximum } else { 0 }
        MemoryStart = if ($memoryValues.Count -gt 0) { [math]::Round($memoryValues[0], 1) } else { 0 }
        MemoryEnd = if ($memoryValues.Count -gt 0) { [math]::Round($memoryValues[-1], 1) } else { 0 }
        MaxTabs = if ($tabCounts.Count -gt 0) { ($tabCounts | Measure-Object -Maximum).Maximum } else { 0 }
        LogEntries = $content.Count - 1
    }
}

# Analyze all log files
$results = @()
$logFiles = @(
    @{ Path = "$LogsPath\PerformanceAnalysis_BASELINE.csv"; Name = "BASELINE" }
    @{ Path = "$LogsPath\PerformanceAnalysis_OPTIMIZED.csv"; Name = "OPTIMIZED" }
    @{ Path = "$LogsPath\PerformanceAnalysis_BATCHSEARCH_FIXED.csv"; Name = "BATCH_FIXED" }
    @{ Path = "$LogsPath\PerformanceAnalysis_CONSOLE_REMOVED.csv"; Name = "CONSOLE_REMOVED" }
)

foreach ($logFile in $logFiles) {
    $analysis = Analyze-PerformanceLog -FilePath $logFile.Path -VersionName $logFile.Name
    if ($analysis) {
        $results += $analysis
    }
}

# Display Results
Write-Host "`nPERFORMANCE COMPARISON TABLE" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

$results | ForEach-Object {
    Write-Host "`n$($_.Version):" -ForegroundColor Cyan
    Write-Host "  Tab Operations: $($_.TabCount) completed, $($_.TabAvg)ms avg, $($_.TabMax)ms max"
    Write-Host "  Search Operations: $($_.SearchCount) completed, $($_.SearchAvg)ms avg, $($_.SearchMax)ms max"
    Write-Host "  Memory: $($_.MemoryStart)MB -> $($_.MemoryEnd)MB"
    Write-Host "  Max Tabs: $($_.MaxTabs)"
    Write-Host "  Log Entries: $($_.LogEntries)"
}

Write-Host "`nAnalysis completed!" -ForegroundColor Green
