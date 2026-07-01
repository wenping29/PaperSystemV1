using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaperSystemApi;

namespace PaperSystemApi.Models
{
    /// <summary>
    /// 聊天室实体
    /// </summary>
    public class ChatRoom : BaseEntity
    {
        /// <summary>
        /// 聊天室名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 聊天室类型：Private, Group, Channel
        /// </summary>
        [Required]
        [StringLength(20)]
        public string RoomType { get; set; } = "Private";

        /// <summary>
        /// 创建者用户ID
        /// </summary>
        [Required]
        public long CreatorId { get; set; }

        /// <summary>
        /// 聊天室头像URL
        /// </summary>
        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// 最大成员数（0表示无限制）
        /// </summary>
        public int MaxMembers { get; set; } = 0;

        /// <summary>
        /// 是否加密（端到端加密）
        /// </summary>
        public bool IsEncrypted { get; set; } = false;

        /// <summary>
        /// 是否公开（可被搜索加入）
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// 是否启用邀请链接
        /// </summary>
        public bool InviteEnabled { get; set; } = true;

        /// <summary>
        /// 邀请链接（唯一标识）
        /// </summary>
        [StringLength(100)]
        public string? InviteCode { get; set; }

        /// <summary>
        /// 邀请链接过期时间
        /// </summary>
        public DateTime? InviteExpiresAt { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 元数据（JSON格式，用于扩展）
        /// </summary>
        [Column(TypeName = "json")]
        public string? Metadata { get; set; }

        /// <summary>
        /// 创建者用户（导航属性）
        /// </summary>
        [ForeignKey("CreatorId")]
        public virtual UserEntity? Creator { get; set; }

        /// <summary>
        /// 聊天室成员（导航属性）
        /// </summary>
        public virtual ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();

        /// <summary>
        /// 聊天消息（导航属性）
        /// </summary>
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    /// <summary>
    /// 聊天室类型枚举
    /// </summary>
    public enum ChatRoomType
    {
        Private,  // 私聊（两人）
        Group,    // 群聊
        Channel   // 频道（广播）
    }
}