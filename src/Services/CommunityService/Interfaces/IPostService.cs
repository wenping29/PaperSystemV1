using CommunityService.DTOs;
using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface IPostService
    {
        Task<PostResponse?> GetPostByIdAsync(long id, long? currentUserId);
        Task<IEnumerable<PostListResponse>> GetPostsAsync(PostQueryParams queryParams, long? currentUserId);
        Task<PostResponse> CreatePostAsync(CreatePostRequest request, long authorId);
        Task<PostResponse?> UpdatePostAsync(long id, UpdatePostRequest request, long currentUserId);
        Task<bool> DeletePostAsync(long id, long currentUserId);
        Task<bool> SoftDeletePostAsync(long id, long currentUserId);
        Task<bool> IncrementViewCountAsync(long id);
        Task<bool> LikePostAsync(long postId, long userId);
        Task<bool> UnlikePostAsync(long postId, long userId);
        Task<bool> CollectPostAsync(long postId, long userId, string? note);
        Task<bool> UncollectPostAsync(long postId, long userId);
        Task<IEnumerable<PostListResponse>> GetHotPostsAsync(int limit, long? currentUserId);
        Task<IEnumerable<PostListResponse>> GetPostsByAuthorIdAsync(long authorId, int page, int pageSize, long? currentUserId);
        Task<bool> UpdatePostStatusAsync(long id, PostStatus status, long currentUserId);
        Task<PostStatsResponse> GetPostStatsAsync(long id);
        Task<bool> IsLikedAsync(long postId, long userId);
        Task<bool> IsCollectedAsync(long postId, long userId);
    }

    public class PostStatsResponse
    {
        public long PostId { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int CollectionCount { get; set; }
        public int ViewCount { get; set; }
        public decimal HotScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}