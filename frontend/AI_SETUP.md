# AI智能控件生成器配置指南

## 🚀 快速开始

AI智能控件生成器需要Claude API密钥才能正常工作。请按照以下步骤进行配置：

### 1. 获取Claude API密钥

1. 访问 [Anthropic Console](https://console.anthropic.com/)
2. 注册或登录您的账户
3. 创建新的API密钥
4. 复制生成的API密钥

### 2. 配置环境变量

1. 在项目根目录创建 `.env` 文件：
```bash
cp .env.example .env
```

2. 编辑 `.env` 文件，设置您的API密钥：
```env
VITE_CLAUDE_API_KEY=your_actual_claude_api_key_here
```

### 3. 启动项目

```bash
npm run dev
```

## 🔧 配置说明

### 环境变量

- `VITE_CLAUDE_API_KEY`: Claude API密钥（必需）
- `VITE_APP_TITLE`: 应用标题
- `VITE_APP_VERSION`: 应用版本
- `VITE_API_BASE_URL`: 后端API地址
- `VITE_WS_URL`: WebSocket地址

### API配置

AI控件生成器使用以下配置：

- **模型**: `claude-sonnet-4-20250514` (最新Claude模型)
- **最大Token数**: 4000
- **温度**: 0.7 (平衡创造性和准确性)
- **Top-P**: 0.9
- **请求限制**: 10次/分钟
- **缓存**: 启用，1小时TTL

## 🎯 使用方法

1. **访问AI生成器**: 在主页点击"AI智能控件生成器"卡片
2. **描述需求**: 用自然语言描述想要的控件功能
3. **配置选项**: 选择控件类型、样式风格等
4. **生成控件**: 点击"生成控件"按钮
5. **查看结果**: 在多个标签页中查看生成的代码
6. **下载使用**: 复制代码或下载文件到项目中

## 🔒 安全注意事项

- **不要提交API密钥**: `.env` 文件已添加到 `.gitignore`
- **保护API密钥**: 不要在代码中硬编码API密钥
- **监控使用量**: 定期检查API使用量和费用
- **限制访问**: 在生产环境中限制API访问权限

## 🐛 故障排除

### API密钥无效
- 检查API密钥是否正确复制
- 确认API密钥是否有效且未过期
- 检查账户是否有足够的额度

### 生成失败
- 检查网络连接
- 确认API服务是否正常
- 查看浏览器控制台错误信息

### 功能降级
如果API调用失败，系统会自动回退到模拟模式，生成示例代码。

## 📞 技术支持

如果遇到问题，请：

1. 检查浏览器控制台错误信息
2. 查看网络请求状态
3. 确认环境变量配置正确
4. 联系技术支持团队

## 🎉 功能特性

- 🧠 **自然语言理解**: 用中文描述控件需求
- ⚡ **秒级代码生成**: 快速生成完整控件代码
- 🎯 **专业品质**: 符合Vue 3最佳实践
- 📊 **智能评估**: 可行性评分和改进建议
- 💾 **一键下载**: 批量下载生成的文件
- 📈 **历史记录**: 保存生成历史和统计数据

---

**注意**: 这是一个实验性功能，生成的代码仅供参考，请在使用前进行适当的测试和验证。
