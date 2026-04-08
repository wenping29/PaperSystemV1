using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using PaperSystemApi.FriendshipServices.DTOs;
using PaperSystemApi.FriendshipServices.Interfaces;

namespace PaperSystemApi.FriendshipServices.Controllers
{
    [ApiController]
    [Route("api/v1/friendships")]
    [Authorize]
    public class FriendshipsController : ControllerBase
    {
        private readonly ILogger<FriendshipsController> _logger;
        private readonly IFriendshipService _friendshipService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public FriendshipsController(
            ILogger<FriendshipsController> logger,
            IFriendshipService friendshipService,
            IDistributedCache cache,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _friendshipService = friendshipService;
            _cache = cache;
            _redis = redis;
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendships([FromQuery] FriendshipQueryParams queryParams)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                // 使用传入的UserId或当前用户ID
                queryParams.UserId ??= userId.Value;

                _logger.LogInformation("Getting friendships for user {UserId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
                    queryParams.UserId, queryParams.Status, queryParams.Page, queryParams.PageSize);

                // 生成缓存�?
                var cacheKey = $"friendships:user:{queryParams.UserId}:status:{queryParams.Status}:favorite:{queryParams.IsFavorite}:page:{queryParams.Page}:size:{queryParams.PageSize}";
                var cachedFriendships = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedFriendships))
                {
                    _logger.LogDebug("Cache hit for friendships of user {UserId}", queryParams.UserId);
                    return Ok(JsonSerializer.Deserialize<object>(cachedFriendships));
                }

                var friendships = await _friendshipService.GetFriendshipsAsync(queryParams);

                // 缓存结果
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(friendships), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(friendships);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friendships");
                return StatusCode(500, new { error = "An error occurred while getting friendships" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetFriendshipStats()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting friendship stats for user {UserId}", userId);

                var cacheKey = $"friendships:stats:{userId}";
                var cachedStats = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedStats))
                {
                    _logger.LogDebug("Cache hit for friendship stats of user {UserId}", userId);
                    return Ok(JsonSerializer.Deserialize<object>(cachedStats));
                }

                var stats = await _friendshipService.GetFriendshipStatsAsync(userId.Value);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friendship stats");
                return StatusCode(500, new { error = "An error occurred while getting friendship stats" });
            }
        }

        [HttpGet("{friendId}")]
        public async Task<IActionResult> GetFriendship(long friendId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting friendship between {UserId} and {FriendId}", userId, friendId);

                var friendship = await _friendshipService.GetFriendshipAsync(userId.Value, friendId);
                if (friendship == null)
                {
                    return NotFound(new { error = "Friendship not found" });
                }

                return Ok(friendship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friendship");
                return StatusCode(500, new { error = "An error occurred while getting friendship" });
            }
        }





        [HttpPut("{friendId}")]
        public async Task<IActionResult> UpdateFriendship(long friendId, [FromBody] UpdateFriendshipRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} updating friendship with {FriendId}", userId, friendId);

                // 先获取友谊关�?
                var friendship = await _friendshipService.GetFriendshipAsync(userId.Value, friendId);
                if (friendship == null)
                {
                    return NotFound(new { error = "Friendship not found" });
                }

                var updatedFriendship = await _friendshipService.UpdateFriendshipAsync(friendship.Id, request, userId.Value);
                if (updatedFriendship == null)
                {
                    return NotFound(new { error = "Friendship not found" });
                }

                // 清除缓存
                await ClearFriendshipCache(userId.Value);

                return Ok(updatedFriendship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating friendship");
                return StatusCode(500, new { error = "An error occurred while updating friendship" });
            }
        }

        [HttpDelete("{friendId}")]
        public async Task<IActionResult> RemoveFriend(long friendId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} removing friend {FriendId}", userId, friendId);

                var success = await _friendshipService.RemoveFriendAsync(userId.Value, friendId);
                if (!success)
                {
                    return NotFound(new { error = "Friendship not found" });
                }

                // 清除缓存
                await ClearFriendshipCache(userId.Value);
                await ClearFriendshipCache(friendId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing friend");
                return StatusCode(500, new { error = "An error occurred while removing friend" });
            }
        }

        [HttpPost("{friendId}/favorite")]
        public async Task<IActionResult> FavoriteFriend(long friendId, [FromBody] bool isFavorite)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} setting friend {FriendId} favorite to {IsFavorite}", userId, friendId, isFavorite);

                var success = await _friendshipService.FavoriteFriendAsync(userId.Value, friendId, isFavorite);
                if (!success)
                {
                    return NotFound(new { error = "Friendship not found" });
                }

                await ClearFriendshipCache(userId.Value);
                return Ok(new { message = "Friend favorite status updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating friend favorite status");
                return StatusCode(500, new { error = "An error occurred while updating friend favorite status" });
            }
        }

        [HttpPost("{friendId}/block")]
        public async Task<IActionResult> BlockUser(long friendId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} blocking user {FriendId}", userId, friendId);

                var success = await _friendshipService.BlockUserAsync(userId.Value, friendId);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to block user" });
                }

                await ClearFriendshipCache(userId.Value);
                return Ok(new { message = "User blocked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking user");
                return StatusCode(500, new { error = "An error occurred while blocking user" });
            }
        }

        [HttpPost("{friendId}/unblock")]
        public async Task<IActionResult> UnblockUser(long friendId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} unblocking user {FriendId}", userId, friendId);

                var success = await _friendshipService.UnblockUserAsync(userId.Value, friendId);
                if (!success)
                {
                    return BadRequest(new { error = "User is not blocked or failed to unblock" });
                }

                await ClearFriendshipCache(userId.Value);
                return Ok(new { message = "User unblocked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking user");
                return StatusCode(500, new { error = "An error occurred while unblocking user" });
            }
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetFriendSuggestions([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting friend suggestions for user {UserId}, limit {Limit}", userId, limit);

                var suggestions = await _friendshipService.GetFriendSuggestionsAsync(userId.Value, limit);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friend suggestions");
                return StatusCode(500, new { error = "An error occurred while getting friend suggestions" });
            }
        }

        [HttpGet("mutual/{otherUserId}")]
        public async Task<IActionResult> GetMutualFriends(long otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting mutual friends between {UserId} and {OtherUserId}", userId, otherUserId);

                var mutualFriends = await _friendshipService.GetMutualFriendsAsync(userId.Value, otherUserId, page, pageSize);
                return Ok(mutualFriends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mutual friends");
                return StatusCode(500, new { error = "An error occurred while getting mutual friends" });
            }
        }

        private long? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        private async Task ClearFriendshipCache(long userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = $"WritingPlatform:FriendshipService:friendships:*:user:{userId}:*";

                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cleared {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
                }

                // 也清除统计缓�?
                var statsKey = $"WritingPlatform:FriendshipService:friendships:stats:{userId}";
                await db.KeyDeleteAsync(statsKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing friendship cache for user {UserId}", userId);
            }
        }
    }
}
