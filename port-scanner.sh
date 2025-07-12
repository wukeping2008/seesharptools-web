#!/bin/bash

# 端口扫描脚本
SERVER="8.155.57.140"

echo "🔍 扫描服务器 $SERVER 的开放端口..."

# 常见SSH端口
SSH_PORTS=(22 2222 2022 8022 10022 22222)
echo "📡 检测SSH端口..."
for port in "${SSH_PORTS[@]}"; do
    echo -n "端口 $port: "
    if nc -z -w 2 $SERVER $port 2>/dev/null; then
        echo "✅ 开放"
        echo "🔑 尝试SSH连接端口 $port..."
        ssh -p $port -o ConnectTimeout=5 -o BatchMode=yes root@$SERVER "echo 'SSH连接成功!'" 2>/dev/null && echo "🎉 SSH端口 $port 可用!" && exit 0
    else
        echo "❌ 关闭"
    fi
done

# 常见Web端口
WEB_PORTS=(80 443 8080 8443 3000 5000 8000 9000)
echo "🌐 检测Web端口..."
for port in "${WEB_PORTS[@]}"; do
    echo -n "端口 $port: "
    if nc -z -w 2 $SERVER $port 2>/dev/null; then
        echo "✅ 开放"
        echo "🌍 测试HTTP连接端口 $port..."
        curl -s --connect-timeout 5 http://$SERVER:$port/ >/dev/null && echo "🎉 HTTP端口 $port 可用!"
    else
        echo "❌ 关闭"
    fi
done

# 其他常见端口
OTHER_PORTS=(21 23 25 53 110 143 993 995 3389 5432 3306 1433 6379)
echo "🔧 检测其他服务端口..."
for port in "${OTHER_PORTS[@]}"; do
    echo -n "端口 $port: "
    if nc -z -w 2 $SERVER $port 2>/dev/null; then
        echo "✅ 开放"
    else
        echo "❌ 关闭"
    fi
done

# 扫描高端口范围
echo "🔍 扫描高端口范围 (10000-65535)..."
for port in {10000..10100} {20000..20100} {30000..30100} {40000..40100} {50000..50100} {60000..60100}; do
    if nc -z -w 1 $SERVER $port 2>/dev/null; then
        echo "✅ 发现开放端口: $port"
        # 如果是高端口，尝试SSH
        if [ $port -gt 1024 ]; then
            echo "🔑 尝试SSH连接端口 $port..."
            ssh -p $port -o ConnectTimeout=3 -o BatchMode=yes root@$SERVER "echo 'SSH连接成功!'" 2>/dev/null && echo "🎉 SSH端口 $port 可用!" && exit 0
        fi
    fi
done

echo "❌ 未发现可用的SSH端口"
echo "💡 建议："
echo "1. 检查服务器防火墙设置"
echo "2. 确认SSH服务是否运行"
echo "3. 联系服务器管理员开放SSH端口"
echo "4. 使用云服务商的Web控制台进行部署"
