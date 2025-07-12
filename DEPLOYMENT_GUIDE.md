# SeeSharpTools-Web 部署指南

## 🚀 一键部署

### 前提条件
1. 确保服务器 `8.155.57.140` 的SSH服务正常运行
2. 安装 `sshpass` 工具：
   - macOS: `brew install sshpass`
   - Linux: `apt install sshpass` 或 `yum install sshpass`

### 自动部署
```bash
./deploy.sh
```

## 📋 手动部署步骤

如果自动部署失败，可以按照以下步骤手动部署：

### 1. 检查服务器连接
```bash
ssh root@8.155.57.140
# 输入密码: Welcome@2025
```

### 2. 准备部署文件
部署脚本已经创建了 `deploy-temp` 目录，包含：
- `frontend/` - 前端构建文件
- `backend/` - 后端发布文件  
- `ssl/` - SSL证书文件
- `config/` - 配置文件

### 3. 手动上传文件
```bash
# 上传部署文件
scp -r deploy-temp root@8.155.57.140:/tmp/deploy

# 或者使用rsync
rsync -avz deploy-temp/ root@8.155.57.140:/tmp/deploy/
```

### 4. 执行服务器端部署
```bash
ssh root@8.155.57.140
cd /tmp/deploy
chmod +x server-deploy.sh
./server-deploy.sh
```

## 🔧 服务器端部署详情

### 安装的软件包
- nginx - Web服务器
- dotnet-runtime-9.0 - .NET运行时
- curl - 测试工具

### 部署目录结构
```
/var/www/seesharpweb/
├── frontend/          # Vue.js前端文件
└── backend/           # ASP.NET Core后端文件
    └── SeeSharpBackend # 可执行文件

/etc/ssl/
├── certs/
│   └── www.alethealab.cn.pem
└── private/
    └── www.alethealab.cn.key
```

### 服务配置
- **Nginx配置**: `/etc/nginx/sites-available/nginx-seesharpweb.conf`
- **Systemd服务**: `/etc/systemd/system/seesharpweb.service`
- **服务端口**: 5000 (后端API)
- **Web端口**: 80/443 (Nginx代理)

## 🌐 访问地址

部署完成后，可以通过以下地址访问：

- **主站**: https://www.alethealab.cn/seesharpweb
- **API文档**: https://www.alethealab.cn/seesharpweb/api/swagger
- **健康检查**: https://www.alethealab.cn/seesharpweb/api/weatherforecast

## 🔧 管理命令

### 服务管理
```bash
# 查看服务状态
systemctl status seesharpweb

# 启动/停止/重启服务
systemctl start seesharpweb
systemctl stop seesharpweb
systemctl restart seesharpweb

# 查看服务日志
journalctl -u seesharpweb -f
```

### Nginx管理
```bash
# 测试配置
nginx -t

# 重新加载配置
systemctl reload nginx

# 查看Nginx状态
systemctl status nginx
```

## 🐛 故障排除

### 1. 服务无法启动
```bash
# 检查日志
journalctl -u seesharpweb --no-pager -l

# 检查端口占用
netstat -tlnp | grep :5000

# 手动启动测试
cd /var/www/seesharpweb/backend
./SeeSharpBackend
```

### 2. Nginx配置问题
```bash
# 测试配置语法
nginx -t

# 查看错误日志
tail -f /var/log/nginx/error.log
```

### 3. SSL证书问题
```bash
# 检查证书文件
ls -la /etc/ssl/certs/www.alethealab.cn.pem
ls -la /etc/ssl/private/www.alethealab.cn.key

# 测试证书
openssl x509 -in /etc/ssl/certs/www.alethealab.cn.pem -text -noout
```

### 4. 防火墙问题
```bash
# 检查防火墙状态
ufw status

# 开放必要端口
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 5000/tcp
```

## 🧪 测试验证

### 本地测试
```bash
# 测试前端
curl -k https://localhost/seesharpweb/

# 测试后端API
curl -k https://localhost/seesharpweb/api/weatherforecast

# 测试WebSocket
curl -k https://localhost/seesharpweb/hubs/datastream
```

### 远程测试
```bash
# 测试域名解析
nslookup www.alethealab.cn

# 测试HTTPS连接
curl -I https://www.alethealab.cn/seesharpweb

# 测试API响应
curl https://www.alethealab.cn/seesharpweb/api/weatherforecast
```

## 📊 性能监控

### 系统资源
```bash
# CPU和内存使用
top
htop

# 磁盘使用
df -h

# 网络连接
netstat -tlnp
```

### 应用监控
```bash
# 查看进程
ps aux | grep SeeSharpBackend

# 查看端口监听
ss -tlnp | grep :5000

# 查看连接数
ss -s
```

## 🔄 更新部署

### 更新前端
```bash
# 重新构建前端
cd frontend
npm run build

# 复制新文件
cp -r dist/* /var/www/seesharpweb/frontend/
```

### 更新后端
```bash
# 重新发布后端
cd backend/SeeSharpBackend
dotnet publish -c Release -o publish

# 停止服务
systemctl stop seesharpweb

# 复制新文件
cp -r publish/* /var/www/seesharpweb/backend/

# 启动服务
systemctl start seesharpweb
```

## 📝 备份和恢复

### 备份
```bash
# 备份应用文件
tar -czf seesharpweb-backup-$(date +%Y%m%d).tar.gz /var/www/seesharpweb

# 备份配置文件
tar -czf seesharpweb-config-$(date +%Y%m%d).tar.gz \
  /etc/nginx/sites-available/nginx-seesharpweb.conf \
  /etc/systemd/system/seesharpweb.service \
  /etc/ssl/certs/www.alethealab.cn.pem \
  /etc/ssl/private/www.alethealab.cn.key
```

### 恢复
```bash
# 恢复应用文件
tar -xzf seesharpweb-backup-YYYYMMDD.tar.gz -C /

# 恢复配置文件
tar -xzf seesharpweb-config-YYYYMMDD.tar.gz -C /

# 重新加载服务
systemctl daemon-reload
systemctl restart seesharpweb nginx
```

## 🎉 部署完成

部署成功后，SeeSharpTools-Web将在以下地址可用：

- **主页**: https://www.alethealab.cn/seesharpweb
- **专业控件库**: 包含23个专业测控控件
- **AI智能生成**: 基于自然语言的控件生成
- **硬件驱动集成**: 支持JY5500、JYUSB1601等硬件
- **实时数据可视化**: 1GS/s数据流显示能力

这是世界首个Web化的专业测控平台，为测控行业的数字化转型提供了完整的解决方案！
