using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.Chat.DTOs
{
    /// <summary>
    /// 创建聊天室请求
    /// </summary>
    public class CreateChatRoomRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string RoomType { get; set; } = "Group";

        public bool IsPublic { get; set; } = false;

        [Range(0, 10000)]
        public int MaxMembers { get; set; } = 0;

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public bool InviteEnabled { get; set; } = true;
    }

    /// <summary>
    /// 更新聊天室请求
    /// </summary>
    public class UpdateChatRoomRequest
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsPublic { get; set; }

        [Range(0, 10000)]
        public int? MaxMembers { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public bool? InviteEnabled { get; set; }
    }

    /// <summary>
    /// 聊天室响应
    /// </summary>
    public class ChatRoomResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public long CreatorId { get; set; }
        public bool IsPublic { get; set; }
        public bool InviteEnabled { get; set; }
        public int MaxMembers { get; set; }
        public int MemberCount { get; set; }
        public string? AvatarUrl { get; set; }
        public string? InviteCode { get; set; }
        public DateTime? InviteExpiresAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // 关联信息
        public UserInfo? Creator { get; set; }
        public ChatRoomMemberInfo? CurrentUserMember { get; set; }
        public IEnumerable<ChatRoomMemberInfo>? Members { get; set; }
    }

    /// <summary>
    /// 聊天室列表响应（简化版）
    /// </summary>
    public class ChatRoomListResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public long CreatorId { get; set; }
        public bool IsPublic { get; set; }
        public int MemberCount { get; set; }
        public int UnreadCount { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime LastActivityAt { get; set; }
        public UserInfo? Creator { get; set; }
        public ChatRoomMemberInfo? CurrentUserMember { get; set; }
        public MessageInfo? LastMessage { get; set; }
    }

    /// <summary>
    /// 聊天室成员信息
    /// </summary>
    public class ChatRoomMemberInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ChatRoomId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LastReadAt { get; set; }
        public bool IsMuted { get; set; }
        public DateTime? MutedUntil { get; set; }
        public bool IsBlocked { get; set; }

        // 关联信息
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// 加入聊天室请求
    /// </summary>
    public class JoinChatRoomRequest
    {
        [StringLength(100)]
        public string? InviteCode { get; set; }
    }

    /// <summary>
    /// 更新成员角色请求
    /// </summary>
    public class UpdateMemberRoleRequest
    {
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// 生成邀请码请求
    /// </summary>
    public class GenerateInviteCodeRequest
    {
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// 邀请码响应
    /// </summary>
    public class InviteCodeResponse
    {
        public string InviteCode { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public string InviteUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 搜索聊天室请求
    /// </summary>
    public class SearchChatRoomsRequest
    {
        [StringLength(100, MinimumLength = 1)]
        public string SearchTerm { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        public bool? IsPublic { get; set; }
        public string? RoomType { get; set; }
    }
}