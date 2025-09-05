@echo off
echo 正在停止 AIGen-SeeSharp-Suite 服务...
echo.

taskkill /F /FI "WINDOWTITLE eq AIGen Backend" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq AIGen Frontend" >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
taskkill /F /IM node.exe >nul 2>&1

echo 服务已停止
pause