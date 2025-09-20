@echo off
echo =====================================
echo   Carasi DF Context Clearing Tests
echo =====================================
echo.

REM Set the path to MSTest
set MSTEST_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\MSTest.exe"
if not exist %MSTEST_PATH% (
    set MSTEST_PATH="C:\Program Files\Microsoft Visual Studio\2019\Professional\Common7\IDE\MSTest.exe"
)
if not exist %MSTEST_PATH% (
    set MSTEST_PATH="vstest.console.exe"
)

REM Get the directory of this script
set SCRIPT_DIR=%~dp0
set TEST_ASSEMBLY=%SCRIPT_DIR%bin\Debug\Check_carasi_DF_ContextClearing.Tests.dll

echo Building test project...
dotnet build "%SCRIPT_DIR%Tests.csproj" --configuration Debug

if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Running Unit Tests...
echo ====================================

%MSTEST_PATH% /testcontainer:"%TEST_ASSEMBLY%" /category:"UnitTest"

echo.
echo Running Integration Tests...
echo ====================================

%MSTEST_PATH% /testcontainer:"%TEST_ASSEMBLY%" /category:"IntegrationTest"

echo.
echo Running All Tests...
echo ====================================

%MSTEST_PATH% /testcontainer:"%TEST_ASSEMBLY%"

echo.
echo Test run completed!
echo ====================================

pause
