using System;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Entities
{
    public class Notification
    {
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; } // 接收通知的用户

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = NotificationType.System; // 通知类型

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty; // 通知标题

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty; // 通知内容

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = NotificationStatus.Unread; // 通知状态

        [Required]
        public bool IsImportant { get; set; } = false; // 是否重要通知

        [StringLength(1000)]
        public string? MetadataJson { get; set; } // 额外元数据，JSON格式

        public long? SourceUserId { get; set; } // 触发通知的用户（可为空，系统通知）

        [StringLength(50)]
        public string? RelatedEntityType { get; set; } // 相关实体类型，如："Writing", "Comment", "FriendRequest"

        public long? RelatedEntityId { get; set; } // 相关实体ID

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 创建时间

        public DateTime? ReadAt { get; set; } // 阅读时间

        public DateTime? ExpiresAt { get; set; } // 过期时间

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // 更新时间

        [Required]
        public bool IsDeleted { get; set; } = false; // 软删除标志

        [Required]
        public int Priority { get; set; } = 0; // 优先级，0=普通，1=重要，2=紧急

        [StringLength(500)]
        public string? ActionUrl { get; set; } // 操作链接

        [StringLength(100)]
        public string? Icon { get; set; } // 图标名称或URL

        [StringLength(500)]
        public string? Tags { get; set; } // 标签，逗号分隔
    }

    public class NotificationTemplate
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // 模板名称（唯一）

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // 模板类型，与Notification.Type对应

        [Required]
        [StringLength(200)]
        public string TitleTemplate { get; set; } = string.Empty; // 标题模板，支持变量如{{UserName}}

        [Required]
        [StringLength(2000)]
        public string ContentTemplate { get; set; } = string.Empty; // 内容模板

        [StringLength(1000)]
        public string? VariablesJson { get; set; } // 变量定义，JSON格式

        [Required]
        public bool IsActive { get; set; } = true; // 是否启用

        [StringLength(500)]
        public string? Description { get; set; } // 模板描述

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;
    }

    public class NotificationSettings
    {
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; } // 用户ID

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty; // 通知类型，或"All"表示所有类型

        [Required]
        [StringLength(20)]
        public string Channel { get; set; } = NotificationChannel.InApp; // 通知渠道

        [Required]
        public bool IsEnabled { get; set; } = true; // 是否启用

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class NotificationType
    {
        public const string System = "system"; // 系统通知
        public const string FriendRequest = "friend_request"; // 好友请求
        public const string FriendRequestAccepted = "friend_request_accepted"; // 好友请求接受
        public const string Message = "message"; // 消息
        public const string Comment = "comment"; // 评论
        public const string Like = "like"; // 点赞
        public const string Mention = "mention"; // 提及
        public const string Follow = "follow"; // 关注
        public const string WritingPublished = "writing_published"; // 作品发布
        public const string WritingUpdated = "writing_updated"; // 作品更新
        public const string Achievement = "achievement"; // 成就
        public const string Warning = "warning"; // 警告
        public const string Info = "info"; // 信息
    }

    public static class NotificationStatus
    {
        public const string Unread = "unread"; // 未读
        public const string Read = "read"; // 已读
        public const string Archived = "archived"; // 已归档
        public const string Deleted = "deleted"; // 已删除（软删除前状态）
    }

    public static class NotificationChannel
    {
        public const string InApp = "in_app"; // 应用内通知
        public const string Email = "email"; // 邮件
        public const string Sms = "sms"; // 短信
        public const string Push = "push"; // 推送通知
        public const string All = "all"; // 所有渠道
    }

    public static class NotificationPriority
    {
        public const int Normal = 0; // 普通
        public const int Important = 1; // 重要
        public const int Urgent = 2; // 紧急
    }
}