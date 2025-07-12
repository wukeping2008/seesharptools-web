# 🚨 SeeSharpTools-Web 部署故障排除指南

## 📋 当前问题分析

根据端口扫描结果，服务器 `8.155.57.140` 的所有端口都显示关闭状态，这表明：

1. **SSH服务未启动** - 端口22无法连接
2. **防火墙阻止连接** - 可能是云服务器安全组或系统防火墙
3. **服务器未完全启动** - 系统可能还在初始化

## 🔧 解决方案

### 方案一：云服务器控制台操作

1. **登录云服务器控制台**
   - 阿里云/腾讯云/华为云等控制台
   - 找到实例 `8.155.57.140`

2. **通过VNC/控制台连接**
   ```bash
   # 检查SSH服务状态
   systemctl status ssh
   
   # 启动SSH服务
   systemctl start ssh
   systemctl enable ssh
   
   # 检查防火墙状态
   ufw status
   
   # 开放必要端口
   ufw allow 22/tcp
   ufw allow 80/tcp
   ufw allow 443/tcp
   ```

3. **检查安全组设置**
   - 确保安全组开放了22, 80, 443端口
   - 允许0.0.0.0/0访问

### 方案二：手动部署文件准备

如果SSH连接问题暂时无法解决，可以先准备好部署文件：

```bash
# 1. 执行部署脚本（会在SSH连接处停止）
./deploy-debian.sh

# 2. 部署文件已准备在 deploy-temp 目录
ls -la deploy-temp/

# 3. 等SSH连接恢复后，手动上传
scp -r deploy-temp root@8.155.57.140:/tmp/deploy
ssh root@8.155.57.140 'bash /tmp/deploy/server-deploy-debian.sh'
```

### 方案三：本地测试部署

在SSH连接恢复前，可以先在本地测试：

```bash
# 1. 启动本地前端开发服务器
cd frontend
npm run dev

# 2. 启动本地后端服务器
cd backend/SeeSharpBackend
dotnet run

# 3. 访问本地测试
# 前端: http://localhost:5173
# 后端: http://localhost:5000
```

## 🔍 诊断命令

### 网络连接测试
```bash
# 测试网络连通性
ping 8.155.57.140

# 测试端口连通性
telnet 8.155.57.140 22
nc -zv 8.155.57.140 22

# 端口扫描
nmap -p 22,80,443 8.155.57.140
```

### SSH连接测试
```bash
# 基本SSH连接
ssh root@8.155.57.140

# 详细调试信息
ssh -v root@8.155.57.140

# 指定超时时间
ssh -o ConnectTimeout=30 root@8.155.57.140
```

## 📋 部署检查清单

### 服务器端检查
- [ ] SSH服务已启动 (`systemctl status ssh`)
- [ ] 防火墙已配置 (`ufw status`)
- [ ] 安全组已开放端口
- [ ] 网络连接正常
- [ ] 系统已更新 (`apt update`)

### 本地端检查
- [ ] sshpass已安装 (`brew install sshpass`)
- [ ] 前端已构建 (`npm run build`)
- [ ] 后端已发布 (`dotnet publish`)
- [ ] SSL证书文件存在
- [ ] 部署脚本有执行权限

## 🚀 一旦SSH连接恢复

### 快速部署命令
```bash
# Debian系统专用部署
./deploy-debian.sh

# 或者通用部署
./deploy-seesharp.sh
```

### 验证部署
```bash
# 检查服务状态
ssh root@8.155.57.140 'systemctl status seesharpweb nginx'

# 测试网站访问
curl -I https://seesharp.alethealab.cn/seesharpweb/
curl -I https://seesharp.alethealab.cn/api/weatherforecast
```

## 🔧 常见问题解决

### 1. SSH连接超时
```bash
# 解决方案：
# 1. 检查云服务器安全组
# 2. 通过控制台VNC连接
# 3. 启动SSH服务：systemctl start ssh
```

### 2. 权限被拒绝
```bash
# 解决方案：
# 1. 确认用户名和密码
# 2. 检查SSH密钥配置
# 3. 重置服务器密码
```

### 3. 端口被占用
```bash
# 解决方案：
# 1. 检查端口占用：netstat -tlnp | grep :5000
# 2. 停止冲突服务：systemctl stop <service>
# 3. 修改配置端口
```

## 📞 技术支持

### 云服务器厂商支持
- **阿里云**: 95187
- **腾讯云**: 95716
- **华为云**: 4000-955-988

### 自助排查步骤
1. 登录云服务器控制台
2. 检查实例状态（运行中）
3. 检查安全组规则
4. 通过VNC连接服务器
5. 检查系统服务状态

## 🎯 部署目标确认

- **服务器**: 8.155.57.140 (Debian 12.10)
- **域名**: seesharp.alethealab.cn
- **技术栈**: Vue3 + .NET 9.0 + Nginx + HTTPS
- **访问地址**: https://seesharp.alethealab.cn/seesharpweb/

---

## 📝 下一步行动

1. **立即**: 通过云服务器控制台检查实例状态
2. **然后**: 通过VNC连接启动SSH服务
3. **最后**: 执行 `./deploy-debian.sh` 完成部署

一旦SSH连接恢复，整个部署过程只需要几分钟即可完成！
