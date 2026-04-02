using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using NotificationService.DTOs;
using NotificationService.Interfaces;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/v1/notification-templates")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class NotificationTemplatesController : ControllerBase
    {
        private readonly ILogger<NotificationTemplatesController> _logger;
        private readonly INotificationService _notificationService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public NotificationTemplatesController(
            ILogger<NotificationTemplatesController> logger,
            INotificationService notificationService,
            IDistributedCache cache,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _notificationService = notificationService;
            _cache = cache;
            _redis = redis;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificationTemplates([FromQuery] TemplateQueryParams queryParams)
        {
            try
            {
                _logger.LogInformation("Getting notification templates, Type: {Type}, IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
                    queryParams.Type, queryParams.IsActive, queryParams.Page, queryParams.PageSize);

                // 生成缓存键
                var cacheKey = $"notification-templates:type:{queryParams.Type}:active:{queryParams.IsActive}:page:{queryParams.Page}:size:{queryParams.PageSize}";
                var cachedTemplates = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedTemplates))
                {
                    _logger.LogDebug("Cache hit for notification templates");
                    return Ok(JsonSerializer.Deserialize<object>(cachedTemplates));
                }

                var templates = await _notificationService.GetNotificationTemplatesAsync(queryParams);

                // 缓存结果
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(templates), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates");
                return StatusCode(500, new { error = "An error occurred while getting notification templates" });
            }
        }

        [HttpGet("{templateId}")]
        public async Task<IActionResult> GetNotificationTemplate(long templateId)
        {
            try
            {
                _logger.LogInformation("Getting notification template {TemplateId}", templateId);

                var template = await _notificationService.GetNotificationTemplateByIdAsync(templateId);
                if (template == null)
                {
                    return NotFound(new { error = "Notification template not found" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template");
                return StatusCode(500, new { error = "An error occurred while getting notification template" });
            }
        }

        [HttpGet("name/{templateName}")]
        public async Task<IActionResult> GetNotificationTemplateByName(string templateName)
        {
            try
            {
                _logger.LogInformation("Getting notification template by name {TemplateName}", templateName);

                var template = await _notificationService.GetNotificationTemplateByNameAsync(templateName);
                if (template == null)
                {
                    return NotFound(new { error = $"Notification template '{templateName}' not found" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template by name");
                return StatusCode(500, new { error = "An error occurred while getting notification template by name" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotificationTemplate([FromBody] CreateTemplateRequest request)
        {
            try
            {
                _logger.LogInformation("Creating notification template: {TemplateName}", request.Name);

                // 检查模板名称是否已存在
                var existingTemplate = await _notificationService.GetNotificationTemplateByNameAsync(request.Name);
                if (existingTemplate != null)
                {
                    return Conflict(new { error = $"Notification template '{request.Name}' already exists" });
                }

                var template = await _notificationService.CreateNotificationTemplateAsync(request);

                // 清除模板缓存
                await ClearTemplateCache();

                return CreatedAtAction(nameof(GetNotificationTemplate), new { templateId = template.Id }, template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template");
                return StatusCode(500, new { error = "An error occurred while creating notification template" });
            }
        }

        [HttpPut("{templateId}")]
        public async Task<IActionResult> UpdateNotificationTemplate(long templateId, [FromBody] UpdateTemplateRequest request)
        {
            try
            {
                _logger.LogInformation("Updating notification template {TemplateId}", templateId);

                // 检查模板是否存在
                var existingTemplate = await _notificationService.GetNotificationTemplateByIdAsync(templateId);
                if (existingTemplate == null)
                {
                    return NotFound(new { error = $"Notification template with ID {templateId} not found" });
                }

                var template = await _notificationService.UpdateNotificationTemplateAsync(templateId, request);

                // 清除模板缓存
                await ClearTemplateCache();

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification template");
                return StatusCode(500, new { error = "An error occurred while updating notification template" });
            }
        }

        [HttpDelete("{templateId}")]
        public async Task<IActionResult> DeleteNotificationTemplate(long templateId)
        {
            try
            {
                _logger.LogInformation("Deleting notification template {TemplateId}", templateId);

                var success = await _notificationService.DeleteNotificationTemplateAsync(templateId);
                if (!success)
                {
                    return NotFound(new { error = $"Notification template with ID {templateId} not found" });
                }

                // 清除模板缓存
                await ClearTemplateCache();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification template");
                return StatusCode(500, new { error = "An error occurred while deleting notification template" });
            }
        }

        [HttpPost("render/{templateName}")]
        [AllowAnonymous]
        public async Task<IActionResult> RenderTemplateNotification(string templateName, [FromBody] RenderTemplateRequest request)
        {
            try
            {
                _logger.LogInformation("Rendering template {TemplateName} for user {UserId}", templateName, request.UserId);

                // 验证用户ID
                if (request.UserId <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID" });
                }

                var notification = await _notificationService.RenderTemplateNotificationAsync(templateName, request.Variables, request.UserId);

                return Ok(new
                {
                    message = "Template rendered and notification sent successfully",
                    notificationId = notification.Id,
                    notification
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering template notification");
                return StatusCode(500, new { error = "An error occurred while rendering template notification" });
            }
        }

        [HttpPost("test-render/{templateName}")]
        [AllowAnonymous]
        public async Task<IActionResult> TestRenderTemplate(string templateName, [FromBody] TestRenderRequest request)
        {
            try
            {
                _logger.LogInformation("Test rendering template {TemplateName}", templateName);

                // 获取模板
                var template = await _notificationService.GetNotificationTemplateByNameAsync(templateName);
                if (template == null)
                {
                    return NotFound(new { error = $"Notification template '{templateName}' not found" });
                }

                // 渲染模板内容（不发送通知）
                var title = template.TitleTemplate;
                var content = template.ContentTemplate;

                foreach (var variable in request.Variables)
                {
                    title = title.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                    content = content.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                }

                return Ok(new
                {
                    templateName,
                    renderedTitle = title,
                    renderedContent = content,
                    variables = request.Variables,
                    testData = request.TestData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error test rendering template");
                return StatusCode(500, new { error = "An error occurred while test rendering template" });
            }
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetTemplateTypes()
        {
            try
            {
                _logger.LogInformation("Getting notification template types");

                // 从缓存获取
                var cacheKey = "notification-templates:types";
                var cachedTypes = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedTypes))
                {
                    _logger.LogDebug("Cache hit for template types");
                    return Ok(JsonSerializer.Deserialize<object>(cachedTypes));
                }

                // 获取所有模板并提取类型
                var templates = await _notificationService.GetNotificationTemplatesAsync(new TemplateQueryParams { PageSize = 1000 });
                var types = templates.Items
                    .Select(t => t.Type)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                var result = new { types, count = types.Count };

                // 缓存结果
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template types");
                return StatusCode(500, new { error = "An error occurred while getting template types" });
            }
        }

        [HttpPost("{templateId}/toggle-active")]
        public async Task<IActionResult> ToggleTemplateActive(long templateId, [FromBody] bool isActive)
        {
            try
            {
                _logger.LogInformation("Toggling template {TemplateId} active status to {IsActive}", templateId, isActive);

                var request = new UpdateTemplateRequest { IsActive = isActive };
                var template = await _notificationService.UpdateNotificationTemplateAsync(templateId, request);

                // 清除模板缓存
                await ClearTemplateCache();

                return Ok(new
                {
                    message = $"Template active status updated to {isActive}",
                    template
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling template active status");
                return StatusCode(500, new { error = "An error occurred while toggling template active status" });
            }
        }

        private async Task ClearTemplateCache()
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = "WritingPlatform:NotificationService:notification-templates:*";

                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cleared {Count} template cache keys", keys.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing template cache");
            }
        }
    }

    public class RenderTemplateRequest
    {
        public long UserId { get; set; }
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }

    public class TestRenderRequest
    {
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
        public object? TestData { get; set; }
    }
}