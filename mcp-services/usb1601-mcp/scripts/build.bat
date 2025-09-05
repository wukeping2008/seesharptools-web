@echo off
echo Building USB-1601 MCP Service...
echo.

cd /d "%~dp0\.."

echo Cleaning previous build...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

echo.
echo Building project...
dotnet build -c Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed!
    exit /b 1
)

echo.
echo Publishing executable...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Publish failed!
    exit /b 1
)

echo.
echo Build completed successfully!
echo Executable location: publish\USB1601MCP.exe
echo.
pause