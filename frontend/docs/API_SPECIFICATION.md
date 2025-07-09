# 📡 SeeSharpTools Web API 接口规范

## 概述
本文档定义了SeeSharpTools Web平台前后端之间的API接口规范，包括RESTful API和WebSocket实时通信接口。

## 🌐 API设计原则

### 1. RESTful设计
- 使用标准HTTP方法（GET、POST、PUT、DELETE）
- 资源导向的URL设计
- 统一的响应格式
- 适当的HTTP状态码

### 2. 版本控制
- API版本通过URL路径控制：`/api/v1/`
- 向后兼容性保证
- 废弃API的优雅过渡

### 3. 安全性
- JWT Token认证
- HTTPS加密传输
- 请求限流和防护
- 输入验证和XSS防护

### 4. 性能优化
- 响应数据压缩
- 分页查询支持
- 缓存策略
- 异步处理

## 🔐 认证和授权

### JWT Token格式
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user_id",
    "username": "admin",
    "role": "administrator",
    "permissions": ["device:read", "device:write", "data:read"],
    "exp": 1640995200,
    "iat": 1640908800
  }
}
```

### 权限级别
```typescript
enum UserRole {
  ADMIN = 'administrator',      // 系统管理员
  ENGINEER = 'engineer',        // 工程师
  OPERATOR = 'operator',        // 操作员
  VIEWER = 'viewer'            // 只读用户
}

enum Permission {
  // 设备权限
  DEVICE_READ = 'device:read',
  DEVICE_WRITE = 'device:write',
  DEVICE_CONTROL = 'device:control',
  
  // 数据权限
  DATA_READ = 'data:read',
  DATA_WRITE = 'data:write',
  DATA_DELETE = 'data:delete',
  
  // 配置权限
  CONFIG_READ = 'config:read',
  CONFIG_WRITE = 'config:write',
  
  // AI权限
  AI_GENERATE = 'ai:generate',
  AI_DEPLOY = 'ai:deploy',
  
  // 系统权限
  SYSTEM_ADMIN = 'system:admin',
  USER_MANAGE = 'user:manage'
}
```

## 📋 通用响应格式

### 成功响应
```typescript
interface ApiResponse<T> {
  success: true
  data: T
  message?: string
  timestamp: string
  requestId: string
}
```

### 错误响应
```typescript
interface ApiError {
  success: false
  error: {
    code: string
    message: string
    details?: any
  }
  timestamp: string
  requestId: string
}
```

### 分页响应
```typescript
interface PaginatedResponse<T> {
  success: true
  data: T[]
  pagination: {
    page: number
    pageSize: number
    total: number
    totalPages: number
    hasNext: boolean
    hasPrev: boolean
  }
  timestamp: string
  requestId: string
}
```

## 🔧 设备管理API

### 1. 设备发现和枚举

#### GET /api/v1/devices
获取所有已连接的设备列表

**请求参数：**
```typescript
interface DeviceListQuery {
  type?: string          // 设备类型过滤
  status?: 'online' | 'offline' | 'error'
  page?: number
  pageSize?: number
}
```

**响应：**
```typescript
interface Device {
  id: string
  name: string
  type: 'daq' | 'signal_generator' | 'oscilloscope' | 'dmm' | 'temperature' | 'dio' | 'switch'
  model: string
  serialNumber: string
  status: 'online' | 'offline' | 'error' | 'busy'
  capabilities: DeviceCapabilities
  lastSeen: string
  configuration?: DeviceConfiguration
}

interface DeviceCapabilities {
  channels: number
  sampleRateRange: [number, number]  // [min, max] in Hz
  voltageRange: [number, number]     // [min, max] in V
  resolution: number                 // bits
  features: string[]
}
```

#### GET /api/v1/devices/{deviceId}
获取特定设备的详细信息

**响应：**
```typescript
interface DeviceDetail extends Device {
  hardware: {
    firmware: string
    driver: string
    calibrationDate: string
    temperature: number
    voltage: number
  }
  statistics: {
    uptime: number
    dataPoints: number
    errors: number
  }
}
```

### 2. 设备配置

#### PUT /api/v1/devices/{deviceId}/config
更新设备配置

**请求体：**
```typescript
interface DeviceConfiguration {
  channels: ChannelConfig[]
  sampling: SamplingConfig
  trigger: TriggerConfig
  calibration?: CalibrationConfig
}

interface ChannelConfig {
  id: number
  enabled: boolean
  name: string
  range: number          // 量程 (V)
  coupling: 'AC' | 'DC' | 'GND'
  impedance: number      // 输入阻抗 (Ohm)
  offset: number         // 偏置 (V)
}

interface SamplingConfig {
  rate: number           // 采样率 (Hz)
  points: number         // 采样点数
  mode: 'continuous' | 'finite'
  bufferSize: number
}

interface TriggerConfig {
  type: 'immediate' | 'edge' | 'level' | 'external'
  source: string
  level: number
  slope: 'rising' | 'falling'
  delay: number          // 预触发延时 (s)
}
```

#### POST /api/v1/devices/{deviceId}/calibrate
执行设备校准

**请求体：**
```typescript
interface CalibrationRequest {
  type: 'zero' | 'full_scale' | 'auto'
  channels: number[]
  reference?: {
    voltage: number
    frequency: number
  }
}
```

### 3. 设备控制

#### POST /api/v1/devices/{deviceId}/start
启动设备

#### POST /api/v1/devices/{deviceId}/stop
停止设备

#### POST /api/v1/devices/{deviceId}/reset
重置设备

## 📊 数据采集API

### 1. 实时数据流

#### POST /api/v1/data/acquisition/start
启动数据采集

**请求体：**
```typescript
interface AcquisitionRequest {
  deviceId: string
  configuration: AcquisitionConfig
  storage?: StorageConfig
}

interface AcquisitionConfig {
  channels: number[]
  sampleRate: number
  duration?: number      // 采集时长 (s)，不设置为连续采集
  trigger: TriggerConfig
  preprocessing?: {
    filter: FilterConfig
    decimation: number
  }
}

interface StorageConfig {
  enabled: boolean
  format: 'binary' | 'csv' | 'hdf5'
  compression: boolean
  retention: number      // 保留天数
}
```

#### GET /api/v1/data/acquisition/{sessionId}/status
获取采集状态

**响应：**
```typescript
interface AcquisitionStatus {
  sessionId: string
  deviceId: string
  status: 'starting' | 'running' | 'stopping' | 'stopped' | 'error'
  startTime: string
  duration: number
  samplesCollected: number
  dataRate: number       // 实际数据率 (Hz)
  bufferUsage: number    // 缓冲区使用率 (%)
  errors: number
}
```

### 2. 历史数据查询

#### GET /api/v1/data/history
查询历史数据

**请求参数：**
```typescript
interface HistoryQuery {
  deviceId: string
  channels: number[]
  startTime: string      // ISO 8601格式
  endTime: string
  sampleRate?: number    // 重采样率
  aggregation?: 'none' | 'avg' | 'min' | 'max' | 'rms'
  format?: 'json' | 'csv' | 'binary'
  maxPoints?: number     // 最大返回点数
}
```

**响应：**
```typescript
interface HistoryData {
  deviceId: string
  channels: ChannelData[]
  timeRange: [string, string]
  sampleRate: number
  totalPoints: number
}

interface ChannelData {
  id: number
  name: string
  unit: string
  data: number[]
  timestamps: string[]
  statistics: {
    min: number
    max: number
    avg: number
    rms: number
    std: number
  }
}
```

### 3. 数据导出

#### POST /api/v1/data/export
导出数据

**请求体：**
```typescript
interface ExportRequest {
  query: HistoryQuery
  format: 'csv' | 'excel' | 'matlab' | 'labview'
  options: {
    includeHeaders: boolean
    timeFormat: 'iso' | 'timestamp' | 'relative'
    compression: boolean
  }
}
```

**响应：**
```typescript
interface ExportResponse {
  taskId: string
  estimatedSize: number
  estimatedTime: number
}
```

#### GET /api/v1/data/export/{taskId}/status
获取导出状态

#### GET /api/v1/data/export/{taskId}/download
下载导出文件

## 🤖 AI控件生成API

### 1. 控件生成

#### POST /api/v1/ai/generate-control
生成自定义控件

**请求体：**
```typescript
interface ControlGenerationRequest {
  description: string
  controlType?: 'auto' | 'gauge' | 'chart' | 'indicator' | 'input'
  requirements: {
    dataBinding: boolean
    realtime: boolean
    responsive: boolean
    accessibility: boolean
  }
  constraints: {
    maxComplexity: 'low' | 'medium' | 'high'
    performance: 'standard' | 'high'
    compatibility: 'legacy' | 'modern'
  }
  context?: {
    existingControls: string[]
    dataTypes: string[]
    theme: string
  }
}
```

**响应：**
```typescript
interface ControlGenerationResponse {
  taskId: string
  estimatedTime: number
  status: 'queued' | 'processing' | 'completed' | 'failed'
}
```

#### GET /api/v1/ai/generate-control/{taskId}
获取生成结果

**响应：**
```typescript
interface GeneratedControl {
  id: string
  name: string
  description: string
  code: {
    template: string
    script: string
    style: string
    types: string
  }
  metadata: {
    complexity: number
    performance: number
    accessibility: number
    compatibility: string[]
  }
  preview: {
    thumbnail: string
    demo: string
  }
  validation: {
    syntax: boolean
    security: boolean
    performance: boolean
    issues: ValidationIssue[]
  }
}

interface ValidationIssue {
  type: 'error' | 'warning' | 'info'
  message: string
  line?: number
  suggestion?: string
}
```

### 2. 控件模板管理

#### GET /api/v1/ai/templates
获取控件模板列表

#### POST /api/v1/ai/templates
保存控件模板

#### PUT /api/v1/ai/templates/{templateId}
更新控件模板

#### DELETE /api/v1/ai/templates/{templateId}
删除控件模板

## 👤 用户管理API

### 1. 用户认证

#### POST /api/v1/auth/login
用户登录

**请求体：**
```typescript
interface LoginRequest {
  username: string
  password: string
  rememberMe?: boolean
}
```

**响应：**
```typescript
interface LoginResponse {
  token: string
  refreshToken: string
  user: UserInfo
  expiresIn: number
}

interface UserInfo {
  id: string
  username: string
  email: string
  role: UserRole
  permissions: Permission[]
  profile: {
    firstName: string
    lastName: string
    avatar?: string
    department?: string
  }
  preferences: {
    language: string
    theme: string
    timezone: string
  }
}
```

#### POST /api/v1/auth/refresh
刷新Token

#### POST /api/v1/auth/logout
用户登出

### 2. 用户管理

#### GET /api/v1/users
获取用户列表（需要管理员权限）

#### POST /api/v1/users
创建用户

#### PUT /api/v1/users/{userId}
更新用户信息

#### DELETE /api/v1/users/{userId}
删除用户

## ⚙️ 系统配置API

### 1. 系统设置

#### GET /api/v1/system/config
获取系统配置

**响应：**
```typescript
interface SystemConfig {
  general: {
    siteName: string
    version: string
    timezone: string
    language: string
  }
  hardware: {
    maxDevices: number
    maxChannels: number
    maxSampleRate: number
    bufferSize: number
  }
  performance: {
    maxConcurrentUsers: number
    dataRetention: number
    cacheSize: number
    compressionLevel: number
  }
  security: {
    sessionTimeout: number
    passwordPolicy: PasswordPolicy
    twoFactorAuth: boolean
  }
  ai: {
    enabled: boolean
    provider: string
    maxRequests: number
    timeout: number
  }
}
```

#### PUT /api/v1/system/config
更新系统配置

### 2. 系统监控

#### GET /api/v1/system/health
系统健康检查

**响应：**
```typescript
interface HealthStatus {
  status: 'healthy' | 'degraded' | 'unhealthy'
  timestamp: string
  services: {
    database: ServiceHealth
    cache: ServiceHealth
    messageQueue: ServiceHealth
    storage: ServiceHealth
    ai: ServiceHealth
  }
  metrics: {
    cpu: number
    memory: number
    disk: number
    network: number
  }
}

interface ServiceHealth {
  status: 'up' | 'down' | 'degraded'
  responseTime: number
  lastCheck: string
  error?: string
}
```

#### GET /api/v1/system/metrics
系统性能指标

## 🔄 WebSocket实时通信

### 连接建立
```typescript
// 连接URL
const wsUrl = 'wss://api.seesharptools.com/ws'

// 连接参数
interface ConnectionParams {
  token: string
  clientId: string
  subscriptions: string[]
}
```

### 消息格式
```typescript
interface WebSocketMessage {
  type: string
  id: string
  timestamp: string
  data: any
}
```

### 1. 实时数据订阅

#### 订阅设备数据
```typescript
// 发送订阅消息
{
  type: 'subscribe',
  id: 'sub_001',
  data: {
    topic: 'device_data',
    deviceId: 'daq_001',
    channels: [0, 1, 2, 3],
    sampleRate: 1000
  }
}

// 接收数据消息
{
  type: 'data',
  id: 'data_001',
  timestamp: '2025-01-08T13:00:00.000Z',
  data: {
    deviceId: 'daq_001',
    sequenceNumber: 12345,
    channels: [
      { id: 0, values: [1.23, 1.24, 1.25] },
      { id: 1, values: [2.34, 2.35, 2.36] }
    ]
  }
}
```

#### 订阅设备状态
```typescript
// 订阅消息
{
  type: 'subscribe',
  id: 'sub_002',
  data: {
    topic: 'device_status',
    deviceId: 'daq_001'
  }
}

// 状态更新消息
{
  type: 'status',
  id: 'status_001',
  timestamp: '2025-01-08T13:00:00.000Z',
  data: {
    deviceId: 'daq_001',
    status: 'running',
    temperature: 45.2,
    voltage: 12.1,
    errors: 0
  }
}
```

### 2. 系统事件通知

#### 订阅系统事件
```typescript
{
  type: 'subscribe',
  id: 'sub_003',
  data: {
    topic: 'system_events',
    severity: ['warning', 'error', 'critical']
  }
}
```

#### 事件通知
```typescript
{
  type: 'event',
  id: 'event_001',
  timestamp: '2025-01-08T13:00:00.000Z',
  data: {
    eventType: 'device_error',
    severity: 'error',
    source: 'daq_001',
    message: 'Channel 2 voltage out of range',
    details: {
      channel: 2,
      voltage: 15.2,
      limit: 10.0
    }
  }
}
```

### 3. AI控件生成进度

#### 订阅生成进度
```typescript
{
  type: 'subscribe',
  id: 'sub_004',
  data: {
    topic: 'ai_generation',
    taskId: 'gen_001'
  }
}
```

#### 进度更新
```typescript
{
  type: 'progress',
  id: 'progress_001',
  timestamp: '2025-01-08T13:00:00.000Z',
  data: {
    taskId: 'gen_001',
    stage: 'code_generation',
    progress: 75,
    message: 'Generating component styles...'
  }
}
```

## 📝 错误代码规范

### HTTP状态码使用
- `200 OK` - 请求成功
- `201 Created` - 资源创建成功
- `400 Bad Request` - 请求参数错误
- `401 Unauthorized` - 未认证
- `403 Forbidden` - 权限不足
- `404 Not Found` - 资源不存在
- `409 Conflict` - 资源冲突
- `422 Unprocessable Entity` - 数据验证失败
- `429 Too Many Requests` - 请求限流
- `500 Internal Server Error` - 服务器内部错误
- `503 Service Unavailable` - 服务不可用

### 业务错误代码
```typescript
enum ErrorCode {
  // 通用错误 (1000-1999)
  INVALID_REQUEST = 'E1001',
  VALIDATION_FAILED = 'E1002',
  RESOURCE_NOT_FOUND = 'E1003',
  PERMISSION_DENIED = 'E1004',
  RATE_LIMIT_EXCEEDED = 'E1005',
  
  // 认证错误 (2000-2999)
  INVALID_CREDENTIALS = 'E2001',
  TOKEN_EXPIRED = 'E2002',
  TOKEN_INVALID = 'E2003',
  ACCOUNT_LOCKED = 'E2004',
  
  // 设备错误 (3000-3999)
  DEVICE_NOT_FOUND = 'E3001',
  DEVICE_OFFLINE = 'E3002',
  DEVICE_BUSY = 'E3003',
  DEVICE_ERROR = 'E3004',
  CONFIGURATION_INVALID = 'E3005',
  CALIBRATION_FAILED = 'E3006',
  
  // 数据错误 (4000-4999)
  DATA_ACQUISITION_FAILED = 'E4001',
  DATA_CORRUPTION = 'E4002',
  STORAGE_FULL = 'E4003',
  EXPORT_FAILED = 'E4004',
  QUERY_TIMEOUT = 'E4005',
  
  // AI错误 (5000-5999)
  AI_SERVICE_UNAVAILABLE = 'E5001',
  GENERATION_FAILED = 'E5002',
  VALIDATION_FAILED = 'E5003',
  TEMPLATE_INVALID = 'E5004',
  QUOTA_EXCEEDED = 'E5005',
  
  // 系统错误 (9000-9999)
  INTERNAL_ERROR = 'E9001',
  SERVICE_UNAVAILABLE = 'E9002',
  MAINTENANCE_MODE = 'E9003',
  DATABASE_ERROR = 'E9004'
}
```

## 🔧 开发工具和测试

### API文档生成
- 使用OpenAPI 3.0规范
- Swagger UI自动生成
- 代码注释自动同步

### 接口测试
```typescript
// Jest测试示例
describe('Device API', () => {
  test('should get device list', async () => {
    const response = await request(app)
      .get('/api/v1/devices')
      .set('Authorization', `Bearer ${token}`)
      .expect(200)
    
    expect(response.body.success).toBe(true)
    expect(response.body.data).toBeInstanceOf(Array)
  })
  
  test('should handle device not found', async () => {
    const response = await request(app)
      .get('/api/v1/devices/invalid-id')
      .set('Authorization', `Bearer ${token}`)
      .expect(404)
    
    expect(response.body.success).toBe(false)
    expect(response.body.error.code).toBe('E3001')
  })
})
```

### 性能测试
```bash
# 使用Artillery进行负载测试
artillery run load-test.yml

# load-test.yml
config:
  target: 'https://api.seesharptools.com'
  phases:
    - duration: 60
      arrivalRate: 10
scenarios:
  - name: "Device API Load Test"
    requests:
      - get:
          url: "/api/v1/devices"
          headers:
            Authorization: "Bearer {{ token }}"
```

## 📚 SDK和客户端库

### TypeScript SDK
```typescript
// SeeSharpTools Web SDK
import { SeeSharpToolsClient } from '@seesharptools/web-sdk'

const client = new SeeSharpToolsClient({
  baseUrl: 'https://api.seesharptools.com',
  apiKey: 'your-api-key',
  timeout: 30000
})

// 设备操作
const devices = await client.devices.list()
const device = await client.devices.get('daq_001')
await client.devices.configure('daq_001', config)

// 数据操作
const stream = client.data.createStream('daq_001')
stream.on('data', (data) => {
  console.log('Received data:', data)
})

// AI控件生成
const control = await client.ai.generateControl({
  description: '温度显示仪表',
  requirements: { realtime: true }
})
```

### Python SDK
```python
# Python SDK示例
from seesharptools import SeeSharpToolsClient

client = SeeSharpToolsClient(
    base_url='https://api.seesharptools.com',
    api_key='your-api-key'
)

# 设备操作
devices = client.devices.list()
device = client.devices.get('daq_001')

# 数据流
def on_data(data):
    print(f"Received data: {data}")

stream = client.data.create_stream('daq_001')
stream.on_data(on_data)
stream.start()
```

这个API规范为SeeSharpTools Web平台提供了完整的接口定义，确保前后端开发的一致性和可维护性。
