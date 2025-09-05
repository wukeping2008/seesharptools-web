@echo off
echo ========================================
echo USB-1601 智能数据采集系统启动脚本
echo ========================================
echo.

echo [1/3] 启动后端服务...
cd backend\USB1601Service
start cmd /k "dotnet run"
timeout /t 5 /nobreak > nul

echo [2/3] 安装前端依赖...
cd ..\..\frontend
if not exist node_modules (
    echo 首次运行，安装依赖包...
    call npm install
)

echo [3/3] 启动前端服务...
start cmd /k "npm run dev"

echo.
echo ========================================
echo 系统启动完成！
echo 后端API: http://localhost:5000
echo 前端界面: http://localhost:3000
echo ========================================
echo.
echo 按任意键打开浏览器...
pause > nul
start http://localhost:3000