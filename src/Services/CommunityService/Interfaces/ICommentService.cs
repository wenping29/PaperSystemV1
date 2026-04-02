using CommunityService.DTOs;
using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponse?> GetCommentByIdAsync(long id, long? currentUserId);
        Task<IEnumerable<CommentResponse>> GetCommentsAsync(CommentQueryParams queryParams, long? currentUserId);
        Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, long authorId);
        Task<CommentResponse?> UpdateCommentAsync(long id, UpdateCommentRequest request, long currentUserId);
        Task<bool> DeleteCommentAsync(long id, long currentUserId);
        Task<bool> SoftDeleteCommentAsync(long id, long currentUserId);
        Task<bool> LikeCommentAsync(long commentId, long userId);
        Task<bool> UnlikeCommentAsync(long commentId, long userId);
        Task<IEnumerable<CommentResponse>> GetRepliesAsync(long parentId, int page, int pageSize, long? currentUserId);
        Task<bool> UpdateCommentStatusAsync(long id, CommentStatus status, long currentUserId);
        Task<CommentStatsResponse> GetCommentStatsAsync(long id);
        Task<bool> IsLikedAsync(long commentId, long userId);
    }

    public class CommentStatsResponse
    {
        public long CommentId { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}