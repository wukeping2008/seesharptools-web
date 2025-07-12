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
