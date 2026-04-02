using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CommunityService.DTOs;
using CommunityService.Interfaces;

namespace CommunityService.Controllers
{
    [ApiController]
    [Route("api/v1/collections")]
    [Authorize]
    public class CollectionsController : ControllerBase
    {
        private readonly ILogger<CollectionsController> _logger;
        private readonly ICollectionService _collectionService;
        private readonly IDistributedCache _cache;

        public CollectionsController(
            ILogger<CollectionsController> logger,
            ICollectionService collectionService,
            IDistributedCache cache)
        {
            _logger = logger;
            _collectionService = collectionService;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetCollections([FromQuery] CollectionQueryParams queryParams)
        {
            try
            {
                _logger.LogInformation("Getting collections with params: {@Params}", queryParams);

                // 如果没有提供userId，默认使用当前用户
                if (!queryParams.UserId.HasValue)
                {
                    queryParams.UserId = GetCurrentUserId();
                }

                var cacheKey = $"collections:page:{queryParams.Page}:size:{queryParams.PageSize}:user:{queryParams.UserId}:post:{queryParams.PostId}";
                var cachedCollections = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCollections))
                {
                    _logger.LogDebug("Cache hit for collections");
                    return Ok(JsonSerializer.Deserialize<object>(cachedCollections));
                }

                var collections = await _collectionService.GetCollectionsAsync(queryParams);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(collections), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

                return Ok(collections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collections");
                return StatusCode(500, new { error = "An error occurred while getting collections" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollectionById(long id)
        {
            try
            {
                _logger.LogInformation("Getting collection with ID: {CollectionId}", id);

                var collection = await _collectionService.GetCollectionByIdAsync(id);
                if (collection == null)
                {
                    return NotFound(new { error = $"Collection with ID {id} not found" });
                }

                // 检查权限：只有创建者可以查看自己的收藏
                var currentUserId = GetCurrentUserId();
                if (collection.UserId != currentUserId)
                {
                    return Forbid();
                }

                return Ok(collection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection with ID: {CollectionId}", id);
                return StatusCode(500, new { error = "An error occurred while getting collection" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new collection for post {PostId}", request.PostId);

                var userId = GetCurrentUserId();
                var collection = await _collectionService.CreateCollectionAsync(request, userId);

                // 清除相关缓存
                await ClearCollectionCache(userId, request.PostId);

                return CreatedAtAction(nameof(GetCollectionById), new { id = collection.Id }, collection);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating collection");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating collection");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating collection");
                return StatusCode(500, new { error = "An error occurred while creating collection" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(long id, [FromBody] UpdateCollectionRequest request)
        {
            try
            {
                _logger.LogInformation("Updating collection with ID: {CollectionId}", id);

                var userId = GetCurrentUserId();
                var collection = await _collectionService.UpdateCollectionAsync(id, request, userId);
                if (collection == null)
                {
                    return NotFound(new { error = $"Collection with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearCollectionCache(userId, collection.PostId);

                return Ok(collection);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update collection {CollectionId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating collection with ID: {CollectionId}", id);
                return StatusCode(500, new { error = "An error occurred while updating collection" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollection(long id)
        {
            try
            {
                _logger.LogInformation("Deleting collection with ID: {CollectionId}", id);

                var userId = GetCurrentUserId();
                var result = await _collectionService.DeleteCollectionAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { error = $"Collection with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearCollectionCache(userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to delete collection {CollectionId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting collection with ID: {CollectionId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting collection" });
            }
        }

        [HttpDelete("post/{postId}")]
        public async Task<IActionResult> DeleteCollectionByPost(long postId)
        {
            try
            {
                _logger.LogInformation("Deleting collection for post {PostId}", postId);

                var userId = GetCurrentUserId();
                var result = await _collectionService.DeleteCollectionByPostAsync(postId, userId);
                if (!result)
                {
                    return NotFound(new { error = "Collection not found" });
                }

                // 清除相关缓存
                await ClearCollectionCache(userId, postId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting collection by post");
                return StatusCode(500, new { error = "An error occurred while deleting collection" });
            }
        }

        [HttpGet("has-collected")]
        public async Task<IActionResult> HasCollected([FromQuery] long postId)
        {
            try
            {
                _logger.LogInformation("Checking if user collected post {PostId}", postId);

                var userId = GetCurrentUserId();
                var hasCollected = await _collectionService.HasCollectedAsync(userId, postId);

                return Ok(new { hasCollected });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking collection status");
                return StatusCode(500, new { error = "An error occurred while checking collection status" });
            }
        }

        [HttpGet("count/user")]
        public async Task<IActionResult> GetCollectionCountByUser([FromQuery] long? userId = null)
        {
            try
            {
                var targetUserId = userId ?? GetCurrentUserId();
                _logger.LogInformation("Getting collection count for user {UserId}", targetUserId);

                var cacheKey = $"collections:count:user:{targetUserId}";
                var cachedCount = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCount) && int.TryParse(cachedCount, out var count))
                {
                    _logger.LogDebug("Cache hit for collection count by user");
                    return Ok(new { userId = targetUserId, count });
                }

                count = await _collectionService.GetCollectionCountByUserAsync(targetUserId);

                await _cache.SetStringAsync(cacheKey, count.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(new { userId = targetUserId, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection count by user");
                return StatusCode(500, new { error = "An error occurred while getting collection count" });
            }
        }

        [HttpGet("count/post")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCollectionCountByPost([FromQuery] long postId)
        {
            try
            {
                _logger.LogInformation("Getting collection count for post {PostId}", postId);

                var cacheKey = $"collections:count:post:{postId}";
                var cachedCount = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCount) && int.TryParse(cachedCount, out var count))
                {
                    _logger.LogDebug("Cache hit for collection count by post");
                    return Ok(new { postId, count });
                }

                count = await _collectionService.GetCollectionCountByPostAsync(postId);

                await _cache.SetStringAsync(cacheKey, count.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(new { postId, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection count by post");
                return StatusCode(500, new { error = "An error occurred while getting collection count" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCollectionsByUser(long userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting collections for user {UserId}", userId);

                // 检查权限：只能查看自己的收藏或其他用户的公开收藏
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId)
                {
                    // TODO: 检查是否可以查看其他用户的收藏（例如公开收藏）
                    return Forbid();
                }

                var cacheKey = $"collections:user:{userId}:page:{page}:size:{pageSize}";
                var cachedCollections = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCollections))
                {
                    _logger.LogDebug("Cache hit for user collections");
                    return Ok(JsonSerializer.Deserialize<object>(cachedCollections));
                }

                var collections = await _collectionService.GetCollectionsByUserIdAsync(userId, page, pageSize);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(collections), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(collections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collections by user");
                return StatusCode(500, new { error = "An error occurred while getting collections by user" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetCollectionStats()
        {
            try
            {
                _logger.LogInformation("Getting collection stats");

                var userId = GetCurrentUserId();
                var stats = await _collectionService.GetCollectionStatsAsync(userId);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection stats");
                return StatusCode(500, new { error = "An error occurred while getting collection stats" });
            }
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return 0;
        }

        private async Task ClearCollectionCache(long userId, long? postId = null)
        {
            // 清除用户相关的收藏缓存
            await _cache.RemoveAsync($"collections:user:{userId}");
            await _cache.RemoveAsync($"collections:count:user:{userId}");

            if (postId.HasValue)
            {
                await _cache.RemoveAsync($"collections:count:post:{postId}");
            }
        }
    }
}