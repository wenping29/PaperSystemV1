using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

// User Service
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Repositories;
using PaperSystemApi.Services;
using PaperSystemApi.Models;
using PaperSystemApi.Interfaces;

// Writing Service
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Repositories;
using PaperSystemApi.Interfaces;
using PaperSystemApi.Services;

// Chat Service
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Repositories;
using PaperSystemApi.Interfaces;
using PaperSystemApi.Services;

// Friendship Service
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Repositories;
using PaperSystemApi.Interfaces;
using PaperSystemApi.Services;

// File Service
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Repositories;
using PaperSystemApi.Interfaces;
using PaperSystemApi.Services;

// AI Service
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Repositories;
using PaperSystemApi.Interfaces;
using PaperSystemApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. 性能优化配置
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 10000;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 10000;
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
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
        Title = "Writing Platform API",
        Version = "v1",
        Description = "AI写作平台后端API - 单体架构"
    });

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

// 4. 数据库上下文配置 - UserService
builder.Services.AddDbContext<PaperSystemApi.Data.UserDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("UserDatabase");
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

// 5. 数据库上下文配置 - WritingService
builder.Services.AddDbContext<PaperSystemApi.Data.WritingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("WritingDatabase");
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

// 6. 数据库上下文配置 - ChatService
builder.Services.AddDbContext<PaperSystemApi.Data.ChatDbContext>(options =>
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

// 7. 数据库上下文配置 - FriendshipService
builder.Services.AddDbContext<PaperSystemApi.Data.FriendshipDbContext>(options =>
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

// 8. 数据库上下文配置 - FileService
builder.Services.AddDbContext<PaperSystemApi.Data.FileDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("FileDatabase");
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

// 9. 数据库上下文配置 - AIService
builder.Services.AddDbContext<PaperSystemApi.Data.AIDbContext>(options =>
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

// 10. JWT认证配置
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});

// 11. Redis分布式缓存
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", true);
var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
if (redisEnabled && !string.IsNullOrEmpty(redisConfiguration))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfiguration;
        options.InstanceName = "WritingPlatform:";
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(redisConfiguration));
}
else
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => null);
}

// 12. 响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// 13. 健康检查
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaperSystemApi.Data.UserDbContext>("UserDatabase", tags: new[] { "ready" })
    .AddDbContextCheck<PaperSystemApi.Data.WritingDbContext>("WritingDatabase", tags: new[] { "ready" })
    .AddDbContextCheck<PaperSystemApi.Data.ChatDbContext>("ChatDatabase", tags: new[] { "ready" })
    .AddDbContextCheck<PaperSystemApi.Data.FriendshipDbContext>("FriendshipDatabase", tags: new[] { "ready" })
    .AddDbContextCheck<PaperSystemApi.Data.FileDbContext>("FileDatabase", tags: new[] { "ready" })
    .AddDbContextCheck<PaperSystemApi.Data.AIDbContext>("AIDatabase", tags: new[] { "ready" });

// 14. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 15. CORS策略
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 16. 注册UserService服务
builder.Services.AddScoped<PaperSystemApi.Models.IPasswordHasher, PaperSystemApi.Models.PasswordHasher>();
builder.Services.AddScoped<PaperSystemApi.Models.IJwtTokenGenerator, PaperSystemApi.Models.JwtTokenGenerator>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IUserRepository, PaperSystemApi.Repositories.UserRepository>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IUserServiceS, PaperSystemApi.Services.UserServiceS>();

// 17. 注册WritingService服务
builder.Services.AddScoped<PaperSystemApi.Interfaces.IWritingRepository, PaperSystemApi.Repositories.WritingRepository>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IWritingServiceS, PaperSystemApi.Services.WritingServiceS>();

// 18. 注册ChatService服务
builder.Services.AddScoped<PaperSystemApi.Interfaces.IMessageRepository, PaperSystemApi.Repositories.MessageRepository>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IChatRoomRepository, PaperSystemApi.Repositories.ChatRoomRepository>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IUserMessageReadRepository, PaperSystemApi.Repositories.UserMessageReadRepository>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IChatService, PaperSystemApi.Services.ChatService>();

// 19. 注册FriendshipService服务
builder.Services.AddScoped<PaperSystemApi.Interfaces.IFriendshipRepository, PaperSystemApi.Repositories.FriendshipRepository>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IFriendshipService, PaperSystemApi.Services.FriendshipService>();

// 20. 注册FileService服务
builder.Services.AddScoped<PaperSystemApi.Interfaces.IFileRepository, PaperSystemApi.Repositories.FileRepository>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IFileService, PaperSystemApi.Services.FileService>();

// 21. 注册AIService服务
builder.Services.AddScoped<PaperSystemApi.Interfaces.IAIAssistantService, PaperSystemApi.Services.AIAssistantService>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IAIReviewService, PaperSystemApi.Services.AIReviewService>();
builder.Services.AddScoped<PaperSystemApi.Interfaces.IAIScoringService, PaperSystemApi.Services.AIScoringService>();

var app = builder.Build();

// 配置中间件管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Paper System API v1");
        options.RoutePrefix = "api-docs";
    });

    app.UseDeveloperExceptionPage();
}

app.UseResponseCompression();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseRouting();
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
