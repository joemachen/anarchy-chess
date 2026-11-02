@echo off
setlocal
rem ============================================================
rem  Anarchy Chess: build, run all tests, then open the game.
rem  Double-click to play with a random seed.
rem  From a terminal:  run.bat 48213        play seed 48213
rem                    run.bat --ci ...     no pauses, wait for the game to close (scripts)
rem  Other arguments in quotes go to the game, e.g. "--screenshot=C:\shot.png" or --overlay
rem  Godot location: defaults below; override in local.bat (copy local.bat.example).
rem ============================================================
set "ROOT=%~dp0"
cd /d "%ROOT%"

set "GODOT=D:\Projects\Godot\Godot_v4.7.1-stable_mono_win64\Godot_v4.7.1-stable_mono_win64.exe"
if exist "%ROOT%local.bat" call "%ROOT%local.bat"

set "CI="
set "GAMEARGS="
:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--ci" (
    set "CI=1"
) else (
    echo %~1| findstr /r "^[0-9][0-9]*$" >nul && (set "GAMEARGS=%GAMEARGS% --seed=%~1") || (set "GAMEARGS=%GAMEARGS% %~1")
)
shift
goto :parse
:parsed

where dotnet >nul 2>nul || (echo [run] ERROR: .NET SDK not found. Install the .NET 8 SDK. & goto :fail)

echo [run] Step 1/3: Building...
dotnet build AnarchyChess.sln --nologo -v q
if errorlevel 1 (echo. & echo [run] BUILD FAILED. See errors above. & goto :fail)

echo [run] Step 2/3: Running tests...
dotnet test AnarchyChess.sln --no-build --nologo -v q
if errorlevel 1 (echo. & echo [run] TESTS FAILED. The game will not start until they pass. & goto :fail)

if not exist "%GODOT%" (echo [run] Godot not found at "%GODOT%". Set GODOT in local.bat. & goto :fail)

echo [run] Step 3/3: Launching the game...
if defined CI (
    start "" /wait "%GODOT%" --path "%ROOT%game" --%GAMEARGS%
) else (
    start "" "%GODOT%" --path "%ROOT%game" --%GAMEARGS%
)
exit /b 0

:fail
if not defined CI pause
exit /b 1
