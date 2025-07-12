#!/bin/bash

# SeeSharpTools-Web 一键部署脚本
# 目标服务器: 8.155.57.140
# 用户: root
# 部署路径: /var/www/seesharpweb

set -e

echo "🚀 开始部署 SeeSharpTools-Web 项目..."

# 配置变量
SERVER_IP="8.155.57.140"
SERVER_USER="root"
SERVER_PASSWORD="Welcome@2025"
DEPLOY_PATH="/var/www/seesharpweb"
DOMAIN="www.alethealab.cn"
SSL_CERT_PATH="./aleathea key pem/www.alethealab.cn.pem"
SSL_KEY_PATH="./aleathea key pem/www.alethealab.cn.key"

echo "📦 准备部署文件..."

# 创建部署目录结构
mkdir -p deploy-temp/frontend
mkdir -p deploy-temp/backend
mkdir -p deploy-temp/ssl
mkdir -p deploy-temp/config

# 复制前端构建文件
echo "📁 复制前端文件..."
cp -r frontend/dist/* deploy-temp/frontend/

# 复制后端发布文件
echo "📁 复制后端文件..."
cp -r backend/SeeSharpBackend/publish/* deploy-temp/backend/

# 复制SSL证书
echo "🔐 复制SSL证书..."
cp "$SSL_CERT_PATH" deploy-temp/ssl/
cp "$SSL_KEY_PATH" deploy-temp/ssl/

# 创建Nginx配置
echo "⚙️ 创建Nginx配置..."
cat > deploy-temp/config/nginx-seesharpweb.conf << 'EOF'
server {
    listen 80;
    server_name www.alethealab.cn alethealab.cn;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name www.alethealab.cn alethealab.cn;

    ssl_certificate /etc/ssl/certs/www.alethealab.cn.pem;
    ssl_certificate_key /etc/ssl/private/www.alethealab.cn.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;

    # 前端静态文件
    location /seesharpweb {
        alias /var/www/seesharpweb/frontend;
        try_files $uri $uri/ /seesharpweb/index.html;
        
        # 静态资源缓存
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }

    # 后端API代理
    location /seesharpweb/api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # SignalR WebSocket代理
    location /seesharpweb/hubs/ {
        proxy_pass http://localhost:5000/hubs/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # 安全头
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;
    add_header Content-Security-Policy "default-src 'self' http: https: data: blob: 'unsafe-inline'" always;
}
EOF

# 创建systemd服务文件
echo "🔧 创建systemd服务..."
cat > deploy-temp/config/seesharpweb.service << 'EOF'
[Unit]
Description=SeeSharpTools Web Backend
After=network.target

[Service]
Type=notify
ExecStart=/var/www/seesharpweb/backend/SeeSharpBackend
WorkingDirectory=/var/www/seesharpweb/backend
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=seesharpweb
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
EOF

# 创建部署脚本
echo "📝 创建服务器端部署脚本..."
cat > deploy-temp/server-deploy.sh << 'EOF'
#!/bin/bash

set -e

echo "🔧 安装必要的软件包..."
apt update
apt install -y nginx dotnet-runtime-9.0 curl

echo "📁 创建部署目录..."
mkdir -p /var/www/seesharpweb/frontend
mkdir -p /var/www/seesharpweb/backend
mkdir -p /etc/ssl/certs
mkdir -p /etc/ssl/private

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
cp /tmp/deploy/config/nginx-seesharpweb.conf /etc/nginx/sites-available/
ln -sf /etc/nginx/sites-available/nginx-seesharpweb.conf /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx

echo "🔧 配置systemd服务..."
cp /tmp/deploy/config/seesharpweb.service /etc/systemd/system/
systemctl daemon-reload
systemctl enable seesharpweb
systemctl start seesharpweb

echo "🔥 配置防火墙..."
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 5000/tcp

echo "✅ 部署完成！"
echo "🌐 网站地址: https://www.alethealab.cn/seesharpweb"
echo "📊 后端API: https://www.alethealab.cn/seesharpweb/api"

echo "🔍 检查服务状态..."
systemctl status seesharpweb --no-pager
systemctl status nginx --no-pager

echo "🧪 测试连接..."
curl -k https://localhost/seesharpweb/ || echo "前端测试失败"
curl -k https://localhost/seesharpweb/api/weatherforecast || echo "后端API测试失败"
EOF

chmod +x deploy-temp/server-deploy.sh

echo "🚀 开始上传文件到服务器..."

# 使用sshpass进行自动化部署
if ! command -v sshpass &> /dev/null; then
    echo "⚠️  需要安装sshpass: brew install sshpass (macOS) 或 apt install sshpass (Linux)"
    echo "📋 手动部署命令："
    echo "1. 上传文件: scp -r deploy-temp root@8.155.57.140:/tmp/deploy"
    echo "2. 执行部署: ssh root@8.155.57.140 'bash /tmp/deploy/server-deploy.sh'"
    exit 1
fi

# 上传部署文件
echo "📤 上传部署文件..."
sshpass -p "$SERVER_PASSWORD" scp -o StrictHostKeyChecking=no -r deploy-temp "$SERVER_USER@$SERVER_IP:/tmp/deploy"

# 执行服务器端部署脚本
echo "🔧 执行服务器端部署..."
sshpass -p "$SERVER_PASSWORD" ssh -o StrictHostKeyChecking=no "$SERVER_USER@$SERVER_IP" 'bash /tmp/deploy/server-deploy.sh'

# 清理临时文件
echo "🧹 清理临时文件..."
rm -rf deploy-temp

echo "🎉 部署完成！"
echo ""
echo "🌐 访问地址:"
echo "   主站: https://www.alethealab.cn/seesharpweb"
echo "   API:  https://www.alethealab.cn/seesharpweb/api"
echo ""
echo "🔧 管理命令:"
echo "   查看服务状态: systemctl status seesharpweb"
echo "   重启服务:     systemctl restart seesharpweb"
echo "   查看日志:     journalctl -u seesharpweb -f"
echo ""
echo "✅ SeeSharpTools-Web 部署成功！"
