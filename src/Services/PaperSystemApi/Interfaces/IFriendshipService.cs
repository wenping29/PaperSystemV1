using PaperSystemApi.DTOs;

namespace PaperSystemApi.Services
{
    public interface IFriendshipService
    {
        // Friendship 操作
        Task<FriendshipDTO?> GetFriendshipByIdAsync(long id);
        Task<FriendshipDTO?> GetFriendshipAsync(long userId, long friendId);
        Task<IEnumerable<FriendshipDTO>> GetFriendshipsAsync(FriendshipQueryParams queryParams);
        Task<int> GetFriendshipsCountAsync(FriendshipQueryParams queryParams);
        Task<FriendshipDTO> SendFriendRequestAsync(CreateFriendRequest request, long requesterId);
        Task<FriendRequestDTO?> RespondToFriendRequestAsync(long requestId, RespondToFriendRequest response, long userId);
        Task<FriendshipDTO?> UpdateFriendshipAsync(long friendshipId, UpdateFriendshipRequest request, long userId);
        Task<bool> DeleteFriendshipAsync(long friendshipId, long userId);
        Task<bool> RemoveFriendAsync(long userId, long friendId);
        Task<bool> BlockUserAsync(long userId, long blockedUserId);
        Task<bool> UnblockUserAsync(long userId, long blockedUserId);
        Task<bool> FavoriteFriendAsync(long userId, long friendId, bool isFavorite);
        Task<bool> UpdateInteractionAsync(long userId, long friendId);

        // FriendRequest 操作
        Task<FriendRequestDTO?> GetFriendRequestByIdAsync(long id);
        Task<IEnumerable<FriendRequestDTO>> GetFriendRequestsAsync(FriendRequestQueryParams queryParams);
        Task<int> GetFriendRequestsCountAsync(FriendRequestQueryParams queryParams);
        Task<bool> CancelFriendRequestAsync(long requestId, long userId);
        Task<bool> CancelFriendRequestByUserIdsAsync(long requesterId, long receiverId);

        // 查询操作
        Task<IEnumerable<FriendInfoDTO>> GetFriendSuggestionsAsync(long userId, int limit = 10);
        Task<MutualFriendsResult> GetMutualFriendsAsync(long user1Id, long user2Id, int page = 1, int pageSize = 20);
        Task<FriendshipStatsDTO> GetFriendshipStatsAsync(long userId);
        Task<bool> AreFriendsAsync(long user1Id, long user2Id);
        Task<string> GetFriendshipStatusAsync(long user1Id, long user2Id);

        // 批量操作
        Task<bool> ImportFriendshipsAsync(long userId, IEnumerable<long> friendIds);
        Task<bool> ExportFriendshipsAsync(long userId, string format = "json");
        Task<bool> CleanupExpiredRequestsAsync(DateTime cutoffDate);
    }
}