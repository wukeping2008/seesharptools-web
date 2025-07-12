#!/bin/bash

# SeeSharpTools-Web HTTP部署脚本
# 目标服务器: 8.155.57.140:80
# 使用HTTP上传方式

set -e

echo "🚀 开始HTTP部署 SeeSharpTools-Web 项目..."

# 配置变量
SERVER_IP="8.155.57.140"
SERVER_PORT="80"
DEPLOY_URL="http://${SERVER_IP}:${SERVER_PORT}"

echo "📦 准备部署文件..."

# 检查部署文件是否存在
if [ ! -d "deploy-temp" ]; then
    echo "❌ 部署文件不存在，请先运行构建脚本"
    exit 1
fi

# 创建HTTP上传脚本
echo "📝 创建HTTP上传脚本..."
cat > upload-via-http.py << 'EOF'
#!/usr/bin/env python3
import os
import requests
import tarfile
import sys
from pathlib import Path

def create_upload_package():
    """创建上传包"""
    print("📦 创建上传包...")
    
    # 创建tar包
    with tarfile.open('deploy-package.tar.gz', 'w:gz') as tar:
        tar.add('deploy-temp', arcname='deploy')
    
    print("✅ 上传包创建完成: deploy-package.tar.gz")
    return 'deploy-package.tar.gz'

def upload_via_http(server_url, package_path):
    """通过HTTP上传文件"""
    print(f"📤 上传到服务器: {server_url}")
    
    try:
        # 尝试上传文件
        with open(package_path, 'rb') as f:
            files = {'file': f}
            data = {
                'action': 'deploy',
                'project': 'seesharptools-web'
            }
            
            response = requests.post(
                f"{server_url}/upload",
                files=files,
                data=data,
                timeout=300
            )
            
        if response.status_code == 200:
            print("✅ 文件上传成功")
            return True
        else:
            print(f"❌ 上传失败: {response.status_code} - {response.text}")
            return False
            
    except requests.exceptions.RequestException as e:
        print(f"❌ 网络错误: {e}")
        return False

def main():
    server_url = sys.argv[1] if len(sys.argv) > 1 else "http://8.155.57.140:80"
    
    # 创建上传包
    package_path = create_upload_package()
    
    # 上传文件
    success = upload_via_http(server_url, package_path)
    
    if success:
        print("🎉 HTTP部署完成！")
        print(f"🌐 访问地址: {server_url}/seesharpweb")
    else:
        print("❌ HTTP部署失败")
        sys.exit(1)

if __name__ == "__main__":
    main()
EOF

chmod +x upload-via-http.py

# 尝试Python HTTP上传
echo "🔄 尝试HTTP上传..."
if command -v python3 &> /dev/null; then
    python3 upload-via-http.py "$DEPLOY_URL"
else
    echo "⚠️  需要Python3来执行HTTP上传"
fi

# 尝试curl上传
echo "🔄 尝试curl上传..."
if command -v curl &> /dev/null; then
    echo "📤 使用curl上传部署包..."
    
    # 创建tar包
    tar -czf deploy-package.tar.gz deploy-temp/
    
    # 尝试curl上传
    curl -X POST \
         -F "file=@deploy-package.tar.gz" \
         -F "action=deploy" \
         -F "project=seesharptools-web" \
         --connect-timeout 60 \
         --max-time 300 \
         "$DEPLOY_URL/upload" || echo "curl上传失败"
else
    echo "⚠️  curl命令不可用"
fi

# 尝试wget上传
echo "🔄 尝试wget上传..."
if command -v wget &> /dev/null; then
    echo "📤 使用wget上传部署包..."
    
    wget --post-file=deploy-package.tar.gz \
         --header="Content-Type: application/gzip" \
         --timeout=300 \
         "$DEPLOY_URL/upload" || echo "wget上传失败"
else
    echo "⚠️  wget命令不可用"
fi

# 创建简单的HTTP服务器用于文件传输
echo "🔧 创建本地HTTP服务器..."
cat > simple-http-server.py << 'EOF'
#!/usr/bin/env python3
import http.server
import socketserver
import os
from urllib.parse import urlparse, parse_qs
import cgi

class DeployHandler(http.server.SimpleHTTPRequestHandler):
    def do_POST(self):
        if self.path == '/deploy':
            self.handle_deploy()
        else:
            self.send_error(404)
    
    def handle_deploy(self):
        try:
            # 解析上传的文件
            form = cgi.FieldStorage(
                fp=self.rfile,
                headers=self.headers,
                environ={'REQUEST_METHOD': 'POST'}
            )
            
            if 'package' in form:
                fileitem = form['package']
                if fileitem.filename:
                    # 保存上传的文件
                    with open('received-deploy.tar.gz', 'wb') as f:
                        f.write(fileitem.file.read())
                    
                    self.send_response(200)
                    self.send_header('Content-type', 'text/plain')
                    self.end_headers()
                    self.wfile.write(b'Deploy package received successfully')
                    return
            
            self.send_error(400, 'No file uploaded')
            
        except Exception as e:
            self.send_error(500, str(e))

if __name__ == "__main__":
    PORT = 8080
    with socketserver.TCPServer(("", PORT), DeployHandler) as httpd:
        print(f"🌐 HTTP服务器启动在端口 {PORT}")
        print(f"📤 上传地址: http://localhost:{PORT}/deploy")
        httpd.serve_forever()
EOF

chmod +x simple-http-server.py

echo "📋 HTTP部署选项："
echo "1. 如果服务器支持HTTP上传，文件已尝试上传"
echo "2. 手动方式："
echo "   - 将 deploy-package.tar.gz 通过FTP/HTTP上传到服务器"
echo "   - 在服务器上解压: tar -xzf deploy-package.tar.gz"
echo "   - 执行部署: cd deploy && ./server-deploy.sh"
echo ""
echo "3. 本地HTTP服务器："
echo "   - 运行: python3 simple-http-server.py"
echo "   - 从服务器下载: wget http://你的IP:8080/deploy-package.tar.gz"
echo ""
echo "🌐 部署完成后访问: http://8.155.57.140/seesharpweb"

# 清理临时文件
# rm -f upload-via-http.py simple-http-server.py

echo "✅ HTTP部署脚本准备完成！"
