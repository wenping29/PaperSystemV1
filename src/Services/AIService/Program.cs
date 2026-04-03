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

var builder = WebApplication.CreateBuilder(args);

// 1. 性能优化配置
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 10000;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 10000;
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
    serverOptions.ListenAnyIP(5003, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
    });
});

// 2. 依赖注入配置
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 3. Swagger配置
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Writing Platform AI Service API",
        Version = "v1",
        Description = "AI服务API"
    });

    // 添加JWT认证支持
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 4. 数据库上下文配置
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

// 5. JWT认证配置
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "WritingPlatform",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "WritingPlatformUsers",
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGenerationAtLeast32Characters"))
    };
});

// 6. Redis分布式缓存
var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConfiguration))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfiguration;
        options.InstanceName = "WritingPlatform:AIService:";
    });

    // 注册IConnectionMultiplexer用于Redis高级操作
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(redisConfiguration));
}

// 7. 响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// 8. 健康检查
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AIDbContext>("AIDatabase", tags: new[] { "ready" });

// 9. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 10. CORS策略
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

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

// AutoMapper配置
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// 配置中间件管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Service API v1");
        options.RoutePrefix = "api-docs";
    });

    app.UseDeveloperExceptionPage();
}

app.UseResponseCompression();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 健康检查端点
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                exception = e.Value.Exception?.Message
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// 根路径重定向到Swagger
app.MapGet("/", () => Results.Redirect("/api-docs"));

app.Run();