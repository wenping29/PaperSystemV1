using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CommunityService.DTOs;
using CommunityService.Interfaces;

namespace CommunityService.Controllers
{
    [ApiController]
    [Route("api/v1/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ILogger<CommentsController> _logger;
        private readonly ICommentService _commentService;
        private readonly IDistributedCache _cache;

        public CommentsController(
            ILogger<CommentsController> logger,
            ICommentService commentService,
            IDistributedCache cache)
        {
            _logger = logger;
            _commentService = commentService;
            _cache = cache;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments([FromQuery] CommentQueryParams queryParams)
        {
            try
            {
                _logger.LogInformation("Getting comments with params: {@Params}", queryParams);

                var cacheKey = $"comments:page:{queryParams.Page}:size:{queryParams.PageSize}:post:{queryParams.PostId}:author:{queryParams.AuthorId}:parent:{queryParams.ParentId}:sort:{queryParams.SortBy}";
                var cachedComments = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedComments))
                {
                    _logger.LogDebug("Cache hit for comments");
                    return Ok(JsonSerializer.Deserialize<object>(cachedComments));
                }

                var currentUserId = GetCurrentUserId();
                var comments = await _commentService.GetCommentsAsync(queryParams, currentUserId);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(comments), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments");
                return StatusCode(500, new { error = "An error occurred while getting comments" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentById(long id)
        {
            try
            {
                _logger.LogInformation("Getting comment with ID: {CommentId}", id);

                var cacheKey = $"comment:{id}";
                var cachedComment = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedComment))
                {
                    _logger.LogDebug("Cache hit for comment {CommentId}", id);
                    return Ok(JsonSerializer.Deserialize<object>(cachedComment));
                }

                var currentUserId = GetCurrentUserId();
                var comment = await _commentService.GetCommentByIdAsync(id, currentUserId);
                if (comment == null)
                {
                    return NotFound(new { error = $"Comment with ID {id} not found" });
                }

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(comment), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment with ID: {CommentId}", id);
                return StatusCode(500, new { error = "An error occurred while getting comment" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new comment");

                var authorId = GetCurrentUserId();
                var comment = await _commentService.CreateCommentAsync(request, authorId);

                // 清除相关缓存
                await ClearCommentCacheByPost(request.PostId);

                return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating comment");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, new { error = "An error occurred while creating comment" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(long id, [FromBody] UpdateCommentRequest request)
        {
            try
            {
                _logger.LogInformation("Updating comment with ID: {CommentId}", id);

                var currentUserId = GetCurrentUserId();
                var comment = await _commentService.UpdateCommentAsync(id, request, currentUserId);
                if (comment == null)
                {
                    return NotFound(new { error = $"Comment with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearCommentCache(id);
                await ClearCommentCacheByPost(comment.PostId);

                return Ok(comment);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update comment {CommentId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment with ID: {CommentId}", id);
                return StatusCode(500, new { error = "An error occurred while updating comment" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(long id)
        {
            try
            {
                _logger.LogInformation("Deleting comment with ID: {CommentId}", id);

                var currentUserId = GetCurrentUserId();
                var result = await _commentService.DeleteCommentAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound(new { error = $"Comment with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearCommentCache(id);
                // 注意：需要知道帖子ID来清除缓存，这里简化处理

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to delete comment {CommentId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment with ID: {CommentId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting comment" });
            }
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeComment(long id)
        {
            try
            {
                _logger.LogInformation("Liking comment with ID: {CommentId}", id);

                var userId = GetCurrentUserId();
                var result = await _commentService.LikeCommentAsync(id, userId);
                if (!result)
                {
                    return BadRequest(new { error = "Already liked or comment not found" });
                }

                await ClearCommentCache(id);
                return Ok(new { message = "Comment liked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking comment with ID: {CommentId}", id);
                return StatusCode(500, new { error = "An error occurred while liking comment" });
            }
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikeComment(long id)
        {
            try
            {
                _logger.LogInformation("Unliking comment with ID: {CommentId}", id);

                var userId = GetCurrentUserId();
                var result = await _commentService.UnlikeCommentAsync(id, userId);
                if (!result)
                {
                    return BadRequest(new { error = "Not liked or comment not found" });
                }

                await ClearCommentCache(id);
                return Ok(new { message = "Comment unliked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking comment with ID: {CommentId}", id);
                return StatusCode(500, new { error = "An error occurred while unliking comment" });
            }
        }

        [HttpGet("{id}/replies")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReplies(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting replies for comment {CommentId}", id);

                var cacheKey = $"comment:{id}:replies:page:{page}:size:{pageSize}";
                var cachedReplies = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedReplies))
                {
                    _logger.LogDebug("Cache hit for comment replies");
                    return Ok(JsonSerializer.Deserialize<object>(cachedReplies));
                }

                var currentUserId = GetCurrentUserId();
                var replies = await _commentService.GetRepliesAsync(id, page, pageSize, currentUserId);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(replies), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

                return Ok(replies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment replies");
                return StatusCode(500, new { error = "An error occurred while getting comment replies" });
            }
        }

        [HttpGet("{id}/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentStats(long id)
        {
            try
            {
                _logger.LogInformation("Getting stats for comment {CommentId}", id);

                var stats = await _commentService.GetCommentStatsAsync(id);
                if (stats == null)
                {
                    return NotFound(new { error = $"Comment with ID {id} not found" });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment stats");
                return StatusCode(500, new { error = "An error occurred while getting comment stats" });
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

        private async Task ClearCommentCache(long commentId)
        {
            await _cache.RemoveAsync($"comment:{commentId}");
        }

        private async Task ClearCommentCacheByPost(long postId)
        {
            // 清除该帖子的所有评论缓存
            await _cache.RemoveAsync($"comments:post:{postId}");
        }
    }
}