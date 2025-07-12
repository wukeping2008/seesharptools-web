# Claude API 配置指南

本文档说明如何配置Claude API以启用真实的AI控件生成功能。

## 1. 获取Claude API密钥

1. 访问 [Anthropic Console](https://console.anthropic.com/)
2. 注册或登录您的账户
3. 在API Keys部分创建新的API密钥
4. 复制生成的密钥

## 2. 配置后端服务

### 方法一：修改appsettings.json（不推荐用于生产环境）

编辑 `backend/SeeSharpBackend/appsettings.json` 文件：

```json
{
  // ... 其他配置
  "Claude": {
    "ApiKey": "YOUR_CLAUDE_API_KEY_HERE",
    "ApiUrl": "https://api.anthropic.com/v1/messages"
  }
}
```

### 方法二：使用环境变量（推荐）

设置环境变量：

**Windows (PowerShell):**
```powershell
$env:Claude__ApiKey="YOUR_CLAUDE_API_KEY_HERE"
```

**macOS/Linux:**
```bash
export Claude__ApiKey="YOUR_CLAUDE_API_KEY_HERE"
```

### 方法三：使用用户机密（开发环境推荐）

在后端项目目录运行：

```bash
cd backend/SeeSharpBackend
dotnet user-secrets init
dotnet user-secrets set "Claude:ApiKey" "YOUR_CLAUDE_API_KEY_HERE"
```

## 3. 验证配置

1. 启动后端服务：
   ```bash
   cd backend/SeeSharpBackend
   dotnet run
   ```

2. 启动前端服务：
   ```bash
   cd frontend
   npm run dev
   ```

3. 访问AI控件生成器页面：http://localhost:5182/ai-control-generator

4. 输入控件描述，例如：
   - "创建一个温度显示仪表，范围0-100度"
   - "设计一个带有红绿黄三色的状态指示灯"
   - "生成一个工业风格的启动/停止按钮"

## 4. API使用限制

- Claude API有速率限制，请参考[官方文档](https://docs.anthropic.com/claude/reference/rate-limits)
- 每次生成会消耗token，注意控制使用量
- 建议在生产环境中实现缓存机制

## 5. 故障排除

### 常见错误

1. **"Claude API密钥未配置"**
   - 确保已正确设置API密钥
   - 检查环境变量是否生效

2. **"AI服务错误: 401"**
   - API密钥无效或过期
   - 检查密钥是否正确复制

3. **"AI服务错误: 429"**
   - 达到速率限制
   - 稍后重试或升级API计划

### 备用方案

如果Claude API不可用，系统会自动使用预定义模板：
- 仪表盘控件
- LED指示灯
- 按钮控件

这确保了即使没有API密钥，基本功能仍然可用。

## 6. 安全建议

1. **不要将API密钥提交到版本控制**
   - 使用 `.gitignore` 排除包含密钥的文件
   - 使用环境变量或用户机密

2. **在生产环境中使用密钥管理服务**
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault

3. **实施API调用限制**
   - 添加用户认证
   - 实现速率限制
   - 记录API使用情况

## 7. 成本控制

Claude API按token计费，建议：
- 监控API使用量
- 实现响应缓存
- 限制生成长度
- 为常见请求使用模板

## 相关链接

- [Claude API文档](https://docs.anthropic.com/claude/reference/getting-started-with-the-api)
- [定价信息](https://www.anthropic.com/api-pricing)
- [最佳实践](https://docs.anthropic.com/claude/docs/best-practices)
