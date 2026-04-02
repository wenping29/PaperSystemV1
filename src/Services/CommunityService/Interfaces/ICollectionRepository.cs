using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface ICollectionRepository
    {
        Task<Collection?> GetByIdAsync(long id);
        Task<Collection?> GetByUserAndPostAsync(long userId, long postId);
        Task<IEnumerable<Collection>> GetAllAsync(int page, int pageSize, long? userId, long? postId);
        Task<Collection> CreateAsync(Collection collection);
        Task<Collection> UpdateAsync(Collection collection);
        Task<bool> DeleteAsync(long id);
        Task<bool> DeleteByUserAndPostAsync(long userId, long postId);
        Task<bool> ExistsAsync(long id);
        Task<bool> ExistsByUserAndPostAsync(long userId, long postId);
        Task<int> GetCountByUserAsync(long userId);
        Task<int> GetCountByPostAsync(long postId);
        Task<IEnumerable<Collection>> GetByUserIdAsync(long userId, int page, int pageSize);
        Task<IEnumerable<Collection>> GetByPostIdAsync(long postId, int page, int pageSize);
    }
}