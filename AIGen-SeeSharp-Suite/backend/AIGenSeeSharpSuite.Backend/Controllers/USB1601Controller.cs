using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIGenSeeSharpSuite.Backend.Controllers
{
    [ApiController]
    [Route("api/usb1601")]
    public class USB1601Controller : ControllerBase
    {
        private static bool _isConnected = false;
        private static bool _isAcquiring = false;
        private static readonly Random _random = new Random();
        private static Timer? _dataTimer;
        private static List<double> _latestData = new List<double>();
        private static readonly object _dataLock = new object();
        private static DateTime _startTime = DateTime.Now;
        private static int _sampleRate = 1000;
        private static List<int> _activeChannels = new List<int> { 0 };

        [HttpGet("test")]
        public IActionResult TestConnection()
        {
            // Simulate hardware test - in production this would check real hardware
            bool hardwareAvailable = CheckHardwareAvailable();
            
            return Ok(new 
            { 
                success = true,  // Always return true for demo
                message = hardwareAvailable ? "Hardware detected" : "Demo mode - no hardware connected",
                isSimulated = !hardwareAvailable
            });
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                isConnected = _isConnected,
                isAcquiring = _isAcquiring,
                deviceId = _isConnected ? "USB1601-SIM-001" : null,
                timestamp = DateTime.Now
            });
        }

        [HttpPost("connect")]
        public IActionResult Connect([FromBody] ConnectRequest request)
        {
            try
            {
                // Simulate connection delay
                Thread.Sleep(500);
                
                _isConnected = true;
                _startTime = DateTime.Now;
                
                return Ok(new 
                { 
                    success = true,
                    deviceId = request?.DeviceId ?? "0",
                    message = "Connected to USB-1601 (Simulation Mode)"
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("disconnect")]
        public IActionResult Disconnect()
        {
            try
            {
                if (_isAcquiring)
                {
                    StopAcquisition();
                }
                
                _isConnected = false;
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("discover")]
        public IActionResult DiscoverDevices()
        {
            // Simulate device discovery
            var devices = new List<object>();
            
            if (CheckHardwareAvailable())
            {
                devices.Add(new 
                {
                    deviceId = "0",
                    model = "USB-1601",
                    serialNumber = "SN2024001",
                    isAvailable = true
                });
            }
            else
            {
                devices.Add(new 
                {
                    deviceId = "SIM",
                    model = "USB-1601 (Simulated)",
                    serialNumber = "SIM-001",
                    isAvailable = true
                });
            }
            
            return Ok(new { devices });
        }

        [HttpGet("info")]
        public IActionResult GetDeviceInfo()
        {
            return Ok(new
            {
                model = "USB-1601",
                channels = 16,
                maxSampleRate = 200000,
                resolution = 12,
                serialNumber = _isConnected ? "SIM-001" : "",
                firmwareVersion = "1.0.0",
                isSimulated = true
            });
        }

        [HttpPost("ai/configure")]
        public IActionResult ConfigureAI([FromBody] AIConfigRequest request)
        {
            if (!_isConnected)
            {
                return Ok(new { success = false, error = "Device not connected" });
            }
            
            _activeChannels = request?.Channels ?? new List<int> { 0 };
            _sampleRate = request?.SampleRate ?? 1000;
            
            return Ok(new { success = true });
        }

        [HttpPost("ai/start-continuous")]
        public IActionResult StartContinuous([FromBody] ContinuousAIRequest request)
        {
            if (!_isConnected)
            {
                return Ok(new { success = false, error = "Device not connected" });
            }
            
            if (_isAcquiring)
            {
                return Ok(new { success = false, error = "Already acquiring" });
            }
            
            _activeChannels = request?.Channels ?? new List<int> { 0 };
            _sampleRate = request?.SampleRate ?? 1000;
            _isAcquiring = true;
            
            // Start data generation timer
            int interval = Math.Max(10, 1000 / _sampleRate);
            _dataTimer = new Timer(GenerateData, null, 0, interval);
            
            return Ok(new { success = true });
        }

        [HttpPost("ai/stop-continuous")]
        public IActionResult StopContinuous()
        {
            StopAcquisition();
            return Ok(new { success = true });
        }

        [HttpGet("ai/read-latest")]
        public IActionResult ReadLatestData()
        {
            if (!_isConnected)
            {
                return Ok(new 
                { 
                    data = new double[0], 
                    timestamp = DateTime.Now,
                    error = "Device not connected" 
                });
            }
            
            lock (_dataLock)
            {
                var data = _latestData.ToArray();
                _latestData.Clear();
                
                return Ok(new
                {
                    data,
                    timestamp = DateTime.Now,
                    channelCount = _activeChannels.Count,
                    sampleRate = _sampleRate
                });
            }
        }

        [HttpGet("ai/read-single/{channel}")]
        public IActionResult ReadSinglePoint(int channel)
        {
            if (!_isConnected)
            {
                return Ok(new { value = 0.0, channel, error = "Device not connected" });
            }
            
            // Generate a simulated value
            double value = GenerateChannelValue(channel);
            
            return Ok(new { value, channel });
        }

        [HttpPost("ai/read-finite")]
        public async Task<IActionResult> ReadFiniteSamples([FromBody] FiniteAIRequest request)
        {
            if (!_isConnected)
            {
                return Ok(new { data = new double[0], error = "Device not connected" });
            }
            
            var channels = request?.Channels ?? new List<int> { 0 };
            var samplesPerChannel = request?.SamplesPerChannel ?? 100;
            var sampleRate = request?.SampleRate ?? 1000;
            
            // Generate simulated data
            var data = new List<double[]>();
            var delay = 1000 / sampleRate;
            
            for (int i = 0; i < samplesPerChannel; i++)
            {
                var sample = new double[channels.Count];
                for (int ch = 0; ch < channels.Count; ch++)
                {
                    sample[ch] = GenerateChannelValue(channels[ch]);
                }
                data.Add(sample);
                
                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
            }
            
            return Ok(new { data, samplesPerChannel, channelCount = channels.Count });
        }

        private void GenerateData(object? state)
        {
            if (!_isAcquiring) return;
            
            lock (_dataLock)
            {
                // Generate data for all active channels
                foreach (var channel in _activeChannels)
                {
                    _latestData.Add(GenerateChannelValue(channel));
                }
                
                // Limit buffer size
                if (_latestData.Count > _activeChannels.Count * 1000)
                {
                    _latestData.RemoveRange(0, _latestData.Count - _activeChannels.Count * 1000);
                }
            }
        }

        private double GenerateChannelValue(int channel)
        {
            // Generate different waveforms for different channels
            double time = (DateTime.Now - _startTime).TotalSeconds;
            double value = 0;
            
            switch (channel % 4)
            {
                case 0: // Sine wave
                    value = 5.0 * Math.Sin(2 * Math.PI * 1.0 * time);
                    break;
                case 1: // Square wave
                    value = 5.0 * Math.Sign(Math.Sin(2 * Math.PI * 0.5 * time));
                    break;
                case 2: // Triangle wave
                    value = 10.0 * (2 * Math.Abs(2 * (time * 0.5 - Math.Floor(time * 0.5 + 0.5))) - 1);
                    break;
                case 3: // Random noise
                    value = (_random.NextDouble() - 0.5) * 10.0;
                    break;
            }
            
            // Add some noise
            value += (_random.NextDouble() - 0.5) * 0.5;
            
            return value;
        }

        private void StopAcquisition()
        {
            _isAcquiring = false;
            _dataTimer?.Dispose();
            _dataTimer = null;
            
            lock (_dataLock)
            {
                _latestData.Clear();
            }
        }

        private bool CheckHardwareAvailable()
        {
            // In a real implementation, this would check if hardware is connected
            // For demo purposes, always return false to use simulation
            return false;
        }

        // Request models
        public class ConnectRequest
        {
            public string? DeviceId { get; set; }
        }

        public class AIConfigRequest
        {
            public List<int>? Channels { get; set; }
            public int SampleRate { get; set; }
            public double Range { get; set; }
        }

        public class ContinuousAIRequest
        {
            public List<int>? Channels { get; set; }
            public int SampleRate { get; set; }
            public int BufferSize { get; set; }
        }

        public class FiniteAIRequest
        {
            public List<int>? Channels { get; set; }
            public int SamplesPerChannel { get; set; }
            public int SampleRate { get; set; }
        }
    }
}