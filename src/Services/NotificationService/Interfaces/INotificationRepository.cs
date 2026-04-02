using NotificationService.Entities;

namespace NotificationService.Interfaces
{
    public interface INotificationRepository
    {
        // Notification 操作
        Task<Notification?> GetNotificationByIdAsync(long id);
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(long userId, string? status, string? type, bool? isImportant, int page, int pageSize);
        Task<IEnumerable<Notification>> SearchNotificationsAsync(long userId, string? searchTerm, int page, int pageSize);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<Notification> UpdateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(long id);
        Task<bool> SoftDeleteNotificationAsync(long id);
        Task<bool> MarkNotificationAsReadAsync(long notificationId, long userId);
        Task<bool> MarkNotificationsAsReadAsync(IEnumerable<long> notificationIds, long userId);
        Task<bool> MarkAllNotificationsAsReadAsync(long userId);
        Task<int> CountNotificationsAsync(long userId, string? status = null, string? type = null);
        Task<bool> NotificationExistsAsync(long id);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(long userId, int limit = 100);
        Task<IEnumerable<Notification>> GetImportantNotificationsAsync(long userId, int page, int pageSize);
        Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(long userId, string type, int page, int pageSize);
        Task<IEnumerable<Notification>> GetNotificationsByRelatedEntityAsync(string entityType, long entityId, int page, int pageSize);

        // 批量操作
        Task<bool> DeleteNotificationsAsync(IEnumerable<long> notificationIds, long userId);
        Task<bool> ArchiveNotificationsAsync(IEnumerable<long> notificationIds, long userId);
        Task<bool> CleanupExpiredNotificationsAsync(DateTime cutoffDate);

        // 统计操作
        Task<NotificationStats> GetNotificationStatsAsync(long userId);

        // NotificationTemplate 操作
        Task<NotificationTemplate?> GetNotificationTemplateByIdAsync(long id);
        Task<NotificationTemplate?> GetNotificationTemplateByNameAsync(string name);
        Task<IEnumerable<NotificationTemplate>> GetNotificationTemplatesAsync(string? type, bool? isActive, int page, int pageSize);
        Task<NotificationTemplate> CreateNotificationTemplateAsync(NotificationTemplate template);
        Task<NotificationTemplate> UpdateNotificationTemplateAsync(NotificationTemplate template);
        Task<bool> DeleteNotificationTemplateAsync(long id);
        Task<bool> NotificationTemplateExistsAsync(string name);
        Task<IEnumerable<NotificationTemplate>> SearchNotificationTemplatesAsync(string? searchTerm, int page, int pageSize);

        // NotificationSettings 操作
        Task<NotificationSettings?> GetNotificationSettingAsync(long userId, string notificationType, string channel);
        Task<IEnumerable<NotificationSettings>> GetNotificationSettingsByUserIdAsync(long userId, string? notificationType = null, string? channel = null);
        Task<NotificationSettings> CreateOrUpdateNotificationSettingAsync(NotificationSettings setting);
        Task<bool> DeleteNotificationSettingAsync(long id);
        Task<bool> DeleteNotificationSettingsByUserIdAsync(long userId, string? notificationType = null, string? channel = null);
        Task<bool> IsNotificationEnabledAsync(long userId, string notificationType, string channel);
        Task<IEnumerable<NotificationSettings>> GetEnabledNotificationSettingsAsync(long userId, string notificationType);
    }

    public class NotificationStats
    {
        public long UserId { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int ImportantNotifications { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; } = new Dictionary<string, int>();
        public DateTime? LastNotificationAt { get; set; }
        public int TodayNotifications { get; set; }
    }
}