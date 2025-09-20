@echo off
echo ======================================================
echo Check Carasi DF Context Clearing Tool - Dependency Checker
echo ======================================================
echo.

set MISSING_COUNT=0

echo Checking system requirements...
echo.

REM Check .NET Framework 4.0+
echo Checking .NET Framework 4.0+...
dir /b "%windir%\Microsoft.NET\Framework64\v4*" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] .NET Framework 4.x is available
) else (
    dir /b "%windir%\Microsoft.NET\Framework\v4*" >nul 2>&1
    if %errorlevel%==0 (
        echo [OK] .NET Framework 4.x is available
    ) else (
        echo [MISSING] .NET Framework 4.0+ required
        set /a MISSING_COUNT+=1
    )
)

REM Check if app can run (tests both .NET and OLEDB)
echo Testing application startup...
cd Application >nul 2>&1
if exist "Check_carasi_DF_ContextClearing.exe" (
    echo [OK] Application files found
    echo.
    echo Testing if all dependencies work...
    timeout /t 1 /nobreak >nul
    echo [INFO] If application starts successfully, all dependencies are OK
    echo [INFO] If application fails, install Access Database Engine:
    echo        https://www.microsoft.com/download/details.aspx?id=54920
) else (
    echo [ERROR] Application files not found in Application folder
    set /a MISSING_COUNT+=1
)
cd .. >nul 2>&1

echo.
echo ======================================================
echo RESULT
echo ======================================================

if %MISSING_COUNT%==0 (
    echo [SUCCESS] System appears ready!
    echo.
    echo NEXT STEPS:
    echo 1. Copy Application folder to your desired location  
    echo 2. Run Check_carasi_DF_ContextClearing.exe
    echo 3. If app fails to start, install Access Database Engine
    echo.
    echo Ready to deploy!
) else (
    echo [ACTION REQUIRED] %MISSING_COUNT% issue(s) found
    echo.
    echo Please install missing components and run this check again.
)

echo ======================================================
pause
