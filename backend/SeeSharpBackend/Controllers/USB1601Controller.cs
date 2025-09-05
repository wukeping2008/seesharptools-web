using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.Hardware;
using System.Threading.Tasks;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// USB-1601硬件控制器
    /// 提供统一的HTTP API接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class USB1601Controller : ControllerBase
    {
        private readonly ILogger<USB1601Controller> _logger;
        private readonly IUSB1601Hardware _hardware;

        public USB1601Controller(
            ILogger<USB1601Controller> logger,
            IUSB1601Hardware hardware)
        {
            _logger = logger;
            _hardware = hardware;
        }

        /// <summary>
        /// 获取设备状态
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var deviceInfo = _hardware.IsInitialized 
                    ? await _hardware.GetDeviceInfoAsync() 
                    : null;

                return Ok(new
                {
                    isInitialized = _hardware.IsInitialized,
                    isSimulationMode = _hardware.IsSimulationMode,
                    deviceId = _hardware.DeviceId,
                    deviceInfo = deviceInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备状态失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 初始化设备
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize([FromBody] InitializeRequest request)
        {
            try
            {
                var result = await _hardware.InitializeAsync(request?.DeviceId ?? "0");
                if (result)
                {
                    var deviceInfo = await _hardware.GetDeviceInfoAsync();
                    return Ok(new
                    {
                        success = true,
                        deviceInfo = deviceInfo
                    });
                }
                
                return BadRequest(new { success = false, error = "初始化失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化设备失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 断开设备连接
        /// </summary>
        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect()
        {
            try
            {
                var result = await _hardware.DisconnectAsync();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开设备连接失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 发现所有设备
        /// </summary>
        [HttpGet("discover")]
        public async Task<IActionResult> DiscoverDevices()
        {
            try
            {
                var devices = await _hardware.DiscoverDevicesAsync();
                return Ok(new
                {
                    success = true,
                    deviceCount = devices.Count,
                    devices = devices
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发现设备失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #region 模拟输入(AI)

        /// <summary>
        /// 配置AI通道
        /// </summary>
        [HttpPost("ai/configure")]
        public async Task<IActionResult> ConfigureAI([FromBody] AIConfiguration config)
        {
            try
            {
                var result = await _hardware.ConfigureAIChannelsAsync(config);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置AI通道失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 单点读取AI
        /// </summary>
        [HttpPost("ai/read-single")]
        public async Task<IActionResult> ReadAISingle([FromBody] ReadAIRequest request)
        {
            try
            {
                var data = await _hardware.ReadAISingleAsync(request.Channels);
                return Ok(new
                {
                    success = true,
                    channels = request.Channels,
                    data = data,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取AI数据失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 有限点采集AI
        /// </summary>
        [HttpPost("ai/read-finite")]
        public async Task<IActionResult> ReadAIFinite([FromBody] ReadAIFiniteRequest request)
        {
            try
            {
                var data = await _hardware.ReadAIFiniteAsync(
                    request.Channels, 
                    request.SamplesPerChannel, 
                    request.SampleRate);
                    
                return Ok(new
                {
                    success = true,
                    channels = request.Channels,
                    samplesPerChannel = request.SamplesPerChannel,
                    sampleRate = request.SampleRate,
                    data = data,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "有限点采集失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region 模拟输出(AO)

        /// <summary>
        /// 配置AO通道
        /// </summary>
        [HttpPost("ao/configure")]
        public async Task<IActionResult> ConfigureAO([FromBody] AOConfiguration config)
        {
            try
            {
                var result = await _hardware.ConfigureAOChannelsAsync(config);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置AO通道失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 单点输出AO
        /// </summary>
        [HttpPost("ao/write-single")]
        public async Task<IActionResult> WriteAOSingle([FromBody] WriteAORequest request)
        {
            try
            {
                var result = await _hardware.WriteAOSingleAsync(request.Channel, request.Value);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AO输出失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 多通道输出AO
        /// </summary>
        [HttpPost("ao/write-multiple")]
        public async Task<IActionResult> WriteAOMultiple([FromBody] WriteAOMultipleRequest request)
        {
            try
            {
                var result = await _hardware.WriteAOMultipleAsync(request.Channels, request.Values);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AO多通道输出失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region 数字输入输出(DIO)

        /// <summary>
        /// 配置DIO端口
        /// </summary>
        [HttpPost("dio/configure")]
        public async Task<IActionResult> ConfigureDIO([FromBody] ConfigureDIORequest request)
        {
            try
            {
                var result = await _hardware.ConfigureDIOPortAsync(request.Port, request.Direction);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置DIO端口失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 读取DI端口
        /// </summary>
        [HttpGet("dio/read/{port}")]
        public async Task<IActionResult> ReadDIPort(int port)
        {
            try
            {
                var value = await _hardware.ReadDIPortAsync(port);
                return Ok(new
                {
                    success = true,
                    port = port,
                    value = value,
                    binaryString = Convert.ToString(value, 2).PadLeft(8, '0')
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取DI端口失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 写入DO端口
        /// </summary>
        [HttpPost("dio/write")]
        public async Task<IActionResult> WriteDOPort([FromBody] WriteDORequest request)
        {
            try
            {
                var result = await _hardware.WriteDOPortAsync(request.Port, request.Value);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入DO端口失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region 计数器

        /// <summary>
        /// 配置计数器
        /// </summary>
        [HttpPost("counter/configure")]
        public async Task<IActionResult> ConfigureCounter([FromBody] ConfigureCounterRequest request)
        {
            try
            {
                var result = await _hardware.ConfigureCounterAsync(request.Counter, request.Mode);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置计数器失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 读取计数值
        /// </summary>
        [HttpGet("counter/read/{counter}")]
        public async Task<IActionResult> ReadCounter(int counter)
        {
            try
            {
                var value = await _hardware.ReadCounterAsync(counter);
                return Ok(new
                {
                    success = true,
                    counter = counter,
                    value = value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取计数器失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 重置计数器
        /// </summary>
        [HttpPost("counter/reset/{counter}")]
        public async Task<IActionResult> ResetCounter(int counter)
        {
            try
            {
                var result = await _hardware.ResetCounterAsync(counter);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置计数器失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion
    }

    #region 请求模型

    public class InitializeRequest
    {
        public string DeviceId { get; set; } = "0";
    }

    public class ReadAIRequest
    {
        public int[] Channels { get; set; } = Array.Empty<int>();
    }

    public class ReadAIFiniteRequest
    {
        public int[] Channels { get; set; } = Array.Empty<int>();
        public int SamplesPerChannel { get; set; } = 100;
        public double SampleRate { get; set; } = 1000;
    }

    public class WriteAORequest
    {
        public int Channel { get; set; }
        public double Value { get; set; }
    }

    public class WriteAOMultipleRequest
    {
        public int[] Channels { get; set; } = Array.Empty<int>();
        public double[] Values { get; set; } = Array.Empty<double>();
    }

    public class ConfigureDIORequest
    {
        public int Port { get; set; }
        public DIODirection Direction { get; set; }
    }

    public class WriteDORequest
    {
        public int Port { get; set; }
        public byte Value { get; set; }
    }

    public class ConfigureCounterRequest
    {
        public int Counter { get; set; }
        public CounterMode Mode { get; set; }
    }

    #endregion
}