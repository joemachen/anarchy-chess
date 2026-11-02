@echo off
setlocal
rem Anarchy Chess: run every automated test. Double-click to run.
cd /d "%~dp0"

dotnet test AnarchyChess.sln --nologo
if errorlevel 1 (
    echo.
    echo ==================== TESTS FAILED ====================
    if /i not "%~1"=="--ci" pause
    exit /b 1
)
echo.
echo ==================== ALL TESTS PASSED ====================
if /i not "%~1"=="--ci" pause
exit /b 0
