using SeeSharpBackend.Services.AI.Models;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// 自然语言处理器接口
    /// </summary>
    public interface INaturalLanguageProcessor
    {
        /// <summary>
        /// 分析用户测试需求
        /// </summary>
        /// <param name="userInput">用户输入的自然语言描述</param>
        /// <returns>解析后的测试需求</returns>
        Task<TestRequirement> AnalyzeRequirementAsync(string userInput);

        /// <summary>
        /// 验证需求解析结果
        /// </summary>
        /// <param name="requirement">解析结果</param>
        /// <returns>验证是否通过</returns>
        Task<bool> ValidateRequirementAsync(TestRequirement requirement);

        /// <summary>
        /// 优化需求解析结果
        /// </summary>
        /// <param name="requirement">原始解析结果</param>
        /// <returns>优化后的需求</returns>
        Task<TestRequirement> OptimizeRequirementAsync(TestRequirement requirement);

        /// <summary>
        /// 提取关键词
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>关键词列表</returns>
        Task<List<string>> ExtractKeywordsAsync(string text);

        /// <summary>
        /// 检查API健康状态
        /// </summary>
        /// <returns>API是否可用</returns>
        Task<bool> CheckApiHealthAsync();
    }
}
