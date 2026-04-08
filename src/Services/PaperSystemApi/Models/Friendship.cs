using System;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.Models
{
    public class Friendship
    {
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public long FriendId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = FriendshipStatus.Active;

        [StringLength(100)]
        public string? Alias { get; set; } // 好友备注

        [StringLength(500)]
        public string? Note { get; set; } // 备注

        [Required]
        public bool IsFavorite { get; set; } = false;

        [Required]
        public int InteractionScore { get; set; } = 0; // 互动分数

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastInteractedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // 好友关系元数据
        [StringLength(1000)]
        public string? MetadataJson { get; set; } // 如：共同好友数、共同兴趣等

        [StringLength(500)]
        public string? Tags { get; set; } // 好友标签，逗号分隔，如："同事,同学,书友"

        [StringLength(500)]
        public string? PrivacySettings { get; set; } // 隐私设置，JSON格式
    }

    public class FriendRequest
    {
        public long Id { get; set; }

        [Required]
        public long RequesterId { get; set; } // 发送请求的用户

        [Required]
        public long ReceiverId { get; set; } // 接收请求的用户

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = FriendRequestStatus.Pending;

        [StringLength(500)]
        public string? Message { get; set; } // 请求消息

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? RespondedAt { get; set; }

        [StringLength(1000)]
        public string? ResponseMessage { get; set; }

        public DateTime? ExpiresAt { get; set; } // 请求过期时间，默认为创建后7天

        public bool IsDeleted { get; set; } = false;
    }

    public static class FriendshipStatus
    {
        public const string Pending = "pending"; // 等待确认（双向好友需要双方确认）
        public const string Active = "active"; // 活跃好友
        public const string Inactive = "inactive"; // 不活跃
        public const string Blocked = "blocked"; // 已屏蔽
        public const string Restricted = "restricted"; // 限制互动
    }

    public static class FriendRequestStatus
    {
        public const string Pending = "pending";
        public const string Accepted = "accepted";
        public const string Rejected = "rejected";
        public const string Cancelled = "cancelled";
        public const string Expired = "expired";
    }
}