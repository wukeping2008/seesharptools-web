using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeeSharpBackend.Services.AI.Models
{
    /// <summary>
    /// 测试模板实体
    /// </summary>
    public class TestTemplate
    {
        /// <summary>
        /// 模板唯一标识
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 模板名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 模板类别
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 支持的设备类型
        /// </summary>
        public List<string> SupportedDevices { get; set; } = new();

        /// <summary>
        /// 功能标签
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// 复杂度级别 (初级、中级、高级、专家)
        /// </summary>
        public string ComplexityLevel { get; set; } = "中级";

        /// <summary>
        /// 应用领域
        /// </summary>
        public List<string> ApplicationDomains { get; set; } = new();

        /// <summary>
        /// 默认参数配置
        /// </summary>
        public Dictionary<string, object> DefaultParameters { get; set; } = new();

        /// <summary>
        /// 模板代码内容
        /// </summary>
        [Required]
        public string CodeTemplate { get; set; } = string.Empty;

        /// <summary>
        /// 代码语言
        /// </summary>
        public string Language { get; set; } = "csharp";

        /// <summary>
        /// 关键词 (用于搜索和匹配)
        /// </summary>
        public List<string> Keywords { get; set; } = new();

        /// <summary>
        /// 模板作者
        /// </summary>
        public string Author { get; set; } = "简仪科技";

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// 使用次数统计
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// 用户评分 (1-5)
        /// </summary>
        [Range(1, 5)]
        public double Rating { get; set; } = 5.0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 是否为系统内置模板
        /// </summary>
        public bool IsBuiltIn { get; set; } = true;

        /// <summary>
        /// 预期输出示例
        /// </summary>
        public string ExpectedOutput { get; set; } = string.Empty;

        /// <summary>
        /// 使用说明
        /// </summary>
        public string UsageInstructions { get; set; } = string.Empty;

        /// <summary>
        /// 依赖项
        /// </summary>
        public List<string> Dependencies { get; set; } = new();

        /// <summary>
        /// 模板向量 (用于相似度计算)
        /// </summary>
        [JsonIgnore]
        public float[]? Vector { get; set; }

        /// <summary>
        /// 获取模板摘要
        /// </summary>
        public string GetSummary()
        {
            return $"{Name} - {Description} (设备: {string.Join(", ", SupportedDevices)})";
        }

        /// <summary>
        /// 检查是否匹配设备类型
        /// </summary>
        public bool MatchesDevice(string deviceType)
        {
            return SupportedDevices.Contains(deviceType) || SupportedDevices.Contains("通用");
        }

        /// <summary>
        /// 检查是否匹配关键词
        /// </summary>
        public bool MatchesKeywords(IEnumerable<string> keywords)
        {
            var templateKeywords = Keywords.Concat(Tags).Select(k => k.ToLower()).ToHashSet();
            return keywords.Any(k => templateKeywords.Contains(k.ToLower()));
        }

        /// <summary>
        /// 获取替换参数列表
        /// </summary>
        public List<string> GetTemplateParameters()
        {
            var parameters = new List<string>();
            var matches = System.Text.RegularExpressions.Regex.Matches(CodeTemplate, @"\{\{(\w+)\}\}");
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var param = match.Groups[1].Value;
                if (!parameters.Contains(param))
                {
                    parameters.Add(param);
                }
            }
            
            return parameters;
        }
    }

    /// <summary>
    /// 模板查询参数
    /// </summary>
    public class TemplateQueryOptions
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// 设备类型过滤
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// 类别过滤
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 复杂度过滤
        /// </summary>
        public string? ComplexityLevel { get; set; }

        /// <summary>
        /// 应用领域过滤
        /// </summary>
        public string? ApplicationDomain { get; set; }

        /// <summary>
        /// 标签过滤
        /// </summary>
        public List<string>? Tags { get; set; }

        /// <summary>
        /// 是否只显示启用的模板
        /// </summary>
        public bool OnlyEnabled { get; set; } = true;

        /// <summary>
        /// 排序方式
        /// </summary>
        public TemplateSortBy SortBy { get; set; } = TemplateSortBy.Rating;

        /// <summary>
        /// 排序方向
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 模板排序方式
    /// </summary>
    public enum TemplateSortBy
    {
        /// <summary>
        /// 按名称排序
        /// </summary>
        Name,

        /// <summary>
        /// 按创建时间排序
        /// </summary>
        CreatedAt,

        /// <summary>
        /// 按更新时间排序
        /// </summary>
        LastUpdated,

        /// <summary>
        /// 按使用次数排序
        /// </summary>
        UsageCount,

        /// <summary>
        /// 按评分排序
        /// </summary>
        Rating,

        /// <summary>
        /// 按相关性排序
        /// </summary>
        Relevance
    }

    /// <summary>
    /// 排序方向
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// 升序
        /// </summary>
        Ascending,

        /// <summary>
        /// 降序
        /// </summary>
        Descending
    }

    /// <summary>
    /// 模板搜索结果
    /// </summary>
    public class TemplateSearchResult
    {
        /// <summary>
        /// 模板列表
        /// </summary>
        public List<TestTemplate> Templates { get; set; } = new();

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 查询耗时 (毫秒)
        /// </summary>
        public long QueryTime { get; set; }
    }
}