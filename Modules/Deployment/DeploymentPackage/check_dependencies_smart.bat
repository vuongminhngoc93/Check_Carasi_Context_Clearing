@echo off
echo ======================================================
echo Check Carasi DF Context Clearing Tool - Dependency Checker
echo ======================================================
echo.

set DOTNET_OK=0
set ACCESS_OK=0
set MISSING_COUNT=0

echo Checking system requirements...
echo.

REM Check Windows version
ver | findstr /i "10\." >nul
if %errorlevel%==0 (
    echo [OK] Windows 10 detected
) else (
    ver | findstr /i "11\." >nul
    if %errorlevel%==0 (
        echo [OK] Windows 11 detected
    ) else (
        echo [WARNING] Windows version may not be fully supported
    )
)

REM Check .NET Framework 4.0+
echo Checking .NET Framework 4.0+...
dir /b "%windir%\Microsoft.NET\Framework64\v4*" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] .NET Framework 4.x detected - NO INSTALLATION NEEDED
    set DOTNET_OK=1
) else (
    dir /b "%windir%\Microsoft.NET\Framework\v4*" >nul 2>&1
    if %errorlevel%==0 (
        echo [OK] .NET Framework 4.x detected - NO INSTALLATION NEEDED
        set DOTNET_OK=1
    ) else (
        echo [MISSING] .NET Framework 4.0+ required
        set /a MISSING_COUNT+=1
    )
)

REM Check Access Database Engine
echo Checking Access Database Engine...
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office\16.0\Access Connectivity Engine" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] Access Database Engine detected - NO INSTALLATION NEEDED
    set ACCESS_OK=1
    goto check_optional
)
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office\15.0\Access Connectivity Engine" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] Access Database Engine detected - NO INSTALLATION NEEDED  
    set ACCESS_OK=1
    goto check_optional
)
echo [MISSING] Access Database Engine required for Excel file processing
set /a MISSING_COUNT+=1

:check_optional
REM Check Visual C++ Redistributable (optional)
echo Checking Visual C++ 2015-2022 Redistributable...
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] Visual C++ Redistributable found
) else (
    echo [OPTIONAL] Visual C++ Redistributable not found - may be needed
)

REM Check Office installation (optional)
echo Checking Microsoft Office...
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] Microsoft Office detected
) else (
    echo [OPTIONAL] Microsoft Office not found - some features may be limited
)

echo.
echo ======================================================
echo DEPENDENCY CHECK RESULTS
echo ======================================================

if %MISSING_COUNT%==0 (
    echo [SUCCESS] All required dependencies are installed!
    echo.
    echo You can run the application immediately:
    echo Check_carasi_DF_ContextClearing.exe
    echo.
    echo No additional installations needed.
) else (
    echo [ACTION REQUIRED] %MISSING_COUNT% dependency^(ies^) missing
    echo.
    
    if %DOTNET_OK%==0 (
        echo 1. INSTALL .NET Framework 4.0+
        echo    Download: https://dotnet.microsoft.com/download/dotnet-framework
        echo    ^(This is usually pre-installed on Windows 10/11^)
        echo.
    )
    
    if %ACCESS_OK%==0 (
        echo 2. INSTALL Microsoft Access Database Engine 2016 Redistributable ^(x64^)
        echo    Download: https://www.microsoft.com/download/details.aspx?id=54920
        echo    ^(Required for Excel file processing^)
        echo.
    )
    
    echo After installing missing dependencies, run this checker again
    echo to verify, then start the application.
)

echo ======================================================
pause
