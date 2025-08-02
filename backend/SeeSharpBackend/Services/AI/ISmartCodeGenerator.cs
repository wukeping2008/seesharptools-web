using SeeSharpBackend.Services.AI.Models;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// 智能代码生成器接口
    /// 根据测试需求和模板匹配结果生成个性化代码
    /// </summary>
    public interface ISmartCodeGenerator
    {
        /// <summary>
        /// 生成测试代码
        /// </summary>
        /// <param name="requirement">测试需求</param>
        /// <param name="templateMatch">匹配的模板</param>
        /// <returns>生成的代码结果</returns>
        Task<CodeGenerationResult> GenerateTestCodeAsync(TestRequirement requirement, TemplateMatch templateMatch);

        /// <summary>
        /// 根据需求生成代码（不使用模板）
        /// </summary>
        /// <param name="requirement">测试需求</param>
        /// <returns>生成的代码结果</returns>
        Task<CodeGenerationResult> GenerateTestCodeAsync(TestRequirement requirement);

        /// <summary>
        /// 优化现有代码
        /// </summary>
        /// <param name="code">原始代码</param>
        /// <param name="requirement">测试需求</param>
        /// <returns>优化后的代码</returns>
        Task<string> OptimizeCodeAsync(string code, TestRequirement requirement);

        /// <summary>
        /// 验证代码安全性
        /// </summary>
        /// <param name="code">要验证的代码</param>
        /// <returns>验证结果</returns>
        Task<CodeValidationResult> ValidateCodeSafetyAsync(string code);

        /// <summary>
        /// 生成代码文档和注释
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="requirement">测试需求</param>
        /// <returns>带文档的代码</returns>
        Task<string> AddDocumentationAsync(string code, TestRequirement requirement);
    }

    /// <summary>
    /// 代码生成结果
    /// </summary>
    public class CodeGenerationResult
    {
        /// <summary>
        /// 生成是否成功
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
        /// 生成的参数
        /// </summary>
        public Dictionary<string, object> GeneratedParameters { get; set; } = new();

        /// <summary>
        /// 代码质量评分
        /// </summary>
        public double QualityScore { get; set; }

        /// <summary>
        /// 预估执行时间（秒）
        /// </summary>
        public double EstimatedExecutionTime { get; set; }

        /// <summary>
        /// 代码复杂度
        /// </summary>
        public string ComplexityLevel { get; set; } = "中级";

        /// <summary>
        /// 优化建议
        /// </summary>
        public List<string> OptimizationSuggestions { get; set; } = new();

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 生成时间戳
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
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
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 相似度得分
        /// </summary>
        public double SimilarityScore { get; set; }

        /// <summary>
        /// 设备兼容性得分
        /// </summary>
        public double DeviceCompatibility { get; set; }

        /// <summary>
        /// 复杂度匹配度
        /// </summary>
        public double ComplexityMatch { get; set; }

        /// <summary>
        /// 领域相关性
        /// </summary>
        public double DomainRelevance { get; set; }

        /// <summary>
        /// 综合得分
        /// </summary>
        public double OverallScore { get; set; }

        /// <summary>
        /// 置信度
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 模板内容
        /// </summary>
        public string TemplateContent { get; set; } = string.Empty;

        /// <summary>
        /// 模板参数
        /// </summary>
        public Dictionary<string, object> TemplateParameters { get; set; } = new();

        /// <summary>
        /// 支持的设备类型
        /// </summary>
        public List<string> SupportedDevices { get; set; } = new();

        /// <summary>
        /// 应用领域
        /// </summary>
        public List<string> ApplicationDomains { get; set; } = new();
    }

    /// <summary>
    /// 代码验证结果
    /// </summary>
    public class CodeValidationResult
    {
        /// <summary>
        /// 验证是否通过
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 安全风险等级
        /// </summary>
        public SecurityRiskLevel RiskLevel { get; set; }

        /// <summary>
        /// 发现的问题
        /// </summary>
        public List<ValidationIssue> Issues { get; set; } = new();

        /// <summary>
        /// 修复建议
        /// </summary>
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// 安全风险等级
    /// </summary>
    public enum SecurityRiskLevel
    {
        /// <summary>
        /// 低风险
        /// </summary>
        Low = 0,

        /// <summary>
        /// 中等风险
        /// </summary>
        Medium = 1,

        /// <summary>
        /// 高风险
        /// </summary>
        High = 2,

        /// <summary>
        /// 严重风险
        /// </summary>
        Critical = 3
    }

    /// <summary>
    /// 验证问题
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// 问题类型
        /// </summary>
        public string IssueType { get; set; } = string.Empty;

        /// <summary>
        /// 问题描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 严重程度
        /// </summary>
        public string Severity { get; set; } = string.Empty;

        /// <summary>
        /// 行号
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// 修复建议
        /// </summary>
        public string FixSuggestion { get; set; } = string.Empty;
    }
}
