using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Models.MISD;
using SeeSharpBackend.Services.MISD;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// MISD API控制器
    /// 提供标准化的硬件抽象层RESTful API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MISDController : ControllerBase
    {
        private readonly IMISDService _misdService;
        private readonly ILogger<MISDController> _logger;

        public MISDController(IMISDService misdService, ILogger<MISDController> logger)
        {
            _misdService = misdService;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有可用设备
        /// </summary>
        /// <returns>设备列表</returns>
        [HttpGet("devices")]
        public async Task<ActionResult<List<HardwareDevice>>> GetDevices()
        {
            try
            {
                var devices = await _misdService.DiscoverDevicesAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备列表失败");
                return StatusCode(500, new { message = "获取设备列表失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取指定设备信息
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>设备信息</returns>
        [HttpGet("devices/{deviceId}")]
        public async Task<ActionResult<HardwareDevice>> GetDevice(int deviceId)
        {
            try
            {
                var device = await _misdService.GetDeviceAsync(deviceId);
                if (device == null)
                {
                    return NotFound(new { message = $"设备 {deviceId} 不存在" });
                }
                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备 {DeviceId} 信息失败", deviceId);
                return StatusCode(500, new { message = "获取设备信息失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取设备支持的MISD功能
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>MISD功能列表</returns>
        [HttpGet("devices/{deviceId}/functions")]
        public async Task<ActionResult<List<MISDDefinition>>> GetDeviceFunctions(int deviceId)
        {
            try
            {
                var functions = await _misdService.GetDeviceFunctionsAsync(deviceId);
                return Ok(functions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备 {DeviceId} 功能列表失败", deviceId);
                return StatusCode(500, new { message = "获取设备功能列表失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="taskConfig">任务配置</param>
        /// <returns>创建的任务配置</returns>
        [HttpPost("tasks")]
        public async Task<ActionResult<MISDTaskConfiguration>> CreateTask([FromBody] MISDTaskConfiguration taskConfig)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdTask = await _misdService.CreateTaskAsync(taskConfig);
                return CreatedAtAction(nameof(GetTask), new { taskId = createdTask.Id }, createdTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务失败");
                return StatusCode(500, new { message = "创建任务失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务配置</returns>
        [HttpGet("tasks/{taskId}")]
        public async Task<ActionResult<MISDTaskConfiguration>> GetTask(int taskId)
        {
            try
            {
                var status = await _misdService.GetTaskStatusAsync(taskId);
                if (status == Models.MISD.TaskStatus.Error)
                {
                    return NotFound(new { message = $"任务 {taskId} 不存在" });
                }

                // 这里应该返回完整的任务配置，暂时只返回状态
                return Ok(new { taskId, status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务 {TaskId} 信息失败", taskId);
                return StatusCode(500, new { message = "获取任务信息失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("tasks/{taskId}/start")]
        public async Task<ActionResult> StartTask(int taskId)
        {
            try
            {
                var success = await _misdService.StartTaskAsync(taskId);
                if (!success)
                {
                    return BadRequest(new { message = $"启动任务 {taskId} 失败" });
                }

                return Ok(new { message = $"任务 {taskId} 已启动" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动任务 {TaskId} 失败", taskId);
                return StatusCode(500, new { message = "启动任务失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("tasks/{taskId}/stop")]
        public async Task<ActionResult> StopTask(int taskId)
        {
            try
            {
                var success = await _misdService.StopTaskAsync(taskId);
                if (!success)
                {
                    return BadRequest(new { message = $"停止任务 {taskId} 失败" });
                }

                return Ok(new { message = $"任务 {taskId} 已停止" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止任务 {TaskId} 失败", taskId);
                return StatusCode(500, new { message = "停止任务失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务状态</returns>
        [HttpGet("tasks/{taskId}/status")]
        public async Task<ActionResult<object>> GetTaskStatus(int taskId)
        {
            try
            {
                var status = await _misdService.GetTaskStatusAsync(taskId);
                var availableSamples = await _misdService.GetAvailableSamplesAsync(taskId);

                return Ok(new 
                { 
                    taskId, 
                    status = status.ToString(), 
                    availableSamples,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务 {TaskId} 状态失败", taskId);
                return StatusCode(500, new { message = "获取任务状态失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="samplesPerChannel">每通道采样点数</param>
        /// <param name="timeout">超时时间(ms)</param>
        /// <returns>采集的数据</returns>
        [HttpGet("tasks/{taskId}/data")]
        public async Task<ActionResult<object>> ReadData(int taskId, [FromQuery] int samplesPerChannel = 1000, [FromQuery] int timeout = 10000)
        {
            try
            {
                var data = await _misdService.ReadDataAsync(taskId, samplesPerChannel, timeout);
                
                // 将二维数组转换为JSON友好的格式
                var result = new
                {
                    taskId,
                    samplesPerChannel,
                    channelCount = data.GetLength(1),
                    data = ConvertToJaggedArray(data),
                    timestamp = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取任务 {TaskId} 数据失败", taskId);
                return StatusCode(500, new { message = "读取数据失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="request">写入数据请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("tasks/{taskId}/data")]
        public async Task<ActionResult> WriteData(int taskId, [FromBody] WriteDataRequest request)
        {
            try
            {
                if (request.Data == null || request.Data.Length == 0)
                {
                    return BadRequest(new { message = "数据不能为空" });
                }

                // 将交错数组转换为二维数组
                var data = ConvertTo2DArray(request.Data);
                var success = await _misdService.WriteDataAsync(taskId, data, request.Timeout);

                if (!success)
                {
                    return BadRequest(new { message = $"写入任务 {taskId} 数据失败" });
                }

                return Ok(new { message = $"任务 {taskId} 数据写入成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入任务 {TaskId} 数据失败", taskId);
                return StatusCode(500, new { message = "写入数据失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 发送软件触发
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("tasks/{taskId}/trigger")]
        public async Task<ActionResult> SendSoftwareTrigger(int taskId)
        {
            try
            {
                var success = await _misdService.SendSoftwareTriggerAsync(taskId);
                if (!success)
                {
                    return BadRequest(new { message = $"发送软件触发到任务 {taskId} 失败" });
                }

                return Ok(new { message = $"软件触发已发送到任务 {taskId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送软件触发到任务 {TaskId} 失败", taskId);
                return StatusCode(500, new { message = "发送软件触发失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 等待任务完成
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="timeout">超时时间(ms)</param>
        /// <returns>操作结果</returns>
        [HttpPost("tasks/{taskId}/wait")]
        public async Task<ActionResult> WaitUntilDone(int taskId, [FromQuery] int timeout = -1)
        {
            try
            {
                var success = await _misdService.WaitUntilDoneAsync(taskId, timeout);
                if (!success)
                {
                    return BadRequest(new { message = $"等待任务 {taskId} 完成失败或超时" });
                }

                return Ok(new { message = $"任务 {taskId} 已完成" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "等待任务 {TaskId} 完成失败", taskId);
                return StatusCode(500, new { message = "等待任务完成失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("tasks/{taskId}")]
        public async Task<ActionResult> DeleteTask(int taskId)
        {
            try
            {
                var success = await _misdService.DisposeTaskAsync(taskId);
                if (!success)
                {
                    return BadRequest(new { message = $"删除任务 {taskId} 失败" });
                }

                return Ok(new { message = $"任务 {taskId} 已删除" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除任务 {TaskId} 失败", taskId);
                return StatusCode(500, new { message = "删除任务失败", error = ex.Message });
            }
        }

        #region 辅助方法

        /// <summary>
        /// 将二维数组转换为交错数组（JSON友好）
        /// </summary>
        private double[][] ConvertToJaggedArray(double[,] array2D)
        {
            int rows = array2D.GetLength(0);
            int cols = array2D.GetLength(1);
            double[][] jaggedArray = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = array2D[i, j];
                }
            }

            return jaggedArray;
        }

        /// <summary>
        /// 将交错数组转换为二维数组
        /// </summary>
        private double[,] ConvertTo2DArray(double[][] jaggedArray)
        {
            int rows = jaggedArray.Length;
            int cols = jaggedArray[0].Length;
            double[,] array2D = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    array2D[i, j] = jaggedArray[i][j];
                }
            }

            return array2D;
        }

        #endregion
    }

    /// <summary>
    /// 写入数据请求模型
    /// </summary>
    public class WriteDataRequest
    {
        /// <summary>
        /// 数据（交错数组格式）
        /// </summary>
        public double[][] Data { get; set; } = Array.Empty<double[]>();

        /// <summary>
        /// 超时时间(ms)
        /// </summary>
        public int Timeout { get; set; } = 10000;
    }
}
