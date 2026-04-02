using CommunityService.DTOs;
using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface ILikeService
    {
        Task<LikeResponse?> GetLikeByIdAsync(long id);
        Task<IEnumerable<LikeResponse>> GetLikesAsync(LikeQueryParams queryParams);
        Task<LikeResponse> CreateLikeAsync(CreateLikeRequest request, long userId);
        Task<bool> DeleteLikeAsync(long id, long userId);
        Task<bool> DeleteLikeByTargetAsync(DeleteLikeRequest request, long userId);
        Task<bool> HasLikedAsync(long userId, LikeTargetType targetType, long targetId);
        Task<int> GetLikeCountAsync(LikeTargetType targetType, long targetId);
        Task<LikeStatsResponse> GetLikeStatsAsync(long userId);
    }

    public class LikeStatsResponse
    {
        public long UserId { get; set; }
        public int PostLikesCount { get; set; }
        public int CommentLikesCount { get; set; }
        public int TotalLikesCount { get; set; }
        public DateTime FirstLikeAt { get; set; }
        public DateTime LastLikeAt { get; set; }
    }
}