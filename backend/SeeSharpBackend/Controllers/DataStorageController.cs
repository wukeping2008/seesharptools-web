using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.DataStorage;
using System;
using System.Threading.Tasks;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// 数据存储控制器
    /// 提供数据存储、查询、统计、删除等API接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataStorageController : ControllerBase
    {
        private readonly IDataStorageService _dataStorageService;
        private readonly ILogger<DataStorageController> _logger;

        public DataStorageController(
            IDataStorageService dataStorageService,
            ILogger<DataStorageController> logger)
        {
            _dataStorageService = dataStorageService;
            _logger = logger;
        }

        /// <summary>
        /// 写入实时数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="data">数据包</param>
        /// <param name="compress">是否压缩</param>
        /// <returns>写入结果</returns>
        [HttpPost("write/{taskId}")]
        public async Task<ActionResult<DataWriteResult>> WriteData(
            int taskId, 
            [FromBody] RealTimeDataPacket data, 
            [FromQuery] bool compress = true)
        {
            try
            {
                var result = await _dataStorageService.WriteDataAsync(taskId, data, compress);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入数据失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 批量写入数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="dataPackets">数据包列表</param>
        /// <param name="compress">是否压缩</param>
        /// <returns>批量写入结果</returns>
        [HttpPost("write-batch/{taskId}")]
        public async Task<ActionResult<BatchWriteResult>> WriteBatchData(
            int taskId, 
            [FromBody] RealTimeDataPacket[] dataPackets, 
            [FromQuery] bool compress = true)
        {
            try
            {
                var result = await _dataStorageService.WriteBatchDataAsync(taskId, dataPackets, compress);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量写入数据失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 查询历史数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="channels">通道列表（逗号分隔）</param>
        /// <param name="maxPoints">最大数据点数</param>
        /// <returns>历史数据</returns>
        [HttpGet("query/{taskId}")]
        public async Task<ActionResult<HistoricalDataResult>> QueryHistoricalData(
            int taskId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] string? channels = null,
            [FromQuery] int maxPoints = 10000)
        {
            try
            {
                int[]? channelArray = null;
                if (!string.IsNullOrEmpty(channels))
                {
                    channelArray = channels.Split(',')
                        .Select(c => int.TryParse(c.Trim(), out var ch) ? ch : -1)
                        .Where(c => c >= 0)
                        .ToArray();
                }

                var result = await _dataStorageService.QueryHistoricalDataAsync(
                    taskId, startTime, endTime, channelArray, maxPoints);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询历史数据失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取数据统计信息
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>数据统计</returns>
        [HttpGet("statistics/{taskId}")]
        public async Task<ActionResult<DataStatistics>> GetDataStatistics(
            int taskId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            try
            {
                var result = await _dataStorageService.GetDataStatisticsAsync(taskId, startTime, endTime);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据统计失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 删除历史数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="beforeTime">删除此时间之前的数据</param>
        /// <returns>删除结果</returns>
        [HttpDelete("delete/{taskId}")]
        public async Task<ActionResult<DataDeleteResult>> DeleteData(
            int taskId,
            [FromQuery] DateTime beforeTime)
        {
            try
            {
                var result = await _dataStorageService.DeleteDataAsync(taskId, beforeTime);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除数据失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取存储状态
        /// </summary>
        /// <returns>存储状态信息</returns>
        [HttpGet("status")]
        public async Task<ActionResult<StorageStatus>> GetStorageStatus()
        {
            try
            {
                var result = await _dataStorageService.GetStorageStatusAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取存储状态失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 优化存储
        /// </summary>
        /// <param name="taskId">任务ID（可选）</param>
        /// <returns>优化结果</returns>
        [HttpPost("optimize")]
        public async Task<ActionResult<StorageOptimizationResult>> OptimizeStorage(
            [FromQuery] int? taskId = null)
        {
            try
            {
                var result = await _dataStorageService.OptimizeStorageAsync(taskId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "存储优化失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取任务存储信息
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务存储信息</returns>
        [HttpGet("task-info/{taskId}")]
        public async Task<ActionResult<object>> GetTaskStorageInfo(int taskId)
        {
            try
            {
                var status = await _dataStorageService.GetStorageStatusAsync();
                if (status.TaskInfo.TryGetValue(taskId, out var taskInfo))
                {
                    return Ok(taskInfo);
                }
                else
                {
                    return NotFound(new { error = $"任务 {taskId} 不存在" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务存储信息失败，任务ID: {TaskId}", taskId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取存储性能指标
        /// </summary>
        /// <returns>性能指标</returns>
        [HttpGet("performance")]
        public async Task<ActionResult<StoragePerformanceMetrics>> GetPerformanceMetrics()
        {
            try
            {
                var status = await _dataStorageService.GetStorageStatusAsync();
                return Ok(status.Performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取性能指标失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 数据回放接口
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="speed">回放速度倍数</param>
        /// <param name="channels">通道列表</param>
        /// <returns>回放数据流</returns>
        [HttpGet("replay/{taskId}")]
        public async Task<ActionResult> ReplayData(
            int taskId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] double speed = 1.0,
            [FromQuery] string? channels = null)
        {
            try
            {
                int[]? channelArray = null;
                if (!string.IsNullOrEmpty(channels))
                {
                    channelArray = channels.Split(',')
                        .Select(c => int.TryParse(c.Trim(), out var ch) ? ch : -1)
                        .Where(c => c >= 0)
                        .ToArray();
                }

                // 设置SSE响应头
                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");

                // 查询历史数据
                var historicalData = await _dataStorageService.QueryHistoricalDataAsync(
                    taskId, startTime, endTime, channelArray, 100000);

                if (!historicalData.Success)
                {
                    await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { error = historicalData.ErrorMessage })}\n\n");
                    return new EmptyResult();
                }

                // 按时间顺序回放数据
                var allPoints = new List<(DateTime timestamp, int channelId, double value)>();
                foreach (var channel in historicalData.Channels)
                {
                    for (int i = 0; i < channel.Value.Timestamps.Length; i++)
                    {
                        var timestamp = DateTime.UnixEpoch.AddSeconds(channel.Value.Timestamps[i]);
                        allPoints.Add((timestamp, channel.Key, channel.Value.Values[i]));
                    }
                }

                allPoints.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

                var startReplayTime = DateTime.UtcNow;
                var dataStartTime = allPoints.FirstOrDefault().timestamp;

                foreach (var point in allPoints)
                {
                    // 计算应该发送数据的时间
                    var elapsedDataTime = point.timestamp - dataStartTime;
                    var targetElapsedTime = TimeSpan.FromMilliseconds(elapsedDataTime.TotalMilliseconds / speed);
                    var actualElapsedTime = DateTime.UtcNow - startReplayTime;

                    // 等待到正确的时间
                    var waitTime = targetElapsedTime - actualElapsedTime;
                    if (waitTime > TimeSpan.Zero)
                    {
                        await Task.Delay(waitTime);
                    }

                    // 发送数据点
                    var dataPoint = new
                    {
                        timestamp = point.timestamp,
                        channelId = point.channelId,
                        value = point.value
                    };

                    await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(dataPoint)}\n\n");
                    await Response.Body.FlushAsync();

                    // 检查客户端是否断开连接
                    if (HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        break;
                    }
                }

                await Response.WriteAsync("data: {\"type\":\"end\"}\n\n");
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据回放失败，任务ID: {TaskId}", taskId);
                await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message })}\n\n");
                return new EmptyResult();
            }
        }
    }
}
