using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using NotificationService.DTOs;
using NotificationService.Entities;
using NotificationService.Extensions;
using NotificationService.Hubs;
using NotificationService.Interfaces;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;
        private readonly INotificationService _notificationService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(
            ILogger<NotificationsController> logger,
            INotificationService notificationService,
            IDistributedCache cache,
            IConnectionMultiplexer redis,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _notificationService = notificationService;
            _cache = cache;
            _redis = redis;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] NotificationQueryParams queryParams)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                // 使用传入的UserId或当前用户ID
                queryParams.UserId ??= userId.Value;

                _logger.LogInformation("Getting notifications for user {UserId}, Status: {Status}, Type: {Type}, Page: {Page}, PageSize: {PageSize}",
                    queryParams.UserId, queryParams.Status, queryParams.Type, queryParams.Page, queryParams.PageSize);

                // 生成缓存键
                var cacheKey = $"notifications:user:{queryParams.UserId}:status:{queryParams.Status}:type:{queryParams.Type}:important:{queryParams.IsImportant}:page:{queryParams.Page}:size:{queryParams.PageSize}";
                var cachedNotifications = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedNotifications))
                {
                    _logger.LogDebug("Cache hit for notifications of user {UserId}", queryParams.UserId);
                    return Ok(JsonSerializer.Deserialize<object>(cachedNotifications));
                }

                var notifications = await _notificationService.GetNotificationsAsync(queryParams, userId.Value);

                // 缓存结果
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(notifications), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { error = "An error occurred while getting notifications" });
            }
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotification(long notificationId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting notification {NotificationId} for user {UserId}", notificationId, userId);

                var notification = await _notificationService.GetNotificationByIdAsync(notificationId, userId.Value);
                if (notification == null)
                {
                    return NotFound(new { error = "Notification not found" });
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification");
                return StatusCode(500, new { error = "An error occurred while getting notification" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetNotificationStats()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting notification stats for user {UserId}", userId);

                var cacheKey = $"notifications:stats:{userId}";
                var cachedStats = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedStats))
                {
                    _logger.LogDebug("Cache hit for notification stats of user {UserId}", userId);
                    return Ok(JsonSerializer.Deserialize<object>(cachedStats));
                }

                var stats = await _notificationService.GetNotificationStatsAsync(userId.Value);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats");
                return StatusCode(500, new { error = "An error occurred while getting notification stats" });
            }
        }

        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting unread notification count for user {UserId}", userId);

                var cacheKey = $"notifications:unread:{userId}";
                var cachedCount = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCount))
                {
                    _logger.LogDebug("Cache hit for unread count of user {UserId}", userId);
                    return Ok(new { count = int.Parse(cachedCount) });
                }

                var count = await _notificationService.GetUnreadNotificationCountAsync(userId.Value);

                await _cache.SetStringAsync(cacheKey, count.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return StatusCode(500, new { error = "An error occurred while getting unread notification count" });
            }
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications([FromQuery] int limit = 100)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting unread notifications for user {UserId}, limit {Limit}", userId, limit);

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId.Value, limit);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return StatusCode(500, new { error = "An error occurred while getting unread notifications" });
            }
        }

        [HttpGet("important")]
        public async Task<IActionResult> GetImportantNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting important notifications for user {UserId}, Page: {Page}, PageSize: {PageSize}",
                    userId, page, pageSize);

                var notifications = await _notificationService.GetImportantNotificationsAsync(userId.Value, page, pageSize);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting important notifications");
                return StatusCode(500, new { error = "An error occurred while getting important notifications" });
            }
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkNotificationsAsRead([FromBody] MarkNotificationsAsReadRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} marking notifications as read, MarkAll: {MarkAll}, Count: {Count}",
                    userId, request.MarkAll, request.NotificationIds?.Count ?? 0);

                var success = await _notificationService.MarkNotificationsAsReadAsync(request, userId.Value);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to mark notifications as read" });
                }

                // 发送实时更新
                var realTimeNotification = new RealTimeNotification
                {
                    Type = "notifications_updated",
                    UnreadCount = await _notificationService.GetUnreadNotificationCountAsync(userId.Value),
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.User(userId.Value.ToString())
                    .SendAsync("ReceiveNotification", realTimeNotification);

                return Ok(new { message = "Notifications marked as read successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as read");
                return StatusCode(500, new { error = "An error occurred while marking notifications as read" });
            }
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} marking all notifications as read", userId);

                var success = await _notificationService.MarkAllNotificationsAsReadAsync(userId.Value);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to mark all notifications as read" });
                }

                // 发送实时更新
                var realTimeNotification = new RealTimeNotification
                {
                    Type = "notifications_updated",
                    UnreadCount = 0,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.User(userId.Value.ToString())
                    .SendAsync("ReceiveNotification", realTimeNotification);

                // 清除相关缓存
                await ClearNotificationCache(userId.Value);

                return Ok(new { message = "All notifications marked as read successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { error = "An error occurred while marking all notifications as read" });
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(long notificationId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} deleting notification {NotificationId}", userId, notificationId);

                var success = await _notificationService.DeleteNotificationAsync(notificationId, userId.Value);
                if (!success)
                {
                    return NotFound(new { error = "Notification not found or you don't have permission to delete it" });
                }

                // 清除缓存
                await ClearNotificationCache(userId.Value);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, new { error = "An error occurred while deleting notification" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteNotifications([FromBody] DeleteNotificationsRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} deleting notifications, DeleteAll: {DeleteAll}, Count: {Count}",
                    userId, request.DeleteAll, request.NotificationIds?.Count ?? 0);

                var success = await _notificationService.DeleteNotificationsAsync(request, userId.Value);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to delete notifications" });
                }

                // 清除缓存
                await ClearNotificationCache(userId.Value);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notifications");
                return StatusCode(500, new { error = "An error occurred while deleting notifications" });
            }
        }

        [HttpPost("{notificationId}/archive")]
        public async Task<IActionResult> ArchiveNotification(long notificationId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} archiving notification {NotificationId}", userId, notificationId);

                var success = await _notificationService.ArchiveNotificationAsync(notificationId, userId.Value);
                if (!success)
                {
                    return NotFound(new { error = "Notification not found or you don't have permission to archive it" });
                }

                // 清除缓存
                await ClearNotificationCache(userId.Value);

                return Ok(new { message = "Notification archived successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving notification");
                return StatusCode(500, new { error = "An error occurred while archiving notification" });
            }
        }

        [HttpPut("{notificationId}/important")]
        public async Task<IActionResult> ToggleNotificationImportance(long notificationId, [FromBody] bool isImportant)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} toggling notification {NotificationId} importance to {IsImportant}",
                    userId, notificationId, isImportant);

                var success = await _notificationService.ToggleNotificationImportanceAsync(notificationId, userId.Value, isImportant);
                if (!success)
                {
                    return NotFound(new { error = "Notification not found or you don't have permission to modify it" });
                }

                // 清除缓存
                await ClearNotificationCache(userId.Value);

                return Ok(new { message = "Notification importance updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling notification importance");
                return StatusCode(500, new { error = "An error occurred while toggling notification importance" });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> ProcessBatchOperations([FromBody] BatchNotificationRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} processing batch operations: {CreateCount} creates, {MarkReadCount} mark read, {DeleteCount} deletes",
                    userId, request.CreateRequests.Count, request.MarkAsReadIds.Count, request.DeleteIds.Count);

                var result = await _notificationService.ProcessBatchOperationsAsync(request, userId.Value);

                // 清除缓存
                await ClearNotificationCache(userId.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch operations");
                return StatusCode(500, new { error = "An error occurred while processing batch operations" });
            }
        }

        [HttpPost("test")]
        [AllowAnonymous]
        public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || !long.TryParse(request.UserId, out var userId))
                {
                    return BadRequest(new { error = "Invalid user ID" });
                }

                _logger.LogInformation("Sending test notification to user {UserId}", userId);

                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = userId,
                    Type = request.Type ?? NotificationType.System,
                    Title = request.Title ?? "Test Notification",
                    Content = request.Content ?? "This is a test notification sent at " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsImportant = request.IsImportant,
                    MetadataJson = request.MetadataJson
                };

                var notification = await _notificationService.SendNotificationAsync(notificationRequest);

                return Ok(new
                {
                    message = "Test notification sent successfully",
                    notificationId = notification.Id,
                    notification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                return StatusCode(500, new { error = "An error occurred while sending test notification" });
            }
        }

        private long? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        private async Task ClearNotificationCache(long userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = $"WritingPlatform:NotificationService:notifications:*:user:{userId}:*";

                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cleared {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
                }

                // 也清除统计缓存
                var statsKey = $"WritingPlatform:NotificationService:notifications:stats:{userId}";
                await db.KeyDeleteAsync(statsKey);

                // 清除未读数缓存
                var unreadKey = $"WritingPlatform:NotificationService:notifications:unread:{userId}";
                await db.KeyDeleteAsync(unreadKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing notification cache for user {UserId}", userId);
            }
        }
    }

    public class TestNotificationRequest
    {
        public string? UserId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool IsImportant { get; set; }
        public string? MetadataJson { get; set; }
    }
}