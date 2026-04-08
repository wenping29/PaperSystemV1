using System;

namespace PaperSystemApi.DTOs
{
    public class FriendshipDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long FriendId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Alias { get; set; }
        public string? Note { get; set; }
        public bool IsFavorite { get; set; }
        public int InteractionScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastInteractedAt { get; set; }
        public string? MetadataJson { get; set; }
        public string? Tags { get; set; }
        public string? PrivacySettings { get; set; }
        // 好友信息（从用户服务获取）
        public FriendInfoDTO? FriendInfo { get; set; }
    }

    public class FriendRequestDTO
    {
        public long Id { get; set; }
        public long RequesterId { get; set; }
        public long ReceiverId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTime? ExpiresAt { get; set; }
        // 请求者信息
        public UserInfoDTO? RequesterInfo { get; set; }
        // 接收者信息
        public UserInfoDTO? ReceiverInfo { get; set; }
    }

    public class CreateFriendRequest
    {
        public long ReceiverId { get; set; }
        public string? Message { get; set; }
    }

    public class RespondToFriendRequest
    {
        public bool Accept { get; set; }
        public string? ResponseMessage { get; set; }
    }

    public class UpdateFriendshipRequest
    {
        public string? Alias { get; set; }
        public string? Note { get; set; }
        public bool? IsFavorite { get; set; }
        public string? Status { get; set; }
    }

    public class FriendshipQueryParams
    {
        public long? UserId { get; set; }
        public string? Status { get; set; }
        public bool? IsFavorite { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "LastInteractedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class FriendRequestQueryParams
    {
        public long? UserId { get; set; }
        public string? Type { get; set; } // sent, received
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class FriendInfoDTO
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastActive { get; set; }
        public int MutualFriends { get; set; }
    }

    public class UserInfoDTO
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
    }

    public class FriendshipStatsDTO
    {
        public long UserId { get; set; }
        public int TotalFriends { get; set; }
        public int ActiveFriends { get; set; }
        public int FavoriteFriends { get; set; }
        public int PendingRequests { get; set; }
        public int SentRequests { get; set; }
        public int MutualFriends { get; set; }
        public DateTime? LastFriendAdded { get; set; }
    }

    public class MutualFriendsResult
    {
        public long User1Id { get; set; }
        public long User2Id { get; set; }
        public int MutualCount { get; set; }
        public List<FriendInfoDTO> MutualFriends { get; set; } = new List<FriendInfoDTO>();
    }
}