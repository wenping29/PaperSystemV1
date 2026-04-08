using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using PaperSystemApi.FriendshipServices.DTOs;
using PaperSystemApi.FriendshipServices.Interfaces;

namespace PaperSystemApi.Controllers
{
    [ApiController]
    [Route("api/v1/friend-requests")]
    [Authorize]
    public class FriendRequestsController : ControllerBase
    {
        private readonly ILogger<FriendRequestsController> _logger;
        private readonly IFriendshipService _friendshipService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public FriendRequestsController(
            ILogger<FriendRequestsController> logger,
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
        public async Task<IActionResult> GetFriendRequests([FromQuery] FriendRequestQueryParams queryParams)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                queryParams.UserId = userId.Value;

                _logger.LogInformation("Getting friend requests for user {UserId}, Type: {Type}, Status: {Status}",
                    userId, queryParams.Type, queryParams.Status);

                var requests = await _friendshipService.GetFriendRequestsAsync(queryParams);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friend requests");
                return StatusCode(500, new { error = "An error occurred while getting friend requests" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendFriendRequest([FromBody] CreateFriendRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} sending friend request to {ReceiverId}", userId, request.ReceiverId);

                var friendship = await _friendshipService.SendFriendRequestAsync(request, userId.Value);

                // 清除相关缓存
                await ClearFriendshipCache(userId.Value);
                await ClearFriendshipCache(request.ReceiverId);

                return CreatedAtAction(nameof(GetFriendRequest), new { requestId = friendship.Id }, friendship);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending friend request");
                return StatusCode(500, new { error = "An error occurred while sending friend request" });
            }
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> GetFriendRequest(long requestId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting friend request {RequestId} for user {UserId}", requestId, userId);

                // 使用服务方法获取单个朋友请求
                var request = await _friendshipService.GetFriendRequestByIdAsync(requestId);
                if (request == null)
                {
                    return NotFound(new { error = "Friend request not found" });
                }

                // 验证用户是否有权限查看此请求
                if (request.RequesterId != userId.Value && request.ReceiverId != userId.Value)
                {
                    return Forbid();
                }

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friend request");
                return StatusCode(500, new { error = "An error occurred while getting friend request" });
            }
        }

        [HttpPost("{requestId}/respond")]
        public async Task<IActionResult> RespondToFriendRequest(long requestId, [FromBody] RespondToFriendRequest response)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} responding to friend request {RequestId}, Accept: {Accept}",
                    userId, requestId, response.Accept);

                var request = await _friendshipService.RespondToFriendRequestAsync(requestId, response, userId.Value);
                if (request == null)
                {
                    return NotFound(new { error = "Friend request not found or you are not the receiver" });
                }

                // 清除相关缓存
                await ClearFriendshipCache(userId.Value);
                if (request.RequesterId != userId.Value)
                {
                    await ClearFriendshipCache(request.RequesterId);
                }

                return Ok(request);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to friend request");
                return StatusCode(500, new { error = "An error occurred while responding to friend request" });
            }
        }

        [HttpDelete("{requestId}")]
        public async Task<IActionResult> CancelFriendRequest(long requestId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("User {UserId} cancelling friend request {RequestId}", userId, requestId);

                var success = await _friendshipService.CancelFriendRequestAsync(requestId, userId.Value);
                if (!success)
                {
                    return NotFound(new { error = "Friend request not found or you are not the requester" });
                }

                // 清除相关缓存
                await ClearFriendshipCache(userId.Value);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling friend request");
                return StatusCode(500, new { error = "An error occurred while cancelling friend request" });
            }
        }

        [HttpGet("pending/count")]
        public async Task<IActionResult> GetPendingFriendRequestCount()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting pending friend request count for user {UserId}", userId);

                var queryParams = new FriendRequestQueryParams
                {
                    UserId = userId.Value,
                    Type = "received",
                    Status = "pending"
                };

                var count = await _friendshipService.GetFriendRequestsCountAsync(queryParams);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending friend request count");
                return StatusCode(500, new { error = "An error occurred while getting pending friend request count" });
            }
        }

        [HttpGet("sent")]
        public async Task<IActionResult> GetSentFriendRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting sent friend requests for user {UserId}, Page: {Page}, PageSize: {PageSize}",
                    userId, page, pageSize);

                var queryParams = new FriendRequestQueryParams
                {
                    UserId = userId.Value,
                    Type = "sent",
                    Page = page,
                    PageSize = pageSize
                };

                var requests = await _friendshipService.GetFriendRequestsAsync(queryParams);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sent friend requests");
                return StatusCode(500, new { error = "An error occurred while getting sent friend requests" });
            }
        }

        [HttpGet("received")]
        public async Task<IActionResult> GetReceivedFriendRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                _logger.LogInformation("Getting received friend requests for user {UserId}, Page: {Page}, PageSize: {PageSize}",
                    userId, page, pageSize);

                var queryParams = new FriendRequestQueryParams
                {
                    UserId = userId.Value,
                    Type = "received",
                    Page = page,
                    PageSize = pageSize
                };

                var requests = await _friendshipService.GetFriendRequestsAsync(queryParams);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting received friend requests");
                return StatusCode(500, new { error = "An error occurred while getting received friend requests" });
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
