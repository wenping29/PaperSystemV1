using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Repositories;
using NotificationService.Services;
using NotificationService.Extensions;
using NotificationService.Interfaces;
using NotificationService.Hubs;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置Kestrel服务器
builder.ConfigureKestrelServer();

// 2. 配置通用Web应用程序服务
builder.Services.AddWebApplicationServices(
    builder.Configuration,
    serviceName: "Writing Platform Notification Service API",
    serviceDescription: "通知服务API");

// 3. 数据库上下文配置
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("NotificationDatabase");
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

// 4. SignalR配置
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 100 * 1024; // 100KB
});

// 5. 响应压缩（共享基础设施中暂时注释掉，此处保留）
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// 6. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 7. CORS策略（覆盖共享基础设施的AllowAll策略）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:5100")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 保留凭据支持
        });
});

// 11. 注册自定义服务
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();

// 12. 健康检查
builder.Services.AddHealthChecks();

// 如果配置了Redis，添加Redis健康检查（需要Microsoft.Extensions.Diagnostics.HealthChecks.Redis包）
// if (!string.IsNullOrEmpty(redisConfiguration))
// {
//     builder.Services.AddHealthChecks()
//         .AddRedis(redisConfiguration, "Redis", HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(5));
// }

var app = builder.Build();

// 响应压缩（共享基础设施中暂时注释掉，此处保留）
app.UseResponseCompression();

// 配置中间件管道
app.ConfigureWebApplicationPipeline(app.Environment, apiDocsPath: "api-docs");

// SignalR Hub端点
app.MapHub<NotificationHub>("/notification-hub");

app.Run();