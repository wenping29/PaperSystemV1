using FriendshipService.Entities;

namespace FriendshipService.Interfaces
{
    public interface IFriendshipRepository
    {
        // Friendship 操作
        Task<Friendship?> GetFriendshipByIdAsync(long id);
        Task<Friendship?> GetFriendshipAsync(long userId, long friendId);
        Task<IEnumerable<Friendship>> GetFriendshipsByUserIdAsync(long userId, string? status, bool? isFavorite, int page, int pageSize);
        Task<IEnumerable<Friendship>> SearchFriendshipsAsync(long userId, string? searchTerm, int page, int pageSize);
        Task<Friendship> CreateFriendshipAsync(Friendship friendship);
        Task<Friendship> UpdateFriendshipAsync(Friendship friendship);
        Task<bool> DeleteFriendshipAsync(long id);
        Task<bool> SoftDeleteFriendshipAsync(long id);
        Task<bool> UpdateFriendshipStatusAsync(long userId, long friendId, string status);
        Task<bool> UpdateInteractionAsync(long userId, long friendId);
        Task<int> CountFriendshipsAsync(long userId, string? status = null);
        Task<bool> FriendshipExistsAsync(long userId, long friendId);
        Task<IEnumerable<long>> GetFriendIdsAsync(long userId, string? status = null);
        Task<IEnumerable<Friendship>> GetMutualFriendshipsAsync(long user1Id, long user2Id);
        Task<IEnumerable<Friendship>> GetFriendshipsByTagsAsync(long userId, string tags, int page, int pageSize);

        // FriendRequest 操作
        Task<FriendRequest?> GetFriendRequestByIdAsync(long id);
        Task<FriendRequest?> GetFriendRequestAsync(long requesterId, long receiverId, string? status = null);
        Task<IEnumerable<FriendRequest>> GetFriendRequestsByUserIdAsync(long userId, string? type, string? status, int page, int pageSize);
        Task<IEnumerable<FriendRequest>> GetPendingFriendRequestsAsync(long userId);
        Task<FriendRequest> CreateFriendRequestAsync(FriendRequest request);
        Task<FriendRequest> UpdateFriendRequestAsync(FriendRequest request);
        Task<bool> DeleteFriendRequestAsync(long id);
        Task<bool> FriendRequestExistsAsync(long requesterId, long receiverId, string? status = null);
        Task<int> CountFriendRequestsAsync(long userId, string? type = null, string? status = null);
        Task<bool> CancelFriendRequestAsync(long requesterId, long receiverId);
        Task<bool> AcceptFriendRequestAsync(long requestId, string? responseMessage = null);
        Task<bool> RejectFriendRequestAsync(long requestId, string? responseMessage = null);
        Task<IEnumerable<FriendRequest>> GetExpiredFriendRequestsAsync(DateTime cutoffDate);
        Task<bool> CleanupExpiredFriendRequestsAsync(DateTime cutoffDate);

        // 统计操作
        Task<FriendshipStats> GetFriendshipStatsAsync(long userId);
    }

    public class FriendshipStats
    {
        public int TotalFriends { get; set; }
        public int ActiveFriends { get; set; }
        public int FavoriteFriends { get; set; }
        public int PendingRequests { get; set; }
        public int SentRequests { get; set; }
        public DateTime? LastFriendAdded { get; set; }
    }
}