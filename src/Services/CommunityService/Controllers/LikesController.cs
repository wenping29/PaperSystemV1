using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CommunityService.DTOs;
using CommunityService.Interfaces;

namespace CommunityService.Controllers
{
    [ApiController]
    [Route("api/v1/likes")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly ILogger<LikesController> _logger;
        private readonly ILikeService _likeService;
        private readonly IDistributedCache _cache;

        public LikesController(
            ILogger<LikesController> logger,
            ILikeService likeService,
            IDistributedCache cache)
        {
            _logger = logger;
            _likeService = likeService;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetLikes([FromQuery] LikeQueryParams queryParams)
        {
            try
            {
                _logger.LogInformation("Getting likes with params: {@Params}", queryParams);

                // 如果没有提供userId，默认使用当前用户
                if (!queryParams.UserId.HasValue)
                {
                    queryParams.UserId = GetCurrentUserId();
                }

                var cacheKey = $"likes:page:{queryParams.Page}:size:{queryParams.PageSize}:user:{queryParams.UserId}:target:{queryParams.TargetType}:id:{queryParams.TargetId}";
                var cachedLikes = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedLikes))
                {
                    _logger.LogDebug("Cache hit for likes");
                    return Ok(JsonSerializer.Deserialize<object>(cachedLikes));
                }

                var likes = await _likeService.GetLikesAsync(queryParams);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(likes), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                return Ok(likes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes");
                return StatusCode(500, new { error = "An error occurred while getting likes" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLikeById(long id)
        {
            try
            {
                _logger.LogInformation("Getting like with ID: {LikeId}", id);

                var like = await _likeService.GetLikeByIdAsync(id);
                if (like == null)
                {
                    return NotFound(new { error = $"Like with ID {id} not found" });
                }

                // 检查权限：只有创建者可以查看自己的点赞
                var currentUserId = GetCurrentUserId();
                if (like.UserId != currentUserId)
                {
                    return Forbid();
                }

                return Ok(like);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like with ID: {LikeId}", id);
                return StatusCode(500, new { error = "An error occurred while getting like" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLike([FromBody] CreateLikeRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new like for target {TargetType}:{TargetId}", request.TargetType, request.TargetId);

                var userId = GetCurrentUserId();
                var like = await _likeService.CreateLikeAsync(request, userId);

                // 清除相关缓存
                await ClearLikeCache(userId, request.TargetType, request.TargetId);

                return CreatedAtAction(nameof(GetLikeById), new { id = like.Id }, like);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating like");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating like");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating like");
                return StatusCode(500, new { error = "An error occurred while creating like" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLike(long id)
        {
            try
            {
                _logger.LogInformation("Deleting like with ID: {LikeId}", id);

                var userId = GetCurrentUserId();
                var result = await _likeService.DeleteLikeAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { error = $"Like with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearLikeCache(userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to delete like {LikeId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting like with ID: {LikeId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting like" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLikeByTarget([FromBody] DeleteLikeRequest request)
        {
            try
            {
                _logger.LogInformation("Deleting like for target {TargetType}:{TargetId}", request.TargetType, request.TargetId);

                var userId = GetCurrentUserId();
                var result = await _likeService.DeleteLikeByTargetAsync(request, userId);
                if (!result)
                {
                    return NotFound(new { error = "Like not found" });
                }

                // 清除相关缓存
                await ClearLikeCache(userId, request.TargetType, request.TargetId);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when deleting like");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting like by target");
                return StatusCode(500, new { error = "An error occurred while deleting like" });
            }
        }

        [HttpGet("has-liked")]
        public async Task<IActionResult> HasLiked([FromQuery] string targetType, [FromQuery] long targetId)
        {
            try
            {
                _logger.LogInformation("Checking if user liked target {TargetType}:{TargetId}", targetType, targetId);

                if (!System.Enum.TryParse<CommunityService.Entities.LikeTargetType>(targetType, true, out var targetTypeEnum))
                {
                    return BadRequest(new { error = "Invalid target type" });
                }

                var userId = GetCurrentUserId();
                var hasLiked = await _likeService.HasLikedAsync(userId, targetTypeEnum, targetId);

                return Ok(new { hasLiked });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking like status");
                return StatusCode(500, new { error = "An error occurred while checking like status" });
            }
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLikeCount([FromQuery] string targetType, [FromQuery] long targetId)
        {
            try
            {
                _logger.LogInformation("Getting like count for target {TargetType}:{TargetId}", targetType, targetId);

                if (!System.Enum.TryParse<CommunityService.Entities.LikeTargetType>(targetType, true, out var targetTypeEnum))
                {
                    return BadRequest(new { error = "Invalid target type" });
                }

                var cacheKey = $"likes:count:{targetType}:{targetId}";
                var cachedCount = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCount) && int.TryParse(cachedCount, out var count))
                {
                    _logger.LogDebug("Cache hit for like count");
                    return Ok(new { count });
                }

                count = await _likeService.GetLikeCountAsync(targetTypeEnum, targetId);

                await _cache.SetStringAsync(cacheKey, count.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like count");
                return StatusCode(500, new { error = "An error occurred while getting like count" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetLikeStats()
        {
            try
            {
                _logger.LogInformation("Getting like stats");

                var userId = GetCurrentUserId();
                var stats = await _likeService.GetLikeStatsAsync(userId);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like stats");
                return StatusCode(500, new { error = "An error occurred while getting like stats" });
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

        private async Task ClearLikeCache(long userId, string? targetType = null, long? targetId = null)
        {
            // 清除用户相关的点赞缓存
            await _cache.RemoveAsync($"likes:user:{userId}");

            if (!string.IsNullOrEmpty(targetType) && targetId.HasValue)
            {
                await _cache.RemoveAsync($"likes:count:{targetType}:{targetId}");
            }
        }
    }
}