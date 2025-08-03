using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SeeSharpBackend.Models;

namespace SeeSharpBackend.Services.TestExecution
{
    /// <summary>
    /// Test execution service interface for persisting and managing test results
    /// </summary>
    public interface ITestExecutionService
    {
        /// <summary>
        /// Save test execution result
        /// </summary>
        Task<int> SaveExecutionResultAsync(TestExecutionRequest request);

        /// <summary>
        /// Get test execution result by ID
        /// </summary>
        Task<TestExecutionRecord?> GetExecutionResultAsync(int id);

        /// <summary>
        /// Get test execution history
        /// </summary>
        Task<List<TestExecutionRecord>> GetExecutionHistoryAsync(TestExecutionHistoryQuery query);

        /// <summary>
        /// Get execution statistics
        /// </summary>
        Task<TestExecutionStatistics> GetExecutionStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Delete test execution record
        /// </summary>
        Task<bool> DeleteExecutionRecordAsync(int id);

        /// <summary>
        /// Get test execution records by device type
        /// </summary>
        Task<List<TestExecutionRecord>> GetExecutionsByDeviceTypeAsync(string deviceType, int maxCount = 50);

        /// <summary>
        /// Get test execution records by test type
        /// </summary>
        Task<List<TestExecutionRecord>> GetExecutionsByTestTypeAsync(string testType, int maxCount = 50);

        /// <summary>
        /// Update execution result with additional data
        /// </summary>
        Task<bool> UpdateExecutionResultAsync(int id, string resultData, bool success, string? errorMessage = null);

        /// <summary>
        /// Get execution performance metrics
        /// </summary>
        Task<ExecutionPerformanceMetrics> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Test execution request model
    /// </summary>
    public class TestExecutionRequest
    {
        /// <summary>
        /// Test requirement description
        /// </summary>
        public string TestRequirement { get; set; } = string.Empty;

        /// <summary>
        /// Generated code
        /// </summary>
        public string GeneratedCode { get; set; } = string.Empty;

        /// <summary>
        /// Device type
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// Test type
        /// </summary>
        public string? TestType { get; set; }

        /// <summary>
        /// AI provider used
        /// </summary>
        public string? AIProvider { get; set; }

        /// <summary>
        /// Tokens used for generation
        /// </summary>
        public int? TokensUsed { get; set; }

        /// <summary>
        /// Code quality score
        /// </summary>
        public int? CodeQualityScore { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Execution result data (JSON)
        /// </summary>
        public string? ResultData { get; set; }

        /// <summary>
        /// Execution success flag
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if execution failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        public int? ExecutionTimeMs { get; set; }
    }

    /// <summary>
    /// Test execution history query parameters
    /// </summary>
    public class TestExecutionHistoryQuery
    {
        /// <summary>
        /// Maximum number of records to return
        /// </summary>
        public int MaxCount { get; set; } = 50;

        /// <summary>
        /// Start date filter
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date filter
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Device type filter
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// Test type filter
        /// </summary>
        public string? TestType { get; set; }

        /// <summary>
        /// Success filter (null = all, true = success only, false = failures only)
        /// </summary>
        public bool? Success { get; set; }

        /// <summary>
        /// User ID filter
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// AI provider filter
        /// </summary>
        public string? AIProvider { get; set; }

        /// <summary>
        /// Minimum code quality score filter
        /// </summary>
        public int? MinCodeQualityScore { get; set; }

        /// <summary>
        /// Sort order (newest first by default)
        /// </summary>
        public TestExecutionSortOrder SortOrder { get; set; } = TestExecutionSortOrder.NewestFirst;
    }

    /// <summary>
    /// Test execution sort order
    /// </summary>
    public enum TestExecutionSortOrder
    {
        NewestFirst,
        OldestFirst,
        QualityScoreDesc,
        QualityScoreAsc,
        ExecutionTimeDesc,
        ExecutionTimeAsc
    }

    /// <summary>
    /// Test execution statistics
    /// </summary>
    public class TestExecutionStatistics
    {
        /// <summary>
        /// Total executions
        /// </summary>
        public int TotalExecutions { get; set; }

        /// <summary>
        /// Successful executions
        /// </summary>
        public int SuccessfulExecutions { get; set; }

        /// <summary>
        /// Failed executions
        /// </summary>
        public int FailedExecutions { get; set; }

        /// <summary>
        /// Success rate percentage
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Average code quality score
        /// </summary>
        public double AverageCodeQuality { get; set; }

        /// <summary>
        /// Average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// Total tokens used
        /// </summary>
        public int TotalTokensUsed { get; set; }

        /// <summary>
        /// Executions by device type
        /// </summary>
        public Dictionary<string, int> ExecutionsByDeviceType { get; set; } = new();

        /// <summary>
        /// Executions by test type
        /// </summary>
        public Dictionary<string, int> ExecutionsByTestType { get; set; } = new();

        /// <summary>
        /// Executions by AI provider
        /// </summary>
        public Dictionary<string, int> ExecutionsByAIProvider { get; set; } = new();

        /// <summary>
        /// Daily execution trends
        /// </summary>
        public List<ExecutionTrend> DailyTrends { get; set; } = new();
    }

    /// <summary>
    /// Execution trend data
    /// </summary>
    public class ExecutionTrend
    {
        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Number of executions
        /// </summary>
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Success count
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Average quality score
        /// </summary>
        public double AverageQuality { get; set; }

        /// <summary>
        /// Total tokens used
        /// </summary>
        public int TokensUsed { get; set; }
    }

    /// <summary>
    /// Execution performance metrics
    /// </summary>
    public class ExecutionPerformanceMetrics
    {
        /// <summary>
        /// Fastest execution time
        /// </summary>
        public int FastestExecutionMs { get; set; }

        /// <summary>
        /// Slowest execution time
        /// </summary>
        public int SlowestExecutionMs { get; set; }

        /// <summary>
        /// Average execution time
        /// </summary>
        public double AverageExecutionMs { get; set; }

        /// <summary>
        /// Median execution time
        /// </summary>
        public double MedianExecutionMs { get; set; }

        /// <summary>
        /// 95th percentile execution time
        /// </summary>
        public double P95ExecutionMs { get; set; }

        /// <summary>
        /// Performance by device type
        /// </summary>
        public Dictionary<string, DevicePerformanceMetrics> PerformanceByDevice { get; set; } = new();

        /// <summary>
        /// Performance by test type
        /// </summary>
        public Dictionary<string, TestTypePerformanceMetrics> PerformanceByTestType { get; set; } = new();
    }

    /// <summary>
    /// Device performance metrics
    /// </summary>
    public class DevicePerformanceMetrics
    {
        /// <summary>
        /// Average execution time
        /// </summary>
        public double AverageExecutionMs { get; set; }

        /// <summary>
        /// Success rate
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Average code quality
        /// </summary>
        public double AverageCodeQuality { get; set; }

        /// <summary>
        /// Total executions
        /// </summary>
        public int TotalExecutions { get; set; }
    }

    /// <summary>
    /// Test type performance metrics
    /// </summary>
    public class TestTypePerformanceMetrics
    {
        /// <summary>
        /// Average execution time
        /// </summary>
        public double AverageExecutionMs { get; set; }

        /// <summary>
        /// Success rate
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Average code quality
        /// </summary>
        public double AverageCodeQuality { get; set; }

        /// <summary>
        /// Total executions
        /// </summary>
        public int TotalExecutions { get; set; }

        /// <summary>
        /// Average tokens used
        /// </summary>
        public double AverageTokensUsed { get; set; }
    }
}