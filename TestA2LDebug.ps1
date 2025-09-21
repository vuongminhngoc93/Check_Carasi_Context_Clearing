# Debug A2L Parser to see what's actually parsed
[System.Reflection.Assembly]::LoadFrom(".\bin\Debug\Check_carasi_DF_ContextClearing.exe") | Out-Null

try {
    Write-Host "=== A2L Parser Debug ===" -ForegroundColor Green
    
    $a2lPath = "d:\5_Automation\Check_carasi_DF_ContextClearing\Input\VC1CP019_V1070C_1.a2l"
    
    # Get parser directly to check internal state
    $parserType = [Check_carasi_DF_ContextClearing.A2LParserManager]
    
    Write-Host "Getting parser for file..." -ForegroundColor Yellow
    $parser = $parserType::GetParser($a2lPath)
    
    if ($parser -ne $null) {
        Write-Host "Parser created successfully!" -ForegroundColor Green
        
        # Access properties if they exist
        Write-Host "Parser statistics:" -ForegroundColor Cyan
        
        try {
            $measurements = $parser.TotalMeasurements
            $characteristics = $parser.TotalCharacteristics  
            $parsedSuccessfully = $parser.ParsedSuccessfully
            
            Write-Host "  Measurements: $measurements"
            Write-Host "  Characteristics: $characteristics"  
            Write-Host "  Parsed Successfully: $parsedSuccessfully"
            
            # If we have characteristics, show a few names
            if ($characteristics -gt 0) {
                Write-Host "  First few characteristic names:" -ForegroundColor Yellow
                
                # Try to access Characteristics dictionary
                $characteristicsDict = $parser.Characteristics
                if ($characteristicsDict -ne $null) {
                    $count = 0
                    foreach ($key in $characteristicsDict.Keys) {
                        if ($count -lt 5) {
                            Write-Host "    $key" -ForegroundColor Gray
                            $count++
                        } else {
                            break
                        }
                    }
                    Write-Host "    ... and $($characteristicsDict.Count - $count) more"
                }
            }
            
        } catch {
            Write-Host "  Could not access parser properties: $($_.Exception.Message)" -ForegroundColor Red
        }
        
    } else {
        Write-Host "Failed to create parser!" -ForegroundColor Red
    }
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack: $($_.Exception.StackTrace)" -ForegroundColor DarkGray
}
