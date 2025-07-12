# 手动部署命令 - seesharpweb.alethealab.cn

由于SSH连接问题，请手动登录服务器执行以下命令：

## 1. 登录服务器
```bash
ssh root@8.155.57.140
# 密码: Welcome@2025
```

## 2. 执行部署脚本
```bash
cd /tmp/deploy/deploy-temp
bash server-deploy-seesharpweb.sh
```

## 3. 如果上述脚本不存在，请手动执行以下命令：

### 3.1 更新系统并安装软件包
```bash
apt update
apt install -y nginx curl wget gnupg2 software-properties-common apt-transport-https
```

### 3.2 安装.NET 9.0运行时
```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y dotnet-runtime-9.0 aspnetcore-runtime-9.0
rm -f packages-microsoft-prod.deb
```

### 3.3 创建部署目录
```bash
mkdir -p /var/www/seesharpweb/frontend
mkdir -p /var/www/seesharpweb/backend
mkdir -p /var/www/seesharpweb/data
mkdir -p /etc/ssl/certs
mkdir -p /etc/ssl/private
mkdir -p /var/log/nginx
```

### 3.4 复制文件
```bash
# 复制前端文件
cp -r /tmp/deploy/deploy-temp/frontend/* /var/www/seesharpweb/frontend/

# 复制后端文件
cp -r /tmp/deploy/deploy-temp/backend/* /var/www/seesharpweb/backend/

# 复制SSL证书
cp /tmp/deploy/deploy-temp/ssl/www.alethealab.cn.pem /etc/ssl/certs/
cp /tmp/deploy/deploy-temp/ssl/www.alethealab.cn.key /etc/ssl/private/
```

### 3.5 设置文件权限
```bash
chown -R www-data:www-data /var/www/seesharpweb
chmod -R 755 /var/www/seesharpweb
chmod 600 /etc/ssl/private/www.alethealab.cn.key
chmod 644 /etc/ssl/certs/www.alethealab.cn.pem
chmod +x /var/www/seesharpweb/backend/SeeSharpBackend
```

### 3.6 配置Nginx
```bash
# 备份现有配置
if [ -f /etc/nginx/sites-available/default ]; then
    cp /etc/nginx/sites-available/default /etc/nginx/sites-available/default.backup.$(date +%Y%m%d_%H%M%S)
fi

# 复制新配置
cp /tmp/deploy/deploy-temp/config/nginx-seesharpweb.conf /etc/nginx/sites-available/seesharpweb
ln -sf /etc/nginx/sites-available/seesharpweb /etc/nginx/sites-enabled/

# 删除默认站点
rm -f /etc/nginx/sites-enabled/default

# 测试配置
nginx -t
```

### 3.7 配置systemd服务
```bash
# 停止现有服务
systemctl stop seesharpweb 2>/dev/null || true

# 复制服务文件
cp /tmp/deploy/deploy-temp/config/seesharpweb.service /etc/systemd/system/
systemctl daemon-reload
systemctl enable seesharpweb
```

### 3.8 配置防火墙
```bash
ufw --force enable
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 5000/tcp
```

### 3.9 启动服务
```bash
systemctl restart nginx
systemctl start seesharpweb

# 等待服务启动
sleep 10
```

## 4. 验证部署

### 4.1 检查服务状态
```bash
systemctl status seesharpweb --no-pager -l
systemctl status nginx --no-pager -l
```

### 4.2 测试连接
```bash
# 测试前端
curl -k -I https://localhost/

# 测试后端API
curl -k -I https://localhost/api/weatherforecast
```

### 4.3 查看日志
```bash
# 查看应用日志
journalctl -u seesharpweb -f

# 查看Nginx日志
tail -f /var/log/nginx/seesharpweb.error.log
```

## 5. 访问地址

部署完成后，可以通过以下地址访问：

- **主站**: https://seesharpweb.alethealab.cn/
- **API文档**: https://seesharpweb.alethealab.cn/swagger/
- **API接口**: https://seesharpweb.alethealab.cn/api/
- **健康检查**: https://seesharpweb.alethealab.cn/health

## 6. 故障排除

如果遇到问题，请检查：

1. **服务状态**: `systemctl status seesharpweb`
2. **Nginx状态**: `systemctl status nginx`
3. **端口占用**: `netstat -tlnp | grep 5000`
4. **防火墙**: `ufw status`
5. **日志文件**: `journalctl -u seesharpweb -n 50`

## 7. 重启服务

如果需要重启服务：
```bash
systemctl restart seesharpweb
systemctl restart nginx
