using System;
using System.Collections.Generic;

namespace NotificationService.DTOs
{
    public class NotificationDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
        public string? MetadataJson { get; set; }
        public long? SourceUserId { get; set; }
        public string? RelatedEntityType { get; set; }
        public long? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Priority { get; set; }
        public string? ActionUrl { get; set; }
        public string? Icon { get; set; }
        public string? Tags { get; set; }
        // 源用户信息（从用户服务获取）
        public UserInfoDTO? SourceUserInfo { get; set; }
        // 相关实体信息（可选）
        public object? RelatedEntityInfo { get; set; }
    }

    public class CreateNotificationRequest
    {
        public long UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
        public string? MetadataJson { get; set; }
        public long? SourceUserId { get; set; }
        public string? RelatedEntityType { get; set; }
        public long? RelatedEntityId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int Priority { get; set; }
        public string? ActionUrl { get; set; }
        public string? Icon { get; set; }
        public string? Tags { get; set; }
    }

    public class SystemNotificationRequest
    {
        public long UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
        public string? MetadataJson { get; set; }
        public string? RelatedEntityType { get; set; }
        public long? RelatedEntityId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int Priority { get; set; }
        public string? ActionUrl { get; set; }
        public string? Icon { get; set; }
        public string? Tags { get; set; }
    }

    public class UserNotificationRequest
    {
        public long ReceiverId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
        public string? MetadataJson { get; set; }
        public string? RelatedEntityType { get; set; }
        public long? RelatedEntityId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int Priority { get; set; }
        public string? ActionUrl { get; set; }
        public string? Icon { get; set; }
        public string? Tags { get; set; }
    }

    public class NotificationQueryParams
    {
        public long? UserId { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public bool? IsImportant { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? RelatedEntityType { get; set; }
        public long? RelatedEntityId { get; set; }
    }

    public class MarkNotificationsAsReadRequest
    {
        public List<long> NotificationIds { get; set; } = new List<long>();
        public bool MarkAll { get; set; }
    }

    public class DeleteNotificationsRequest
    {
        public List<long> NotificationIds { get; set; } = new List<long>();
        public bool DeleteAll { get; set; }
    }

    public class BatchNotificationRequest
    {
        public List<CreateNotificationRequest> CreateRequests { get; set; } = new List<CreateNotificationRequest>();
        public List<long> MarkAsReadIds { get; set; } = new List<long>();
        public List<long> DeleteIds { get; set; } = new List<long>();
        public bool MarkAllAsRead { get; set; }
        public bool DeleteAll { get; set; }
    }

    public class NotificationStatsDTO
    {
        public long UserId { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int ImportantNotifications { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; } = new Dictionary<string, int>();
        public DateTime? LastNotificationAt { get; set; }
        public int TodayNotifications { get; set; }
    }

    // 模板相关DTOs
    public class NotificationTemplateDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TitleTemplate { get; set; } = string.Empty;
        public string ContentTemplate { get; set; } = string.Empty;
        public string? VariablesJson { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TitleTemplate { get; set; } = string.Empty;
        public string ContentTemplate { get; set; } = string.Empty;
        public string? VariablesJson { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTemplateRequest
    {
        public string? TitleTemplate { get; set; }
        public string? ContentTemplate { get; set; }
        public string? VariablesJson { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class TemplateQueryParams
    {
        public string? Type { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // 设置相关DTOs
    public class NotificationSettingsDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateNotificationSettingsRequest
    {
        public string NotificationType { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    // 通用DTOs
    public class UserInfoDTO
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
    }

    public class RealTimeNotification
    {
        public string Type { get; set; } = string.Empty;
        public NotificationDTO? Notification { get; set; }
        public int UnreadCount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}