using SeeSharpBackend.Services.AI.Models;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// 模板仓库接口
    /// 管理测试模板的存储、检索和更新
    /// </summary>
    public interface ITemplateRepository
    {
        /// <summary>
        /// 获取所有模板
        /// </summary>
        /// <returns>模板列表</returns>
        Task<List<TestTemplate>> GetAllTemplatesAsync();

        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <returns>模板实体</returns>
        Task<TestTemplate?> GetTemplateByIdAsync(string templateId);

        /// <summary>
        /// 搜索模板
        /// </summary>
        /// <param name="options">查询选项</param>
        /// <returns>搜索结果</returns>
        Task<TemplateSearchResult> SearchTemplatesAsync(TemplateQueryOptions options);

        /// <summary>
        /// 根据设备类型获取模板
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <returns>匹配的模板列表</returns>
        Task<List<TestTemplate>> GetTemplatesByDeviceAsync(string deviceType);

        /// <summary>
        /// 根据类别获取模板
        /// </summary>
        /// <param name="category">类别</param>
        /// <returns>匹配的模板列表</returns>
        Task<List<TestTemplate>> GetTemplatesByCategoryAsync(string category);

        /// <summary>
        /// 根据关键词搜索模板
        /// </summary>
        /// <param name="keywords">关键词列表</param>
        /// <returns>匹配的模板列表</returns>
        Task<List<TestTemplate>> SearchByKeywordsAsync(List<string> keywords);

        /// <summary>
        /// 添加新模板
        /// </summary>
        /// <param name="template">模板实体</param>
        /// <returns>是否成功</returns>
        Task<bool> AddTemplateAsync(TestTemplate template);

        /// <summary>
        /// 更新模板
        /// </summary>
        /// <param name="template">模板实体</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateTemplateAsync(TestTemplate template);

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteTemplateAsync(string templateId);

        /// <summary>
        /// 增加模板使用次数
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <returns>是否成功</returns>
        Task<bool> IncrementUsageCountAsync(string templateId);

        /// <summary>
        /// 更新模板评分
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <param name="rating">评分</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateRatingAsync(string templateId, double rating);

        /// <summary>
        /// 获取热门模板
        /// </summary>
        /// <param name="count">返回数量</param>
        /// <returns>热门模板列表</returns>
        Task<List<TestTemplate>> GetPopularTemplatesAsync(int count = 10);

        /// <summary>
        /// 获取最近添加的模板
        /// </summary>
        /// <param name="count">返回数量</param>
        /// <returns>最新模板列表</returns>
        Task<List<TestTemplate>> GetRecentTemplatesAsync(int count = 10);

        /// <summary>
        /// 检查模板是否存在
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string templateId);

        /// <summary>
        /// 批量导入模板
        /// </summary>
        /// <param name="templates">模板列表</param>
        /// <returns>成功导入的数量</returns>
        Task<int> BulkImportTemplatesAsync(List<TestTemplate> templates);

        /// <summary>
        /// 获取统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        Task<TemplateStatistics> GetStatisticsAsync();
    }

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
        /// 用户自定义模板数
        /// </summary>
        public int CustomTemplates { get; set; }

        /// <summary>
        /// 按设备类型分组的统计
        /// </summary>
        public Dictionary<string, int> ByDevice { get; set; } = new();

        /// <summary>
        /// 按类别分组的统计
        /// </summary>
        public Dictionary<string, int> ByCategory { get; set; } = new();

        /// <summary>
        /// 按复杂度分组的统计
        /// </summary>
        public Dictionary<string, int> ByComplexity { get; set; } = new();

        /// <summary>
        /// 总使用次数
        /// </summary>
        public long TotalUsageCount { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}