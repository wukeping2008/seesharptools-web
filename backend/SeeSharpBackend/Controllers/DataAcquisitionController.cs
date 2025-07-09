using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.DataAcquisition;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// 数据采集控制器
    /// 提供高性能多线程数据采集功能的REST API接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataAcquisitionController : ControllerBase
    {
        private readonly IDataAcquisitionEngine _acquisitionEngine;
        private readonly ILogger<DataAcquisitionController> _logger;

        public DataAcquisitionController(
            IDataAcquisitionEngine acquisitionEngine,
            ILogger<DataAcquisitionController> logger)
        {
            _acquisitionEngine = acquisitionEngine;
            _logger = logger;
        }

        /// <summary>
        /// 启动数据采集任务
        /// </summary>
        /// <param name="request">采集任务启动请求</param>
        /// <returns>启动结果</returns>
        [HttpPost("start")]
        public async Task<IActionResult> StartAcquisition([FromBody] StartAcquisitionRequest request)
        {
            try
            {
                _logger.LogInformation("启动数据采集任务请求: TaskId={TaskId}, DeviceId={DeviceId}", 
                    request.TaskId, request.Configuration.DeviceId);

                var result = await _acquisitionEngine.StartAcquisitionAsync(request.TaskId, request.Configuration);
                
                if (result)
                {
                    return Ok(new { success = true, message = "数据采集任务启动成功", taskId = request.TaskId });
                }
                else
                {
                    return BadRequest(new { success = false, message = "数据采集任务启动失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动数据采集任务时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 停止数据采集任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>停止结果</returns>
        [HttpPost("stop/{taskId}")]
        public async Task<IActionResult> StopAcquisition(int taskId)
        {
            try
            {
                _logger.LogInformation("停止数据采集任务: TaskId={TaskId}", taskId);

                var result = await _acquisitionEngine.StopAcquisitionAsync(taskId);
                
                if (result)
                {
                    return Ok(new { success = true, message = "数据采集任务停止成功", taskId });
                }
                else
                {
                    return BadRequest(new { success = false, message = "数据采集任务停止失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止数据采集任务时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 暂停数据采集任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>暂停结果</returns>
        [HttpPost("pause/{taskId}")]
        public async Task<IActionResult> PauseAcquisition(int taskId)
        {
            try
            {
                var result = await _acquisitionEngine.PauseAcquisitionAsync(taskId);
                
                if (result)
                {
                    return Ok(new { success = true, message = "数据采集任务暂停成功", taskId });
                }
                else
                {
                    return BadRequest(new { success = false, message = "数据采集任务暂停失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "暂停数据采集任务时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 恢复数据采集任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>恢复结果</returns>
        [HttpPost("resume/{taskId}")]
        public async Task<IActionResult> ResumeAcquisition(int taskId)
        {
            try
            {
                var result = await _acquisitionEngine.ResumeAcquisitionAsync(taskId);
                
                if (result)
                {
                    return Ok(new { success = true, message = "数据采集任务恢复成功", taskId });
                }
                else
                {
                    return BadRequest(new { success = false, message = "数据采集任务恢复失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "恢复数据采集任务时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务状态</returns>
        [HttpGet("status/{taskId}")]
        public async Task<IActionResult> GetTaskStatus(int taskId)
        {
            try
            {
                var status = await _acquisitionEngine.GetTaskStatusAsync(taskId);
                return Ok(status);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务状态时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取缓冲区状态
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>缓冲区状态</returns>
        [HttpGet("buffer-status/{taskId}")]
        public async Task<IActionResult> GetBufferStatus(int taskId)
        {
            try
            {
                var status = await _acquisitionEngine.GetBufferStatusAsync(taskId);
                return Ok(status);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓冲区状态时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 配置缓冲区
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="config">缓冲区配置</param>
        /// <returns>配置结果</returns>
        [HttpPost("configure-buffer/{taskId}")]
        public async Task<IActionResult> ConfigureBuffer(int taskId, [FromBody] BufferConfiguration config)
        {
            try
            {
                var result = await _acquisitionEngine.ConfigureBufferAsync(taskId, config);
                
                if (result)
                {
                    return Ok(new { success = true, message = "缓冲区配置成功", taskId });
                }
                else
                {
                    return BadRequest(new { success = false, message = "缓冲区配置失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置缓冲区时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>清空结果</returns>
        [HttpPost("clear-buffer/{taskId}")]
        public async Task<IActionResult> ClearBuffer(int taskId)
        {
            try
            {
                var result = await _acquisitionEngine.ClearBufferAsync(taskId);
                
                if (result)
                {
                    return Ok(new { success = true, message = "缓冲区清空成功", taskId });
                }
                else
                {
                    return BadRequest(new { success = false, message = "缓冲区清空失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空缓冲区时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取性能统计
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>性能统计</returns>
        [HttpGet("performance/{taskId}")]
        public async Task<IActionResult> GetPerformanceStats(int taskId)
        {
            try
            {
                var stats = await _acquisitionEngine.GetPerformanceStatsAsync(taskId);
                return Ok(stats);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取性能统计时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取所有活跃任务
        /// </summary>
        /// <returns>活跃任务列表</returns>
        [HttpGet("active-tasks")]
        public async Task<IActionResult> GetActiveTasks()
        {
            try
            {
                var tasks = await _acquisitionEngine.GetActiveTasksAsync();
                return Ok(new { tasks = tasks.ToArray() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活跃任务时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 设置数据质量检查
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="config">质量检查配置</param>
        /// <returns>设置结果</returns>
        [HttpPost("quality-check/{taskId}")]
        public async Task<IActionResult> SetDataQualityCheck(int taskId, [FromBody] DataQualityConfiguration config)
        {
            try
            {
                var result = await _acquisitionEngine.SetDataQualityCheckAsync(taskId, config);
                
                if (result)
                {
                    return Ok(new { success = true, message = "数据质量检查配置成功", taskId });
                }
                else
                {
                    return BadRequest(new { success = false, message = "数据质量检查配置失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置数据质量检查时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取数据质量报告
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>质量报告</returns>
        [HttpGet("quality-report/{taskId}")]
        public async Task<IActionResult> GetDataQualityReport(int taskId)
        {
            try
            {
                var report = await _acquisitionEngine.GetDataQualityReportAsync(taskId);
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据质量报告时发生错误");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取实时数据流（Server-Sent Events）
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>数据流</returns>
        [HttpGet("stream/{taskId}")]
        public async Task GetDataStream(int taskId)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                await foreach (var dataPacket in _acquisitionEngine.GetDataStreamAsync(taskId, HttpContext.RequestAborted))
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        taskId = dataPacket.TaskId,
                        timestamp = dataPacket.Timestamp,
                        sequenceNumber = dataPacket.SequenceNumber,
                        sampleCount = dataPacket.SampleCount,
                        sampleRate = dataPacket.SampleRate,
                        qualityFlags = dataPacket.QualityFlags.ToString(),
                        channelCount = dataPacket.ChannelData.Count,
                        // 只发送少量样本数据以避免过大的响应
                        sampleData = dataPacket.ChannelData.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Take(10).ToArray() // 只取前10个样本
                        )
                    });

                    await Response.WriteAsync($"data: {json}\n\n");
                    await Response.Body.FlushAsync();

                    // 控制发送频率
                    await Task.Delay(100, HttpContext.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("数据流连接已断开: TaskId={TaskId}", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据流传输时发生错误: TaskId={TaskId}", taskId);
                await Response.WriteAsync($"event: error\ndata: {ex.Message}\n\n");
            }
        }
    }

    /// <summary>
    /// 启动采集任务请求
    /// </summary>
    public class StartAcquisitionRequest
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 采集配置
        /// </summary>
        public AcquisitionConfiguration Configuration { get; set; } = new();
    }
}
