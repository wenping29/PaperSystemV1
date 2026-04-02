using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface ILikeRepository
    {
        Task<Like?> GetByIdAsync(long id);
        Task<Like?> GetByUserAndTargetAsync(long userId, LikeTargetType targetType, long targetId);
        Task<IEnumerable<Like>> GetAllAsync(int page, int pageSize, long? userId, string? targetType, long? targetId);
        Task<Like> CreateAsync(Like like);
        Task<bool> DeleteAsync(long id);
        Task<bool> DeleteByUserAndTargetAsync(long userId, LikeTargetType targetType, long targetId);
        Task<bool> ExistsAsync(long id);
        Task<bool> ExistsByUserAndTargetAsync(long userId, LikeTargetType targetType, long targetId);
        Task<int> GetCountByTargetAsync(LikeTargetType targetType, long targetId);
        Task<int> GetCountByUserAsync(long userId, LikeTargetType? targetType);
    }
}