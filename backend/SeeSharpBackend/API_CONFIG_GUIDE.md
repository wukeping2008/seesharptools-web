# AI功能API配置指南

## 📋 概述

本项目支持两种AI功能配置方式：
1. **本地配置文件** (推荐开发环境)
2. **环境变量** (推荐生产环境)

## 🔐 安全配置方案

### 方案一：本地配置文件 (开发环境推荐)

**步骤1**: 创建本地配置文件
```bash
# 在 backend/SeeSharpBackend/ 目录下创建
touch appsettings.Local.json
```

**步骤2**: 配置API密钥
```json
{
  "VolcesDeepseek": {
    "ApiKey": "your-api-key-here",
    "Url": "https://ark.cn-beijing.volces.com/api/v3/chat/completions",
    "Model": "deepseek-r1-250528",
    "MaxTokens": 16191
  }
}
```

**步骤3**: 验证配置
- 本地配置文件已在`.gitignore`中配置，不会被推送到GitHub
- 启动项目后访问 `/api/ai/status` 检查配置状态

### 方案二：环境变量 (生产环境推荐)

**Linux/macOS**:
```bash
export VOLCES_DEEPSEEK_APIKEY="your-api-key-here"
export VOLCES_DEEPSEEK_URL="https://ark.cn-beijing.volces.com/api/v3/chat/completions"
export VOLCES_DEEPSEEK_MODEL="deepseek-r1-250528"
export VOLCES_DEEPSEEK_MAXTOKENS="16191"
```

**Windows**:
```cmd
set VOLCES_DEEPSEEK_APIKEY=your-api-key-here
set VOLCES_DEEPSEEK_URL=https://ark.cn-beijing.volces.com/api/v3/chat/completions
set VOLCES_DEEPSEEK_MODEL=deepseek-r1-250528
set VOLCES_DEEPSEEK_MAXTOKENS=16191
```

## 🚀 功能特性

### ✅ 已启用功能
- **AI智能测试平台**: 自然语言生成测试代码
- **AI控件生成器**: 智能生成Vue组件
- **代码质量分析**: 自动代码质量评估
- **智能错误诊断**: AI辅助问题分析

### 📈 功能对比

| 功能模块 | 无API Key | 有API Key |
|---------|-----------|-----------|
| AI智能测试平台 | ✅ 基于规则生成 | ✅ AI增强生成 |
| AI控件生成器 | ✅ 预制模板 | ✅ 自定义AI生成 |
| 代码质量评估 | ✅ 基础检查 | ✅ 深度分析 |
| 测试模板推荐 | ✅ 固定模板 | ✅ 智能推荐 |

## 🔑 API密钥获取

### 火山引擎DeepSeek
1. 访问：https://console.volcengine.com/
2. 注册/登录火山引擎账号
3. 申请DeepSeek模型服务
4. 获取API Key

## ⚡ 配置优先级

1. **环境变量** (最高优先级)
2. **appsettings.Local.json**
3. **appsettings.json** (默认配置)

## 🛡️ 安全提醒

⚠️ **重要**: 
- 绝不将API密钥直接提交到Git仓库
- 使用本地配置文件或环境变量
- 生产环境建议使用密钥管理服务

## 🧪 测试配置

启动项目后访问以下端点验证配置：

```bash
# 检查API状态
curl http://localhost:5001/api/ai/status

# 测试AI控件生成
curl -X POST http://localhost:5001/api/ai/generate-control \
  -H "Content-Type: application/json" \
  -d '{"description": "温度仪表盘"}'
```

## 📞 技术支持

如遇配置问题，请查看：
- 后端日志文件
- 浏览器开发者工具Console
- API响应状态码和错误信息
