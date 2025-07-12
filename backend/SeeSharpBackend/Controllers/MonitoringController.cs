using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.Monitoring;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// 性能监控控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MonitoringController : ControllerBase
    {
        private readonly IPerformanceMonitoringService _performanceMonitoringService;
        private readonly ILogger<MonitoringController> _logger;

        public MonitoringController(
            IPerformanceMonitoringService performanceMonitoringService,
            ILogger<MonitoringController> logger)
        {
            _performanceMonitoringService = performanceMonitoringService;
            _logger = logger;
        }

        /// <summary>
        /// 获取系统性能指标
        /// </summary>
        /// <returns>系统性能指标</returns>
        [HttpGet("system")]
        public async Task<ActionResult<SystemPerformanceMetrics>> GetSystemMetrics()
        {
            try
            {
                var metrics = await _performanceMonitoringService.GetSystemMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
                return StatusCode(500, new { error = "Failed to get system metrics", details = ex.Message });
            }
        }

        /// <summary>
        /// 获取应用程序性能指标
        /// </summary>
        /// <returns>应用程序性能指标</returns>
        [HttpGet("application")]
        public async Task<ActionResult<ApplicationPerformanceMetrics>> GetApplicationMetrics()
        {
            try
            {
                var metrics = await _performanceMonitoringService.GetApplicationMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application metrics");
                return StatusCode(500, new { error = "Failed to get application metrics", details = ex.Message });
            }
        }

        /// <summary>
        /// 获取数据采集性能指标
        /// </summary>
        /// <returns>数据采集性能指标</returns>
        [HttpGet("data-acquisition")]
        public async Task<ActionResult<DataAcquisitionMetrics>> GetDataAcquisitionMetrics()
        {
            try
            {
                var metrics = await _performanceMonitoringService.GetDataAcquisitionMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data acquisition metrics");
                return StatusCode(500, new { error = "Failed to get data acquisition metrics", details = ex.Message });
            }
        }

        /// <summary>
        /// 获取所有性能指标
        /// </summary>
        /// <returns>完整的性能指标集合</returns>
        [HttpGet("all")]
        public async Task<ActionResult<object>> GetAllMetrics()
        {
            try
            {
                var systemMetrics = await _performanceMonitoringService.GetSystemMetricsAsync();
                var applicationMetrics = await _performanceMonitoringService.GetApplicationMetricsAsync();
                var dataAcquisitionMetrics = await _performanceMonitoringService.GetDataAcquisitionMetricsAsync();

                var allMetrics = new
                {
                    System = systemMetrics,
                    Application = applicationMetrics,
                    DataAcquisition = dataAcquisitionMetrics,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(allMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all metrics");
                return StatusCode(500, new { error = "Failed to get all metrics", details = ex.Message });
            }
        }

        /// <summary>
        /// 记录自定义指标
        /// </summary>
        /// <param name="request">自定义指标请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("custom-metric")]
        public ActionResult RecordCustomMetric([FromBody] CustomMetricRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest(new { error = "Metric name is required" });
                }

                _performanceMonitoringService.RecordCustomMetric(request.Name, request.Value, request.Labels);
                return Ok(new { message = "Custom metric recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording custom metric");
                return StatusCode(500, new { error = "Failed to record custom metric", details = ex.Message });
            }
        }

        /// <summary>
        /// 增加计数器
        /// </summary>
        /// <param name="request">计数器请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("counter")]
        public ActionResult IncrementCounter([FromBody] CounterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest(new { error = "Counter name is required" });
                }

                _performanceMonitoringService.IncrementCounter(request.Name, request.Labels);
                return Ok(new { message = "Counter incremented successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter");
                return StatusCode(500, new { error = "Failed to increment counter", details = ex.Message });
            }
        }

        /// <summary>
        /// 记录直方图数据
        /// </summary>
        /// <param name="request">直方图请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("histogram")]
        public ActionResult RecordHistogram([FromBody] HistogramRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest(new { error = "Histogram name is required" });
                }

                _performanceMonitoringService.RecordHistogram(request.Name, request.Value, request.Labels);
                return Ok(new { message = "Histogram data recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording histogram");
                return StatusCode(500, new { error = "Failed to record histogram", details = ex.Message });
            }
        }

        /// <summary>
        /// 健康检查端点
        /// </summary>
        /// <returns>健康状态</returns>
        [HttpGet("health")]
        public async Task<ActionResult<object>> GetHealthStatus()
        {
            try
            {
                var systemMetrics = await _performanceMonitoringService.GetSystemMetricsAsync();
                var applicationMetrics = await _performanceMonitoringService.GetApplicationMetricsAsync();

                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Uptime = systemMetrics.Uptime,
                    CpuUsage = systemMetrics.CpuUsagePercent,
                    MemoryUsage = systemMetrics.MemoryUsagePercent,
                    DiskUsage = systemMetrics.DiskUsagePercent,
                    ThreadCount = systemMetrics.ThreadCount,
                    GcMemory = applicationMetrics.GcTotalMemoryBytes,
                    Checks = new
                    {
                        CpuOk = systemMetrics.CpuUsagePercent < 90,
                        MemoryOk = systemMetrics.MemoryUsagePercent < 90,
                        DiskOk = systemMetrics.DiskUsagePercent < 90
                    }
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                return StatusCode(500, new 
                { 
                    Status = "Unhealthy", 
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message 
                });
            }
        }
    }

    /// <summary>
    /// 自定义指标请求
    /// </summary>
    public class CustomMetricRequest
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public Dictionary<string, string>? Labels { get; set; }
    }

    /// <summary>
    /// 计数器请求
    /// </summary>
    public class CounterRequest
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string>? Labels { get; set; }
    }

    /// <summary>
    /// 直方图请求
    /// </summary>
    public class HistogramRequest
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public Dictionary<string, string>? Labels { get; set; }
    }
}
