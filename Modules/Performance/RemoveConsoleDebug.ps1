# Script to remove Console.WriteLine statements while preserving PerformanceLogger
param(
    [string]$FilePath = "d:\5_Automation\Check_carasi_DF_ContextClearing\Form1.cs"
)

Write-Host "Removing Console.WriteLine statements from $FilePath..." -ForegroundColor Yellow

# Read the file content
$content = Get-Content $FilePath -Raw

# Count original Console.WriteLine statements
$originalCount = ([regex]::Matches($content, "Console\.WriteLine")).Count
Write-Host "Found $originalCount Console.WriteLine statements" -ForegroundColor Cyan

# Remove all lines containing Console.WriteLine (with proper whitespace handling)
$lines = Get-Content $FilePath
$filteredLines = @()

foreach ($line in $lines) {
    # Skip lines that contain Console.WriteLine
    if ($line -notmatch "^\s*Console\.WriteLine") {
        $filteredLines += $line
    }
}

# Write the filtered content back to the file
$filteredLines | Out-File -FilePath $FilePath -Encoding UTF8

# Count remaining Console.WriteLine statements
$newContent = Get-Content $FilePath -Raw
$remainingCount = ([regex]::Matches($newContent, "Console\.WriteLine")).Count

Write-Host "Removed $($originalCount - $remainingCount) Console.WriteLine statements" -ForegroundColor Green
Write-Host "Remaining Console.WriteLine statements: $remainingCount" -ForegroundColor Yellow

if ($remainingCount -eq 0) {
    Write-Host "✅ All Console.WriteLine statements successfully removed!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some Console.WriteLine statements remain - manual review needed" -ForegroundColor Yellow
}

# Verify PerformanceLogger calls are preserved
$loggerCount = ([regex]::Matches($newContent, "PerformanceLogger\.")).Count
Write-Host "PerformanceLogger calls preserved: $loggerCount" -ForegroundColor Cyan
