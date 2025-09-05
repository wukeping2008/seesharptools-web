using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Services.Security;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// 百度AI控件生成服务
    /// 专门用于生成Vue 3测试测量仪器控件
    /// </summary>
    public interface IBaiduControlGeneratorService
    {
        Task<ControlGenerationResult> GenerateVueComponentAsync(string description);
        Task<string> OptimizeComponentCodeAsync(string code, string description);
        Task<ControlType> IdentifyControlTypeAsync(string description);
        bool IsChineseDescription(string description);
    }

    public class BaiduControlGeneratorService : IBaiduControlGeneratorService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BaiduControlGeneratorService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISecureApiKeyService _secureApiKeyService;
        
        private string? _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        // 模型配置
        private readonly Dictionary<string, string> _modelEndpoints = new()
        {
            ["ernie-tiny-8k"] = "ernie-tiny-8k",
            ["ernie-speed-8k"] = "ernie-speed-8k", 
            ["ernie-3.5-8k"] = "completions",
            ["ernie-4.0-8k"] = "completions_pro"
        };

        public BaiduControlGeneratorService(
            IConfiguration configuration,
            ILogger<BaiduControlGeneratorService> logger,
            IHttpClientFactory httpClientFactory,
            ISecureApiKeyService secureApiKeyService)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _secureApiKeyService = secureApiKeyService;
        }

        /// <summary>
        /// 生成Vue组件
        /// </summary>
        public async Task<ControlGenerationResult> GenerateVueComponentAsync(string description)
        {
            try
            {
                _logger.LogInformation("开始使用百度AI生成控件: {Description}", description);

                // 1. 识别控件类型
                var controlType = await IdentifyControlTypeAsync(description);
                _logger.LogInformation("识别到控件类型: {ControlType}", controlType);

                // 2. 选择最优模型
                var model = DetermineOptimalModel(description, controlType);
                _logger.LogInformation("选择模型: {Model}", model);

                // 3. 构建专业提示词
                var prompt = BuildControlGenerationPrompt(description, controlType);

                // 4. 调用百度AI生成代码
                var code = await CallBaiduAPIAsync(model, prompt);

                // 5. 优化生成的代码
                if (!string.IsNullOrEmpty(code))
                {
                    code = await OptimizeComponentCodeAsync(code, description);
                }

                return new ControlGenerationResult
                {
                    Success = true,
                    Code = code,
                    ControlType = controlType.ToString(),
                    Model = model,
                    Source = "baidu-ai",
                    GenerationTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "百度AI生成控件失败");
                return new ControlGenerationResult
                {
                    Success = false,
                    Error = ex.Message,
                    Source = "baidu-ai"
                };
            }
        }

        /// <summary>
        /// 识别控件类型
        /// </summary>
        public async Task<ControlType> IdentifyControlTypeAsync(string description)
        {
            var normalizedDesc = description.ToLower();
            
            // 关键词映射
            var keywordMap = new Dictionary<ControlType, string[]>
            {
                [ControlType.Gauge] = new[] { "仪表", "gauge", "表盘", "刻度", "指针", "仪表盘" },
                [ControlType.Oscilloscope] = new[] { "示波器", "oscilloscope", "波形显示", "波形图" },
                [ControlType.LED] = new[] { "led", "指示灯", "状态灯", "信号灯", "灯" },
                [ControlType.Button] = new[] { "按钮", "button", "开关", "控制按钮", "触发" },
                [ControlType.Chart] = new[] { "图表", "chart", "曲线", "趋势图", "数据图" },
                [ControlType.DigitalDisplay] = new[] { "数字显示", "数码管", "七段", "digital", "lcd" },
                [ControlType.Slider] = new[] { "滑块", "slider", "滑动条", "调节器" },
                [ControlType.Knob] = new[] { "旋钮", "knob", "旋转", "调节旋钮" },
                [ControlType.Progress] = new[] { "进度", "progress", "进度条", "百分比" },
                [ControlType.Meter] = new[] { "万用表", "电压表", "电流表", "功率表", "meter" }
            };

            // 计算匹配分数
            var scores = new Dictionary<ControlType, int>();
            foreach (var kvp in keywordMap)
            {
                var score = kvp.Value.Count(keyword => normalizedDesc.Contains(keyword));
                if (score > 0)
                {
                    scores[kvp.Key] = score;
                }
            }

            // 如果有明确匹配，返回最高分的类型
            if (scores.Any())
            {
                return scores.OrderByDescending(s => s.Value).First().Key;
            }

            // 使用AI进行更智能的识别
            try
            {
                var aiPrompt = $"请识别以下描述对应的测控仪器控件类型：{description}\n" +
                              "只返回一个类型名称，从以下选项中选择：仪表盘、示波器、LED指示灯、按钮、图表、数字显示器、滑块、旋钮、进度条、电表";
                
                var response = await CallBaiduAPIAsync("ernie-tiny-8k", aiPrompt);
                
                // 解析AI响应
                if (response.Contains("仪表")) return ControlType.Gauge;
                if (response.Contains("示波器")) return ControlType.Oscilloscope;
                if (response.Contains("LED") || response.Contains("指示灯")) return ControlType.LED;
                if (response.Contains("按钮")) return ControlType.Button;
                if (response.Contains("图表")) return ControlType.Chart;
                if (response.Contains("数字显示")) return ControlType.DigitalDisplay;
                if (response.Contains("滑块")) return ControlType.Slider;
                if (response.Contains("旋钮")) return ControlType.Knob;
                if (response.Contains("进度")) return ControlType.Progress;
                if (response.Contains("电表")) return ControlType.Meter;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI识别控件类型失败，使用默认类型");
            }

            // 默认返回仪表盘
            return ControlType.Gauge;
        }

        /// <summary>
        /// 判断是否为中文描述
        /// </summary>
        public bool IsChineseDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return false;
            
            // 统计中文字符数量
            int chineseCount = 0;
            int totalCount = 0;
            
            foreach (char c in description)
            {
                if (!char.IsWhiteSpace(c))
                {
                    totalCount++;
                    if (c >= 0x4e00 && c <= 0x9fff)
                    {
                        chineseCount++;
                    }
                }
            }
            
            // 如果中文字符占比超过30%，认为是中文描述
            return totalCount > 0 && (chineseCount * 100 / totalCount) > 30;
        }

        /// <summary>
        /// 优化组件代码
        /// </summary>
        public async Task<string> OptimizeComponentCodeAsync(string code, string description)
        {
            if (string.IsNullOrEmpty(code)) return code;

            try
            {
                var optimizationPrompt = $@"
优化以下Vue 3组件代码，使其更符合测试测量仪器的专业标准：

原始需求：{description}

当前代码：
{code}

优化要求：
1. 确保TypeScript类型定义完整
2. 添加必要的Props默认值
3. 优化样式，使用专业的工业设计风格
4. 添加适当的动画效果
5. 确保响应式设计
6. 优化性能，避免不必要的重渲染

直接返回优化后的完整代码，不需要解释。";

                // 使用ERNIE-3.5进行代码优化
                var optimizedCode = await CallBaiduAPIAsync("ernie-3.5-8k", optimizationPrompt);
                
                // 验证优化后的代码
                if (IsValidVueComponent(optimizedCode))
                {
                    return optimizedCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "代码优化失败，返回原始代码");
            }

            return code;
        }

        #region 私有方法

        /// <summary>
        /// 确定最优模型
        /// </summary>
        private string DetermineOptimalModel(string description, ControlType controlType)
        {
            // 根据复杂度选择模型
            var complexity = EstimateComplexity(description, controlType);
            
            return complexity switch
            {
                ComplexityLevel.Simple => "ernie-speed-8k",    // 简单控件用Speed
                ComplexityLevel.Medium => "ernie-3.5-8k",       // 中等复杂度用3.5
                ComplexityLevel.Complex => "ernie-4.0-8k",      // 复杂控件用4.0
                _ => "ernie-3.5-8k"
            };
        }

        /// <summary>
        /// 估算控件复杂度
        /// </summary>
        private ComplexityLevel EstimateComplexity(string description, ControlType controlType)
        {
            // 复杂控件类型
            if (controlType == ControlType.Oscilloscope || controlType == ControlType.Chart)
            {
                return ComplexityLevel.Complex;
            }
            
            // 简单控件类型
            if (controlType == ControlType.LED || controlType == ControlType.Button)
            {
                return ComplexityLevel.Simple;
            }
            
            // 根据描述长度和关键词判断
            if (description.Length > 100 || 
                description.Contains("复杂") || 
                description.Contains("高级") ||
                description.Contains("实时"))
            {
                return ComplexityLevel.Complex;
            }
            
            return ComplexityLevel.Medium;
        }

        /// <summary>
        /// 构建控件生成提示词
        /// </summary>
        private string BuildControlGenerationPrompt(string description, ControlType controlType)
        {
            var basePrompt = $@"你是一个专业的测试测量仪器UI专家，精通Vue 3和TypeScript。

任务：生成一个{GetControlTypeName(controlType)}控件的Vue 3组件。

用户需求：{description}

严格要求：
1. 使用Vue 3 Composition API语法（<script setup lang=""ts"">）
2. 包含完整的TypeScript类型定义和Props接口
3. 使用专业的测试测量仪器设计风格
4. 颜色方案：主色#409eff，警告色#ff9900，危险色#ff3333，成功色#00cc66
5. 包含template、script和style三个部分
6. style必须使用scoped
7. 确保代码可直接运行，不依赖外部库
8. 使用SVG或Canvas绘制图形元素";

            // 根据控件类型添加特定要求
            var specificRequirements = GetSpecificRequirements(controlType);
            
            return $@"{basePrompt}

特定要求：
{specificRequirements}

示例Props接口：
interface Props {{
  value: number
  min?: number
  max?: number
  unit?: string
  title?: string
  precision?: number
}}

直接返回完整的Vue组件代码，从<template>开始，到</style>结束，不要包含任何解释或markdown标记。";
        }

        /// <summary>
        /// 获取控件类型特定要求
        /// </summary>
        private string GetSpecificRequirements(ControlType controlType)
        {
            return controlType switch
            {
                ControlType.Gauge => @"
- 使用SVG绘制圆形仪表盘
- 包含刻度线和刻度值
- 指针动画效果
- 支持自定义量程和单位
- 数值显示在中心
- 可选的警告和危险区域标记",

                ControlType.Oscilloscope => @"
- 使用Canvas绘制波形
- 网格背景
- 支持多通道显示
- 实时数据更新
- 触发电平线
- 时基和幅度刻度",

                ControlType.LED => @"
- 圆形LED造型
- 发光效果（box-shadow）
- 支持多种颜色
- 闪烁动画选项
- 标签文字",

                ControlType.DigitalDisplay => @"
- 七段数码管样式
- LCD/LED显示效果
- 支持小数点
- 自动调整位数
- 可选单位显示",

                ControlType.Chart => @"
- 使用Canvas或SVG绘制
- 支持实时数据更新
- 网格线和坐标轴
- 多系列数据支持
- 图例显示
- 响应式缩放",

                _ => "- 专业的工业设计风格\n- 良好的交互反馈\n- 清晰的视觉层次"
            };
        }

        /// <summary>
        /// 调用百度AI API
        /// </summary>
        private async Task<string> CallBaiduAPIAsync(string model, string prompt)
        {
            try
            {
                // 获取访问令牌
                await EnsureAccessTokenAsync();
                
                var httpClient = _httpClientFactory.CreateClient();
                
                // 构建请求URL
                var endpoint = _modelEndpoints.GetValueOrDefault(model, "completions");
                var url = $"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/{endpoint}?access_token={_accessToken}";
                
                // 构建请求体
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,  // 降低随机性
                    top_p = 0.8,
                    penalty_score = 1.0
                };
                
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // 发送请求
                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("百度AI API调用失败: {StatusCode}, {Content}", 
                        response.StatusCode, responseContent);
                    throw new Exception($"API调用失败: {response.StatusCode}");
                }
                
                // 解析响应
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("result", out var result))
                {
                    return result.GetString() ?? string.Empty;
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用百度AI API失败");
                throw;
            }
        }

        /// <summary>
        /// 确保访问令牌有效
        /// </summary>
        private async Task EnsureAccessTokenAsync()
        {
            await _tokenSemaphore.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                {
                    return;
                }
                
                // 获取API密钥
                var apiKey = await _secureApiKeyService.GetApiKeyAsync("Baidu");
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("百度AI API密钥未配置");
                }
                
                // API密钥格式: "api_key:secret_key"
                var keys = apiKey.Split(':');
                if (keys.Length != 2)
                {
                    throw new InvalidOperationException("百度AI API密钥格式错误");
                }
                
                // 获取新的访问令牌
                var httpClient = _httpClientFactory.CreateClient();
                var tokenUrl = $"https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={keys[0]}&client_secret={keys[1]}";
                
                var response = await httpClient.PostAsync(tokenUrl, null);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("access_token", out var token))
                {
                    _accessToken = token.GetString();
                    
                    // 设置过期时间（提前5分钟刷新）
                    var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 300);
                }
                else
                {
                    throw new Exception("获取百度AI访问令牌失败");
                }
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        /// <summary>
        /// 验证Vue组件代码
        /// </summary>
        private bool IsValidVueComponent(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            
            return code.Contains("<template>") && 
                   code.Contains("</template>") &&
                   code.Contains("<script") && 
                   code.Contains("</script>") &&
                   code.Contains("<style") && 
                   code.Contains("</style>");
        }

        /// <summary>
        /// 获取控件类型名称
        /// </summary>
        private string GetControlTypeName(ControlType type)
        {
            return type switch
            {
                ControlType.Gauge => "仪表盘",
                ControlType.Oscilloscope => "示波器",
                ControlType.LED => "LED指示灯",
                ControlType.Button => "按钮",
                ControlType.Chart => "图表",
                ControlType.DigitalDisplay => "数字显示器",
                ControlType.Slider => "滑块",
                ControlType.Knob => "旋钮",
                ControlType.Progress => "进度条",
                ControlType.Meter => "电表",
                _ => "控件"
            };
        }

        #endregion
    }

    /// <summary>
    /// 控件类型枚举
    /// </summary>
    public enum ControlType
    {
        Gauge,           // 仪表盘
        Oscilloscope,    // 示波器
        LED,             // LED指示灯
        Button,          // 按钮
        Chart,           // 图表
        DigitalDisplay,  // 数字显示器
        Slider,          // 滑块
        Knob,            // 旋钮
        Progress,        // 进度条
        Meter            // 电表
    }

    /// <summary>
    /// 复杂度级别
    /// </summary>
    public enum ComplexityLevel
    {
        Simple,   // 简单
        Medium,   // 中等
        Complex   // 复杂
    }

    /// <summary>
    /// 控件生成结果
    /// </summary>
    public class ControlGenerationResult
    {
        public bool Success { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Error { get; set; }
        public string ControlType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime GenerationTime { get; set; }
    }
}