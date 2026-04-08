using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaperSystemApi.Common;

namespace PaperSystemApi.Chat.Entities
{
    /// <summary>
    /// 用户消息阅读状态实体
    /// </summary>
    public class UserMessageRead : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        [Required]
        public long MessageId { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 设备标识
        /// </summary>
        [StringLength(100)]
        public string? DeviceId { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 用户（导航属性）
        /// </summary>
        [ForeignKey("UserId")]
        public virtual UserEntity? User { get; set; }

        /// <summary>
        /// 消息（导航属性）
        /// </summary>
        [ForeignKey("MessageId")]
        public virtual Message? Message { get; set; }
    }
}