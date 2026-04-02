using NotificationService.DTOs;

namespace NotificationService.Interfaces
{
    public interface INotificationService
    {
        // Notification 操作
        Task<NotificationDTO?> GetNotificationByIdAsync(long id, long userId);
        Task<NotificationListResult> GetNotificationsAsync(NotificationQueryParams queryParams, long userId);
        Task<NotificationDTO> SendNotificationAsync(CreateNotificationRequest request, long? sourceUserId = null);
        Task<NotificationDTO> SendSystemNotificationAsync(SystemNotificationRequest request);
        Task<NotificationDTO> SendUserNotificationAsync(UserNotificationRequest request, long sourceUserId);
        Task<bool> MarkNotificationAsReadAsync(long notificationId, long userId);
        Task<bool> MarkNotificationsAsReadAsync(MarkNotificationsAsReadRequest request, long userId);
        Task<bool> MarkAllNotificationsAsReadAsync(long userId);
        Task<bool> DeleteNotificationAsync(long notificationId, long userId);
        Task<bool> DeleteNotificationsAsync(DeleteNotificationsRequest request, long userId);
        Task<bool> ArchiveNotificationAsync(long notificationId, long userId);
        Task<bool> ToggleNotificationImportanceAsync(long notificationId, long userId, bool isImportant);

        // 实时通知
        Task SendRealTimeNotificationAsync(NotificationDTO notification);
        Task NotifyUserAsync(long userId, string type, string title, string content, object? metadata = null);

        // 统计和查询
        Task<NotificationStatsDTO> GetNotificationStatsAsync(long userId);
        Task<int> GetUnreadNotificationCountAsync(long userId);
        Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsAsync(long userId, int limit = 100);
        Task<IEnumerable<NotificationDTO>> GetImportantNotificationsAsync(long userId, int page = 1, int pageSize = 20);

        // NotificationTemplate 操作
        Task<NotificationTemplateDTO?> GetNotificationTemplateByIdAsync(long id);
        Task<NotificationTemplateDTO?> GetNotificationTemplateByNameAsync(string name);
        Task<NotificationTemplateListResult> GetNotificationTemplatesAsync(TemplateQueryParams queryParams);
        Task<NotificationTemplateDTO> CreateNotificationTemplateAsync(CreateTemplateRequest request);
        Task<NotificationTemplateDTO> UpdateNotificationTemplateAsync(long templateId, UpdateTemplateRequest request);
        Task<bool> DeleteNotificationTemplateAsync(long templateId);
        Task<NotificationDTO> RenderTemplateNotificationAsync(string templateName, Dictionary<string, string> variables, long userId);

        // NotificationSettings 操作
        Task<NotificationSettingsDTO?> GetNotificationSettingAsync(long userId, string notificationType, string channel);
        Task<IEnumerable<NotificationSettingsDTO>> GetNotificationSettingsAsync(long userId, string? notificationType = null, string? channel = null);
        Task<NotificationSettingsDTO> UpdateNotificationSettingAsync(long userId, UpdateNotificationSettingsRequest request);
        Task<bool> DeleteNotificationSettingAsync(long settingId, long userId);
        Task<bool> IsNotificationEnabledAsync(long userId, string notificationType, string channel);

        // 批量操作
        Task<BatchOperationResult> ProcessBatchOperationsAsync(BatchNotificationRequest request, long userId);
        Task<bool> CleanupExpiredNotificationsAsync(DateTime cutoffDate);

        // 工具方法
        Task<bool> AreNotificationsEnabledAsync(long userId, string notificationType);
        Task<string> GetNotificationStatusAsync(long notificationId, long userId);
        Task<bool> ValidateNotificationAccessAsync(long notificationId, long userId);
    }

    public class NotificationListResult
    {
        public IEnumerable<NotificationDTO> Items { get; set; } = new List<NotificationDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }

    public class NotificationTemplateListResult
    {
        public IEnumerable<NotificationTemplateDTO> Items { get; set; } = new List<NotificationTemplateDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }

    public class BatchOperationResult
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}