using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Services.AI.Models
{
    /// <summary>
    /// 测试需求数据模型
    /// </summary>
    public class TestRequirement
    {
        /// <summary>
        /// 测试对象 (如：电机轴承、信号发生器、温度传感器)
        /// </summary>
        public string TestObject { get; set; } = string.Empty;

        /// <summary>
        /// 测试类型 (如：振动测试、电气测试、温度测量)
        /// </summary>
        public string TestType { get; set; } = string.Empty;

        /// <summary>
        /// 频率范围 (如：1-10kHz、0-50Hz)
        /// </summary>
        public string FrequencyRange { get; set; } = string.Empty;

        /// <summary>
        /// 分析方法 (如：FFT分析、THD分析、统计分析)
        /// </summary>
        public string AnalysisMethod { get; set; } = string.Empty;

        /// <summary>
        /// 推荐设备 (如：JYUSB1601、JY5500)
        /// </summary>
        public string RecommendedDevice { get; set; } = string.Empty;

        /// <summary>
        /// 优先级 (低、中、高)
        /// </summary>
        public string Priority { get; set; } = "中";

        /// <summary>
        /// AI识别置信度 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double Confidence { get; set; }

        /// <summary>
        /// 附加参数 (采样率、通道数、缓冲区大小等)
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();

        /// <summary>
        /// 提取的关键词
        /// </summary>
        public List<string> Keywords { get; set; } = new();

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// 原始用户输入
        /// </summary>
        public string OriginalInput { get; set; } = string.Empty;

        /// <summary>
        /// 应用领域 (汽车、航空、电子、机械、通用)
        /// </summary>
        public List<string> ApplicationDomains { get; set; } = new();

        /// <summary>
        /// 复杂度级别 (初级、中级、高级、专家)
        /// </summary>
        public string ComplexityLevel { get; set; } = "中级";
    }

    /// <summary>
    /// 模板匹配结果
    /// </summary>
    public class TemplateMatch
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 模板信息
        /// </summary>
        public TestTemplate Template { get; set; } = new();

        /// <summary>
        /// 相似度得分 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double SimilarityScore { get; set; }

        /// <summary>
        /// 设备兼容性得分 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double DeviceCompatibility { get; set; }

        /// <summary>
        /// 复杂度匹配得分 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double ComplexityMatch { get; set; }

        /// <summary>
        /// 领域相关性得分 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double DomainRelevance { get; set; }

        /// <summary>
        /// 综合得分 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double OverallScore { get; set; }

        /// <summary>
        /// 匹配置信度 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double Confidence { get; set; }

        /// <summary>
        /// 匹配原因说明
        /// </summary>
        public string MatchReason { get; set; } = string.Empty;
    }


    /// <summary>
    /// 代码生成结果
    /// </summary>
    public class CodeGenerationResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 生成的代码
        /// </summary>
        public string GeneratedCode { get; set; } = string.Empty;

        /// <summary>
        /// 使用的模板ID
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 应用的参数
        /// </summary>
        public Dictionary<string, object> AppliedParameters { get; set; } = new();

        /// <summary>
        /// 优化建议
        /// </summary>
        public List<string> OptimizationSuggestions { get; set; } = new();

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 代码质量得分 (0-1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double QualityScore { get; set; }
    }

    /// <summary>
    /// 综合评分结果
    /// </summary>
    public class CompositeScore
    {
        /// <summary>
        /// 语义相似度得分
        /// </summary>
        public double Similarity { get; set; }

        /// <summary>
        /// 设备匹配得分
        /// </summary>
        public double DeviceMatch { get; set; }

        /// <summary>
        /// 复杂度匹配得分
        /// </summary>
        public double ComplexityMatch { get; set; }

        /// <summary>
        /// 领域匹配得分
        /// </summary>
        public double DomainMatch { get; set; }

        /// <summary>
        /// 参数匹配得分
        /// </summary>
        public double ParameterMatch { get; set; }

        /// <summary>
        /// 综合得分
        /// </summary>
        public double Overall { get; set; }

        /// <summary>
        /// 置信度
        /// </summary>
        public double Confidence { get; set; }
    }
}
