using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;

namespace Shared.Infrastructure;

/// <summary>
/// 服务配置扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 配置Web应用程序的通用服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="serviceName">服务名称（用于Swagger标题）</param>
    /// <param name="serviceDescription">服务描述（用于Swagger描述）</param>
    /// <returns>配置后的服务集合</returns>
    public static IServiceCollection AddWebApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName = "API Service",
        string serviceDescription = "API Service Description")
    {
        // 1. 控制器和JSON配置
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

        // 2. Swagger配置
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = serviceName,
                Version = "v1",
                Description = serviceDescription
            });

            // 添加JWT认证支持
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // 3. JWT认证配置
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "WritingPlatform",
                ValidAudience = configuration["Jwt:Audience"] ?? "WritingPlatformUsers",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ??
                        "YourSuperSecretKeyForJWTTokenGenerationAtLeast32Characters"))
            };
        });

        // 4. Redis分布式缓存
        var redisEnabled = configuration.GetValue<bool>("Redis:Enabled", true);
        var redisConfiguration = configuration.GetConnectionString("Redis");
        if (redisEnabled && !string.IsNullOrEmpty(redisConfiguration))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConfiguration;
                options.InstanceName = $"{serviceName.Replace(" ", "")}:";
            });

            // 注册IConnectionMultiplexer用于Redis高级操作
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConfiguration));
        }

        // 5. 响应压缩 (暂时注释掉，包引用问题)
        // Microsoft.Extensions.DependencyInjection.ResponseCompressionServiceCollectionExtensions.AddResponseCompression(services, options =>
        // {
        //     options.EnableForHttps = true;
        //     options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        //     options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        // });

        // 6. 健康检查
        services.AddHealthChecks();

        // 7. CORS策略
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    // .AllowCredentials(); // 支持带Cookie/Token
                });
        });

        return services;
    }

    /// <summary>
    /// 配置Kestrel服务器
    /// </summary>
    public static void ConfigureKestrelServer(this IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxConcurrentConnections = 10000;
            serverOptions.Limits.MaxConcurrentUpgradedConnections = 10000;
            serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
            // 端口配置已移至 appsettings.json 中的 Kestrel 配置
        });
    }

    /// <summary>
    /// 配置Kestrel服务器（WebApplicationBuilder扩展）
    /// </summary>
    public static void ConfigureKestrelServer(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.WebHost.ConfigureKestrelServer();
    }
}