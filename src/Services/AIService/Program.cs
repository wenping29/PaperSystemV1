using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIService.Data;
using AIService.Entities;
using AIService.Repositories;
using AIService.Services;
using AIService.Helpers;
using AIService.Extensions;
using AIService.Interfaces;
using AIService.Clients;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置Kestrel服务器
builder.ConfigureKestrelServer();

// 2. 配置通用Web应用程序服务
builder.Services.AddWebApplicationServices(
    builder.Configuration,
    serviceName: "Writing Platform AI Service API",
    serviceDescription: "AI服务API");

// 3. 数据库上下文配置
builder.Services.AddDbContext<AIDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AIDatabase");
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 28)),
            mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                mysqlOptions.CommandTimeout(30);
                mysqlOptions.MaxBatchSize(100);
                mysqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
    }
});

// 4. 响应压缩（共享基础设施中暂时注释掉，此处保留）
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// 5. 健康检查（添加数据库健康检查）
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AIDbContext>("AIDatabase", tags: new[] { "ready" });

// 6. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 11. 注册自定义服务
builder.Services.AddScoped<IAIAssistantService, AIAssistantService>();
builder.Services.AddScoped<IAIReviewService, AIReviewService>();
builder.Services.AddScoped<IAIScoringService, AIScoringService>();
builder.Services.AddScoped<IAIAuditLogRepository, AIAuditLogRepository>();

// 根据配置注册AI客户端
var aiProvider = builder.Configuration["AI:Provider"] ?? "Mock";
if (aiProvider.Equals("Mock", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IApiClient, MockApiClient>();
}
else
{
    // 实际API客户端将根据提供商配置注册
    builder.Services.AddSingleton<IApiClient, MockApiClient>();
}


var app = builder.Build();

// 响应压缩（共享基础设施中暂时注释掉，此处保留）
app.UseResponseCompression();

// 配置中间件管道
app.ConfigureWebApplicationPipeline(app.Environment, apiDocsPath: "api-docs");

app.Run();