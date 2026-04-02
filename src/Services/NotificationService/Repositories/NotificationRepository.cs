using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Interfaces;

namespace NotificationService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        // Notification 操作实现
        public async Task<Notification?> GetNotificationByIdAsync(long id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(long userId, string? status, string? type, bool? isImportant, int page, int pageSize)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(n => n.Status == status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(n => n.Type == type);
            }

            if (isImportant.HasValue)
            {
                query = query.Where(n => n.IsImportant == isImportant.Value);
            }

            return await query
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> SearchNotificationsAsync(long userId, string? searchTerm, int page, int pageSize)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(n =>
                    n.Title.Contains(searchTerm) ||
                    n.Content.Contains(searchTerm) ||
                    (n.Tags != null && n.Tags.Contains(searchTerm)));
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> UpdateNotificationAsync(Notification notification)
        {
            notification.UpdatedAt = DateTime.UtcNow;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteNotificationAsync(long id)
        {
            var notification = await GetNotificationByIdAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteNotificationAsync(long id)
        {
            var notification = await GetNotificationByIdAsync(id);
            if (notification == null) return false;

            notification.IsDeleted = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkNotificationAsReadAsync(long notificationId, long userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null) return false;

            if (notification.Status != NotificationStatus.Read)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> MarkNotificationsAsReadAsync(IEnumerable<long> notificationIds, long userId)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId && !n.IsDeleted)
                .ToListAsync();

            if (!notifications.Any()) return false;

            var now = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                if (notification.Status != NotificationStatus.Read)
                {
                    notification.Status = NotificationStatus.Read;
                    notification.ReadAt = now;
                    notification.UpdatedAt = now;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(long userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread && !n.IsDeleted)
                .ToListAsync();

            if (!notifications.Any()) return false;

            var now = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = now;
                notification.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountNotificationsAsync(long userId, string? status = null, string? type = null)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(n => n.Status == status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(n => n.Type == type);
            }

            return await query.CountAsync();
        }

        public async Task<bool> NotificationExistsAsync(long id)
        {
            return await _context.Notifications
                .AnyAsync(n => n.Id == id && !n.IsDeleted);
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(long userId, int limit = 100)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread && !n.IsDeleted)
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetImportantNotificationsAsync(long userId, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.IsImportant && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(long userId, string type, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.Type == type && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByRelatedEntityAsync(string entityType, long entityId, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.RelatedEntityType == entityType && n.RelatedEntityId == entityId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // 批量操作
        public async Task<bool> DeleteNotificationsAsync(IEnumerable<long> notificationIds, long userId)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId)
                .ToListAsync();

            if (!notifications.Any()) return false;

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveNotificationsAsync(IEnumerable<long> notificationIds, long userId)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId && !n.IsDeleted)
                .ToListAsync();

            if (!notifications.Any()) return false;

            var now = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Archived;
                notification.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CleanupExpiredNotificationsAsync(DateTime cutoffDate)
        {
            var expiredNotifications = await _context.Notifications
                .Where(n => n.ExpiresAt != null && n.ExpiresAt < cutoffDate && !n.IsDeleted)
                .ToListAsync();

            if (!expiredNotifications.Any()) return false;

            foreach (var notification in expiredNotifications)
            {
                notification.IsDeleted = true;
                notification.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // 统计操作
        public async Task<NotificationStats> GetNotificationStatsAsync(long userId)
        {
            var stats = new NotificationStats
            {
                UserId = userId
            };

            // 总通知数
            stats.TotalNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .CountAsync();

            // 未读通知数
            stats.UnreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread && !n.IsDeleted)
                .CountAsync();

            // 重要通知数
            stats.ImportantNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsImportant && !n.IsDeleted)
                .CountAsync();

            // 按类型统计
            var typeStats = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .GroupBy(n => n.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var typeStat in typeStats)
            {
                stats.NotificationsByType[typeStat.Type] = typeStat.Count;
            }

            // 最后通知时间
            var lastNotification = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync();

            stats.LastNotificationAt = lastNotification?.CreatedAt;

            // 今日通知数
            var todayStart = DateTime.UtcNow.Date;
            stats.TodayNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.CreatedAt >= todayStart && !n.IsDeleted)
                .CountAsync();

            return stats;
        }

        // NotificationTemplate 操作
        public async Task<NotificationTemplate?> GetNotificationTemplateByIdAsync(long id)
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<NotificationTemplate?> GetNotificationTemplateByNameAsync(string name)
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted);
        }

        public async Task<IEnumerable<NotificationTemplate>> GetNotificationTemplatesAsync(string? type, bool? isActive, int page, int pageSize)
        {
            var query = _context.NotificationTemplates
                .Where(t => !t.IsDeleted);

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(t => t.Type == type);
            }

            if (isActive.HasValue)
            {
                query = query.Where(t => t.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<NotificationTemplate> CreateNotificationTemplateAsync(NotificationTemplate template)
        {
            _context.NotificationTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<NotificationTemplate> UpdateNotificationTemplateAsync(NotificationTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            _context.NotificationTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteNotificationTemplateAsync(long id)
        {
            var template = await GetNotificationTemplateByIdAsync(id);
            if (template == null) return false;

            template.IsDeleted = true;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> NotificationTemplateExistsAsync(string name)
        {
            return await _context.NotificationTemplates
                .AnyAsync(t => t.Name == name && !t.IsDeleted);
        }

        public async Task<IEnumerable<NotificationTemplate>> SearchNotificationTemplatesAsync(string? searchTerm, int page, int pageSize)
        {
            var query = _context.NotificationTemplates
                .Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Name.Contains(searchTerm) ||
                    t.Description.Contains(searchTerm) ||
                    t.TitleTemplate.Contains(searchTerm));
            }

            return await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // NotificationSettings 操作
        public async Task<NotificationSettings?> GetNotificationSettingAsync(long userId, string notificationType, string channel)
        {
            return await _context.NotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == userId && s.NotificationType == notificationType && s.Channel == channel);
        }

        public async Task<IEnumerable<NotificationSettings>> GetNotificationSettingsByUserIdAsync(long userId, string? notificationType = null, string? channel = null)
        {
            var query = _context.NotificationSettings
                .Where(s => s.UserId == userId);

            if (!string.IsNullOrEmpty(notificationType))
            {
                query = query.Where(s => s.NotificationType == notificationType);
            }

            if (!string.IsNullOrEmpty(channel))
            {
                query = query.Where(s => s.Channel == channel);
            }

            return await query.ToListAsync();
        }

        public async Task<NotificationSettings> CreateOrUpdateNotificationSettingAsync(NotificationSettings setting)
        {
            var existing = await GetNotificationSettingAsync(setting.UserId, setting.NotificationType, setting.Channel);

            if (existing != null)
            {
                existing.IsEnabled = setting.IsEnabled;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.NotificationSettings.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
            }
            else
            {
                _context.NotificationSettings.Add(setting);
                await _context.SaveChangesAsync();
                return setting;
            }
        }

        public async Task<bool> DeleteNotificationSettingAsync(long id)
        {
            var setting = await _context.NotificationSettings.FindAsync(id);
            if (setting == null) return false;

            _context.NotificationSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationSettingsByUserIdAsync(long userId, string? notificationType = null, string? channel = null)
        {
            var query = _context.NotificationSettings
                .Where(s => s.UserId == userId);

            if (!string.IsNullOrEmpty(notificationType))
            {
                query = query.Where(s => s.NotificationType == notificationType);
            }

            if (!string.IsNullOrEmpty(channel))
            {
                query = query.Where(s => s.Channel == channel);
            }

            var settings = await query.ToListAsync();
            if (!settings.Any()) return false;

            _context.NotificationSettings.RemoveRange(settings);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsNotificationEnabledAsync(long userId, string notificationType, string channel)
        {
            var setting = await GetNotificationSettingAsync(userId, notificationType, channel);

            // 如果存在设置，返回设置值；否则返回默认值（启用）
            return setting?.IsEnabled ?? true;
        }

        public async Task<IEnumerable<NotificationSettings>> GetEnabledNotificationSettingsAsync(long userId, string notificationType)
        {
            return await _context.NotificationSettings
                .Where(s => s.UserId == userId && s.NotificationType == notificationType && s.IsEnabled)
                .ToListAsync();
        }
    }
}