using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityService.Data;
using CommunityService.Entities;
using CommunityService.Extensions;
using CommunityService.Interfaces;
using CommunityService.Repositories;
using CommunityService.Services;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置Kestrel服务器
builder.ConfigureKestrelServer();

// 2. 配置通用Web应用程序服务
builder.Services.AddWebApplicationServices(
    builder.Configuration,
    serviceName: "Writing Platform Community Service API",
    serviceDescription: "社区服务API：作品广场、评论、点赞、收藏");

// 3. 数据库上下文配置
builder.Services.AddDbContext<CommunityDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CommunityDatabase");
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

// 4. 响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// 5. 健康检查（数据库健康检查需要EF Core）
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CommunityDbContext>("CommunityDatabase", tags: new[] { "ready" });

// 6. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 7. CORS策略（覆盖共享基础设施的AllowAll策略）
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

// 11. 注册Repository服务
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();

// 12. 注册Service服务
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();

// 13. 注册HttpClient用于调用其他服务（如用户服务）
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 14. 注册内存缓存（备用）
builder.Services.AddMemoryCache();

var app = builder.Build();

// 响应压缩（共享基础设施中暂时注释掉，此处保留）
app.UseResponseCompression();

// 配置中间件管道
app.ConfigureWebApplicationPipeline(app.Environment, apiDocsPath: "api-docs");

// 数据库迁移（开发环境）
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CommunityDbContext>();
   // await dbContext.Database.EnsureCreatedAsync();
}

app.Run();