using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentService.Data;
using PaymentService.Entities;
using PaymentService.Repositories;
using PaymentService.Services;
using PaymentService.Helpers;
using PaymentService.Extensions;
using PaymentService.Interfaces;
using PaymentService.Clients;
using PaymentService.Factories;
using PaymentService.Options;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置Kestrel服务器
builder.ConfigureKestrelServer();

// 2. 配置通用Web应用程序服务
builder.Services.AddWebApplicationServices(
    builder.Configuration,
    serviceName: "Writing Platform Payment Service API",
    serviceDescription: "支付服务API");

// 3. 数据库上下文配置
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PaymentDatabase");
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
    .AddDbContextCheck<PaymentDbContext>("PaymentDatabase", tags: new[] { "ready" });

// 6. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 11. 注册自定义服务
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IRefundTransactionRepository, RefundTransactionRepository>();
builder.Services.AddScoped<IIPaymentService, IPaymentService>();

// 12. 支付网关配置选项
builder.Services.AddOptions<AlipayOptions>()
    .Bind(builder.Configuration.GetSection("Alipay"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<WeChatPayOptions>()
    .Bind(builder.Configuration.GetSection("WeChatPay"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// 13. 注册支付网关客户端
builder.Services.AddSingleton<MockPaymentGatewayClient>();
builder.Services.AddSingleton<AlipayGatewayClient>();
builder.Services.AddSingleton<WeChatPayGatewayClient>();

// 14. 注册支付网关工厂
builder.Services.AddSingleton<IPaymentGatewayFactory, PaymentGatewayFactory>();

// AutoMapper配置
builder.Services.AddAutoMapper(typeof(MappingProfile));
// 🔥 关键：注册 HttpClient 工厂
builder.Services.AddHttpClient();

var app = builder.Build();

// 响应压缩（共享基础设施中暂时注释掉，此处保留）
app.UseResponseCompression();

// 配置中间件管道
app.ConfigureWebApplicationPipeline(app.Environment, apiDocsPath: "api-docs");

app.Run();