using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using USB1601MCP;
using USB1601MCP.Tools;

namespace USB1601MCP
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // 配置控制台编码
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.InputEncoding = System.Text.Encoding.UTF8;

                // 创建主机
                var host = CreateHostBuilder(args).Build();
                
                // 运行MCP服务器
                await host.RunAsync();
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fatal error: {ex.Message}");
                return 1;
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("config/mcp.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    // 注册工具服务
                    services.AddSingleton<DeviceTools>();
                    services.AddSingleton<AnalogTools>();
                    services.AddSingleton<DigitalTools>();
                    services.AddSingleton<StreamTools>();
                    
                    // 注册MCP服务器
                    services.AddHostedService<USB1601MCPServer>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    
                    // 在调试模式下输出到控制台
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    }
                    else
                    {
                        // 生产环境只记录到文件
                        logging.AddFile("logs/usb1601-mcp-{Date}.log");
                        logging.SetMinimumLevel(LogLevel.Information);
                    }
                })
                .UseConsoleLifetime();
    }

    /// <summary>
    /// 文件日志扩展
    /// </summary>
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string pathTemplate)
        {
            // 简单的文件日志实现
            builder.AddProvider(new FileLoggerProvider(pathTemplate));
            return builder;
        }
    }

    /// <summary>
    /// 文件日志提供者
    /// </summary>
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _pathTemplate;
        
        public FileLoggerProvider(string pathTemplate)
        {
            _pathTemplate = pathTemplate;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _pathTemplate);
        }

        public void Dispose() { }
    }

    /// <summary>
    /// 文件日志记录器
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _pathTemplate;
        private readonly object _lock = new object();

        public FileLogger(string categoryName, string pathTemplate)
        {
            _categoryName = categoryName;
            _pathTemplate = pathTemplate;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logLine = $"[{timestamp}] [{logLevel}] [{_categoryName}] {message}";

            if (exception != null)
            {
                logLine += Environment.NewLine + exception.ToString();
            }

            var logPath = _pathTemplate.Replace("{Date}", DateTime.Now.ToString("yyyyMMdd"));
            var directory = Path.GetDirectoryName(logPath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            lock (_lock)
            {
                File.AppendAllText(logPath, logLine + Environment.NewLine);
            }
        }
    }
}