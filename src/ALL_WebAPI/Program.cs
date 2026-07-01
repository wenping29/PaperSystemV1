using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

// User Service
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Repositories;
using PaperSystemApi.Services;
using PaperSystemApi.Interfaces;
using PaperSystemApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// 1. 性能优化配置
// =========================================
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 10000;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 10000;
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

// =========================================
// 2. 获取数据库配置
// =========================================
var databaseOptions = builder.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
var dbProvider = databaseOptions.Provider;
Console.WriteLine($"[Database] Using database provider: {dbProvider}");

// =========================================
// 3. 添加控制器
// =========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// =========================================
// 4. Swagger配置 - 必须在AddControllers之后
// =========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Writing Platform API",
        Version = "v1",
        Description = $"AI写作平台后端API - {dbProvider}"
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

// =========================================
// 5. 数据库上下文配置 - 使用多数据库支持
// =========================================
void ConfigureDbContext<TDbContext>(IServiceCollection services, string connectionStringName)
    where TDbContext : DbContext
{
    var connectionString = builder.Configuration.GetConnectionString(connectionStringName);
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException($"Connection string '{connectionStringName}' is not configured.");
    }

    // 如果是 SQLite 且连接字符串是简单的文件名，则自动补全
    if (dbProvider == DatabaseProvider.Sqlite && !connectionString.StartsWith("Data Source="))
    {
        connectionString = DatabaseHelper.GetSqliteConnectionString(connectionStringName);
    }

    services.AddDbContext<TDbContext>(options =>
    {
        // 应用数据库配置
        options.ConfigureDatabase(dbProvider, connectionString);

        // 配置详细错误和敏感数据日志
        if (databaseOptions.EnableDetailedErrors)
        {
            options.EnableDetailedErrors();
        }
        if (databaseOptions.EnableSensitiveDataLogging)
        {
            options.EnableSensitiveDataLogging();
        }
    });
}

// 注册所有 DbContext
ConfigureDbContext<UserDbContext>(builder.Services, "UserDatabase");
ConfigureDbContext<WritingDbContext>(builder.Services, "WritingDatabase");
ConfigureDbContext<ChatDbContext>(builder.Services, "ChatDatabase");
ConfigureDbContext<FriendshipDbContext>(builder.Services, "FriendshipDatabase");
ConfigureDbContext<FileDbContext>(builder.Services, "FileDatabase");
ConfigureDbContext<AIDbContext>(builder.Services, "AIDatabase");

// =========================================
// 6. JWT认证配置
// =========================================
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
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "DefaultSecretKeyForDevelopment"))
    };
});

// =========================================
// 7. 分布式缓存（内存缓存）
// =========================================
builder.Services.AddDistributedMemoryCache();

// =========================================
// 8. 响应压缩
// =========================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// =========================================
// 9. 健康检查
// =========================================
var healthChecksBuilder = builder.Services.AddHealthChecks();
healthChecksBuilder.AddDbContextCheck<UserDbContext>("UserDatabase", tags: new[] { "ready" });
healthChecksBuilder.AddDbContextCheck<WritingDbContext>("WritingDatabase", tags: new[] { "ready" });
healthChecksBuilder.AddDbContextCheck<ChatDbContext>("ChatDatabase", tags: new[] { "ready" });
healthChecksBuilder.AddDbContextCheck<FriendshipDbContext>("FriendshipDatabase", tags: new[] { "ready" });
healthChecksBuilder.AddDbContextCheck<FileDbContext>("FileDatabase", tags: new[] { "ready" });
healthChecksBuilder.AddDbContextCheck<AIDbContext>("AIDatabase", tags: new[] { "ready" });

// =========================================
// 10. AutoMapper
// =========================================
builder.Services.AddAutoMapper(typeof(Program));

// =========================================
// 11. CORS策略
// =========================================
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

// =========================================
// 12. 注册服务
// =========================================
builder.Services.AddHttpClient();
builder.Services.AddScoped<IApiClient, MockApiClient>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserServiceS, UserServiceS>();
builder.Services.AddScoped<IWritingRepository, WritingRepository>();
builder.Services.AddScoped<IWritingServiceS, WritingServiceS>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IUserMessageReadRepository, UserMessageReadRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAIAssistantService, AIAssistantService>();
builder.Services.AddScoped<IAIReviewService, AIReviewService>();
builder.Services.AddScoped<IAIScoringService, AIScoringService>();
builder.Services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
builder.Services.AddScoped<IUserActivityLogService, UserActivityLogService>();

// =========================================
// 构建应用
// =========================================
var app = builder.Build();

// =========================================
// 配置中间件管道 - Swagger 必须在最前面
// =========================================

// 1. Swagger - 放在最前面
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", $"Paper System API v1 ({dbProvider})");
});

// 2. 其他中间件
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseResponseCompression();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 3. 端点映射
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

// 4. 健康检查
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            databaseProvider = dbProvider.ToString(),
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

// 启动消息
Console.WriteLine($"[Startup] Application is starting with database: {dbProvider}");
Console.WriteLine($"[Startup] Environment: {app.Environment.EnvironmentName}");

app.Run();
