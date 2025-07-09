using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SeeSharpBackend.Models.MISD;
using System.Text.Json;

namespace SeeSharpBackend.Services.Drivers
{
    /// <summary>
    /// 驱动管理器
    /// 统一管理所有类型的硬件驱动适配器
    /// </summary>
    public class DriverManager : IDriverManager
    {
        private readonly ILogger<DriverManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DriverManagerOptions _options;
        private readonly Dictionary<string, IDriverAdapter> _loadedDrivers = new();
        private readonly Dictionary<string, DriverConfiguration> _driverConfigs = new();

        public DriverManager(
            ILogger<DriverManager> logger,
            IServiceProvider serviceProvider,
            IOptions<DriverManagerOptions> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        /// <summary>
        /// 初始化驱动管理器
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("开始初始化驱动管理器...");

                // 加载驱动配置
                await LoadDriverConfigurationsAsync();

                // 自动加载启用的驱动
                if (_options.AutoLoadDrivers)
                {
                    await LoadEnabledDriversAsync();
                }

                _logger.LogInformation("驱动管理器初始化完成，已加载 {Count} 个驱动", _loadedDrivers.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驱动管理器初始化失败");
                return false;
            }
        }

        /// <summary>
        /// 加载驱动配置
        /// </summary>
        private async Task LoadDriverConfigurationsAsync()
        {
            var configPath = _options.ConfigurationPath;
            
            if (File.Exists(configPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(configPath);
                    var configRoot = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (configRoot.TryGetProperty("Drivers", out var driversElement))
                    {
                        foreach (var driverProperty in driversElement.EnumerateObject())
                        {
                            var config = JsonSerializer.Deserialize<DriverConfiguration>(driverProperty.Value.GetRawText());
                            if (config != null)
                            {
                                _driverConfigs[driverProperty.Name] = config;
                            }
                        }
                    }
                    
                    _logger.LogInformation("加载了 {Count} 个驱动配置", _driverConfigs.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加载驱动配置失败: {ConfigPath}", configPath);
                }
            }
            else
            {
                _logger.LogWarning("驱动配置文件不存在: {ConfigPath}", configPath);
            }
        }

        /// <summary>
        /// 加载启用的驱动
        /// </summary>
        private async Task LoadEnabledDriversAsync()
        {
            foreach (var config in _driverConfigs)
            {
                try
                {
                    await LoadDriverAsync(config.Key, config.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "加载驱动失败: {DriverName}", config.Key);
                }
            }
        }

        /// <summary>
        /// 加载指定驱动
        /// </summary>
        public async Task<bool> LoadDriverAsync(string driverName, DriverConfiguration config)
        {
            try
            {
                if (_loadedDrivers.ContainsKey(driverName))
                {
                    _logger.LogWarning("驱动已加载: {DriverName}", driverName);
                    return true;
                }

                // 根据驱动类型创建适配器
                var adapter = CreateDriverAdapter(DriverType.CSharpDll, driverName);
                if (adapter == null)
                {
                    _logger.LogError("无法创建驱动适配器: {DriverName}", driverName);
                    return false;
                }

                // 初始化驱动
                var success = await adapter.InitializeAsync(config);
                if (success)
                {
                    _loadedDrivers[driverName] = adapter;
                    _logger.LogInformation("驱动加载成功: {DriverName} ({Type})", driverName, adapter.Type);
                    return true;
                }
                else
                {
                    _logger.LogError("驱动初始化失败: {DriverName}", driverName);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载驱动异常: {DriverName}", driverName);
                return false;
            }
        }

        /// <summary>
        /// 创建驱动适配器实例
        /// </summary>
        private IDriverAdapter? CreateDriverAdapter(DriverType type, string name)
        {
            return type switch
            {
                DriverType.CSharpDll when name == "MockDriver" => _serviceProvider.GetRequiredService<MockDriverAdapter>(),
                DriverType.CSharpDll when name == "JY5500" => _serviceProvider.GetRequiredService<JY5500DllDriverAdapter>(),
                DriverType.CSharpDll => _serviceProvider.GetRequiredService<CSharpDllDriverAdapter>(),
                _ => throw new NotSupportedException($"不支持的驱动类型: {type}")
            };
        }

        /// <summary>
        /// 卸载驱动
        /// </summary>
        public async Task<bool> UnloadDriverAsync(string driverName)
        {
            try
            {
                if (!_loadedDrivers.TryGetValue(driverName, out var adapter))
                {
                    _logger.LogWarning("驱动未加载: {DriverName}", driverName);
                    return false;
                }

                var success = await adapter.UnloadAsync();
                if (success)
                {
                    _loadedDrivers.Remove(driverName);
                    _logger.LogInformation("驱动卸载成功: {DriverName}", driverName);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "卸载驱动异常: {DriverName}", driverName);
                return false;
            }
        }

        /// <summary>
        /// 获取驱动适配器
        /// </summary>
        public IDriverAdapter? GetDriver(string driverName)
        {
            _loadedDrivers.TryGetValue(driverName, out var adapter);
            return adapter;
        }

        /// <summary>
        /// 获取所有已加载的驱动
        /// </summary>
        public IEnumerable<string> GetLoadedDrivers()
        {
            return _loadedDrivers.Keys;
        }

        /// <summary>
        /// 发现所有设备
        /// </summary>
        public async Task<List<HardwareDevice>> DiscoverAllDevicesAsync()
        {
            var allDevices = new List<HardwareDevice>();

            foreach (var driver in _loadedDrivers.Values)
            {
                try
                {
                    var devices = await driver.DiscoverDevicesAsync();
                    allDevices.AddRange(devices);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "驱动设备发现失败: {DriverName}", driver.Name);
                }
            }

            _logger.LogInformation("总共发现 {Count} 个设备", allDevices.Count);
            return allDevices;
        }

        /// <summary>
        /// 重新加载驱动配置
        /// </summary>
        public async Task<bool> ReloadConfigurationAsync()
        {
            try
            {
                _driverConfigs.Clear();
                await LoadDriverConfigurationsAsync();
                _logger.LogInformation("驱动配置重新加载完成");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新加载驱动配置失败");
                return false;
            }
        }

        /// <summary>
        /// 保存驱动配置
        /// </summary>
        public async Task<bool> SaveConfigurationAsync()
        {
            try
            {
                var configRoot = new
                {
                    Drivers = _driverConfigs,
                    GlobalSettings = new
                    {
                        AutoLoadDrivers = _options.AutoLoadDrivers,
                        LoadTimeoutMs = _options.LoadTimeoutMs
                    }
                };

                var json = JsonSerializer.Serialize(configRoot, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await File.WriteAllTextAsync(_options.ConfigurationPath, json);
                _logger.LogInformation("驱动配置保存成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存驱动配置失败");
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("开始释放驱动管理器资源...");
            
            foreach (var driver in _loadedDrivers.Values)
            {
                try
                {
                    await driver.UnloadAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "释放驱动资源失败: {DriverName}", driver.Name);
                }
            }
            
            _loadedDrivers.Clear();
            _logger.LogInformation("驱动管理器资源释放完成");
        }
    }

    /// <summary>
    /// 驱动管理器接口
    /// </summary>
    public interface IDriverManager : IAsyncDisposable
    {
        Task<bool> InitializeAsync();
        Task<bool> LoadDriverAsync(string driverName, DriverConfiguration config);
        Task<bool> UnloadDriverAsync(string driverName);
        IDriverAdapter? GetDriver(string driverName);
        IEnumerable<string> GetLoadedDrivers();
        Task<List<HardwareDevice>> DiscoverAllDevicesAsync();
        Task<bool> ReloadConfigurationAsync();
        Task<bool> SaveConfigurationAsync();
    }

    /// <summary>
    /// 驱动管理器选项
    /// </summary>
    public class DriverManagerOptions
    {
        /// <summary>
        /// 驱动配置文件路径
        /// </summary>
        public string ConfigurationPath { get; set; } = "Config/drivers.json";

        /// <summary>
        /// 驱动文件搜索路径
        /// </summary>
        public List<string> SearchPaths { get; set; } = new()
        {
            "C:\\SeeSharp\\JYTEK\\Hardware",
            "Drivers",
            "."
        };

        /// <summary>
        /// 是否自动加载驱动
        /// </summary>
        public bool AutoLoadDrivers { get; set; } = true;

        /// <summary>
        /// 驱动加载超时时间(毫秒)
        /// </summary>
        public int LoadTimeoutMs { get; set; } = 30000;
    }
}
