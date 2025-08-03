using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Data;
using SeeSharpBackend.Models;
using System.Text.Json;

namespace SeeSharpBackend.Services.TestExecution
{
    /// <summary>
    /// Test execution service implementation
    /// </summary>
    public class TestExecutionService : ITestExecutionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestExecutionService> _logger;

        public TestExecutionService(
            ApplicationDbContext context,
            ILogger<TestExecutionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> SaveExecutionResultAsync(TestExecutionRequest request)
        {
            try
            {
                var record = new TestExecutionRecord
                {
                    TestRequirement = request.TestRequirement,
                    GeneratedCode = request.GeneratedCode,
                    DeviceType = request.DeviceType,
                    TestType = request.TestType,
                    AIProvider = request.AIProvider,
                    TokensUsed = request.TokensUsed,
                    CodeQualityScore = request.CodeQualityScore,
                    UserId = request.UserId,
                    ResultData = request.ResultData,
                    Success = request.Success,
                    ErrorMessage = request.ErrorMessage,
                    ExecutionTimeMs = request.ExecutionTimeMs,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.TestExecutionRecords.Add(record);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Saved test execution record with ID {record.Id}");
                return record.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving test execution result");
                throw;
            }
        }

        public async Task<TestExecutionRecord?> GetExecutionResultAsync(int id)
        {
            try
            {
                return await _context.TestExecutionRecords.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting test execution result {id}");
                throw;
            }
        }

        public async Task<List<TestExecutionRecord>> GetExecutionHistoryAsync(TestExecutionHistoryQuery query)
        {
            try
            {
                var queryable = _context.TestExecutionRecords.AsQueryable();

                // Apply filters
                if (query.StartDate.HasValue)
                {
                    queryable = queryable.Where(r => r.CreatedAt >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    queryable = queryable.Where(r => r.CreatedAt <= query.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(query.DeviceType))
                {
                    queryable = queryable.Where(r => r.DeviceType == query.DeviceType);
                }

                if (!string.IsNullOrEmpty(query.TestType))
                {
                    queryable = queryable.Where(r => r.TestType == query.TestType);
                }

                if (query.Success.HasValue)
                {
                    queryable = queryable.Where(r => r.Success == query.Success.Value);
                }

                if (!string.IsNullOrEmpty(query.UserId))
                {
                    queryable = queryable.Where(r => r.UserId == query.UserId);
                }

                if (!string.IsNullOrEmpty(query.AIProvider))
                {
                    queryable = queryable.Where(r => r.AIProvider == query.AIProvider);
                }

                if (query.MinCodeQualityScore.HasValue)
                {
                    queryable = queryable.Where(r => r.CodeQualityScore >= query.MinCodeQualityScore.Value);
                }

                // Apply sorting
                queryable = query.SortOrder switch
                {
                    TestExecutionSortOrder.NewestFirst => queryable.OrderByDescending(r => r.CreatedAt),
                    TestExecutionSortOrder.OldestFirst => queryable.OrderBy(r => r.CreatedAt),
                    TestExecutionSortOrder.QualityScoreDesc => queryable.OrderByDescending(r => r.CodeQualityScore),
                    TestExecutionSortOrder.QualityScoreAsc => queryable.OrderBy(r => r.CodeQualityScore),
                    TestExecutionSortOrder.ExecutionTimeDesc => queryable.OrderByDescending(r => r.ExecutionTimeMs),
                    TestExecutionSortOrder.ExecutionTimeAsc => queryable.OrderBy(r => r.ExecutionTimeMs),
                    _ => queryable.OrderByDescending(r => r.CreatedAt)
                };

                return await queryable.Take(query.MaxCount).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution history");
                throw;
            }
        }

        public async Task<TestExecutionStatistics> GetExecutionStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var records = await _context.TestExecutionRecords
                    .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                    .ToListAsync();

                var statistics = new TestExecutionStatistics
                {
                    TotalExecutions = records.Count,
                    SuccessfulExecutions = records.Count(r => r.Success),
                    FailedExecutions = records.Count(r => !r.Success)
                };

                statistics.SuccessRate = statistics.TotalExecutions > 0 
                    ? (double)statistics.SuccessfulExecutions / statistics.TotalExecutions * 100 
                    : 0;

                var qualityScores = records.Where(r => r.CodeQualityScore.HasValue).Select(r => r.CodeQualityScore!.Value);
                statistics.AverageCodeQuality = qualityScores.Any() ? qualityScores.Average() : 0;

                var executionTimes = records.Where(r => r.ExecutionTimeMs.HasValue).Select(r => r.ExecutionTimeMs!.Value);
                statistics.AverageExecutionTime = executionTimes.Any() ? executionTimes.Average() : 0;

                statistics.TotalTokensUsed = records.Where(r => r.TokensUsed.HasValue).Sum(r => r.TokensUsed!.Value);

                // Group by device type
                statistics.ExecutionsByDeviceType = records
                    .Where(r => !string.IsNullOrEmpty(r.DeviceType))
                    .GroupBy(r => r.DeviceType!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Group by test type
                statistics.ExecutionsByTestType = records
                    .Where(r => !string.IsNullOrEmpty(r.TestType))
                    .GroupBy(r => r.TestType!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Group by AI provider
                statistics.ExecutionsByAIProvider = records
                    .Where(r => !string.IsNullOrEmpty(r.AIProvider))
                    .GroupBy(r => r.AIProvider!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Calculate daily trends
                statistics.DailyTrends = records
                    .GroupBy(r => r.CreatedAt.Date)
                    .Select(g => new ExecutionTrend
                    {
                        Date = g.Key,
                        ExecutionCount = g.Count(),
                        SuccessCount = g.Count(r => r.Success),
                        AverageQuality = g.Where(r => r.CodeQualityScore.HasValue).Select(r => r.CodeQualityScore!.Value).DefaultIfEmpty(0).Average(),
                        TokensUsed = g.Where(r => r.TokensUsed.HasValue).Sum(r => r.TokensUsed!.Value)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution statistics");
                throw;
            }
        }

        public async Task<bool> DeleteExecutionRecordAsync(int id)
        {
            try
            {
                var record = await _context.TestExecutionRecords.FindAsync(id);
                if (record == null)
                {
                    return false;
                }

                _context.TestExecutionRecords.Remove(record);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted test execution record {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting test execution record {id}");
                throw;
            }
        }

        public async Task<List<TestExecutionRecord>> GetExecutionsByDeviceTypeAsync(string deviceType, int maxCount = 50)
        {
            try
            {
                return await _context.TestExecutionRecords
                    .Where(r => r.DeviceType == deviceType)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(maxCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting executions by device type {deviceType}");
                throw;
            }
        }

        public async Task<List<TestExecutionRecord>> GetExecutionsByTestTypeAsync(string testType, int maxCount = 50)
        {
            try
            {
                return await _context.TestExecutionRecords
                    .Where(r => r.TestType == testType)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(maxCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting executions by test type {testType}");
                throw;
            }
        }

        public async Task<bool> UpdateExecutionResultAsync(int id, string resultData, bool success, string? errorMessage = null)
        {
            try
            {
                var record = await _context.TestExecutionRecords.FindAsync(id);
                if (record == null)
                {
                    return false;
                }

                record.ResultData = resultData;
                record.Success = success;
                record.ErrorMessage = errorMessage;
                record.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated test execution record {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating test execution record {id}");
                throw;
            }
        }

        public async Task<ExecutionPerformanceMetrics> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var records = await _context.TestExecutionRecords
                    .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate && r.ExecutionTimeMs.HasValue)
                    .ToListAsync();

                var executionTimes = records.Select(r => r.ExecutionTimeMs!.Value).ToList();

                var metrics = new ExecutionPerformanceMetrics();

                if (executionTimes.Any())
                {
                    metrics.FastestExecutionMs = executionTimes.Min();
                    metrics.SlowestExecutionMs = executionTimes.Max();
                    metrics.AverageExecutionMs = executionTimes.Average();

                    // Calculate median
                    var sortedTimes = executionTimes.OrderBy(t => t).ToList();
                    metrics.MedianExecutionMs = sortedTimes.Count % 2 == 0
                        ? (sortedTimes[sortedTimes.Count / 2 - 1] + sortedTimes[sortedTimes.Count / 2]) / 2.0
                        : sortedTimes[sortedTimes.Count / 2];

                    // Calculate 95th percentile
                    var p95Index = (int)Math.Ceiling(sortedTimes.Count * 0.95) - 1;
                    metrics.P95ExecutionMs = sortedTimes[Math.Max(0, p95Index)];
                }

                // Performance by device type
                metrics.PerformanceByDevice = records
                    .Where(r => !string.IsNullOrEmpty(r.DeviceType))
                    .GroupBy(r => r.DeviceType!)
                    .ToDictionary(g => g.Key, g => new DevicePerformanceMetrics
                    {
                        AverageExecutionMs = g.Average(r => r.ExecutionTimeMs!.Value),
                        SuccessRate = (double)g.Count(r => r.Success) / g.Count() * 100,
                        AverageCodeQuality = g.Where(r => r.CodeQualityScore.HasValue).Select(r => r.CodeQualityScore!.Value).DefaultIfEmpty(0).Average(),
                        TotalExecutions = g.Count()
                    });

                // Performance by test type
                metrics.PerformanceByTestType = records
                    .Where(r => !string.IsNullOrEmpty(r.TestType))
                    .GroupBy(r => r.TestType!)
                    .ToDictionary(g => g.Key, g => new TestTypePerformanceMetrics
                    {
                        AverageExecutionMs = g.Average(r => r.ExecutionTimeMs!.Value),
                        SuccessRate = (double)g.Count(r => r.Success) / g.Count() * 100,
                        AverageCodeQuality = g.Where(r => r.CodeQualityScore.HasValue).Select(r => r.CodeQualityScore!.Value).DefaultIfEmpty(0).Average(),
                        TotalExecutions = g.Count(),
                        AverageTokensUsed = g.Where(r => r.TokensUsed.HasValue).Select(r => r.TokensUsed!.Value).DefaultIfEmpty(0).Average()
                    });

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                throw;
            }
        }
    }
}