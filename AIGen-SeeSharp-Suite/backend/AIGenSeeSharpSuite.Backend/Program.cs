using AIGenSeeSharpSuite.Backend.Hubs;
using AIGenSeeSharpSuite.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Add caching
builder.Services.AddMemoryCache();

// Register services with proper dependencies
builder.Services.AddSingleton<MisdExcelReader>();
builder.Services.AddSingleton<CodeCleanerService>();
builder.Services.AddSingleton<AdvancedCodeGeneratorService>();
builder.Services.AddSingleton<ProjectTemplateService>();
builder.Services.AddSingleton<BaiduAiService>();
builder.Services.AddSingleton<KnowledgeService>();
builder.Services.AddSingleton<PromptEngineeringService>();
builder.Services.AddSingleton<CodeValidationService>();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHub<DataStreamHub>("/dataStreamHub");

app.Run();
