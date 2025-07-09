using Microsoft.OpenApi.Models;
using SeeSharpBackend.Services.MISD;
using SeeSharpBackend.Services.Drivers;
using SeeSharpBackend.Services.DataCompression;
using SeeSharpBackend.Services.Connection;
using SeeSharpBackend.Services.DataStorage;
using SeeSharpBackend.Hubs;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 配置Serilog日志
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/seesharp-backend-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 添加服务到容器
builder.Services.AddControllers();

// 配置API文档
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SeeSharpTools Backend API",
        Version = "v1",
        Description = "基于MISD标准的专业测控仪器后端服务API",
        Contact = new OpenApiContact
        {
            Name = "简仪科技",
            Email = "support@jytek.com",
            Url = new Uri("https://www.jytek.com")
        },
        License = new OpenApiLicense
        {
            Name = "版权所有 © 2025 简仪科技"
        }
    });

    // 包含XML注释
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // 添加授权支持
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5176", "http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 配置SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB - 支持大数据包
    options.StreamBufferCapacity = 10;
    options.MaximumParallelInvocationsPerClient = 2;
});

// 注册驱动管理器
builder.Services.Configure<DriverManagerOptions>(options =>
{
    options.ConfigurationPath = "Config/drivers.json";
    options.AutoLoadDrivers = true;
});
builder.Services.AddSingleton<IDriverManager, DriverManager>();
builder.Services.AddTransient<CSharpDllDriverAdapter>();
builder.Services.AddTransient<MockDriverAdapter>();
builder.Services.AddTransient<JY5500DllDriverAdapter>();

// 注册MISD服务
builder.Services.AddScoped<IMISDService, MISDService>();

// 注册数据压缩服务
builder.Services.AddSingleton<IDataCompressionService, DataCompressionService>();

// 注册连接管理服务
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

// 注册数据采集引擎
builder.Services.AddScoped<SeeSharpBackend.Services.DataAcquisition.IDataAcquisitionEngine, SeeSharpBackend.Services.DataAcquisition.DataAcquisitionEngine>();

// 注册数据存储服务
builder.Services.Configure<DataStorageOptions>(builder.Configuration.GetSection("DataStorage"));
builder.Services.AddSingleton<IDataStorageService, DataStorageService>();

// 配置AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 配置健康检查
builder.Services.AddHealthChecks();

// 配置内存缓存
builder.Services.AddMemoryCache();

// 配置后台服务
builder.Services.AddHostedService<MISDInitializationService>();

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SeeSharpTools Backend API v1");
        c.RoutePrefix = string.Empty; // 设置Swagger UI为根路径
        c.DocumentTitle = "SeeSharpTools Backend API";
        c.DefaultModelsExpandDepth(-1); // 隐藏模型
        c.DisplayRequestDuration();
    });
    
    app.UseCors("Development");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseCors("AllowAll");
}

// 使用HTTPS重定向
app.UseHttpsRedirection();

// 静态文件
app.UseStaticFiles();

// 路由
app.UseRouting();

// 授权
app.UseAuthorization();

// 控制器路由
app.MapControllers();

// SignalR Hub路由
app.MapHub<DataStreamHub>("/hubs/datastream");

// 健康检查
app.MapHealthChecks("/health");

// 根路径重定向到Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// 启动应用
try
{
    Log.Information("启动SeeSharpTools后端服务...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用启动失败");
}
finally
{
    Log.CloseAndFlush();
}

// MISD初始化后台服务
public class MISDInitializationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MISDInitializationService> _logger;

    public MISDInitializationService(IServiceProvider serviceProvider, ILogger<MISDInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("开始初始化MISD服务...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var misdService = scope.ServiceProvider.GetRequiredService<IMISDService>();
            
            await misdService.InitializeAsync();
            
            _logger.LogInformation("MISD服务初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MISD服务初始化失败");
        }
    }
}


// 临时实现类（后续需要完整实现）
public class DeviceDiscoveryService : IDeviceDiscoveryService
{
    private readonly ILogger<DeviceDiscoveryService> _logger;

    public DeviceDiscoveryService(ILogger<DeviceDiscoveryService> logger)
    {
        _logger = logger;
    }

    public async Task<List<SeeSharpBackend.Models.MISD.HardwareDevice>> ScanPXIDevicesAsync()
    {
        _logger.LogInformation("扫描PXI设备...");
        await Task.Delay(100); // 模拟扫描延迟
        return new List<SeeSharpBackend.Models.MISD.HardwareDevice>();
    }

    public async Task<List<SeeSharpBackend.Models.MISD.HardwareDevice>> ScanUSBDevicesAsync()
    {
        _logger.LogInformation("扫描USB设备...");
        await Task.Delay(100);
        return new List<SeeSharpBackend.Models.MISD.HardwareDevice>();
    }

    public async Task<List<SeeSharpBackend.Models.MISD.HardwareDevice>> ScanPCIeDevicesAsync()
    {
        _logger.LogInformation("扫描PCIe设备...");
        await Task.Delay(100);
        return new List<SeeSharpBackend.Models.MISD.HardwareDevice>();
    }

    public async Task<SeeSharpBackend.Models.MISD.DeviceStatus> VerifyDeviceStatusAsync(SeeSharpBackend.Models.MISD.HardwareDevice device)
    {
        await Task.Delay(50);
        return SeeSharpBackend.Models.MISD.DeviceStatus.Online;
    }

    public async Task<Dictionary<string, object>> GetDeviceDriverInfoAsync(SeeSharpBackend.Models.MISD.HardwareDevice device)
    {
        await Task.Delay(50);
        return new Dictionary<string, object>();
    }
}

public class HardwareAbstractionLayer : IHardwareAbstractionLayer
{
    private readonly ILogger<HardwareAbstractionLayer> _logger;

    public HardwareAbstractionLayer(ILogger<HardwareAbstractionLayer> logger)
    {
        _logger = logger;
    }

    public async Task<bool> LoadDriverAsync(string driverPath, string deviceModel)
    {
        _logger.LogInformation("加载驱动: {DriverPath} for {DeviceModel}", driverPath, deviceModel);
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> UnloadDriverAsync(string deviceModel)
    {
        _logger.LogInformation("卸载驱动: {DeviceModel}", deviceModel);
        await Task.Delay(50);
        return true;
    }

    public async Task<object?> ExecuteMISDMethodAsync(string deviceModel, string className, string methodName, object[] parameters)
    {
        _logger.LogInformation("执行MISD方法: {DeviceModel}.{ClassName}.{MethodName}", deviceModel, className, methodName);
        await Task.Delay(10);
        return null;
    }

    public async Task<object?> GetMISDPropertyAsync(string deviceModel, string className, string propertyName)
    {
        _logger.LogInformation("获取MISD属性: {DeviceModel}.{ClassName}.{PropertyName}", deviceModel, className, propertyName);
        await Task.Delay(10);
        return null;
    }

    public async Task<bool> SetMISDPropertyAsync(string deviceModel, string className, string propertyName, object value)
    {
        _logger.LogInformation("设置MISD属性: {DeviceModel}.{ClassName}.{PropertyName} = {Value}", deviceModel, className, propertyName, value);
        await Task.Delay(10);
        return true;
    }

    public async Task<object?> CreateMISDInstanceAsync(string deviceModel, string className, object[] constructorParams)
    {
        _logger.LogInformation("创建MISD实例: {DeviceModel}.{ClassName}", deviceModel, className);
        await Task.Delay(50);
        return new object();
    }

    public async Task<bool> DisposeMISDInstanceAsync(object instance)
    {
        _logger.LogInformation("释放MISD实例");
        await Task.Delay(10);
        return true;
    }
}

public class DataAcquisitionService : IDataAcquisitionService
{
    private readonly ILogger<DataAcquisitionService> _logger;

    public DataAcquisitionService(ILogger<DataAcquisitionService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> StartContinuousAcquisitionAsync(int taskId)
    {
        _logger.LogInformation("开始连续采集: Task {TaskId}", taskId);
        await Task.Delay(10);
        return true;
    }

    public async Task<bool> StopContinuousAcquisitionAsync(int taskId)
    {
        _logger.LogInformation("停止连续采集: Task {TaskId}", taskId);
        await Task.Delay(10);
        return true;
    }

    public async IAsyncEnumerable<double[,]> GetRealTimeDataStreamAsync(int taskId)
    {
        _logger.LogInformation("获取实时数据流: Task {TaskId}", taskId);
        
        while (true)
        {
            await Task.Delay(100);
            var data = new double[100, 4]; // 模拟数据
            yield return data;
        }
    }

    public async Task<bool> ConfigureBufferAsync(int taskId, int bufferSize)
    {
        _logger.LogInformation("配置缓冲区: Task {TaskId}, Size {BufferSize}", taskId, bufferSize);
        await Task.Delay(10);
        return true;
    }

    public async Task<SeeSharpBackend.Services.MISD.BufferStatus> GetBufferStatusAsync(int taskId)
    {
        await Task.Delay(10);
        return new SeeSharpBackend.Services.MISD.BufferStatus
        {
            BufferSize = 10000,
            AvailableSamples = 1000,
            TransferredSamples = 9000,
            IsOverflow = false
        };
    }

    public async Task<bool> ClearBufferAsync(int taskId)
    {
        _logger.LogInformation("清空缓冲区: Task {TaskId}", taskId);
        await Task.Delay(10);
        return true;
    }
}
