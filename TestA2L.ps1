# Test A2L Integration
Add-Type -Path ".\bin\Debug\Check_carasi_DF_ContextClearing.exe" -PassThru

# Load assembly
[System.Reflection.Assembly]::LoadFrom(".\bin\Debug\Check_carasi_DF_ContextClearing.exe")

try {
    Write-Host "=== A2L Integration Quick Test ===" -ForegroundColor Green
    
    $a2lPath = "d:\5_Automation\Check_carasi_DF_ContextClearing\Input\VC1CP019_V1070C_1.a2l"
    Write-Host "Testing file: $(Split-Path $a2lPath -Leaf)"
    Write-Host "File exists: $(Test-Path $a2lPath)"
    
    if (Test-Path $a2lPath) {
        $fileSize = (Get-Item $a2lPath).Length / 1MB
        Write-Host "File size: $([math]::Round($fileSize, 1)) MB"
        
        # Test parsing - This will use reflection to call the method
        Write-Host "`n--- Attempting to call A2LParserManager.FindVariable ---" -ForegroundColor Yellow
        
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        
        # Using .NET reflection to call the static method
        $type = [Check_carasi_DF_ContextClearing.A2LParserManager]
        $result = $type::FindVariable($a2lPath, "MDG1C")
        
        $stopwatch.Stop()
        
        Write-Host "Search result for 'MDG1C':"
        Write-Host "  Found: $($result.Found)" -ForegroundColor $(if($result.Found) { "Green" } else { "Red" })
        Write-Host "  Search time: $($stopwatch.ElapsedMilliseconds) ms"
        
        if ($result.Found) {
            Write-Host "  In Measurements: $($result.FoundInMeasurements)"
            Write-Host "  In Characteristics: $($result.FoundInCharacteristics)"
        }
        
        Write-Host "`n--- Testing cache performance ---" -ForegroundColor Yellow
        $stopwatch.Restart()
        $result2 = $type::FindVariable($a2lPath, "MDG1C")
        $stopwatch.Stop()
        Write-Host "Second search (cached): $($stopwatch.ElapsedMilliseconds) ms" -ForegroundColor Cyan
        
    } else {
        Write-Host "ERROR: A2L file not found!" -ForegroundColor Red
    }
    
    Write-Host "`n=== Test Complete ===" -ForegroundColor Green
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
}
