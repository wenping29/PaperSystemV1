using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PaperSystemApi.UserServices.DTOs;
using PaperSystemApi.UserServices.Interfaces;

namespace PaperSystemApi.UserServices.Controllers
{
    [ApiController]
    [Route("api/v1/activity-logs")]
    [Authorize]
    public class UserActivityLogsController : ControllerBase
    {
        private readonly ILogger<UserActivityLogsController> _logger;
        private readonly IUserActivityLogService _activityLogService;
        private readonly IDistributedCache _cache;

        public UserActivityLogsController(
            ILogger<UserActivityLogsController> logger,
            IUserActivityLogService activityLogService,
            IDistributedCache cache)
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _cache = cache;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateActivityLog([FromBody] CreateActivityLogRequest request)
        {
            try
            {
                _logger.LogInformation("Creating activity log for user {UserId}: {ActivityType}",
                    request.UserId, request.ActivityType);

                var activityLog = await _activityLogService.CreateActivityLogAsync(request);
                return CreatedAtAction(nameof(GetActivityLogById), new { id = activityLog.Id }, activityLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity log");
                return StatusCode(500, new { error = "An error occurred while creating activity log" });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetActivityLogById(long id)
        {
            try
            {
                _logger.LogDebug("Getting activity log by ID: {LogId}", id);

                var activityLog = await _activityLogService.GetActivityLogByIdAsync(id);
                if (activityLog == null)
                {
                    return NotFound(new { error = $"Activity log with ID {id} not found" });
                }

                return Ok(activityLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity log by ID: {LogId}", id);
                return StatusCode(500, new { error = "An error occurred while getting activity log" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> SearchActivityLogs([FromQuery] ActivityLogSearchRequest request)
        {
            try
            {
                _logger.LogDebug("Searching activity logs with filters: UserId={UserId}, ActivityType={ActivityType}, SearchTerm={SearchTerm}",
                    request.UserId, request.ActivityType, request.SearchTerm);

                var response = await _activityLogService.SearchActivityLogsAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching activity logs");
                return StatusCode(500, new { error = "An error occurred while searching activity logs" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserActivityLogs(long userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // 检查权限：用户只能查看自己的日志，管理员可以查看所有
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !long.TryParse(currentUserIdClaim.Value, out var currentUserId))
                {
                    return Unauthorized();
                }

                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
                if (!isAdmin && currentUserId != userId)
                {
                    return Forbid();
                }

                _logger.LogDebug("Getting activity logs for user {UserId}, Page: {Page}, PageSize: {PageSize}",
                    userId, page, pageSize);

                var logs = await _activityLogService.GetUserActivityLogsAsync(userId, page, pageSize);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity logs for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while getting user activity logs" });
            }
        }

        [HttpGet("recent")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetRecentActivityLogs([FromQuery] int limit = 50)
        {
            try
            {
                _logger.LogDebug("Getting recent activity logs, Limit: {Limit}", limit);

                var logs = await _activityLogService.GetRecentActivityLogsAsync(limit);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity logs");
                return StatusCode(500, new { error = "An error occurred while getting recent activity logs" });
            }
        }

        [HttpDelete("cleanup")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CleanupOldLogs([FromQuery] DateTime cutoffDate)
        {
            try
            {
                _logger.LogInformation("Cleaning up activity logs older than {CutoffDate}", cutoffDate);

                var result = await _activityLogService.CleanupOldLogsAsync(cutoffDate);
                if (result)
                {
                    return Ok(new { message = $"Successfully cleaned up activity logs older than {cutoffDate:yyyy-MM-dd}" });
                }
                else
                {
                    return Ok(new { message = "No activity logs to clean up" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old activity logs");
                return StatusCode(500, new { error = "An error occurred while cleaning up old activity logs" });
            }
        }
    }
}