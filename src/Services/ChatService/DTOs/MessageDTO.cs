using System.ComponentModel.DataAnnotations;

namespace ChatService.DTOs
{
    /// <summary>
    /// 发送消息请求
    /// </summary>
    public class SendMessageRequest
    {
        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        [StringLength(20)]
        public string MessageType { get; set; } = "Text";

        public long? ReceiverId { get; set; }

        public long? ChatRoomId { get; set; }

        public long? ParentMessageId { get; set; }

        [StringLength(500)]
        public string? FileUrl { get; set; }

        public long? FileSize { get; set; }

        [StringLength(100)]
        public string? FileType { get; set; }
    }

    /// <summary>
    /// 更新消息请求
    /// </summary>
    public class UpdateMessageRequest
    {
        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 消息响应
    /// </summary>
    public class MessageResponse
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public long SenderId { get; set; }
        public long? ReceiverId { get; set; }
        public long? ChatRoomId { get; set; }
        public long? ParentMessageId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? FileUrl { get; set; }
        public long? FileSize { get; set; }
        public string? FileType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // 关联信息
        public UserInfo? Sender { get; set; }
        public UserInfo? Receiver { get; set; }
        public ChatRoomInfo? ChatRoom { get; set; }
        public MessageInfo? ParentMessage { get; set; }
    }

    /// <summary>
    /// 消息列表响应（简化版）
    /// </summary>
    public class MessageListResponse
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public long SenderId { get; set; }
        public long? ReceiverId { get; set; }
        public long? ChatRoomId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public UserInfo? Sender { get; set; }
        public bool IsOwnMessage { get; set; }
    }

    /// <summary>
    /// 用户信息（简化）
    /// </summary>
    public class UserInfo
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// 聊天室信息（简化）
    /// </summary>
    public class ChatRoomInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// 消息信息（简化）
    /// </summary>
    public class MessageInfo
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public long SenderId { get; set; }
        public UserInfo? Sender { get; set; }
    }

    /// <summary>
    /// 搜索消息请求
    /// </summary>
    public class SearchMessagesRequest
    {
        public long? ChatRoomId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string SearchTerm { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
}