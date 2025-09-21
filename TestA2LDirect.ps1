# Test A2LParser directly without A2LParserManager
[System.Reflection.Assembly]::LoadFrom(".\bin\Debug\Check_carasi_DF_ContextClearing.exe") | Out-Null

try {
    Write-Host "=== Direct A2LParser Test ===" -ForegroundColor Green
    
    $a2lPath = "d:\5_Automation\Check_carasi_DF_ContextClearing\Input\VC1CP019_V1070C_1.a2l"
    
    Write-Host "Creating A2LParser directly..." -ForegroundColor Yellow
    $parser = New-Object Check_carasi_DF_ContextClearing.A2LParser($a2lPath)
    
    if ($parser -ne $null) {
        Write-Host "Parser created successfully!" -ForegroundColor Green
        
        Write-Host "Parser statistics:" -ForegroundColor Cyan
        Write-Host "  Measurements: $($parser.TotalMeasurements)"
        Write-Host "  Characteristics: $($parser.TotalCharacteristics)"  
        Write-Host "  Parsed Successfully: $($parser.ParsedSuccessfully)"
        Write-Host "  Parse Time: $($parser.ParseTime.TotalMilliseconds) ms"
        
        if ($parser.TotalCharacteristics -gt 0) {
            Write-Host "  SUCCESS: Found characteristics in A2L file!" -ForegroundColor Green
            
            # Test direct FindVariable method
            Write-Host "`nTesting FindVariable method:" -ForegroundColor Yellow
            $result = $parser.FindVariable("Can_CAN_EPT_CANBaudrateCfgId_C")
            Write-Host "  Search result for 'Can_CAN_EPT_CANBaudrateCfgId_C': $($result.Found)" -ForegroundColor $(if($result.Found) { "Green" } else { "Red" })
            
            if ($result.Found) {
                Write-Host "  Found in Characteristics: $($result.FoundInCharacteristics)"
                Write-Host "  Found in Measurements: $($result.FoundInMeasurements)"
            }
        } else {
            Write-Host "  WARNING: No characteristics parsed - check parsing logic" -ForegroundColor Red
        }
        
    } else {
        Write-Host "Failed to create parser!" -ForegroundColor Red
    }
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack: $($_.Exception.StackTrace)" -ForegroundColor DarkGray
}
