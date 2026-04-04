using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Shared.Infrastructure;

/// <summary>
/// 应用程序构建器扩展方法
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 配置Web应用程序中间件管道
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="environment">环境信息</param>
    /// <param name="apiDocsPath">API文档路径前缀，默认为"api-docs"</param>
    /// <returns>配置后的应用程序构建器</returns>
    public static WebApplication ConfigureWebApplicationPipeline(
        this WebApplication app,
        IWebHostEnvironment environment,
        string apiDocsPath = "api-docs")
    {
        // 配置中间件管道
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                options.RoutePrefix = apiDocsPath;
            });

            app.UseDeveloperExceptionPage();
        }

        // app.UseResponseCompression(); // 暂时注释掉，包引用问题
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
        app.MapGet("/", () => Results.Redirect($"/{apiDocsPath}"));

        return app;
    }
}