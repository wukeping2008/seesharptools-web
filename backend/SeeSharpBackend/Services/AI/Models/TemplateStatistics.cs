using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Services.AI.Models
{
    /// <summary>
    /// 模板统计信息
    /// </summary>
    public class TemplateStatistics
    {
        /// <summary>
        /// 总模板数
        /// </summary>
        public int TotalTemplates { get; set; }

        /// <summary>
        /// 启用的模板数
        /// </summary>
        public int EnabledTemplates { get; set; }

        /// <summary>
        /// 内置模板数
        /// </summary>
        public int BuiltInTemplates { get; set; }

        /// <summary>
        /// 自定义模板数
        /// </summary>
        public int CustomTemplates { get; set; }

        /// <summary>
        /// 按设备类型分类统计
        /// </summary>
        public Dictionary<string, int> ByDevice { get; set; } = new();

        /// <summary>
        /// 按类别分类统计
        /// </summary>
        public Dictionary<string, int> ByCategory { get; set; } = new();

        /// <summary>
        /// 按复杂度分类统计
        /// </summary>
        public Dictionary<string, int> ByComplexity { get; set; } = new();

        /// <summary>
        /// 总使用次数
        /// </summary>
        public int TotalUsageCount { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
