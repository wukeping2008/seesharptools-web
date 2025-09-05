using USB1601Service.Hubs;
using USB1601Service.Services;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加SignalR
builder.Services.AddSignalR();

// 添加CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 注册服务
builder.Services.AddSingleton<USB1601Manager>();
builder.Services.AddSingleton<BaiduAIService>();
builder.Services.AddSingleton<BaiduAIServiceV2>();
builder.Services.AddSingleton<IntelligentDataPipeline>();
builder.Services.AddSingleton<SimulationManager>(); // 添加模拟管理器

// 配置管道
builder.Services.AddSingleton(new PipelineConfig
{
    SampleRate = 1000,
    CriticalThreshold = 9.0,
    WarningThreshold = 7.0,
    NormalRange = 10.0
});

// 配置日志
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapHub<DataHub>("/hubs/data");

// 启动提示
Console.WriteLine("========================================");
Console.WriteLine("USB-1601 智能数据采集服务已启动");
Console.WriteLine($"API地址: http://localhost:5000");
Console.WriteLine($"SignalR地址: http://localhost:5000/hubs/data");
Console.WriteLine("Swagger文档: http://localhost:5000/swagger");
Console.WriteLine("========================================");

app.Run();
