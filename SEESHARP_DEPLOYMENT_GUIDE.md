# 🚀 SeeSharpTools-Web 部署指南

## 📋 部署信息

- **服务器IP**: 8.155.57.140
- **用户名**: root
- **密码**: Welcome@2025
- **域名**: seesharp.alethealab.cn
- **SSL证书**: 已包含在项目中

## 🎯 一键部署

### 方法一：自动化部署（推荐）

1. **安装sshpass（如果没有）**
   ```bash
   # macOS
   brew install sshpass
   
   # Ubuntu/Debian
   sudo apt install sshpass
   
   # CentOS/RHEL
   sudo yum install sshpass
   ```

2. **执行部署脚本**
   ```bash
   ./deploy-seesharp.sh
   ```

### 方法二：手动部署

如果无法安装sshpass，可以手动执行以下步骤：

1. **运行部署脚本（会在sshpass检查处停止）**
   ```bash
   ./deploy-seesharp.sh
   ```

2. **手动上传文件**
   ```bash
   scp -r deploy-temp root@8.155.57.140:/tmp/deploy
   ```

3. **手动执行服务器端部署**
   ```bash
   ssh root@8.155.57.140 'bash /tmp/deploy/server-deploy.sh'
   ```

## 🔧 部署过程详解

### 1. 前端构建
- 自动检查并构建Vue.js前端项目
- 使用淘宝镜像加速npm安装
- 生成优化的生产版本

### 2. 后端发布
- 自动检查并发布.NET Core后端项目
- 生成Release版本
- 包含所有依赖项

### 3. 服务器配置
- 安装Nginx和.NET 9.0运行时
- 配置SSL证书（HTTPS）
- 设置systemd服务
- 配置防火墙规则

### 4. Nginx配置
- HTTP自动重定向到HTTPS
- 前端静态文件服务
- 后端API反向代理
- SignalR WebSocket支持
- 安全头配置

## 🌐 访问地址

部署完成后，可以通过以下地址访问：

- **主站**: https://seesharp.alethealab.cn/
- **应用**: https://seesharp.alethealab.cn/seesharpweb/
- **API**: https://seesharp.alethealab.cn/api/
- **API文档**: https://seesharp.alethealab.cn/swagger/

## 🔍 验证部署

### 1. 检查服务状态
```bash
ssh root@8.155.57.140 'systemctl status seesharpweb'
ssh root@8.155.57.140 'systemctl status nginx'
```

### 2. 查看日志
```bash
# 应用日志
ssh root@8.155.57.140 'journalctl -u seesharpweb -f'

# Nginx日志
ssh root@8.155.57.140 'tail -f /var/log/nginx/seesharp.error.log'
```

### 3. 测试连接
```bash
# 测试前端
curl -I https://seesharp.alethealab.cn/seesharpweb/

# 测试API
curl -I https://seesharp.alethealab.cn/api/weatherforecast
```

## 🛠️ 管理命令

### 重启服务
```bash
# 重启应用
ssh root@8.155.57.140 'systemctl restart seesharpweb'

# 重启Nginx
ssh root@8.155.57.140 'systemctl restart nginx'
```

### 更新部署
```bash
# 重新运行部署脚本
./deploy-seesharp.sh
```

### 查看资源使用
```bash
ssh root@8.155.57.140 'htop'
ssh root@8.155.57.140 'df -h'
ssh root@8.155.57.140 'free -h'
```

## 🔐 安全配置

### SSL/TLS
- 使用TLS 1.2和1.3
- 强加密套件
- HSTS安全头
- 自动HTTP到HTTPS重定向

### 防火墙
- 开放端口：22 (SSH), 80 (HTTP), 443 (HTTPS)
- 内部端口5000仅本地访问

### 权限
- 应用运行在www-data用户下
- SSL证书权限严格控制
- 文件权限最小化原则

## 📊 性能优化

### Nginx优化
- 静态资源缓存（1年）
- Gzip压缩
- HTTP/2支持
- 连接复用

### 应用优化
- 生产环境配置
- 日志级别优化
- 资源限制配置

## 🚨 故障排除

### 常见问题

1. **服务无法启动**
   ```bash
   # 检查端口占用
   ssh root@8.155.57.140 'netstat -tlnp | grep :5000'
   
   # 检查.NET运行时
   ssh root@8.155.57.140 'dotnet --version'
   ```

2. **SSL证书问题**
   ```bash
   # 检查证书文件
   ssh root@8.155.57.140 'ls -la /etc/ssl/certs/www.alethealab.cn.pem'
   ssh root@8.155.57.140 'ls -la /etc/ssl/private/www.alethealab.cn.key'
   
   # 测试Nginx配置
   ssh root@8.155.57.140 'nginx -t'
   ```

3. **前端无法访问**
   ```bash
   # 检查文件权限
   ssh root@8.155.57.140 'ls -la /var/www/seesharpweb/frontend/'
   
   # 检查Nginx配置
   ssh root@8.155.57.140 'nginx -T | grep seesharp'
   ```

### 日志位置
- 应用日志: `journalctl -u seesharpweb`
- Nginx访问日志: `/var/log/nginx/seesharp.access.log`
- Nginx错误日志: `/var/log/nginx/seesharp.error.log`
- 系统日志: `/var/log/syslog`

## 📈 监控建议

### 基础监控
```bash
# CPU和内存使用
ssh root@8.155.57.140 'top'

# 磁盘使用
ssh root@8.155.57.140 'df -h'

# 网络连接
ssh root@8.155.57.140 'ss -tlnp'
```

### 应用监控
```bash
# 应用进程
ssh root@8.155.57.140 'ps aux | grep SeeSharpBackend'

# 端口监听
ssh root@8.155.57.140 'netstat -tlnp | grep :5000'
```

## 🔄 备份策略

### 重要文件备份
```bash
# 配置文件
/etc/nginx/sites-available/seesharp
/etc/systemd/system/seesharpweb.service
/var/www/seesharpweb/backend/appsettings.Production.json

# SSL证书
/etc/ssl/certs/www.alethealab.cn.pem
/etc/ssl/private/www.alethealab.cn.key

# 应用数据
/var/www/seesharpweb/data/
```

### 备份命令
```bash
# 创建备份
ssh root@8.155.57.140 'tar -czf /tmp/seesharpweb-backup-$(date +%Y%m%d).tar.gz /var/www/seesharpweb /etc/nginx/sites-available/seesharp /etc/systemd/system/seesharpweb.service'

# 下载备份
scp root@8.155.57.140:/tmp/seesharpweb-backup-*.tar.gz ./
```

## 📞 技术支持

如果遇到问题，请检查：

1. **部署日志**: 部署脚本的输出信息
2. **服务状态**: systemctl status命令的输出
3. **应用日志**: journalctl -u seesharpweb的输出
4. **网络连接**: 确保域名解析正确

---

## 🎉 部署完成

恭喜！SeeSharpTools-Web已成功部署到生产环境。

**访问地址**: https://seesharp.alethealab.cn/seesharpweb/

这是一个世界领先的Web化专业测控平台，具有：
- 🚀 1GS/s数据流实时显示
- 🔧 多硬件平台支持
- 🧠 AI智能控件生成
- 📊 23个专业控件库
- 🌐 完整的Web化解决方案

享受您的专业测控平台吧！
