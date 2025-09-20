@echo off
echo ======================================================
echo Check Carasi DF Context Clearing Tool - Dependencies
echo ======================================================
echo.

echo Checking system requirements...
echo.

REM Check .NET Framework 4.0+
echo Checking .NET Framework 4.0+...
dir /b "%windir%\Microsoft.NET\Framework64\v4*" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] .NET Framework 4.x is available
    goto check_app
)
dir /b "%windir%\Microsoft.NET\Framework\v4*" >nul 2>&1
if %errorlevel%==0 (
    echo [OK] .NET Framework 4.x is available
    goto check_app
)
echo [MISSING] .NET Framework 4.0+ required
echo Download: https://dotnet.microsoft.com/download/dotnet-framework
goto end_check

:check_app
echo Checking application files...
cd Application
if exist "Check_carasi_DF_ContextClearing.exe" (
    echo [OK] Application files found
) else (
    echo [ERROR] Application files missing
    cd ..
    goto end_check
)
cd ..

echo.
echo ======================================================
echo RESULT
echo ======================================================
echo [SUCCESS] System ready for deployment!
echo.
echo DEPLOYMENT STEPS:
echo 1. Copy Application folder to target location
echo 2. Run Check_carasi_DF_ContextClearing.exe ^(NO ADMIN RIGHTS NEEDED^)
echo 3. If Excel reading fails, install Access Database Engine:
echo    https://www.microsoft.com/download/details.aspx?id=54920
echo.
echo Most systems only need Access Database Engine.
echo Application runs with normal user privileges.
goto end_script

:end_check
echo.
echo ======================================================
echo ACTION REQUIRED
echo ======================================================
echo Please install missing components and try again.

:end_script
echo ======================================================
pause
