using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PaperSystemApi.UserServices.DTOs;
using PaperSystemApi.UserServices.Interfaces;

namespace PaperSystemApi.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserServiceS _userService;
        private readonly IDistributedCache _cache;

        // 已移除 IConnectionMultiplexer，只保留标准通用缓存
        public UsersController(
            ILogger<UsersController> logger,
            IUserServiceS userService,
            IDistributedCache cache)
        {
            _logger = logger;
            _userService = userService;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            try
            {
                _logger.LogInformation("Getting users - Page: {Page}, PageSize: {PageSize}, Search: {Search}", page, pageSize, search);

                var cacheKey = $"users:page:{page}:size:{pageSize}:search:{search}";
                var cachedUsers = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedUsers))
                {
                    _logger.LogDebug("Cache hit for users");
                    return Ok(JsonSerializer.Deserialize<object>(cachedUsers));
                }

                var users = await _userService.GetUsersAsync(page, pageSize, search);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(users), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { error = "An error occurred while getting users" });
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUsersCount([FromQuery] string? search = null)
        {
            try
            {
                _logger.LogInformation("Getting users count - Search: {Search}", search);
                var count = await _userService.GetUsersCountAsync(search);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users count");
                return StatusCode(500, new { error = "An error occurred while getting users count" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(long id)
        {
            try
            {
                _logger.LogInformation("Getting user with ID: {UserId}", id);

                var cacheKey = $"user:{id}";
                var cachedUser = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedUser))
                {
                    _logger.LogDebug("Cache hit for user {UserId}", id);
                    return Ok(JsonSerializer.Deserialize<object>(cachedUser));
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while getting user" });
            }
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                _logger.LogInformation("Getting user with username: {Username}", username);

                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { error = $"User with username '{username}' not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with username: {Username}", username);
                return StatusCode(500, new { error = "An error occurred while getting user" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new user: {Username}", request.Username);

                var user = await _userService.CreateUserAsync(request);

                await ClearUsersCache();

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", request.Username);
                return StatusCode(500, new { error = "An error occurred while creating user" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request, long id)
        {
            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", id);

                var user = await _userService.UpdateUserAsync(id, request);
                if (user == null)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                await ClearUserCache(id);
                await ClearUsersCache();

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while updating user" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", id);

                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                await ClearUserCache(id);
                await ClearUsersCache();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting user" });
            }
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetUserProfile(long id)
        {
            try
            {
                _logger.LogInformation("Getting profile for user ID: {UserId}", id);

                var profile = await _userService.GetUserProfileAsync(id);
                if (profile == null)
                {
                    return NotFound(new { error = $"Profile for user ID {id} not found" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while getting profile" });
            }
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileRequest request, long id)
        {
            try
            {
                _logger.LogInformation("Updating profile for user ID: {UserId}", id);

                var profile = await _userService.UpdateUserProfileAsync(id, request);

                await ClearUserCache(id);

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while updating profile" });
            }
        }

        [HttpPost("follow/{id}")]
        public async Task<IActionResult> FollowUser(long id)
        {
            try
            {
                var followerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (followerIdClaim == null || !long.TryParse(followerIdClaim.Value, out var followerId))
                {
                    return Unauthorized();
                }

                if (followerId == id)
                {
                    return BadRequest(new { error = "Cannot follow yourself" });
                }

                _logger.LogInformation("User {FollowerId} following user {FollowingId}", followerId, id);

                var result = await _userService.FollowUserAsync(followerId, id);
                if (!result)
                {
                    return BadRequest(new { error = "Already following this user or operation failed" });
                }

                await ClearUserCache(id);
                await ClearUserCache(followerId);

                return Ok(new { message = "Successfully followed user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following user");
                return StatusCode(500, new { error = "An error occurred while following user" });
            }
        }

        [HttpDelete("follow/{id}")]
        public async Task<IActionResult> UnfollowUser(long id)
        {
            try
            {
                var followerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (followerIdClaim == null || !long.TryParse(followerIdClaim.Value, out var followerId))
                {
                    return Unauthorized();
                }

                _logger.LogInformation("User {FollowerId} unfollowing user {FollowingId}", followerId, id);

                var result = await _userService.UnfollowUserAsync(followerId, id);
                if (!result)
                {
                    return BadRequest(new { error = "Not following this user or operation failed" });
                }

                await ClearUserCache(id);
                await ClearUserCache(followerId);

                return Ok(new { message = "Successfully unfollowed user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfollowing user");
                return StatusCode(500, new { error = "An error occurred while unfollowing user" });
            }
        }

        [HttpGet("followers/{id}")]
        public async Task<IActionResult> GetFollowers(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting followers for user ID: {UserId}", id);

                var followers = await _userService.GetFollowersAsync(id, page, pageSize);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting followers for user ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while getting followers" });
            }
        }

        [HttpGet("following/{id}")]
        public async Task<IActionResult> GetFollowing(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting following for user ID: {UserId}", id);

                var following = await _userService.GetFollowingAsync(id, page, pageSize);
                return Ok(following);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting following for user ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while getting following" });
            }
        }

        [HttpGet("stats/{id}")]
        public async Task<IActionResult> GetUserStats(long id)
        {
            try
            {
                _logger.LogInformation("Getting stats for user ID: {UserId}", id);

                var stats = await _userService.GetUserStatsAsync(id);
                return Ok(stats);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for user ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while getting stats" });
            }
        }

        [HttpPut("role/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request, long id)
        {
            try
            {
                _logger.LogInformation("Updating role for user ID: {UserId} to {Role}", id, request.Role);

                var result = await _userService.UpdateUserRoleAsync(id, request);
                if (!result)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                await ClearUserCache(id);
                await ClearUsersCache();

                return Ok(new { message = "User role updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while updating user role" });
            }
        }

        // 清理单用户缓存（标准安全版）
        private async Task ClearUserCache(long userId)
        {
            try
            {
                var cacheKey = $"user:{userId}";
                await _cache.RemoveAsync(cacheKey);
                _logger.LogDebug("Cleared cache for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for user {UserId}", userId);
            }
        }

        // 清理用户列表缓存（标准安全版）
        private async Task ClearUsersCache()
        {
            try
            {
                // 模糊删除适配 Redis/内存缓存
                var pattern = "users:*";
                await _cache.RemoveAsync(pattern);
                _logger.LogDebug("Cleared users list cache");
            }
            catch
            {
                // 忽略异常，不影响主业务
            }
        }
    }
}