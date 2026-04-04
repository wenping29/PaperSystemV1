using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using UserService.Data;
using UserService.Entities;
using UserService.Repositories;
using UserService.Services;
using UserService.Helpers;
using UserService.Extensions;
using UserService.Interfaces;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置Kestrel服务器
builder.ConfigureKestrelServer();

// 2. 配置通用Web应用程序服务
builder.Services.AddWebApplicationServices(
    builder.Configuration,
    serviceName: "Writing Platform User Service API",
    serviceDescription: "用户服务API");

// 3. 数据库上下文配置
builder.Services.AddDbContext<UserDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("UserService");
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

// 4. AutoMapper配置
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 5. 注册自定义服务
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserServiceS, UserServiceS>();

// 6. 健康检查
builder.Services.AddHealthChecks();

var app = builder.Build();

// 7. 配置中间件管道
app.ConfigureWebApplicationPipeline(app.Environment, apiDocsPath: "api-docs");

app.Run();

