using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Models;

namespace ChatService.Entities
{
    /// <summary>
    /// 聊天室成员实体
    /// </summary>
    public class ChatRoomMember : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// 聊天室ID
        /// </summary>
        [Required]
        public long ChatRoomId { get; set; }

        /// <summary>
        /// 成员角色：Owner, Admin, Member, Guest
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Member";

        /// <summary>
        /// 成员昵称（在聊天室中的昵称）
        /// </summary>
        [StringLength(50)]
        public string? Nickname { get; set; }

        /// <summary>
        /// 加入时间
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最后阅读消息ID
        /// </summary>
        public long? LastReadMessageId { get; set; }

        /// <summary>
        /// 最后阅读时间
        /// </summary>
        public DateTime? LastReadAt { get; set; }

        /// <summary>
        /// 是否被禁言
        /// </summary>
        public bool IsMuted { get; set; } = false;

        /// <summary>
        /// 禁言结束时间
        /// </summary>
        public DateTime? MutedUntil { get; set; }

        /// <summary>
        /// 是否被屏蔽
        /// </summary>
        public bool IsBlocked { get; set; } = false;

        /// <summary>
        /// 元数据（JSON格式，用于扩展）
        /// </summary>
        [Column(TypeName = "json")]
        public string? Metadata { get; set; }

        /// <summary>
        /// 用户（导航属性）
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// 聊天室（导航属性）
        /// </summary>
        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom? ChatRoom { get; set; }

        /// <summary>
        /// 最后阅读的消息（导航属性）
        /// </summary>
        [ForeignKey("LastReadMessageId")]
        public virtual Message? LastReadMessage { get; set; }
    }

    /// <summary>
    /// 聊天室成员角色枚举
    /// </summary>
    public enum ChatRoomMemberRole
    {
        Owner,   // 拥有者
        Admin,   // 管理员
        Member,  // 普通成员
        Guest    // 访客
    }
}