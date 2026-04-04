using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatService.Data;
using ChatService.Entities;
using ChatService.Repositories;
using ChatService.Services;
using ChatService.Extensions;
using ChatService.Interfaces;
using Shared.Infrastructure;
// using Microsoft.AspNetCore.SignalR;
// using ChatService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置Kestrel服务器
builder.ConfigureKestrelServer();

// 2. 配置通用Web应用程序服务
builder.Services.AddWebApplicationServices(
    builder.Configuration,
    serviceName: "Writing Platform Chat Service API",
    serviceDescription: "聊天服务API");

// 3. SignalR配置（实时聊天）- 暂时注释掉，因为包版本问题
// builder.Services.AddSignalR(options =>
// {
//     options.EnableDetailedErrors = true;
//     options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
// });

// 4. 数据库上下文配置
builder.Services.AddDbContext<ChatDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ChatDatabase");
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

// 5. 响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// 6. 健康检查（数据库健康检查需要EF Core）
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ChatDbContext>("ChatDatabase", tags: new[] { "ready" });

// 7. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 8. CORS策略（覆盖共享基础设施的AllowAll策略）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:3001")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials(); // 支持带Cookie/Token
        }
        );
});

// 12. 注册自定义服务
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IUserMessageReadRepository, UserMessageReadRepository>();
builder.Services.AddScoped<IChatService, ChatService.Services.ChatService>();

// 13. AutoMapper配置
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// 响应压缩（共享基础设施中暂时注释掉，此处保留）
app.UseResponseCompression();

// 配置中间件管道
app.ConfigureWebApplicationPipeline(app.Environment, apiDocsPath: "api-docs");

// SignalR Hub端点 - 暂时注释掉
// app.MapHub<ChatHub>("/chatHub");

app.Run();