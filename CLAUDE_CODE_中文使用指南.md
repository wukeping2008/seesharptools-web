# Claude Code 中文使用指南

## 🎯 快速开始

### 1. 安装 Claude Code
```bash
npm install -g @anthropic/claude-code
# 或者
yarn global add @anthropic/claude-code
```

### 2. 配置 API 密钥
```bash
claude-code config set apiKey YOUR_API_KEY
```

### 3. 启动 Claude Code
```bash
# 在项目目录中运行
claude-code
```

## 💬 常用中文命令

### 基础命令
- `帮助` - 显示帮助信息
- `清屏` - 清除屏幕内容
- `退出` - 退出 Claude Code

### 开发命令
- `创建文件 [文件名]` - 创建新文件
- `编辑文件 [文件名]` - 编辑现有文件
- `删除文件 [文件名]` - 删除文件
- `运行测试` - 执行项目测试
- `构建项目` - 构建项目

### Git 命令
- `查看状态` - 查看 Git 状态
- `提交代码` - 提交更改
- `推送代码` - 推送到远程仓库
- `拉取代码` - 从远程仓库拉取

## 🛠️ 中文界面配置

### 设置文件位置
`.claude/settings.local.json`

### 配置示例
```json
{
  "language": {
    "locale": "zh-CN",
    "display": "中文（简体）"
  },
  "ui": {
    "theme": "dark",
    "fontSize": "medium",
    "showLineNumbers": true,
    "wordWrap": true
  }
}
```

## 📝 中文提示词最佳实践

### 1. 清晰的需求描述
```
错误示例：帮我写代码
正确示例：请帮我创建一个 React 组件，用于显示用户列表，包含搜索和分页功能
```

### 2. 具体的技术栈
```
请使用 Vue 3 + TypeScript + Pinia 创建一个购物车管理系统
```

### 3. 明确的输出要求
```
请生成以下内容：
1. 组件代码
2. 单元测试
3. 使用文档
4. API 接口定义
```

## 🔧 常见问题解决

### 1. 中文显示乱码
- 检查终端编码设置为 UTF-8
- Windows 用户运行：`chcp 65001`

### 2. 中文输入问题
- 确保输入法切换正常
- 使用支持中文的终端（如 Windows Terminal）

### 3. API 响应慢
- 检查网络连接
- 考虑使用代理服务器

## 🎨 自定义中文提示

### 创建自定义提示文件
`.claude/prompts.zh-CN.json`

```json
{
  "templates": {
    "创建组件": "请创建一个 {framework} 组件，名称为 {name}，功能是 {description}",
    "修复错误": "请修复以下错误：{error}，文件位置：{file}",
    "优化代码": "请优化 {file} 文件中的代码，重点关注 {aspect}",
    "添加测试": "为 {component} 组件添加单元测试，覆盖率要求 {coverage}%"
  }
}
```

## 📊 项目模板（中文）

### React 项目
```bash
claude-code create react-app --template chinese --name 我的应用
```

### Vue 项目
```bash
claude-code create vue-app --template chinese --name 我的应用
```

### Node.js 项目
```bash
claude-code create node-app --template chinese --name 我的服务
```

## 🚀 高级功能

### 1. 批量操作
```bash
# 批量重命名文件
claude-code batch rename "*.js" "*.ts"

# 批量格式化代码
claude-code batch format "src/**/*.{js,ts,jsx,tsx}"
```

### 2. 代码审查
```bash
# 审查最近的提交
claude-code review --commits 5 --lang zh-CN

# 审查特定文件
claude-code review --file src/components/*.vue --lang zh-CN
```

### 3. 文档生成
```bash
# 生成中文 API 文档
claude-code docs generate --lang zh-CN --format markdown

# 生成中文用户手册
claude-code docs manual --lang zh-CN --output docs/manual.md
```

## 📚 快捷键

| 快捷键 | 功能 |
|--------|------|
| Ctrl+C | 复制/中断 |
| Ctrl+V | 粘贴 |
| Ctrl+Z | 撤销 |
| Ctrl+L | 清屏 |
| Tab | 自动补全 |
| ↑/↓ | 历史命令 |

## 🔗 相关资源

- [官方文档](https://docs.anthropic.com/claude-code)
- [GitHub 仓库](https://github.com/anthropics/claude-code)
- [问题反馈](https://github.com/anthropics/claude-code/issues)
- [中文社区](https://forum.claude-code.cn)

## 💡 提示与技巧

1. **使用中文变量名**：Claude Code 完全支持中文变量名和注释
2. **智能补全**：输入中文描述后按 Tab 键可获得代码建议
3. **上下文记忆**：Claude Code 会记住对话历史，可以引用之前的内容
4. **多文件操作**：支持同时编辑多个文件，使用 `@文件名` 引用
5. **代码解释**：使用 `解释 [代码片段]` 获取中文代码解释

## 📮 获取帮助

- 命令行输入：`/help` 或 `帮助`
- 反馈问题：https://github.com/anthropics/claude-code/issues
- 中文文档：本文档
- 社区支持：加入中文用户群组

---

**注意**：Claude Code 的中文支持正在持续改进中，如遇到问题请及时反馈。