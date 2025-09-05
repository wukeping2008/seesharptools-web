# 项目结构优化计划

## 已完成的清理工作
- [x] 删除临时Python测试文件
- [x] 删除WeatherForecast模板代码
- [ ] 合并usb1601-web-app到主项目
- [ ] 整理驱动文件到专门目录

## 新的项目结构规划
```
seesharptools-web/
├── frontend/               # Vue前端应用
├── backend/               # ASP.NET后端服务
├── drivers/               # 硬件驱动文件集中管理
│   ├── JYUSB1601/
│   └── JY5500/
├── services/              # 微服务
│   └── csharp-runner/     # C#代码执行服务
├── tests/                 # 测试文件
│   ├── unit/
│   └── integration/
├── docs/                  # 文档
└── config/                # 配置文件
```

## 驱动文件迁移
- 将根目录的JYUSB1601.dll移至drivers目录
- 统一管理所有硬件驱动依赖