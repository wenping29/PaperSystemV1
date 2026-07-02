using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaperSystemApi;

namespace PaperSystemApi.Models
{
    /// <summary>
    /// 聊天消息实体
    /// </summary>
    public class Message : BaseEntity
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型：Text, Image, File, System
        /// </summary>
        [Required]
        [StringLength(20)]
        public string MessageType { get; set; } = "Text";

        /// <summary>
        /// 发送者用户ID
        /// </summary>
        [Required]
        public long SenderId { get; set; }

        /// <summary>
        /// 接收者用户ID（私聊时使用）
        /// </summary>
        public long? ReceiverId { get; set; }

        /// <summary>
        /// 聊天室ID（群聊时使用）
        /// </summary>
        public long? ChatRoomId { get; set; }

        /// <summary>
        /// 父消息ID（用于回复）
        /// </summary>
        public long? ParentMessageId { get; set; }

        /// <summary>
        /// 消息状态：Sending, Sent, Delivered, Read, Failed
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Sent";

        /// <summary>
        /// 消息发送时间
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 消息送达时间
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// 消息阅读时间
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// 文件URL（如果是文件或图片消息）
        /// </summary>
        [StringLength(500)]
        public string? FileUrl { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        [StringLength(100)]
        public string? FileType { get; set; }

        /// <summary>
        /// 消息元数据（JSON格式，用于扩展）
        /// </summary>
        [Column(TypeName = "json")]
        public string? Metadata { get; set; }

        /// <summary>
        /// 发送者用户（导航属性）
        /// </summary>
        [ForeignKey("SenderId")]
        public virtual UserEntity? Sender { get; set; }

        /// <summary>
        /// 接收者用户（导航属性）
        /// </summary>
        [ForeignKey("ReceiverId")]
        public virtual UserEntity? Receiver { get; set; }

        /// <summary>
        /// 聊天室（导航属性）
        /// </summary>
        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom? ChatRoom { get; set; }

        /// <summary>
        /// 父消息（导航属性）
        /// </summary>
        [ForeignKey("ParentMessageId")]
        public virtual Message? ParentMessage { get; set; }
    }

    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        Text,
        Image,
        File,
        System
    }

    /// <summary>
    /// 消息状态枚举
    /// </summary>
    public enum MessageStatus
    {
        Sending,
        Sent,
        Delivered,
        Read,
        Failed
    }
}