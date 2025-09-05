@echo off
echo ====================================================
echo   AIGen-SeeSharp-Suite 启动脚本
echo ====================================================
echo.

echo [1/3] 检查环境...
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [错误] 未安装 .NET SDK，请先安装 .NET 8.0 SDK
    pause
    exit /b 1
)

where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [错误] 未安装 Node.js，请先安装 Node.js 18+
    pause
    exit /b 1
)

echo [√] 环境检查通过
echo.

echo [2/3] 启动后端服务...
cd backend\AIGenSeeSharpSuite.Backend
start "AIGen Backend" cmd /k "dotnet run --urls http://localhost:5100"
echo [√] 后端服务启动中 (http://localhost:5100)
echo.

timeout /t 3 /nobreak >nul

echo [3/3] 启动前端服务...
cd ..\..\frontend
start "AIGen Frontend" cmd /k "npm run dev"
echo [√] 前端服务启动中 (http://localhost:5173)
echo.

echo ====================================================
echo   启动完成！
echo ====================================================
echo.
echo 后端API: http://localhost:5100
echo 前端界面: http://localhost:5173
echo 测试页面: 请在浏览器打开 test-api.html
echo.
echo 提示：
echo - 首次启动可能需要几秒钟
echo - 按 Ctrl+C 可以停止服务
echo - 关闭此窗口不会停止服务
echo.

timeout /t 5 /nobreak >nul
start http://localhost:5173