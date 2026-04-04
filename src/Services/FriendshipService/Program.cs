using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using FriendshipService.Data;
using FriendshipService.Entities;
using FriendshipService.Repositories;
using FriendshipService.Services;
using FriendshipService.Extensions;
using FriendshipService.Interfaces;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置Kestrel服务器
builder.ConfigureKestrelServer();

// 2. 配置通用Web应用程序服务
builder.Services.AddWebApplicationServices(
    builder.Configuration,
    serviceName: "Writing Platform Friendship Service API",
    serviceDescription: "好友关系服务API");

// 3. 数据库上下文配置
builder.Services.AddDbContext<FriendshipDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("FriendshipDatabase");
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

// 5. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 6. CORS策略（覆盖共享基础设施的AllowAll策略）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:3001")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials(); // 支持带Cookie/Token
        });
});

// 11. 注册自定义服务
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
// FriendshipService 是命名空间，需替换为实际实现类名
// 假设实现类为 FriendshipServiceImpl，如有不同请替换为实际类名
builder.Services.AddScoped<IIFriendshipService, IFriendshipService>();

// AutoMapper配置
builder.Services.AddAutoMapper(typeof(MappingProfile));
// 👇 关键代码：注册 HttpClient（推荐方式）
builder.Services.AddHttpClient();

var app = builder.Build();

// 响应压缩（共享基础设施中暂时注释掉，此处保留）
app.UseResponseCompression();

// 配置中间件管道
app.ConfigureWebApplicationPipeline(app.Environment, apiDocsPath: "api-docs");

app.Run();
