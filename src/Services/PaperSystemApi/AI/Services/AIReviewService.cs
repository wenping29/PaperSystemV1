using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaperSystemApi.AI.Clients;
using PaperSystemApi.AI.DTOs;
using PaperSystemApi.AI.Entities;
using PaperSystemApi.AI.Interfaces;

namespace PaperSystemApi.AI.Services
{
    public class AIReviewService : IAIReviewService
    {
        private readonly IApiClient _apiClient;
        private readonly IAIAuditLogRepository _auditLogRepository;
        private readonly ILogger<AIReviewService> _logger;

        public AIReviewService(
            IApiClient apiClient,
            IAIAuditLogRepository auditLogRepository,
            ILogger<AIReviewService> logger)
        {
            _apiClient = apiClient;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<GrammarCheckResponse> CheckGrammarAsync(GrammarCheckRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("语法检查，文本长度: {Length}", request.Text.Length);

                var response = await _apiClient.CheckGrammarAsync(request);

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "CheckGrammar",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "语法检查失败");

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "CheckGrammar",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<StyleAnalysisResponse> AnalyzeStyleAsync(StyleAnalysisRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("风格分析，文本长度: {Length}", request.Text.Length);

                var response = await _apiClient.AnalyzeStyleAsync(request);

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "AnalyzeStyle",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "风格分析失败");

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "AnalyzeStyle",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<PlagiarismCheckResponse> CheckPlagiarismAsync(PlagiarismCheckRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("抄袭检测，文本长度: {Length}", request.Text.Length);

                var response = await _apiClient.CheckPlagiarismAsync(request);

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "CheckPlagiarism",
                    requestData: request,
                    responseData: response,
                    processingTime : DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "抄袭检测失败");

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "CheckPlagiarism",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<ComprehensiveReviewResponse> PerformComprehensiveReviewAsync(ComprehensiveReviewRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("综合审查，文本长度: {Length}", request.Text.Length);

                var response = await _apiClient.PerformComprehensiveReviewAsync(request);

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "PerformComprehensiveReview",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "综合审查失败");

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "PerformComprehensiveReview",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<string> GetWritingReportAsync(string text, string reportType = "basic")
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("获取写作报告，类型: {ReportType}", reportType);

                // 模拟报告生成
                await Task.Delay(100); // 模拟处理时间

                var report = $"写作报告 ({reportType}):\n" +
                            $"文本长度: {text.Length} 字符\n" +
                            $"分析时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n" +
                            $"报告内容: 这是一个模拟的写作报告，实际需要集成AI服务生成详细分析。";

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "GetWritingReport",
                    requestData: new { Text = text, ReportType = reportType },
                    responseData: report,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取写作报告失败");

                await LogAuditAsync(
                    serviceType: "Review",
                    requestType: "GetWritingReport",
                    requestData: new { Text = text, ReportType = reportType },
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        private async Task LogAuditAsync(
            string serviceType,
            string requestType,
            object requestData,
            object? responseData = null,
            string? errorMessage = null,
            TimeSpan? processingTime = null,
            bool success = true,
            Guid? userId = null)
        {
            try
            {
                var auditLog = new AIAuditLog
                {
                    UserId = userId ?? Guid.Empty,
                    ServiceType = serviceType,
                    RequestType = requestType,
                    RequestData = System.Text.Json.JsonSerializer.Serialize(requestData),
                    ResponseData = responseData != null ? System.Text.Json.JsonSerializer.Serialize(responseData) : null,
                    StatusCode = success ? 200 : 500,
                    ErrorMessage = errorMessage,
                    ProcessingTimeMs = (long)(processingTime?.TotalMilliseconds ?? 0),
                    TokenCount = 0,
                    Cost = 0,
                    ModelUsed = "Mock",
                    Provider = "Mock",
                    CorrelationId = Guid.NewGuid().ToString()
                };

                await _auditLogRepository.AddAsync(auditLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录审计日志失败");
            }
        }
    }
}