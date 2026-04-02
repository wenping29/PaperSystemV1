using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(long id);
        Task<IEnumerable<Comment>> GetAllAsync(int page, int pageSize, long? postId, long? authorId, long? parentId, string? sortBy);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task<bool> DeleteAsync(long id);
        Task<bool> SoftDeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<int> GetCountAsync(long? postId, long? authorId, long? parentId);
        Task<IEnumerable<Comment>> GetByPostIdAsync(long postId, int page, int pageSize, bool includeReplies);
        Task<IEnumerable<Comment>> GetRepliesAsync(long parentId, int page, int pageSize);
        Task<bool> IncrementLikeCountAsync(long id, int delta);
        Task<bool> UpdateStatusAsync(long id, CommentStatus status);
    }
}