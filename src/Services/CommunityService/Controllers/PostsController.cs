using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CommunityService.DTOs;
using CommunityService.Interfaces;
using StackExchange.Redis;

namespace CommunityService.Controllers
{
    [ApiController]
    [Route("api/v1/posts")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly ILogger<PostsController> _logger;
        private readonly IPostService _postService;
        private readonly IDistributedCache _cache;

        public PostsController(
            ILogger<PostsController> logger,
            IPostService postService,
           IConnectionMultiplexer redis,
            IDistributedCache cache)
        {
            _logger = logger;
            _postService = postService;
            _cache = cache;
            _redis = redis;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPosts([FromQuery] PostQueryParams queryParams)
        {
            try
            {
                _logger.LogInformation("Getting posts with params: {@Params}", queryParams);

                // 尝试从缓存获取
                var cacheKey = $"posts:page:{queryParams.Page}:size:{queryParams.PageSize}:category:{queryParams.Category}:tag:{queryParams.Tag}:keyword:{queryParams.Keyword}:sort:{queryParams.SortBy}:author:{queryParams.AuthorId}:status:{queryParams.Status}:visibility:{queryParams.Visibility}";
                var cachedPosts = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedPosts))
                {
                    _logger.LogDebug("Cache hit for posts");
                    return Ok(JsonSerializer.Deserialize<object>(cachedPosts));
                }

                var currentUserId = GetCurrentUserId();
                var posts = await _postService.GetPostsAsync(queryParams, currentUserId);

                // 缓存结果
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(posts), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                return StatusCode(500, new { error = "An error occurred while getting posts" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostById(long id)
        {
            try
            {
                _logger.LogInformation("Getting post with ID: {PostId}", id);

                var cacheKey = $"post:{id}";
                var cachedPost = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedPost))
                {
                    _logger.LogDebug("Cache hit for post {PostId}", id);
                    return Ok(JsonSerializer.Deserialize<object>(cachedPost));
                }

                var currentUserId = GetCurrentUserId();
                var post = await _postService.GetPostByIdAsync(id, currentUserId);
                if (post == null)
                {
                    return NotFound(new { error = $"Post with ID {id} not found" });
                }

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(post), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post with ID: {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while getting post" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new post");

                var authorId = GetCurrentUserId();
                var post = await _postService.CreatePostAsync(request, authorId);

                // 清除相关缓存
                await ClearPostsCache();

                return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { error = "An error occurred while creating post" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(long id, [FromBody] UpdatePostRequest request)
        {
            try
            {
                _logger.LogInformation("Updating post with ID: {PostId}", id);

                var currentUserId = GetCurrentUserId();
                var post = await _postService.UpdatePostAsync(id, request, currentUserId);
                if (post == null)
                {
                    return NotFound(new { error = $"Post with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearPostCache(id);
                await ClearPostsCache();

                return Ok(post);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update post {PostId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post with ID: {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while updating post" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(long id)
        {
            try
            {
                _logger.LogInformation("Deleting post with ID: {PostId}", id);

                var currentUserId = GetCurrentUserId();
                var result = await _postService.DeletePostAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound(new { error = $"Post with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearPostCache(id);
                await ClearPostsCache();

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to delete post {PostId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post with ID: {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting post" });
            }
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(long id)
        {
            try
            {
                _logger.LogInformation("Liking post with ID: {PostId}", id);

                var userId = GetCurrentUserId();
                var result = await _postService.LikePostAsync(id, userId);
                if (!result)
                {
                    return BadRequest(new { error = "Already liked or post not found" });
                }

                await ClearPostCache(id);
                return Ok(new { message = "Post liked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post with ID: {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while liking post" });
            }
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikePost(long id)
        {
            try
            {
                _logger.LogInformation("Unliking post with ID: {PostId}", id);

                var userId = GetCurrentUserId();
                var result = await _postService.UnlikePostAsync(id, userId);
                if (!result)
                {
                    return BadRequest(new { error = "Not liked or post not found" });
                }

                await ClearPostCache(id);
                return Ok(new { message = "Post unliked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post with ID: {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while unliking post" });
            }
        }

        [HttpPost("{id}/collect")]
        public async Task<IActionResult> CollectPost(long id, [FromBody] CollectPostRequest? request = null)
        {
            try
            {
                _logger.LogInformation("Collecting post with ID: {PostId}", id);

                var userId = GetCurrentUserId();
                var note = request?.Note;
                var result = await _postService.CollectPostAsync(id, userId, note);
                if (!result)
                {
                    return BadRequest(new { error = "Already collected or post not found" });
                }

                await ClearPostCache(id);
                return Ok(new { message = "Post collected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting post with ID: {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while collecting post" });
            }
        }

        [HttpDelete("{id}/collect")]
        public async Task<IActionResult> UncollectPost(long id)
        {
            try
            {
                _logger.LogInformation("Uncollecting post with ID: {PostId}", id);

                var userId = GetCurrentUserId();
                var result = await _postService.UncollectPostAsync(id, userId);
                if (!result)
                {
                    return BadRequest(new { error = "Not collected or post not found" });
                }

                await ClearPostCache(id);
                return Ok(new { message = "Post uncollected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uncollecting post with ID: {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while uncollecting post" });
            }
        }

        [HttpGet("hot")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotPosts([FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Getting hot posts with limit: {Limit}", limit);

                var cacheKey = $"posts:hot:limit:{limit}";
                var cachedPosts = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedPosts))
                {
                    _logger.LogDebug("Cache hit for hot posts");
                    return Ok(JsonSerializer.Deserialize<object>(cachedPosts));
                }

                var currentUserId = GetCurrentUserId();
                var posts = await _postService.GetHotPostsAsync(limit, currentUserId);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(posts), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hot posts");
                return StatusCode(500, new { error = "An error occurred while getting hot posts" });
            }
        }

        [HttpGet("author/{authorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostsByAuthor(long authorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting posts by author {AuthorId}", authorId);

                var cacheKey = $"posts:author:{authorId}:page:{page}:size:{pageSize}";
                var cachedPosts = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedPosts))
                {
                    _logger.LogDebug("Cache hit for author posts");
                    return Ok(JsonSerializer.Deserialize<object>(cachedPosts));
                }

                var currentUserId = GetCurrentUserId();
                var posts = await _postService.GetPostsByAuthorIdAsync(authorId, page, pageSize, currentUserId);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(posts), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by author");
                return StatusCode(500, new { error = "An error occurred while getting posts by author" });
            }
        }

        [HttpGet("{id}/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostStats(long id)
        {
            try
            {
                _logger.LogInformation("Getting stats for post {PostId}", id);

                var stats = await _postService.GetPostStatsAsync(id);
                if (stats == null)
                {
                    return NotFound(new { error = $"Post with ID {id} not found" });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post stats");
                return StatusCode(500, new { error = "An error occurred while getting post stats" });
            }
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            // 如果没有认证用户，返回0表示匿名
            return 0;
        }

        private async Task ClearPostCache(long postId)
        {
            await _cache.RemoveAsync($"post:{postId}");
        }
        private readonly IConnectionMultiplexer _redis;

        private async Task ClearPostsCache()
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var patterns = new[] { "posts:*", "posts:hot:*", "posts:author:*" };

                foreach (var pattern in patterns)
                {
                    var keys = server.Keys(pattern: pattern).ToArray();
                    if (keys.Any())
                    {
                        await db.KeyDeleteAsync(keys);
                        _logger.LogDebug("Cleared {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing posts cache");
                // 不抛出异常，避免影响主要业务逻辑
            }
        }

        public class CollectPostRequest
        {
            public string? Note { get; set; }
        }
    }
}