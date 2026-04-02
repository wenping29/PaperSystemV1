using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface IPostRepository
    {
        Task<Post?> GetByIdAsync(long id);
        Task<IEnumerable<Post>> GetAllAsync(int page, int pageSize, string? category, string? tag, string? keyword, string? sortBy, long? authorId, string? status, string? visibility);
        Task<Post> CreateAsync(Post post);
        Task<Post> UpdateAsync(Post post);
        Task<bool> DeleteAsync(long id);
        Task<bool> SoftDeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<int> GetCountAsync(string? category, string? tag, string? keyword, long? authorId, string? status, string? visibility);
        Task<IEnumerable<Post>> GetHotPostsAsync(int limit);
        Task<IEnumerable<Post>> GetByAuthorIdAsync(long authorId, int page, int pageSize, string? status);
        Task<bool> IncrementViewCountAsync(long id);
        Task<bool> IncrementLikeCountAsync(long id, int delta);
        Task<bool> IncrementCommentCountAsync(long id, int delta);
        Task<bool> IncrementCollectionCountAsync(long id, int delta);
        Task<bool> UpdateHotScoreAsync(long id, decimal hotScore);
        Task<bool> UpdateStatusAsync(long id, PostStatus status);
        Task<IEnumerable<Post>> SearchAsync(string keyword, int page, int pageSize);
    }
}