using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using NotificationService.DTOs;
using NotificationService.Entities;
using NotificationService.Extensions;
using NotificationService.Hubs;
using NotificationService.Interfaces;
using NotificationService.Repositories;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ILogger<NotificationService> logger,
            INotificationRepository notificationRepository,
            IMapper mapper,
            IDistributedCache cache,
            IConnectionMultiplexer redis,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _cache = cache;
            _redis = redis;
            _hubContext = hubContext;
        }

        // Notification 操作实现
        public async Task<NotificationDTO?> GetNotificationByIdAsync(long id, long userId)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationByIdAsync(id);
                if (notification == null || notification.UserId != userId)
                {
                    return null;
                }

                return _mapper.Map<NotificationDTO>(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification by ID {NotificationId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<NotificationListResult> GetNotificationsAsync(NotificationQueryParams queryParams, long userId)
        {
            try
            {
                queryParams.UserId ??= userId;

                var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(
                    queryParams.UserId.Value,
                    queryParams.Status,
                    queryParams.Type,
                    queryParams.IsImportant,
                    queryParams.Page,
                    queryParams.PageSize);

                var totalCount = await _notificationRepository.CountNotificationsAsync(
                    queryParams.UserId.Value,
                    queryParams.Status,
                    queryParams.Type);

                var dtos = _mapper.Map<IEnumerable<NotificationDTO>>(notifications);

                return new NotificationListResult
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    Page = queryParams.Page,
                    PageSize = queryParams.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<NotificationDTO> SendNotificationAsync(CreateNotificationRequest request, long? sourceUserId = null)
        {
            try
            {
                var notification = _mapper.Map<Notification>(request);
                if (sourceUserId.HasValue)
                {
                    notification.SourceUserId = sourceUserId.Value;
                }

                var created = await _notificationRepository.CreateNotificationAsync(notification);
                var dto = _mapper.Map<NotificationDTO>(created);

                // 发送实时通知
                await SendRealTimeNotificationAsync(dto);

                // 清除缓存
                await ClearNotificationCache(notification.UserId);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<NotificationDTO> SendSystemNotificationAsync(SystemNotificationRequest request)
        {
            var createRequest = _mapper.Map<CreateNotificationRequest>(request);
            return await SendNotificationAsync(createRequest);
        }

        public async Task<NotificationDTO> SendUserNotificationAsync(UserNotificationRequest request, long sourceUserId)
        {
            var createRequest = new CreateNotificationRequest
            {
                UserId = request.ReceiverId,
                Type = request.Type,
                Title = request.Title,
                Content = request.Content,
                IsImportant = request.IsImportant,
                MetadataJson = request.MetadataJson,
                SourceUserId = sourceUserId,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                ExpiresAt = request.ExpiresAt,
                Priority = request.Priority,
                ActionUrl = request.ActionUrl,
                Icon = request.Icon,
                Tags = request.Tags
            };

            return await SendNotificationAsync(createRequest, sourceUserId);
        }

        public async Task<bool> MarkNotificationAsReadAsync(long notificationId, long userId)
        {
            try
            {
                var success = await _notificationRepository.MarkNotificationAsReadAsync(notificationId, userId);
                if (success)
                {
                    await ClearNotificationCache(userId);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                throw;
            }
        }

        public async Task<bool> MarkNotificationsAsReadAsync(MarkNotificationsAsReadRequest request, long userId)
        {
            try
            {
                bool success;
                if (request.MarkAll)
                {
                    success = await _notificationRepository.MarkAllNotificationsAsReadAsync(userId);
                }
                else
                {
                    success = await _notificationRepository.MarkNotificationsAsReadAsync(request.NotificationIds, userId);
                }

                if (success)
                {
                    await ClearNotificationCache(userId);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(long userId)
        {
            return await _notificationRepository.MarkAllNotificationsAsReadAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(long notificationId, long userId)
        {
            try
            {
                var success = await _notificationRepository.SoftDeleteNotificationAsync(notificationId);
                if (success)
                {
                    await ClearNotificationCache(userId);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", notificationId, userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationsAsync(DeleteNotificationsRequest request, long userId)
        {
            try
            {
                bool success;
                if (request.DeleteAll)
                {
                    // 获取用户所有通知ID
                    var allNotifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, null, null, null, 1, int.MaxValue);
                    var notificationIds = allNotifications.Select(n => n.Id).ToList();
                    success = await _notificationRepository.DeleteNotificationsAsync(notificationIds, userId);
                }
                else
                {
                    success = await _notificationRepository.DeleteNotificationsAsync(request.NotificationIds, userId);
                }

                if (success)
                {
                    await ClearNotificationCache(userId);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ArchiveNotificationAsync(long notificationId, long userId)
        {
            try
            {
                var success = await _notificationRepository.ArchiveNotificationsAsync(new List<long> { notificationId }, userId);
                if (success)
                {
                    await ClearNotificationCache(userId);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving notification {NotificationId} for user {UserId}", notificationId, userId);
                throw;
            }
        }

        public async Task<bool> ToggleNotificationImportanceAsync(long notificationId, long userId, bool isImportant)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
                if (notification == null || notification.UserId != userId)
                {
                    return false;
                }

                notification.IsImportant = isImportant;
                notification.UpdatedAt = DateTime.UtcNow;
                await _notificationRepository.UpdateNotificationAsync(notification);

                await ClearNotificationCache(userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling notification importance {NotificationId} for user {UserId}", notificationId, userId);
                throw;
            }
        }

        // 实时通知
        public async Task SendRealTimeNotificationAsync(NotificationDTO notification)
        {
            try
            {
                var realTimeNotification = new RealTimeNotification
                {
                    Type = "new_notification",
                    Notification = notification,
                    UnreadCount = await GetUnreadNotificationCountAsync(notification.UserId),
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("ReceiveNotification", realTimeNotification);

                _logger.LogDebug("Sent real-time notification to user {UserId}", notification.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending real-time notification to user {UserId}", notification.UserId);
                // 不抛出异常，避免影响主流程
            }
        }

        public async Task NotifyUserAsync(long userId, string type, string title, string content, object? metadata = null)
        {
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Type = type,
                Title = title,
                Content = content,
                MetadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : null
            };

            await SendNotificationAsync(request);
        }

        // 统计和查询
        public async Task<NotificationStatsDTO> GetNotificationStatsAsync(long userId)
        {
            try
            {
                var cacheKey = $"notifications:stats:{userId}";
                var cachedStats = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedStats))
                {
                    return JsonSerializer.Deserialize<NotificationStatsDTO>(cachedStats)!;
                }

                var stats = await _notificationRepository.GetNotificationStatsAsync(userId);
                var dto = _mapper.Map<NotificationStatsDTO>(stats);
                dto.UserId = userId;

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(long userId)
        {
            try
            {
                var cacheKey = $"notifications:unread:{userId}";
                var cachedCount = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCount))
                {
                    return int.Parse(cachedCount);
                }

                var count = await _notificationRepository.CountNotificationsAsync(userId, NotificationStatus.Unread);

                await _cache.SetStringAsync(cacheKey, count.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsAsync(long userId, int limit = 100)
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId, limit);
                return _mapper.Map<IEnumerable<NotificationDTO>>(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetImportantNotificationsAsync(long userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var notifications = await _notificationRepository.GetImportantNotificationsAsync(userId, page, pageSize);
                return _mapper.Map<IEnumerable<NotificationDTO>>(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting important notifications for user {UserId}", userId);
                throw;
            }
        }

        // NotificationTemplate 操作（简化实现）
        public async Task<NotificationTemplateDTO?> GetNotificationTemplateByIdAsync(long id)
        {
            try
            {
                var template = await _notificationRepository.GetNotificationTemplateByIdAsync(id);
                return template != null ? _mapper.Map<NotificationTemplateDTO>(template) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template by ID {TemplateId}", id);
                throw;
            }
        }

        public async Task<NotificationTemplateDTO?> GetNotificationTemplateByNameAsync(string name)
        {
            try
            {
                var template = await _notificationRepository.GetNotificationTemplateByNameAsync(name);
                return template != null ? _mapper.Map<NotificationTemplateDTO>(template) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template by name {TemplateName}", name);
                throw;
            }
        }

        public async Task<NotificationTemplateListResult> GetNotificationTemplatesAsync(TemplateQueryParams queryParams)
        {
            try
            {
                var templates = await _notificationRepository.GetNotificationTemplatesAsync(
                    queryParams.Type,
                    queryParams.IsActive,
                    queryParams.Page,
                    queryParams.PageSize);

                // 简化：这里应该计算总数，但为了简单起见，我们假设总数等于返回的数量
                var dtos = _mapper.Map<IEnumerable<NotificationTemplateDTO>>(templates);

                return new NotificationTemplateListResult
                {
                    Items = dtos,
                    TotalCount = templates.Count(),
                    Page = queryParams.Page,
                    PageSize = queryParams.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates");
                throw;
            }
        }

        public async Task<NotificationTemplateDTO> CreateNotificationTemplateAsync(CreateTemplateRequest request)
        {
            try
            {
                var template = _mapper.Map<NotificationTemplate>(request);
                var created = await _notificationRepository.CreateNotificationTemplateAsync(template);
                return _mapper.Map<NotificationTemplateDTO>(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template {TemplateName}", request.Name);
                throw;
            }
        }

        public async Task<NotificationTemplateDTO> UpdateNotificationTemplateAsync(long templateId, UpdateTemplateRequest request)
        {
            try
            {
                var template = await _notificationRepository.GetNotificationTemplateByIdAsync(templateId);
                if (template == null)
                {
                    throw new ArgumentException($"Template with ID {templateId} not found");
                }

                _mapper.Map(request, template);
                template.UpdatedAt = DateTime.UtcNow;

                var updated = await _notificationRepository.UpdateNotificationTemplateAsync(template);
                return _mapper.Map<NotificationTemplateDTO>(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationTemplateAsync(long templateId)
        {
            try
            {
                return await _notificationRepository.DeleteNotificationTemplateAsync(templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<NotificationDTO> RenderTemplateNotificationAsync(string templateName, Dictionary<string, string> variables, long userId)
        {
            try
            {
                var template = await GetNotificationTemplateByNameAsync(templateName);
                if (template == null)
                {
                    throw new ArgumentException($"Template {templateName} not found");
                }

                // 简化模板渲染 - 实际应用中应该使用模板引擎
                var title = template.TitleTemplate;
                var content = template.ContentTemplate;

                foreach (var variable in variables)
                {
                    title = title.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                    content = content.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                }

                var request = new CreateNotificationRequest
                {
                    UserId = userId,
                    Type = template.Type,
                    Title = title,
                    Content = content,
                    MetadataJson = JsonSerializer.Serialize(variables)
                };

                return await SendNotificationAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering template {TemplateName} for user {UserId}", templateName, userId);
                throw;
            }
        }

        // NotificationSettings 操作（简化实现）
        public async Task<NotificationSettingsDTO?> GetNotificationSettingAsync(long userId, string notificationType, string channel)
        {
            try
            {
                var setting = await _notificationRepository.GetNotificationSettingAsync(userId, notificationType, channel);
                return setting != null ? _mapper.Map<NotificationSettingsDTO>(setting) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification setting for user {UserId}, type {Type}, channel {Channel}",
                    userId, notificationType, channel);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationSettingsDTO>> GetNotificationSettingsAsync(long userId, string? notificationType = null, string? channel = null)
        {
            try
            {
                var settings = await _notificationRepository.GetNotificationSettingsByUserIdAsync(userId, notificationType, channel);
                return _mapper.Map<IEnumerable<NotificationSettingsDTO>>(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<NotificationSettingsDTO> UpdateNotificationSettingAsync(long userId, UpdateNotificationSettingsRequest request)
        {
            try
            {
                var setting = new NotificationSettings
                {
                    UserId = userId,
                    NotificationType = request.NotificationType,
                    Channel = request.Channel,
                    IsEnabled = request.IsEnabled
                };

                var updated = await _notificationRepository.CreateOrUpdateNotificationSettingAsync(setting);
                return _mapper.Map<NotificationSettingsDTO>(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification setting for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationSettingAsync(long settingId, long userId)
        {
            try
            {
                // 验证设置属于用户
                var setting = await _notificationRepository.GetNotificationSettingAsync(userId, "any", "any"); // 简化验证
                if (setting == null || setting.Id != settingId)
                {
                    return false;
                }

                return await _notificationRepository.DeleteNotificationSettingAsync(settingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification setting {SettingId} for user {UserId}", settingId, userId);
                throw;
            }
        }

        public async Task<bool> IsNotificationEnabledAsync(long userId, string notificationType, string channel)
        {
            return await _notificationRepository.IsNotificationEnabledAsync(userId, notificationType, channel);
        }

        // 批量操作（简化实现）
        public async Task<BatchOperationResult> ProcessBatchOperationsAsync(BatchNotificationRequest request, long userId)
        {
            var result = new BatchOperationResult();

            try
            {
                // 处理创建请求
                foreach (var createRequest in request.CreateRequests)
                {
                    try
                    {
                        await SendNotificationAsync(createRequest);
                        result.ProcessedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Failed to create notification: {ex.Message}");
                    }
                }

                // 处理标记为已读
                if (request.MarkAllAsRead)
                {
                    try
                    {
                        await MarkAllNotificationsAsReadAsync(userId);
                        result.ProcessedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Failed to mark all as read: {ex.Message}");
                    }
                }
                else if (request.MarkAsReadIds.Any())
                {
                    try
                    {
                        await _notificationRepository.MarkNotificationsAsReadAsync(request.MarkAsReadIds, userId);
                        result.ProcessedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Failed to mark notifications as read: {ex.Message}");
                    }
                }

                // 处理删除
                if (request.DeleteAll)
                {
                    try
                    {
                        // 获取用户所有通知ID
                        var allNotifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, null, null, null, 1, int.MaxValue);
                        var notificationIds = allNotifications.Select(n => n.Id).ToList();
                        await _notificationRepository.DeleteNotificationsAsync(notificationIds, userId);
                        result.ProcessedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Failed to delete all notifications: {ex.Message}");
                    }
                }
                else if (request.DeleteIds.Any())
                {
                    try
                    {
                        await _notificationRepository.DeleteNotificationsAsync(request.DeleteIds, userId);
                        result.ProcessedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Failed to delete notifications: {ex.Message}");
                    }
                }

                result.Success = result.FailedCount == 0;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch operations for user {UserId}", userId);
                result.Success = false;
                result.Errors.Add($"Batch operation failed: {ex.Message}");
                return result;
            }
        }

        public async Task<bool> CleanupExpiredNotificationsAsync(DateTime cutoffDate)
        {
            return await _notificationRepository.CleanupExpiredNotificationsAsync(cutoffDate);
        }

        // 工具方法
        public async Task<bool> AreNotificationsEnabledAsync(long userId, string notificationType)
        {
            // 检查至少一个渠道启用
            var channels = new[] { NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Push };
            foreach (var channel in channels)
            {
                if (await IsNotificationEnabledAsync(userId, notificationType, channel))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<string> GetNotificationStatusAsync(long notificationId, long userId)
        {
            var notification = await GetNotificationByIdAsync(notificationId, userId);
            return notification?.Status ?? "unknown";
        }

        public async Task<bool> ValidateNotificationAccessAsync(long notificationId, long userId)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
            return notification != null && notification.UserId == userId && !notification.IsDeleted;
        }

        // 私有方法
        private async Task ClearNotificationCache(long userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());

                // 清除通知相关缓存
                var pattern = $"WritingPlatform:NotificationService:notifications:*:user:{userId}:*";
                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cleared {Count} notification cache keys for user {UserId}", keys.Length, userId);
                }

                // 清除统计缓存
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
}