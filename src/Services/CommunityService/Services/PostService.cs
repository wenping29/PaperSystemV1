using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using CommunityService.DTOs;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;
        private readonly ICollectionRepository _collectionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PostService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;

        public PostService(
            IPostRepository postRepository,
            ILikeRepository likeRepository,
            ICollectionRepository collectionRepository,
            IMapper mapper,
            ILogger<PostService> logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _collectionRepository = collectionRepository;
            _mapper = mapper;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        public async Task<PostResponse?> GetPostByIdAsync(long id, long? currentUserId)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return null;

            await _postRepository.IncrementViewCountAsync(id);
            var response = _mapper.Map<PostResponse>(post);

            if (currentUserId.HasValue)
            {
                response.IsLiked = await _likeRepository.ExistsByUserAndTargetAsync(currentUserId.Value, LikeTargetType.Post, id);
                response.IsCollected = await _collectionRepository.ExistsByUserAndPostAsync(currentUserId.Value, id);
            }

            // 从用户服务获取作者信息
            var userInfo = await GetUserInfoAsync(post.AuthorId);
            response.AuthorName = userInfo.Username;
            response.AuthorAvatar = userInfo.AvatarUrl;

            return response;
        }

        public async Task<IEnumerable<PostListResponse>> GetPostsAsync(PostQueryParams queryParams, long? currentUserId)
        {
            var posts = await _postRepository.GetAllAsync(
                queryParams.Page, queryParams.PageSize, queryParams.Category,
                queryParams.Tag, queryParams.Keyword, queryParams.SortBy,
                queryParams.AuthorId, queryParams.Status, queryParams.Visibility);

            var responses = new List<PostListResponse>();
            foreach (var post in posts)
            {
                var response = _mapper.Map<PostListResponse>(post);
                // 从用户服务获取作者信息
                var userInfo = await GetUserInfoAsync(post.AuthorId);
                response.AuthorName = userInfo.Username;
                response.AuthorAvatar = userInfo.AvatarUrl;
                responses.Add(response);
            }

            return responses;
        }

        public async Task<PostResponse> CreatePostAsync(CreatePostRequest request, long authorId)
        {
            var post = _mapper.Map<Post>(request);
            post.AuthorId = authorId;
            post.Status = PostStatus.Pending;
            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            var createdPost = await _postRepository.CreateAsync(post);
            return _mapper.Map<PostResponse>(createdPost);
        }

        public async Task<PostResponse?> UpdatePostAsync(long id, UpdatePostRequest request, long currentUserId)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return null;

            if (post.AuthorId != currentUserId)
            {
                // 检查是否是管理员
                var isAdmin = await IsUserAdminAsync(currentUserId);
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this post");
                }
            }

            _mapper.Map(request, post);
            post.UpdatedAt = DateTime.UtcNow;

            var updatedPost = await _postRepository.UpdateAsync(post);
            return _mapper.Map<PostResponse>(updatedPost);
        }

        public async Task<bool> DeletePostAsync(long id, long currentUserId)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return false;

            if (post.AuthorId != currentUserId)
            {
                // 检查是否是管理员
                var isAdmin = await IsUserAdminAsync(currentUserId);
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this post");
                }
            }

            return await _postRepository.DeleteAsync(id);
        }

        public async Task<bool> SoftDeletePostAsync(long id, long currentUserId)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return false;

            if (post.AuthorId != currentUserId)
            {
                // 检查是否是管理员
                var isAdmin = await IsUserAdminAsync(currentUserId);
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this post");
                }
            }

            return await _postRepository.SoftDeleteAsync(id);
        }

        public async Task<bool> IncrementViewCountAsync(long id)
        {
            return await _postRepository.IncrementViewCountAsync(id);
        }

        public async Task<bool> LikePostAsync(long postId, long userId)
        {
            var alreadyLiked = await _likeRepository.ExistsByUserAndTargetAsync(userId, LikeTargetType.Post, postId);
            if (alreadyLiked) return false;

            var like = new Like
            {
                UserId = userId,
                TargetType = LikeTargetType.Post,
                TargetId = postId,
                CreatedAt = DateTime.UtcNow
            };

            await _likeRepository.CreateAsync(like);
            await _postRepository.IncrementLikeCountAsync(postId, 1);
            return true;
        }

        public async Task<bool> UnlikePostAsync(long postId, long userId)
        {
            var like = await _likeRepository.GetByUserAndTargetAsync(userId, LikeTargetType.Post, postId);
            if (like == null) return false;

            await _likeRepository.DeleteAsync(like.Id);
            await _postRepository.IncrementLikeCountAsync(postId, -1);
            return true;
        }

        public async Task<bool> CollectPostAsync(long postId, long userId, string? note)
        {
            var alreadyCollected = await _collectionRepository.ExistsByUserAndPostAsync(userId, postId);
            if (alreadyCollected) return false;

            var collection = new Collection
            {
                UserId = userId,
                PostId = postId,
                Note = note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _collectionRepository.CreateAsync(collection);
            await _postRepository.IncrementCollectionCountAsync(postId, 1);
            return true;
        }

        public async Task<bool> UncollectPostAsync(long postId, long userId)
        {
            var collection = await _collectionRepository.GetByUserAndPostAsync(userId, postId);
            if (collection == null) return false;

            await _collectionRepository.DeleteAsync(collection.Id);
            await _postRepository.IncrementCollectionCountAsync(postId, -1);
            return true;
        }

        public async Task<IEnumerable<PostListResponse>> GetHotPostsAsync(int limit, long? currentUserId)
        {
            var posts = await _postRepository.GetHotPostsAsync(limit);
            var responses = new List<PostListResponse>();
            foreach (var post in posts)
            {
                var response = _mapper.Map<PostListResponse>(post);
                var userInfo = await GetUserInfoAsync(post.AuthorId);
                response.AuthorName = userInfo.Username;
                response.AuthorAvatar = userInfo.AvatarUrl;
                responses.Add(response);
            }
            return responses;
        }

        public async Task<IEnumerable<PostListResponse>> GetPostsByAuthorIdAsync(long authorId, int page, int pageSize, long? currentUserId)
        {
            var posts = await _postRepository.GetByAuthorIdAsync(authorId, page, pageSize, null);
            var userInfo = await GetUserInfoAsync(authorId);
            var responses = new List<PostListResponse>();
            foreach (var post in posts)
            {
                var response = _mapper.Map<PostListResponse>(post);
                response.AuthorName = userInfo.Username;
                response.AuthorAvatar = userInfo.AvatarUrl;
                responses.Add(response);
            }
            return responses;
        }

        public async Task<bool> UpdatePostStatusAsync(long id, PostStatus status, long currentUserId)
        {
            // 检查是否是管理员
            var isAdmin = await IsUserAdminAsync(currentUserId);
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to update post status");
            }

            return await _postRepository.UpdateStatusAsync(id, status);
        }

        public async Task<PostStatsResponse> GetPostStatsAsync(long id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return null;

            return new PostStatsResponse
            {
                PostId = post.Id,
                LikeCount = post.LikeCount,
                CommentCount = post.CommentCount,
                CollectionCount = post.CollectionCount,
                ViewCount = post.ViewCount,
                HotScore = post.HotScore,
                CreatedAt = post.CreatedAt,
                PublishedAt = post.PublishedAt
            };
        }

        public async Task<bool> IsLikedAsync(long postId, long userId)
        {
            return await _likeRepository.ExistsByUserAndTargetAsync(userId, LikeTargetType.Post, postId);
        }

        public async Task<bool> IsCollectedAsync(long postId, long userId)
        {
            return await _collectionRepository.ExistsByUserAndPostAsync(userId, postId);
        }

        private async Task<(string? Username, string? AvatarUrl)> GetUserInfoAsync(long userId)
        {
            var cacheKey = $"user:{userId}";
            if (_memoryCache.TryGetValue(cacheKey, out (string? Username, string? AvatarUrl) userInfo))
            {
                return userInfo;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("UserService");
                var response = await client.GetAsync($"api/v1/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var username = root.GetProperty("username").GetString();
                    var avatarUrl = root.GetProperty("avatarUrl").GetString();

                    userInfo = (username, avatarUrl);

                    // 缓存5分钟
                    _memoryCache.Set(cacheKey, userInfo, TimeSpan.FromMinutes(5));
                    return userInfo;
                }
                else
                {
                    _logger.LogWarning("Failed to get user info for {UserId}: {StatusCode}", userId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info for {UserId}", userId);
            }

            // 返回默认值
            return ($"User{userId}", null);
        }

        private async Task<bool> IsUserAdminAsync(long userId)
        {
            var cacheKey = $"user:role:{userId}";
            if (_memoryCache.TryGetValue(cacheKey, out bool isAdmin))
            {
                return isAdmin;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("UserService");
                var response = await client.GetAsync($"api/v1/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("role", out var roleElement))
                    {
                        var role = roleElement.GetString();
                        isAdmin = role == "Admin" || role == "SuperAdmin";
                        _memoryCache.Set(cacheKey, isAdmin, TimeSpan.FromMinutes(5));
                        return isAdmin;
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to get user role for {UserId}: {StatusCode}", userId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role for {UserId}", userId);
            }

            return false;
        }
    }
}