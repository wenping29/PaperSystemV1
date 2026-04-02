using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AIService.Clients;
using AIService.DTOs;
using AIService.Entities;
using AIService.Interfaces;

namespace AIService.Services
{
    public class AIScoringService : IAIScoringService
    {
        private readonly IApiClient _apiClient;
        private readonly IAIAuditLogRepository _auditLogRepository;
        private readonly ILogger<AIScoringService> _logger;

        public AIScoringService(
            IApiClient apiClient,
            IAIAuditLogRepository auditLogRepository,
            ILogger<AIScoringService> logger)
        {
            _apiClient = apiClient;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<ScoringResponse> ScoreTextAsync(ScoringRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("文本评分，文本长度: {Length}", request.Text.Length);

                var response = await _apiClient.ScoreTextAsync(request);

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "ScoreText",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文本评分失败");

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "ScoreText",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<RubricDefinition> GetRubricAsync(string rubricId)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("获取评分标准，ID: {RubricId}", rubricId);

                var response = await _apiClient.GetRubricAsync(rubricId);

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "GetRubric",
                    requestData: new { RubricId = rubricId },
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取评分标准失败");

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "GetRubric",
                    requestData: new { RubricId = rubricId },
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<RubricDefinition> CreateRubricAsync(RubricDefinition rubric)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("创建评分标准，名称: {Name}", rubric.Name);

                var response = await _apiClient.CreateRubricAsync(rubric);

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "CreateRubric",
                    requestData: rubric,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评分标准失败");

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "CreateRubric",
                    requestData: rubric,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<BatchScoringResponse> ScoreBatchAsync(BatchScoringRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("批量评分，文本数量: {Count}", request.Texts.Count);

                var response = await _apiClient.ScoreBatchAsync(request);

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "ScoreBatch",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量评分失败");

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "ScoreBatch",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<FeedbackResponse> SubmitFeedbackAsync(FeedbackRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("提交反馈，文本长度: {Length}", request.Text.Length);

                // 模拟反馈处理
                await Task.Delay(100);

                var response = new FeedbackResponse
                {
                    Success = true,
                    Message = "反馈已接收，感谢您的意见！",
                    ReceivedAt = DateTime.UtcNow
                };

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "SubmitFeedback",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交反馈失败");

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "SubmitFeedback",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<string> GenerateGradingReportAsync(string textId, string rubricId = "default")
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("生成评分报告，文本ID: {TextId}, 标准ID: {RubricId}", textId, rubricId);

                // 模拟报告生成
                await Task.Delay(150);

                var report = $"评分报告\n" +
                            $"文本ID: {textId}\n" +
                            $"评分标准: {rubricId}\n" +
                            $"生成时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n" +
                            $"报告内容: 这是一个模拟的评分报告，实际需要根据具体评分结果生成详细分析。";

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "GenerateGradingReport",
                    requestData: new { TextId = textId, RubricId = rubricId },
                    responseData: report,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成评分报告失败");

                await LogAuditAsync(
                    serviceType: "Scoring",
                    requestType: "GenerateGradingReport",
                    requestData: new { TextId = textId, RubricId = rubricId },
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