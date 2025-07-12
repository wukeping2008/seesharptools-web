# SeeSharpTools-Web 部署指南 (seesharpweb.alethealab.cn)

## 服务器信息
- **IP地址**: 8.155.57.140
- **用户名**: root
- **密码**: Welcome@2025
- **域名**: seesharpweb.alethealab.cn

## 快速部署

### 方法一：一键自动部署（推荐）

1. **安装sshpass（如果尚未安装）**
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
   ./deploy-seesharpweb.sh
   ```

### 方法二：手动部署

如果无法安装sshpass，可以手动执行以下步骤：

1. **构建项目**
   ```bash
   # 构建前端
   cd frontend
   npm install --registry https://registry.npmmirror.com
   npm run build
   cd ..
   
   # 构建后端
   cd backend/SeeSharpBackend
   dotnet publish -c Release -o publish
   cd ../..
   ```

2. **运行部署脚本生成配置文件**
   ```bash
   ./deploy-seesharpweb.sh
   ```
   脚本会在检测到没有sshpass时停止，并生成deploy-temp目录

3. **手动上传文件**
   ```bash
   scp -r deploy-temp root@8.155.57.140:/tmp/deploy
   ```

4. **执行服务器端部署**
   ```bash
   ssh root@8.155.57.140 'bash /tmp/deploy/server-deploy-seesharpweb.sh'
   ```

## 部署内容

### Nginx配置
- **域名**: seesharpweb.alethealab.cn
- **SSL证书**: 使用现有的www.alethealab.cn通配符证书
- **前端路径**: 根路径 `/` 直接指向前端应用
- **API路径**: `/api/` 代理到后端服务
- **WebSocket**: `/hubs/` 支持SignalR实时通信

### 系统服务
- **服务名**: seesharpweb
- **运行用户**: www-data
- **工作目录**: /var/www/seesharpweb/backend
- **端口**: 5000 (内部)
- **自动重启**: 是

### 目录结构
```
/var/www/seesharpweb/
├── frontend/          # 前端静态文件
├── backend/           # 后端应用文件
└── data/             # 数据存储目录
```

## 访问地址

部署完成后，可以通过以下地址访问：

- **主站**: https://seesharpweb.alethealab.cn/
- **API文档**: https://seesharpweb.alethealab.cn/swagger/
- **API接口**: https://seesharpweb.alethealab.cn/api/
- **健康检查**: https://seesharpweb.alethealab.cn/health

## 管理命令

### 服务管理
```bash
# 查看服务状态
systemctl status seesharpweb

# 重启应用服务
systemctl restart seesharpweb

# 停止应用服务
systemctl stop seesharpweb

# 启动应用服务
systemctl start seesharpweb

# 查看应用日志
journalctl -u seesharpweb -f
```

### Nginx管理
```bash
# 重启Nginx
systemctl restart nginx

# 查看Nginx状态
systemctl status nginx

# 查看Nginx错误日志
tail -f /var/log/nginx/seesharpweb.error.log

# 查看Nginx访问日志
tail -f /var/log/nginx/seesharpweb.access.log

# 测试Nginx配置
nginx -t
```

### 远程管理
```bash
# 连接服务器
ssh root@8.155.57.140

# 查看应用日志
ssh root@8.155.57.140 'journalctl -u seesharpweb -f'

# 重启应用
ssh root@8.155.57.140 'systemctl restart seesharpweb'

# 重启Nginx
ssh root@8.155.57.140 'systemctl restart nginx'
```

## 故障排除

### 常见问题

1. **服务无法启动**
   ```bash
   # 检查服务状态
   systemctl status seesharpweb
   
   # 查看详细日志
   journalctl -u seesharpweb -n 50
   
   # 检查端口占用
   netstat -tlnp | grep 5000
   ```

2. **Nginx配置错误**
   ```bash
   # 测试配置
   nginx -t
   
   # 查看错误日志
   tail -f /var/log/nginx/error.log
   ```

3. **SSL证书问题**
   ```bash
   # 检查证书文件
   ls -la /etc/ssl/certs/www.alethealab.cn.pem
   ls -la /etc/ssl/private/www.alethealab.cn.key
   
   # 检查证书权限
   chmod 644 /etc/ssl/certs/www.alethealab.cn.pem
   chmod 600 /etc/ssl/private/www.alethealab.cn.key
   ```

4. **防火墙问题**
   ```bash
   # 检查防火墙状态
   ufw status
   
   # 开放必要端口
   ufw allow 80/tcp
   ufw allow 443/tcp
   ```

### 重新部署

如果需要重新部署，可以直接运行部署脚本：

```bash
./deploy-seesharpweb.sh
```

脚本会自动：
- 重新构建前端和后端
- 上传新文件
- 重启服务

## 安全注意事项

1. **定期更新系统**
   ```bash
   apt update && apt upgrade -y
   ```

2. **监控日志**
   ```bash
   # 监控应用日志
   journalctl -u seesharpweb -f
   
   # 监控Nginx日志
   tail -f /var/log/nginx/seesharpweb.error.log
   ```

3. **备份数据**
   ```bash
   # 备份应用数据
   tar -czf backup-$(date +%Y%m%d).tar.gz /var/www/seesharpweb/data/
   ```

## 性能优化

1. **Nginx缓存配置**
   - 静态资源缓存1年
   - HTML文件不缓存
   - 启用gzip压缩

2. **应用配置**
   - 生产环境配置
   - 日志级别优化
   - 资源限制设置

3. **监控指标**
   - CPU使用率
   - 内存使用率
   - 磁盘空间
   - 网络流量

## 联系信息

如有问题，请联系系统管理员或查看相关文档。
