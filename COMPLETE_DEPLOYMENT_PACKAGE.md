# 🎯 SeeSharpTools-Web 完整部署包

## 📋 服务器信息确认

- **IP地址**: 8.155.57.140
- **用户名**: root
- **密码**: Welcome@2025
- **目标域名**: seesharp.alethealab.cn
- **操作系统**: Debian 12.10 64位

## ✅ 部署文件完全准备就绪

### 文件结构
```
SeeSharpTools-Web/
├── deploy-temp/                    # 部署文件包
│   ├── frontend/                   # Vue3前端构建文件
│   ├── backend/                    # .NET 9.0后端发布文件
│   ├── ssl/                        # SSL证书文件
│   ├── config/                     # 配置文件
│   │   ├── nginx-seesharp.conf     # Nginx配置
│   │   └── seesharpweb.service     # systemd服务配置
│   └── server-deploy-debian.sh     # 服务器端部署脚本
├── deploy-debian.sh                # Debian专用部署脚本
├── deploy-seesharp.sh              # 通用部署脚本
└── 文档/
    ├── FINAL_DEPLOYMENT_STATUS.md
    ├── VNC_SETUP_GUIDE.md
    ├── DEPLOYMENT_TROUBLESHOOTING.md
    └── QUICK_DEPLOY.md
```

## 🔧 当前问题和解决方案

### 问题：SSH连接无法建立
- 端口22: 无响应
- 端口10000: 发现开放但SSH连接超时
- 需要通过阿里云VNC控制台解决

### 解决方案：VNC连接

1. **登录阿里云控制台**
   - 找到ECS实例：8.155.57.140
   - 点击"远程连接"

2. **选择连接方式**
   - VNC连接（推荐）
   - Workbench远程连接

3. **在VNC控制台执行**
   ```bash
   # 检查SSH服务
   systemctl status ssh
   
   # 启动SSH服务
   systemctl start ssh
   systemctl enable ssh
   
   # 检查端口监听
   netstat -tlnp | grep :22
   
   # 如果SSH未安装
   apt update
   apt install openssh-server
   
   # 配置SSH允许root登录
   echo "PermitRootLogin yes" >> /etc/ssh/sshd_config
   echo "PasswordAuthentication yes" >> /etc/ssh/sshd_config
   
   # 重启SSH服务
   systemctl restart ssh
   
   # 检查防火墙
   ufw status
   ufw allow 22/tcp
   ```

## 🚀 SSH恢复后的部署方案

### 方案一：一键自动部署
```bash
# 测试SSH连接
ssh root@8.155.57.140

# 执行Debian专用部署脚本
./deploy-debian.sh
```

### 方案二：手动分步部署
```bash
# 1. 上传部署文件
scp -r deploy-temp root@8.155.57.140:/tmp/deploy

# 2. 连接服务器
ssh root@8.155.57.140

# 3. 执行部署脚本
bash /tmp/deploy/server-deploy-debian.sh
```

### 方案三：逐步手动部署
如果自动脚本有问题，可以逐步执行：

```bash
# 连接服务器后执行
ssh root@8.155.57.140

# 更新系统
apt update && apt upgrade -y

# 安装必要软件
apt install -y nginx curl wget

# 安装.NET 9.0运行时
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y dotnet-runtime-9.0 aspnetcore-runtime-9.0

# 创建目录
mkdir -p /var/www/seesharpweb/{frontend,backend,data}

# 上传并部署文件（需要先通过scp上传）
cp -r /tmp/deploy/frontend/* /var/www/seesharpweb/frontend/
cp -r /tmp/deploy/backend/* /var/www/seesharpweb/backend/
cp /tmp/deploy/ssl/* /etc/ssl/certs/
cp /tmp/deploy/config/nginx-seesharp.conf /etc/nginx/sites-available/seesharp
cp /tmp/deploy/config/seesharpweb.service /etc/systemd/system/

# 配置Nginx
ln -sf /etc/nginx/sites-available/seesharp /etc/nginx/sites-enabled/
rm -f /etc/nginx/sites-enabled/default
nginx -t

# 配置服务
systemctl daemon-reload
systemctl enable seesharpweb
systemctl enable nginx

# 设置权限
chown -R www-data:www-data /var/www/seesharpweb
chmod +x /var/www/seesharpweb/backend/SeeSharpBackend

# 启动服务
systemctl start nginx
systemctl start seesharpweb
```

## 📊 部署完成后的验证

### 服务状态检查
```bash
# 检查服务状态
systemctl status nginx
systemctl status seesharpweb

# 检查端口监听
netstat -tlnp | grep -E ':80|:443|:5000'

# 检查进程
ps aux | grep -E 'nginx|SeeSharpBackend'
```

### 网站访问测试
```bash
# 测试本地访问
curl -I http://localhost/seesharpweb/
curl -I http://localhost/api/weatherforecast

# 测试HTTPS（如果SSL配置成功）
curl -k -I https://localhost/seesharpweb/
```

## 🌐 最终访问地址

部署成功后，可以通过以下地址访问：

- **主站**: https://seesharp.alethealab.cn/
- **应用**: https://seesharp.alethealab.cn/seesharpweb/
- **API**: https://seesharp.alethealab.cn/api/
- **API文档**: https://seesharp.alethealab.cn/swagger/

## 🔍 故障排除

### 如果Nginx启动失败
```bash
# 检查配置语法
nginx -t

# 查看错误日志
tail -f /var/log/nginx/error.log

# 检查端口占用
netstat -tlnp | grep :80
```

### 如果后端服务启动失败
```bash
# 查看服务日志
journalctl -u seesharpweb -f

# 手动启动测试
cd /var/www/seesharpweb/backend
./SeeSharpBackend

# 检查.NET运行时
dotnet --version
```

### 如果SSL证书有问题
```bash
# 检查证书文件
ls -la /etc/ssl/certs/www.alethealab.cn.pem
ls -la /etc/ssl/private/www.alethealab.cn.key

# 验证证书
openssl x509 -in /etc/ssl/certs/www.alethealab.cn.pem -text -noout
```

## 📞 技术支持

1. **阿里云技术支持**: 95187
2. **查看部署日志**: 所有脚本都有详细输出
3. **参考文档**: 项目根目录下的各种MD文档

## 🎯 预期结果

部署完成后，您将拥有：

**🌟 世界领先的Web化专业测控平台**
- 🚀 高性能数据采集和显示
- 🔧 多硬件平台支持
- 🧠 AI智能控件生成
- 📊 23个专业控件库
- 🌐 完整的Web化解决方案
- 🔒 企业级HTTPS安全
- 📱 响应式设计

---

## 🚀 立即行动

**下一步**: 通过阿里云VNC控制台连接服务器，启动SSH服务
**目标**: 5-8分钟内完成整个平台部署
**结果**: https://seesharp.alethealab.cn/seesharpweb/

所有技术准备工作已100%完成，等待您的操作！
