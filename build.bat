@echo off
cd /d "%~dp0"
echo Building BattlefieldAnalysisBaseDeliver...
dotnet build -c Release
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build succeeded. Output: bin\Release\net472\BattlefieldAnalysisBaseDeliver.dll
) else (
    echo Build failed.
)
pause
