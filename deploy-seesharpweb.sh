#!/bin/bash

# SeeSharpTools-Web 一键部署脚本 (seesharpweb.alethealab.cn)
# 目标服务器: 8.155.57.140
# 用户: root
# 部署域名: seesharpweb.alethealab.cn

set -e

echo "🚀 开始部署 SeeSharpTools-Web 项目到 seesharpweb.alethealab.cn..."

# 配置变量
SERVER_IP="8.155.57.140"
SERVER_USER="root"
SERVER_PASSWORD="Welcome@2025"
DEPLOY_PATH="/var/www/seesharpweb"
DOMAIN="seesharpweb.alethealab.cn"
SSL_CERT_PATH="./aleathea key pem/www.alethealab.cn.pem"
SSL_KEY_PATH="./aleathea key pem/www.alethealab.cn.key"

echo "📦 准备部署文件..."

# 创建部署目录结构
mkdir -p deploy-temp/frontend
mkdir -p deploy-temp/backend
mkdir -p deploy-temp/ssl
mkdir -p deploy-temp/config

# 检查前端构建文件
if [ ! -d "frontend/dist" ]; then
    echo "📦 构建前端项目..."
    cd frontend
    npm install --registry https://registry.npmmirror.com
    npm run build
    cd ..
fi

# 复制前端构建文件
echo "📁 复制前端文件..."
cp -r frontend/dist/* deploy-temp/frontend/

# 检查后端发布文件
if [ ! -d "backend/SeeSharpBackend/publish" ]; then
    echo "📦 发布后端项目..."
    cd backend/SeeSharpBackend
    dotnet publish -c Release -o publish
    cd ../..
fi

# 复制后端发布文件
echo "📁 复制后端文件..."
cp -r backend/SeeSharpBackend/publish/* deploy-temp/backend/

# 复制SSL证书
echo "🔐 复制SSL证书..."
cp "$SSL_CERT_PATH" deploy-temp/ssl/
cp "$SSL_KEY_PATH" deploy-temp/ssl/

# 创建Nginx配置 - 针对 seesharpweb.alethealab.cn
echo "⚙️ 创建Nginx配置..."
cat > deploy-temp/config/nginx-seesharpweb.conf << 'EOF'
# HTTP重定向到HTTPS
server {
    listen 80;
    server_name seesharpweb.alethealab.cn;
    return 301 https://$server_name$request_uri;
}

# HTTPS主配置
server {
    listen 443 ssl http2;
    server_name seesharpweb.alethealab.cn;

    # SSL证书配置
    ssl_certificate /etc/ssl/certs/www.alethealab.cn.pem;
    ssl_certificate_key /etc/ssl/private/www.alethealab.cn.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-RSA-CHACHA20-POLY1305;
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # 安全头
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;
    add_header Content-Security-Policy "default-src 'self' 'unsafe-inline' 'unsafe-eval' data: blob: https:; connect-src 'self' wss: https:;" always;
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    # 根路径直接指向前端应用
    location / {
        root /var/www/seesharpweb/frontend;
        try_files $uri $uri/ /index.html;
        
        # HTML文件不缓存
        location ~* \.html$ {
            expires -1;
            add_header Cache-Control "no-cache, no-store, must-revalidate";
        }
        
        # 静态资源缓存
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
            access_log off;
        }
    }

    # 后端API代理
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }

    # SignalR WebSocket代理
    location /hubs/ {
        proxy_pass http://localhost:5000/hubs/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }

    # Swagger文档
    location /swagger {
        proxy_pass http://localhost:5000/swagger;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # 健康检查
    location /health {
        proxy_pass http://localhost:5000/health;
        access_log off;
    }

    # 日志配置
    access_log /var/log/nginx/seesharpweb.access.log;
    error_log /var/log/nginx/seesharpweb.error.log;
}
EOF

# 创建systemd服务文件
echo "🔧 创建systemd服务..."
cat > deploy-temp/config/seesharpweb.service << 'EOF'
[Unit]
Description=SeeSharpTools Web Backend Service
After=network.target
Wants=network.target

[Service]
Type=notify
ExecStart=/var/www/seesharpweb/backend/SeeSharpBackend
WorkingDirectory=/var/www/seesharpweb/backend
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=seesharpweb
User=www-data
Group=www-data

# 环境变量
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=DOTNET_CLI_TELEMETRY_OPTOUT=1

# 资源限制
LimitNOFILE=65536
LimitNPROC=4096

[Install]
WantedBy=multi-user.target
EOF

# 创建应用配置文件
echo "⚙️ 创建应用配置..."
cat > deploy-temp/backend/appsettings.Production.json << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Hosting.Diagnostics": "Information"
    }
  },
  "AllowedHosts": "seesharpweb.alethealab.cn",
  "Urls": "http://localhost:5000",
  "DataStorage": {
    "BasePath": "/var/www/seesharpweb/data",
    "MaxFileSizeMB": 1024,
    "CompressionEnabled": true,
    "CompressionThreshold": 1024
  },
  "CORS": {
    "AllowedOrigins": [
      "https://seesharpweb.alethealab.cn",
      "http://localhost:5176",
      "http://localhost:5177"
    ]
  }
}
EOF

# 创建服务器端部署脚本
echo "📝 创建服务器端部署脚本..."
cat > deploy-temp/server-deploy-seesharpweb.sh << 'EOF'
#!/bin/bash

set -e

echo "🔧 更新系统并安装必要的软件包..."
apt update
apt install -y nginx curl wget gnupg2 software-properties-common apt-transport-https

# 安装.NET 9.0运行时
echo "📦 安装.NET 9.0运行时..."
if ! command -v dotnet &> /dev/null; then
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    apt update
    apt install -y dotnet-runtime-9.0 aspnetcore-runtime-9.0
    rm -f packages-microsoft-prod.deb
fi

echo "📁 创建部署目录..."
mkdir -p /var/www/seesharpweb/frontend
mkdir -p /var/www/seesharpweb/backend
mkdir -p /var/www/seesharpweb/data
mkdir -p /etc/ssl/certs
mkdir -p /etc/ssl/private
mkdir -p /var/log/nginx

echo "📦 部署文件..."
cp -r /tmp/deploy/frontend/* /var/www/seesharpweb/frontend/
cp -r /tmp/deploy/backend/* /var/www/seesharpweb/backend/
cp /tmp/deploy/ssl/www.alethealab.cn.pem /etc/ssl/certs/
cp /tmp/deploy/ssl/www.alethealab.cn.key /etc/ssl/private/

echo "🔐 设置文件权限..."
chown -R www-data:www-data /var/www/seesharpweb
chmod -R 755 /var/www/seesharpweb
chmod 600 /etc/ssl/private/www.alethealab.cn.key
chmod 644 /etc/ssl/certs/www.alethealab.cn.pem
chmod +x /var/www/seesharpweb/backend/SeeSharpBackend

echo "⚙️ 配置Nginx..."
# 备份现有配置
if [ -f /etc/nginx/sites-available/default ]; then
    cp /etc/nginx/sites-available/default /etc/nginx/sites-available/default.backup.$(date +%Y%m%d_%H%M%S)
fi

# 安装新配置
cp /tmp/deploy/config/nginx-seesharpweb.conf /etc/nginx/sites-available/seesharpweb
ln -sf /etc/nginx/sites-available/seesharpweb /etc/nginx/sites-enabled/

# 删除默认站点（如果存在）
rm -f /etc/nginx/sites-enabled/default

# 测试Nginx配置
nginx -t

echo "🔧 配置systemd服务..."
# 停止现有服务（如果存在）
systemctl stop seesharpweb 2>/dev/null || true

cp /tmp/deploy/config/seesharpweb.service /etc/systemd/system/
systemctl daemon-reload
systemctl enable seesharpweb

echo "🔥 配置防火墙..."
ufw --force enable
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 5000/tcp

echo "🚀 启动服务..."
systemctl restart nginx
systemctl start seesharpweb

# 等待服务启动
sleep 10

echo "✅ 部署完成！"
echo ""
echo "🌐 网站地址:"
echo "   主站: https://seesharpweb.alethealab.cn/"
echo "   API:  https://seesharpweb.alethealab.cn/api/"
echo "   文档: https://seesharpweb.alethealab.cn/swagger/"
echo ""

echo "🔍 检查服务状态..."
systemctl status seesharpweb --no-pager -l
echo ""
systemctl status nginx --no-pager -l
echo ""

echo "🧪 测试连接..."
echo "测试前端..."
curl -k -I https://localhost/ || echo "⚠️ 前端测试失败"
echo ""
echo "测试后端API..."
curl -k -I https://localhost/api/weatherforecast || echo "⚠️ 后端API测试失败"
echo ""

echo "📋 管理命令:"
echo "   查看应用日志: journalctl -u seesharpweb -f"
echo "   查看Nginx日志: tail -f /var/log/nginx/seesharpweb.error.log"
echo "   重启应用: systemctl restart seesharpweb"
echo "   重启Nginx: systemctl restart nginx"
echo ""
echo "🎉 SeeSharpTools-Web 部署成功！"
EOF

chmod +x deploy-temp/server-deploy-seesharpweb.sh

echo "🚀 开始上传文件到服务器..."

# 检查sshpass
if ! command -v sshpass &> /dev/null; then
    echo "⚠️  需要安装sshpass进行自动化部署"
    echo "📋 安装命令:"
    echo "   macOS: brew install sshpass"
    echo "   Ubuntu/Debian: apt install sshpass"
    echo "   CentOS/RHEL: yum install sshpass"
    echo ""
    echo "📋 手动部署命令："
    echo "1. 上传文件: scp -r deploy-temp root@8.155.57.140:/tmp/deploy"
    echo "2. 执行部署: ssh root@8.155.57.140 'bash /tmp/deploy/server-deploy-seesharpweb.sh'"
    echo ""
    echo "🔧 或者使用密钥认证："
    echo "1. ssh-copy-id root@8.155.57.140"
    echo "2. scp -r deploy-temp root@8.155.57.140:/tmp/deploy"
    echo "3. ssh root@8.155.57.140 'bash /tmp/deploy/server-deploy-seesharpweb.sh'"
    exit 1
fi

# 上传部署文件
echo "📤 上传部署文件..."
sshpass -p "$SERVER_PASSWORD" scp -o StrictHostKeyChecking=no -r deploy-temp "$SERVER_USER@$SERVER_IP:/tmp/deploy"

# 执行服务器端部署脚本
echo "🔧 执行服务器端部署..."
sshpass -p "$SERVER_PASSWORD" ssh -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" 'bash /tmp/deploy/server-deploy-seesharpweb.sh'

# 清理临时文件
echo "🧹 清理临时文件..."
rm -rf deploy-temp

echo ""
echo "🎉 部署完成！"
echo ""
echo "🌐 访问地址:"
echo "   主站: https://seesharpweb.alethealab.cn/"
echo "   API:  https://seesharpweb.alethealab.cn/api/"
echo "   文档: https://seesharpweb.alethealab.cn/swagger/"
echo ""
echo "🔧 远程管理命令:"
echo "   连接服务器: ssh root@8.155.57.140"
echo "   查看应用日志: ssh root@8.155.57.140 'journalctl -u seesharpweb -f'"
echo "   重启应用: ssh root@8.155.57.140 'systemctl restart seesharpweb'"
echo ""
echo "✅ SeeSharpTools-Web 已成功部署到 seesharpweb.alethealab.cn！"
