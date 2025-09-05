using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using USB1601MCP.Tools;
using USB1601MCP.Models;

namespace USB1601MCP
{
    /// <summary>
    /// USB-1601 MCP服务器主类
    /// 实现Model Context Protocol (MCP)规范，提供USB-1601硬件控制能力
    /// </summary>
    public class USB1601MCPServer : BackgroundService
    {
        private readonly ILogger<USB1601MCPServer> _logger;
        private readonly DeviceTools _deviceTools;
        private readonly AnalogTools _analogTools;
        private readonly DigitalTools _digitalTools;
        private readonly StreamTools _streamTools;
        private readonly Stream _input;
        private readonly Stream _output;
        private readonly JsonSerializer _serializer;

        public USB1601MCPServer(
            ILogger<USB1601MCPServer> logger,
            DeviceTools deviceTools,
            AnalogTools analogTools,
            DigitalTools digitalTools,
            StreamTools streamTools)
        {
            _logger = logger;
            _deviceTools = deviceTools;
            _analogTools = analogTools;
            _digitalTools = digitalTools;
            _streamTools = streamTools;
            
            // 使用标准输入输出进行通信
            _input = Console.OpenStandardInput();
            _output = Console.OpenStandardOutput();
            
            _serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("USB-1601 MCP Server starting...");
            
            // 发送初始化消息
            await SendInitializeResponse();
            
            // 主消息循环
            using var reader = new StreamReader(_input, Encoding.UTF8);
            using var writer = new StreamWriter(_output, Encoding.UTF8) { AutoFlush = true };
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;
                    
                    var request = JsonConvert.DeserializeObject<MCPRequest>(line);
                    if (request == null) continue;
                    
                    var response = await ProcessRequest(request);
                    await writer.WriteLineAsync(JsonConvert.SerializeObject(response));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MCP request");
                    await SendErrorResponse(ex.Message);
                }
            }
            
            _logger.LogInformation("USB-1601 MCP Server stopped");
        }

        private async Task SendInitializeResponse()
        {
            var response = new
            {
                jsonrpc = "2.0",
                method = "initialize",
                @params = new
                {
                    protocolVersion = "0.1.0",
                    capabilities = new
                    {
                        tools = new
                        {
                            listTools = true
                        },
                        prompts = new
                        {
                            listPrompts = false
                        },
                        resources = new
                        {
                            listResources = false
                        }
                    },
                    serverInfo = new
                    {
                        name = "usb1601-mcp",
                        version = "1.0.0",
                        description = "MCP server for JYTEK USB-1601 Data Acquisition Device"
                    }
                }
            };
            
            using var writer = new StreamWriter(_output, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteLineAsync(JsonConvert.SerializeObject(response));
        }

        private async Task<MCPResponse> ProcessRequest(MCPRequest request)
        {
            switch (request.Method)
            {
                case "tools/list":
                    return await HandleListTools(request);
                    
                case "tools/call":
                    return await HandleToolCall(request);
                    
                default:
                    return new MCPResponse
                    {
                        JsonRpc = "2.0",
                        Id = request.Id,
                        Error = new MCPError
                        {
                            Code = -32601,
                            Message = "Method not found"
                        }
                    };
            }
        }

        private async Task<MCPResponse> HandleListTools(MCPRequest request)
        {
            var tools = new List<object>
            {
                // 设备管理工具
                new
                {
                    name = "usb1601_discover",
                    description = "Discover all connected USB-1601 devices",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                new
                {
                    name = "usb1601_connect",
                    description = "Connect to a USB-1601 device",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string", description = "Device board number" }
                        },
                        required = new[] { "deviceId" }
                    }
                },
                new
                {
                    name = "usb1601_disconnect",
                    description = "Disconnect from USB-1601 device",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string", description = "Device board number" }
                        },
                        required = new[] { "deviceId" }
                    }
                },
                
                // 模拟输入工具
                new
                {
                    name = "ai_read_single",
                    description = "Read single analog input value",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string" },
                            channel = new { type = "integer", minimum = 0, maximum = 15 }
                        },
                        required = new[] { "deviceId", "channel" }
                    }
                },
                new
                {
                    name = "ai_read_multiple",
                    description = "Read multiple analog input channels",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string" },
                            channels = new { type = "array", items = new { type = "integer" } },
                            sampleCount = new { type = "integer", minimum = 1 }
                        },
                        required = new[] { "deviceId", "channels" }
                    }
                },
                
                // 模拟输出工具
                new
                {
                    name = "ao_write_single",
                    description = "Write single analog output value",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string" },
                            channel = new { type = "integer", minimum = 0, maximum = 1 },
                            value = new { type = "number", minimum = -10, maximum = 10 }
                        },
                        required = new[] { "deviceId", "channel", "value" }
                    }
                },
                
                // 数字I/O工具
                new
                {
                    name = "dio_read",
                    description = "Read digital input port",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string" },
                            port = new { type = "integer", minimum = 0, maximum = 1 }
                        },
                        required = new[] { "deviceId", "port" }
                    }
                },
                new
                {
                    name = "dio_write",
                    description = "Write digital output port",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string" },
                            port = new { type = "integer", minimum = 0, maximum = 1 },
                            value = new { type = "integer", minimum = 0, maximum = 255 }
                        },
                        required = new[] { "deviceId", "port", "value" }
                    }
                },
                
                // 数据流工具
                new
                {
                    name = "stream_start",
                    description = "Start continuous data streaming",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string" },
                            channels = new { type = "array", items = new { type = "integer" } },
                            sampleRate = new { type = "number", minimum = 1, maximum = 200000 }
                        },
                        required = new[] { "deviceId", "channels", "sampleRate" }
                    }
                },
                new
                {
                    name = "stream_stop",
                    description = "Stop data streaming",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            deviceId = new { type = "string" }
                        },
                        required = new[] { "deviceId" }
                    }
                }
            };
            
            return new MCPResponse
            {
                JsonRpc = "2.0",
                Id = request.Id,
                Result = new { tools }
            };
        }

        private async Task<MCPResponse> HandleToolCall(MCPRequest request)
        {
            var toolName = request.Params?["name"]?.ToString();
            var arguments = request.Params?["arguments"] as JObject;
            
            if (toolName == null || arguments == null)
            {
                return CreateErrorResponse(request.Id, "Invalid tool call parameters");
            }
            
            try
            {
                object result = toolName switch
                {
                    "usb1601_discover" => await _deviceTools.DiscoverDevices(),
                    "usb1601_connect" => await _deviceTools.ConnectDevice(arguments["deviceId"]?.ToString()),
                    "usb1601_disconnect" => await _deviceTools.DisconnectDevice(arguments["deviceId"]?.ToString()),
                    "ai_read_single" => await _analogTools.ReadSingle(
                        arguments["deviceId"]?.ToString(),
                        arguments["channel"]?.Value<int>() ?? 0),
                    "ai_read_multiple" => await _analogTools.ReadMultiple(
                        arguments["deviceId"]?.ToString(),
                        arguments["channels"]?.ToObject<int[]>(),
                        arguments["sampleCount"]?.Value<int>() ?? 1),
                    "ao_write_single" => await _analogTools.WriteSingle(
                        arguments["deviceId"]?.ToString(),
                        arguments["channel"]?.Value<int>() ?? 0,
                        arguments["value"]?.Value<double>() ?? 0),
                    "dio_read" => await _digitalTools.ReadPort(
                        arguments["deviceId"]?.ToString(),
                        arguments["port"]?.Value<int>() ?? 0),
                    "dio_write" => await _digitalTools.WritePort(
                        arguments["deviceId"]?.ToString(),
                        arguments["port"]?.Value<int>() ?? 0,
                        arguments["value"]?.Value<byte>() ?? 0),
                    "stream_start" => await _streamTools.StartStream(
                        arguments["deviceId"]?.ToString(),
                        arguments["channels"]?.ToObject<int[]>(),
                        arguments["sampleRate"]?.Value<double>() ?? 1000),
                    "stream_stop" => await _streamTools.StopStream(arguments["deviceId"]?.ToString()),
                    _ => throw new NotSupportedException($"Unknown tool: {toolName}")
                };
                
                return new MCPResponse
                {
                    JsonRpc = "2.0",
                    Id = request.Id,
                    Result = new
                    {
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = JsonConvert.SerializeObject(result, Formatting.Indented)
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing tool {toolName}");
                return CreateErrorResponse(request.Id, ex.Message);
            }
        }

        private MCPResponse CreateErrorResponse(string? id, string message)
        {
            return new MCPResponse
            {
                JsonRpc = "2.0",
                Id = id,
                Error = new MCPError
                {
                    Code = -32603,
                    Message = message
                }
            };
        }

        private async Task SendErrorResponse(string message)
        {
            var response = CreateErrorResponse(null, message);
            using var writer = new StreamWriter(_output, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteLineAsync(JsonConvert.SerializeObject(response));
        }
    }
}