@echo off
echo Starting USB-1601 MCP Service...
echo.

cd /d "%~dp0\.."

if not exist "bin\Debug\net8.0\USB1601MCP.exe" (
    echo Building project first...
    dotnet build
    echo.
)

echo Running MCP server...
echo Press Ctrl+C to stop
echo.

dotnet run

pause