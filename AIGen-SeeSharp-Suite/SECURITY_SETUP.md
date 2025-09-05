# 安全配置说明 / Security Setup Guide

## ⚠️ 重要安全提醒 / Important Security Notice

**永远不要将API密钥提交到Git仓库！**
**NEVER commit API keys to Git repository!**

## 配置步骤 / Setup Steps

### 1. 配置百度AI密钥 / Configure Baidu AI Token

在 `backend/AIGenSeeSharpSuite.Backend/` 目录下创建 `appsettings.Development.json` 文件：

Create `appsettings.Development.json` in `backend/AIGenSeeSharpSuite.Backend/` directory:

```json
{
  "BaiduAiBearerToken": "YOUR_ACTUAL_TOKEN_HERE",
  "BaiduAI": {
    "BearerToken": "YOUR_ACTUAL_TOKEN_HERE"
  }
}
```

### 2. 获取百度AI Token / Get Baidu AI Token

1. 访问 [百度千帆大模型平台](https://qianfan.baidubce.com/)
2. 注册并创建应用
3. 获取API Key和Secret Key
4. 生成Bearer Token

### 3. 环境变量配置（推荐）/ Environment Variables (Recommended)

您也可以使用环境变量来配置密钥：

You can also use environment variables:

```bash
# Windows
set BaiduAI__BearerToken=YOUR_TOKEN_HERE

# Linux/Mac
export BaiduAI__BearerToken=YOUR_TOKEN_HERE
```

### 4. 验证配置 / Verify Configuration

运行后端服务测试配置是否正确：

Run backend service to test configuration:

```bash
cd backend/AIGenSeeSharpSuite.Backend
dotnet run
```

## 安全最佳实践 / Security Best Practices

1. ✅ 使用 `appsettings.Development.json` 存储本地密钥
2. ✅ 确保 `.gitignore` 包含所有敏感文件
3. ✅ 定期轮换API密钥
4. ✅ 使用环境变量在生产环境
5. ✅ 限制API密钥的权限范围

## 已包含的.gitignore规则 / Included .gitignore Rules

```
appsettings.Development.json
appsettings.Local.json
appsettings.*.json
!appsettings.json
secrets.json
*.key
```

## 如果密钥已泄露 / If Keys Are Leaked

1. 立即在百度AI平台重置密钥
2. 更新本地配置文件
3. 检查并清理Git历史记录
4. 考虑使用密钥管理服务

---

**记住：安全第一！/ Remember: Security First!**