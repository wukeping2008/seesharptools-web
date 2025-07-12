# 🚀 SeeSharpTools-Web 快速部署

## 📋 部署概览

**目标**: 将SeeSharpTools-Web部署到 `seesharp.alethealab.cn`

**服务器**: 8.155.57.140 (Ubuntu 22.04)

**技术栈**: Vue3 + .NET 9.0 + Nginx + HTTPS

## ⚡ 一键部署

```bash
# 1. 确保在项目根目录
cd /Users/kepingwu/Desktop/SeeSharpTools-Web

# 2. 安装sshpass（如果需要）
brew install sshpass  # macOS

# 3. 执行部署
./deploy-seesharp.sh
```

## 🎯 部署结果

部署成功后，访问以下地址：

- **主页**: https://seesharp.alethealab.cn/
- **应用**: https://seesharp.alethealab.cn/seesharpweb/
- **API**: https://seesharp.alethealab.cn/api/
- **文档**: https://seesharp.alethealab.cn/swagger/

## 📁 项目结构

```
SeeSharpTools-Web/
├── frontend/                 # Vue3前端项目
├── backend/SeeSharpBackend/   # .NET后端项目
├── aleathea key pem/          # SSL证书
├── deploy-seesharp.sh         # 部署脚本
└── SEESHARP_DEPLOYMENT_GUIDE.md  # 详细部署指南
```

## 🔧 部署脚本功能

`deploy-seesharp.sh` 自动完成：

1. ✅ 前端构建（Vue3 + Vite）
2. ✅ 后端发布（.NET 9.0）
3. ✅ 文件上传到服务器
4. ✅ 服务器环境配置
5. ✅ Nginx + SSL配置
6. ✅ systemd服务配置
7. ✅ 防火墙配置
8. ✅ 服务启动

## 🛠️ 手动部署（备选）

如果自动部署失败，可以手动执行：

```bash
# 1. 生成部署文件
./deploy-seesharp.sh  # 会在sshpass检查处停止

# 2. 手动上传
scp -r deploy-temp root@8.155.57.140:/tmp/deploy

# 3. 手动执行服务器端脚本
ssh root@8.155.57.140 'bash /tmp/deploy/server-deploy.sh'
```

## 🔍 验证部署

```bash
# 检查服务状态
ssh root@8.155.57.140 'systemctl status seesharpweb nginx'

# 测试访问
curl -I https://seesharp.alethealab.cn/seesharpweb/
curl -I https://seesharp.alethealab.cn/api/weatherforecast
```

## 🚨 常见问题

### 1. sshpass未安装
```bash
# 解决方案
brew install sshpass  # macOS
sudo apt install sshpass  # Ubuntu
```

### 2. 前端构建失败
```bash
# 手动构建
cd frontend
npm install --registry https://registry.npmmirror.com
npm run build
cd ..
```

### 3. 后端发布失败
```bash
# 手动发布
cd backend/SeeSharpBackend
dotnet publish -c Release -o publish
cd ../..
```

### 4. 服务器连接失败
```bash
# 检查网络连接
ping 8.155.57.140

# 检查SSH连接
ssh root@8.155.57.140
```

## 📊 服务管理

```bash
# 重启应用
ssh root@8.155.57.140 'systemctl restart seesharpweb'

# 查看日志
ssh root@8.155.57.140 'journalctl -u seesharpweb -f'

# 重启Nginx
ssh root@8.155.57.140 'systemctl restart nginx'
```

## 📖 详细文档

更多详细信息请参考：
- [SEESHARP_DEPLOYMENT_GUIDE.md](./SEESHARP_DEPLOYMENT_GUIDE.md) - 完整部署指南
- [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) - 原始部署文档

## 🎉 部署完成

恭喜！您已成功部署了世界领先的Web化专业测控平台！

**SeeSharpTools-Web** 特性：
- 🚀 1GS/s数据流实时显示
- 🔧 多硬件平台支持  
- 🧠 AI智能控件生成
- 📊 23个专业控件库
- 🌐 完整的Web化解决方案

**立即访问**: https://seesharp.alethealab.cn/seesharpweb/
