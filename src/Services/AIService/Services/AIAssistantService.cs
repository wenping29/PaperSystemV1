using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AIService.Clients;
using AIService.DTOs;
using AIService.Entities;
using AIService.Interfaces;

namespace AIService.Services
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly IApiClient _apiClient;
        private readonly IAIAuditLogRepository _auditLogRepository;
        private readonly ILogger<AIAssistantService> _logger;

        public AIAssistantService(
            IApiClient apiClient,
            IAIAuditLogRepository auditLogRepository,
            ILogger<AIAssistantService> logger)
        {
            _apiClient = apiClient;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<AIAssistantResponse> GetWritingSuggestionAsync(AIAssistantRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("获取写作建议，文本长度: {Length}", request.Text.Length);

                var response = await _apiClient.GetWritingSuggestionAsync(request);

                // 记录审计日志（示例，实际需要用户ID）
                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GetWritingSuggestion",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取写作建议失败");

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GetWritingSuggestion",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                // 返回一个错误响应或重新抛出异常
                throw;
            }
        }

        public async Task<WritingTemplateResponse> GetWritingTemplateAsync(WritingTemplateRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("获取写作模板，类型: {TemplateType}", request.TemplateType);

                var response = await _apiClient.GetWritingTemplateAsync(request);

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GetWritingTemplate",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取写作模板失败");

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GetWritingTemplate",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<ContentGenerationResponse> GenerateContentAsync(ContentGenerationRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("生成内容，主题: {Topic}", request.Topic);

                var response = await _apiClient.GenerateContentAsync(request);

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GenerateContent",
                    requestData: request,
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成内容失败");

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GenerateContent",
                    requestData: request,
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<string> SummarizeTextAsync(string text, int maxLength = 200)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("文本摘要，长度: {Length}", text.Length);

                var response = await _apiClient.SummarizeTextAsync(text, maxLength);

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "SummarizeText",
                    requestData: new { Text = text, MaxLength = maxLength },
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文本摘要失败");

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "SummarizeText",
                    requestData: new { Text = text, MaxLength = maxLength },
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("文本翻译: {SourceLanguage} -> {TargetLanguage}", sourceLanguage, targetLanguage);

                var response = await _apiClient.TranslateTextAsync(text, sourceLanguage, targetLanguage);

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "TranslateText",
                    requestData: new { Text = text, SourceLanguage = sourceLanguage, TargetLanguage = targetLanguage },
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文本翻译失败");

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "TranslateText",
                    requestData: new { Text = text, SourceLanguage = sourceLanguage, TargetLanguage = targetLanguage },
                    errorMessage: ex.Message,
                    processingTime: DateTime.UtcNow - startTime,
                    success: false);

                throw;
            }
        }

        public async Task<List<string>> GetWritingPromptsAsync(string category = "general", int count = 5)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("获取写作提示，分类: {Category}, 数量: {Count}", category, count);

                var response = await _apiClient.GetWritingPromptsAsync(category, count);

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GetWritingPrompts",
                    requestData: new { Category = category, Count = count },
                    responseData: response,
                    processingTime: DateTime.UtcNow - startTime,
                    success: true);

                return response.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取写作提示失败");

                await LogAuditAsync(
                    serviceType: "Assistant",
                    requestType: "GetWritingPrompts",
                    requestData: new { Category = category, Count = count },
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
            Guid? userId = null) // 实际应从JWT令牌获取用户ID
        {
            try
            {
                var auditLog = new AIAuditLog
                {
                    UserId = userId ?? Guid.Empty, // 暂时使用空GUID
                    ServiceType = serviceType,
                    RequestType = requestType,
                    RequestData = System.Text.Json.JsonSerializer.Serialize(requestData),
                    ResponseData = responseData != null ? System.Text.Json.JsonSerializer.Serialize(responseData) : null,
                    StatusCode = success ? 200 : 500,
                    ErrorMessage = errorMessage,
                    ProcessingTimeMs = (long)(processingTime?.TotalMilliseconds ?? 0),
                    TokenCount = 0, // 需要从API响应中获取
                    Cost = 0, // 需要计算
                    ModelUsed = "Mock",
                    Provider = "Mock",
                    ClientIp = null, // 需要从HttpContext获取
                    UserAgent = null, // 需要从HttpContext获取
                    CorrelationId = Guid.NewGuid().ToString(),
                    SessionId = null
                };

                await _auditLogRepository.AddAsync(auditLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录审计日志失败");
                // 不抛出异常，避免影响主要业务逻辑
            }
        }
    }
}