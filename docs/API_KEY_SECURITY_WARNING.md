# ⚠️ API密钥安全警告

## 重要提示

如果您的API密钥已经在公开场合（如聊天记录、GitHub、论坛等）暴露，请立即：

1. **登录 [Anthropic Console](https://console.anthropic.com/)**
2. **撤销已暴露的API密钥**
3. **创建新的API密钥**

## 安全配置API密钥的方法

### 方法1：使用dotnet用户机密（推荐用于开发）

```bash
# 1. 进入后端项目目录
cd backend/SeeSharpBackend

# 2. 初始化用户机密（已完成）
dotnet user-secrets init

# 3. 设置API密钥（请替换YOUR_NEW_API_KEY）
dotnet user-secrets set "Claude:ApiKey" "YOUR_NEW_API_KEY"
```

### 方法2：使用环境变量

**macOS/Linux:**
```bash
export Claude__ApiKey="YOUR_NEW_API_KEY"
```

**Windows PowerShell:**
```powershell
$env:Claude__ApiKey="YOUR_NEW_API_KEY"
```

### 方法3：使用.env文件（需要额外配置）

1. 创建 `.env` 文件（确保已添加到 `.gitignore`）
2. 添加：`Claude__ApiKey=YOUR_NEW_API_KEY`
3. 使用 dotenv 库加载环境变量

## 安全最佳实践

1. **永远不要**将API密钥硬编码在代码中
2. **永远不要**将API密钥提交到版本控制系统
3. **永远不要**在公开场合分享API密钥
4. **定期轮换**API密钥
5. **使用最小权限原则**
6. **监控API使用情况**

## 如果密钥已泄露

1. 立即在Anthropic Console撤销密钥
2. 生成新密钥
3. 检查API使用记录是否有异常
4. 更新所有使用该密钥的应用

## 生产环境建议

- 使用密钥管理服务（如Azure Key Vault、AWS Secrets Manager）
- 实施API网关和速率限制
- 添加身份验证和授权层
- 记录和监控所有API调用

记住：API密钥就像密码一样重要，请妥善保管！
