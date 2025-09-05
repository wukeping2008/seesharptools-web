export default {
  common: {
    language: '语言',
    switchLanguage: '切换语言',
    loading: '加载中...',
    error: '错误',
    success: '成功',
    cancel: '取消',
    confirm: '确认',
    save: '保存',
    delete: '删除',
    edit: '编辑',
    add: '添加',
    search: '搜索',
    reset: '重置',
    export: '导出',
    import: '导入',
    download: '下载',
    upload: '上传'
  },
  nav: {
    solutionGenerator: '解决方案生成器',
    realtimeDashboard: '实时仪表板',
    home: '首页',
    about: '关于'
  },
  hero: {
    title: '简仪模块化仪器C#解决方案生成器',
    subtitle: '基于简仪科技SeeSharp平台，为模块化测试测量仪器快速生成专业C#应用程序',
    solutionGeneratorTitle: '简仪模块化仪器C#解决方案生成器',
    solutionGeneratorSubtitle: '基于简仪科技SeeSharp平台，为模块化测试测量仪器快速生成专业C#应用程序',
    dashboardTitle: '仪器数据实时监控中心',
    dashboardSubtitle: '实时监控和可视化JYTEK模块化仪器的测量数据流'
  },
  generator: {
    title: '生成您的解决方案',
    description: '描述您的测试测量需求，智能生成符合MISD标准的JYTEK模块化仪器C#控制程序',
    requirements: '项目需求',
    requirementsPlaceholder: '示例：创建一个JYUSB-1601数据采集程序，通道0，采样率1000Hz...',
    aiModel: 'AI模型',
    generateButton: '生成解决方案',
    generating: '正在生成解决方案...',
    errorEmpty: '请输入您的项目需求',
    successGenerated: '解决方案生成成功！下载已开始。',
    errorGeneration: '生成解决方案失败，请重试。',
    charCount: '{count} / 5000 字符',
    models: {
      speedRecommended: 'ERNIE Speed 128K（快速高效）',
      turboRecommended: 'ERNIE 4.5 Turbo（最佳质量）',
      x1Turbo: 'ERNIE X1 Turbo 32K',
      lite: 'ERNIE Lite 8K（简单任务）'
    },
    modelGroups: {
      recommended: '推荐',
      other: '其他模型'
    }
  },
  features: {
    title: '功能与能力',
    misd: {
      title: 'MISD标准兼容',
      description: '遵循模块化仪器软件字典(MISD)国际标准，确保代码规范性和可移植性'
    },
    jytek: {
      title: 'JYTEK平台深度集成',
      description: '全面支持JYUSB、JYPXI等全系列简仪模块化仪器产品'
    },
    realtime: {
      title: '智能快速生成',
      description: '基于SeeSharp平台和AI技术，秒级生成专业测试测量应用程序'
    },
    deploy: {
      title: '即刻可用',
      description: '生成完整的.NET解决方案，包含驱动、接口和示例代码，直接部署运行'
    }
  },
  dashboard: {
    connectionStatus: '连接状态',
    connected: '已连接',
    disconnected: '未连接',
    connect: '连接',
    disconnect: '断开',
    server: '服务器',
    uptime: '运行时间',
    messages: '消息数',
    liveDataStream: '实时数据流',
    clear: '清除',
    noData: '尚未收到数据。连接以开始接收数据。',
    statistics: '统计信息',
    average: '平均值',
    maximum: '最大值',
    minimum: '最小值',
    dataPoints: '数据点',
    controlPanel: '控制面板',
    sampleRate: '采样率（Hz）',
    bufferSize: '缓冲区大小',
    channel: '通道',
    autoScroll: '自动滚动'
  },
  footer: {
    poweredBy: '© 简仪科技 JYTEK | 基于SeeSharp平台 | Powered by Baidu AI & ASP.NET Core'
  }
};