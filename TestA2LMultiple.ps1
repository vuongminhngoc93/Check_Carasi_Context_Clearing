# Test A2L with multiple variables
[System.Reflection.Assembly]::LoadFrom(".\bin\Debug\Check_carasi_DF_ContextClearing.exe") | Out-Null

try {
    Write-Host "=== A2L Multiple Variable Test ===" -ForegroundColor Green
    
    $a2lPath = "d:\5_Automation\Check_carasi_DF_ContextClearing\Input\VC1CP019_V1070C_1.a2l"
    $type = [Check_carasi_DF_ContextClearing.A2LParserManager]
    
    # Test variables from the A2L file header we saw
    $testVariables = @("MDG1C", "VC1CP019", "DIM", "ASAP2_VERSION", "Protocol_Layer", "V1070C", "1070")
    
    foreach ($variable in $testVariables) {
        Write-Host "`nTesting variable: '$variable'" -ForegroundColor Yellow
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        
        $result = $type::FindVariable($a2lPath, $variable)
        $stopwatch.Stop()
        
        $color = if ($result.Found) { "Green" } else { "Red" }
        Write-Host "  Result: $($result.Found)" -ForegroundColor $color
        Write-Host "  Time: $($stopwatch.ElapsedMilliseconds) ms"
        
        if ($result.Found) {
            Write-Host "    In Measurements: $($result.FoundInMeasurements)" -ForegroundColor Cyan
            Write-Host "    In Characteristics: $($result.FoundInCharacteristics)" -ForegroundColor Cyan
            
            # Show summary if available
            $summary = $result.GetSummary()
            if ($summary -and $summary.Length -gt 0) {
                Write-Host "    Summary: $($summary.Substring(0, [Math]::Min(100, $summary.Length)))..." -ForegroundColor Gray
            }
        }
    }
    
    Write-Host "`n=== Testing Cache Manager Statistics ===" -ForegroundColor Green
    # Test cache info if available
    Write-Host "Cache should now contain parsers for: $a2lPath"
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
