using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.TestExecution;
using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// Test execution management API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestExecutionController : ControllerBase
    {
        private readonly ITestExecutionService _testExecutionService;
        private readonly ILogger<TestExecutionController> _logger;

        public TestExecutionController(
            ITestExecutionService testExecutionService,
            ILogger<TestExecutionController> logger)
        {
            _testExecutionService = testExecutionService;
            _logger = logger;
        }

        /// <summary>
        /// Save test execution result
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> SaveExecutionResult([FromBody] TestExecutionRequest request)
        {
            try
            {
                var id = await _testExecutionService.SaveExecutionResultAsync(request);
                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving execution result");
                return StatusCode(500, new { error = "Failed to save execution result", details = ex.Message });
            }
        }

        /// <summary>
        /// Get test execution result by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetExecutionResult(int id)
        {
            try
            {
                var result = await _testExecutionService.GetExecutionResultAsync(id);
                if (result == null)
                {
                    return NotFound(new { error = "Execution result not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting execution result {id}");
                return StatusCode(500, new { error = "Failed to get execution result", details = ex.Message });
            }
        }

        /// <summary>
        /// Get test execution history
        /// </summary>
        [HttpPost("history")]
        public async Task<ActionResult> GetExecutionHistory([FromBody] TestExecutionHistoryQuery query)
        {
            try
            {
                var results = await _testExecutionService.GetExecutionHistoryAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution history");
                return StatusCode(500, new { error = "Failed to get execution history", details = ex.Message });
            }
        }

        /// <summary>
        /// Get execution statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetExecutionStatistics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _testExecutionService.GetExecutionStatisticsAsync(startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution statistics");
                return StatusCode(500, new { error = "Failed to get execution statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete test execution record
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteExecutionRecord(int id)
        {
            try
            {
                var deleted = await _testExecutionService.DeleteExecutionRecordAsync(id);
                if (!deleted)
                {
                    return NotFound(new { error = "Execution record not found" });
                }
                return Ok(new { message = "Execution record deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting execution record {id}");
                return StatusCode(500, new { error = "Failed to delete execution record", details = ex.Message });
            }
        }

        /// <summary>
        /// Get executions by device type
        /// </summary>
        [HttpGet("device/{deviceType}")]
        public async Task<ActionResult> GetExecutionsByDeviceType(
            string deviceType,
            [FromQuery] int maxCount = 50)
        {
            try
            {
                var results = await _testExecutionService.GetExecutionsByDeviceTypeAsync(deviceType, maxCount);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting executions by device type {deviceType}");
                return StatusCode(500, new { error = "Failed to get executions by device type", details = ex.Message });
            }
        }

        /// <summary>
        /// Get executions by test type
        /// </summary>
        [HttpGet("test/{testType}")]
        public async Task<ActionResult> GetExecutionsByTestType(
            string testType,
            [FromQuery] int maxCount = 50)
        {
            try
            {
                var results = await _testExecutionService.GetExecutionsByTestTypeAsync(testType, maxCount);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting executions by test type {testType}");
                return StatusCode(500, new { error = "Failed to get executions by test type", details = ex.Message });
            }
        }

        /// <summary>
        /// Update execution result
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateExecutionResult(
            int id,
            [FromBody] UpdateExecutionRequest request)
        {
            try
            {
                var updated = await _testExecutionService.UpdateExecutionResultAsync(
                    id, 
                    request.ResultData, 
                    request.Success, 
                    request.ErrorMessage);
                
                if (!updated)
                {
                    return NotFound(new { error = "Execution record not found" });
                }
                
                return Ok(new { message = "Execution result updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating execution result {id}");
                return StatusCode(500, new { error = "Failed to update execution result", details = ex.Message });
            }
        }

        /// <summary>
        /// Get performance metrics
        /// </summary>
        [HttpGet("performance")]
        public async Task<ActionResult> GetPerformanceMetrics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var metrics = await _testExecutionService.GetPerformanceMetricsAsync(startDate, endDate);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return StatusCode(500, new { error = "Failed to get performance metrics", details = ex.Message });
            }
        }

        /// <summary>
        /// Get recent executions (last 24 hours)
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult> GetRecentExecutions([FromQuery] int maxCount = 20)
        {
            try
            {
                var query = new TestExecutionHistoryQuery
                {
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now,
                    MaxCount = maxCount,
                    SortOrder = TestExecutionSortOrder.NewestFirst
                };
                
                var results = await _testExecutionService.GetExecutionHistoryAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent executions");
                return StatusCode(500, new { error = "Failed to get recent executions", details = ex.Message });
            }
        }

        /// <summary>
        /// Get execution summary for dashboard
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult> GetExecutionSummary()
        {
            try
            {
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30); // Last 30 days
                
                var statistics = await _testExecutionService.GetExecutionStatisticsAsync(startDate, endDate);
                var recentExecutions = await _testExecutionService.GetExecutionHistoryAsync(new TestExecutionHistoryQuery
                {
                    StartDate = endDate.AddDays(-7), // Last 7 days
                    MaxCount = 10,
                    SortOrder = TestExecutionSortOrder.NewestFirst
                });
                
                var summary = new
                {
                    Statistics = statistics,
                    RecentExecutions = recentExecutions,
                    Period = new { StartDate = startDate, EndDate = endDate }
                };
                
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution summary");
                return StatusCode(500, new { error = "Failed to get execution summary", details = ex.Message });
            }
        }
    }

    /// <summary>
    /// Update execution request model
    /// </summary>
    public class UpdateExecutionRequest
    {
        /// <summary>
        /// Result data (JSON)
        /// </summary>
        [Required]
        public string ResultData { get; set; } = string.Empty;

        /// <summary>
        /// Success flag
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}