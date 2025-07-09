# Claude API 真实调用配置指南

## 问题说明

当前AI控件生成器使用智能模拟生成，而非真实的Claude API调用，原因是浏览器CORS限制。

## 解决方案

### 方案1：Node.js后端代理服务

1. **创建后端API服务**
```bash
# 在项目根目录创建后端服务
mkdir seesharp-api
cd seesharp-api
npm init -y
npm install express cors dotenv @anthropic-ai/sdk
```

2. **创建代理服务器**
```javascript
// server.js
const express = require('express');
const cors = require('cors');
const Anthropic = require('@anthropic-ai/sdk');
require('dotenv').config();

const app = express();
const port = 3001;

app.use(cors());
app.use(express.json());

const anthropic = new Anthropic({
  apiKey: process.env.CLAUDE_API_KEY,
});

app.post('/api/generate-control', async (req, res) => {
  try {
    const { description, type, style } = req.body;
    
    const prompt = `你是一个专业的Vue 3 + TypeScript控件开发专家。请根据用户需求生成一个完整的专业控件。

用户需求: ${description}
控件类型: ${type}
样式风格: ${style}

请生成一个专业的工业级控件，包含以下四个部分：
1. Vue 3组件代码 - 使用Composition API
2. TypeScript类型定义 - 完整的类型支持  
3. SCSS样式代码 - 专业工业风格
4. 使用示例代码 - 完整的使用演示

请严格按照以下JSON格式返回：
{
  "componentCode": "完整的Vue组件代码",
  "typeDefinitions": "完整的TypeScript类型定义", 
  "styleCode": "完整的SCSS样式代码",
  "exampleCode": "完整的使用示例代码"
}`;

    const message = await anthropic.messages.create({
      model: 'claude-3-sonnet-20240229',
      max_tokens: 4000,
      temperature: 0.7,
      messages: [
        {
          role: 'user',
          content: prompt
        }
      ]
    });

    res.json({
      success: true,
      content: message.content[0].text
    });
  } catch (error) {
    console.error('Claude API Error:', error);
    res.status(500).json({
      success: false,
      error: error.message
    });
  }
});

app.listen(port, () => {
  console.log(`Claude API代理服务运行在 http://localhost:${port}`);
});
```

3. **修改前端API调用**
```typescript
// 修改 AIControlService.ts 中的 callAIAPI 方法
private async callAIAPI(prompt: string): Promise<string> {
  try {
    const response = await fetch('http://localhost:3001/api/generate-control', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        description: this.extractDescription(prompt),
        type: this.extractType(prompt),
        style: 'professional'
      })
    });

    if (!response.ok) {
      throw new Error(`API请求失败: ${response.status}`);
    }

    const data = await response.json();
    return data.content;
  } catch (error) {
    console.error('Claude API调用失败:', error);
    // 回退到智能模拟
    return this.generateIntelligentMockResponse(prompt);
  }
}
```

### 方案2：Vercel/Netlify无服务器函数

1. **创建API函数**
```javascript
// api/generate-control.js (Vercel)
import Anthropic from '@anthropic-ai/sdk';

const anthropic = new Anthropic({
  apiKey: process.env.CLAUDE_API_KEY,
});

export default async function handler(req, res) {
  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method not allowed' });
  }

  try {
    const { description, type, style } = req.body;
    
    const message = await anthropic.messages.create({
      model: 'claude-3-sonnet-20240229',
      max_tokens: 4000,
      temperature: 0.7,
      messages: [
        {
          role: 'user',
          content: `生成Vue 3控件: ${description}`
        }
      ]
    });

    res.json({
      success: true,
      content: message.content[0].text
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message
    });
  }
}
```

## 部署步骤

### 本地开发
1. 启动后端代理服务：`node server.js`
2. 启动前端开发服务器：`npm run dev`
3. 测试Claude API调用

### 生产部署
1. 将后端服务部署到云平台（Vercel、Railway、Heroku等）
2. 更新前端API_BASE_URL环境变量
3. 配置CORS和环境变量

## 环境变量配置

```bash
# 后端 .env
CLAUDE_API_KEY=your_claude_api_key_here
PORT=3001

# 前端 .env
VITE_API_BASE_URL=http://localhost:3001
VITE_CLAUDE_API_KEY=  # 移除，改为后端调用
```

## 测试验证

1. **API连通性测试**
```bash
curl -X POST http://localhost:3001/api/generate-control \
  -H "Content-Type: application/json" \
  -d '{"description":"温度显示控件","type":"gauge","style":"professional"}'
```

2. **前端集成测试**
- 在AI控件生成器中输入需求
- 检查网络请求是否发送到后端
- 验证返回的是Claude生成的内容

## 预期效果

实现后端代理后，AI控件生成器将：
- 真正调用Claude API
- 根据具体需求生成定制化控件
- 提供更智能、更准确的代码生成
- 支持复杂的自然语言理解
