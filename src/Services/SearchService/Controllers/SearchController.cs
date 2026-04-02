using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using SearchService.DTOs;
using SearchService.Interfaces;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/v1/search")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;
        private readonly IISearchService _searchService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public SearchController(
            ILogger<SearchController> logger,
            IISearchService searchService,
            IDistributedCache cache,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _searchService = searchService;
            _cache = cache;
            _redis = redis;
        }

        [HttpGet("writings")]
        public async Task<IActionResult> SearchWritings([FromQuery] SearchRequest request)
        {
            try
            {
                _logger.LogInformation("Search writings: {Query}, Page: {Page}, PageSize: {PageSize}", request.Query, request.Page, request.PageSize);

                // 从JWT令牌获取用户ID
                var userId = GetUserIdFromToken();

                // 生成缓存键
                var cacheKey = $"search:writings:{request.Query}:{request.SearchType}:{request.Page}:{request.PageSize}:{request.SortBy}:{request.SortDescending}:{userId}";
                var cachedResult = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger.LogDebug("Cache hit for writings search: {Query}", request.Query);
                    return Ok(JsonSerializer.Deserialize<object>(cachedResult));
                }

                var result = await _searchService.SearchWritingsAsync(request, userId);

                // 缓存结果（仅当成功且不是太动态）
                if (result.Success && result.TotalResults > 0)
                {
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching writings: {Query}", request.Query);
                return StatusCode(500, new { error = "An error occurred while searching writings" });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers([FromQuery] SearchRequest request)
        {
            try
            {
                _logger.LogInformation("Search users: {Query}, Page: {Page}, PageSize: {PageSize}", request.Query, request.Page, request.PageSize);

                var userId = GetUserIdFromToken();
                var cacheKey = $"search:users:{request.Query}:{request.SearchType}:{request.Page}:{request.PageSize}:{request.SortBy}:{request.SortDescending}:{userId}";
                var cachedResult = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger.LogDebug("Cache hit for users search: {Query}", request.Query);
                    return Ok(JsonSerializer.Deserialize<object>(cachedResult));
                }

                var result = await _searchService.SearchUsersAsync(request, userId);

                if (result.Success && result.TotalResults > 0)
                {
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users: {Query}", request.Query);
                return StatusCode(500, new { error = "An error occurred while searching users" });
            }
        }

        [HttpGet("tags")]
        public async Task<IActionResult> SearchTags([FromQuery] SearchRequest request)
        {
            try
            {
                _logger.LogInformation("Search tags: {Query}, Page: {Page}, PageSize: {PageSize}", request.Query, request.Page, request.PageSize);

                var userId = GetUserIdFromToken();
                var result = await _searchService.SearchTagsAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tags: {Query}", request.Query);
                return StatusCode(500, new { error = "An error occurred while searching tags" });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> SearchAll([FromQuery] SearchRequest request)
        {
            try
            {
                _logger.LogInformation("Search all: {Query}, Page: {Page}, PageSize: {PageSize}", request.Query, request.Page, request.PageSize);

                var userId = GetUserIdFromToken();
                var cacheKey = $"search:all:{request.Query}:{request.SearchType}:{request.Page}:{request.PageSize}:{request.SortBy}:{request.SortDescending}:{userId}";
                var cachedResult = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger.LogDebug("Cache hit for all search: {Query}", request.Query);
                    return Ok(JsonSerializer.Deserialize<object>(cachedResult));
                }

                var result = await _searchService.SearchAllAsync(request, userId);

                if (result.Success && result.TotalResults > 0)
                {
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching all: {Query}", request.Query);
                return StatusCode(500, new { error = "An error occurred while searching" });
            }
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return Ok(new List<SearchSuggestion>());
                }

                _logger.LogInformation("Get search suggestions: {Query}, Limit: {Limit}", query, limit);

                var cacheKey = $"search:suggestions:{query}:{limit}";
                var cachedSuggestions = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedSuggestions))
                {
                    _logger.LogDebug("Cache hit for suggestions: {Query}", query);
                    return Ok(JsonSerializer.Deserialize<object>(cachedSuggestions));
                }

                var suggestions = await _searchService.GetSearchSuggestionsAsync(query, limit);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(suggestions), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions: {Query}", query);
                return StatusCode(500, new { error = "An error occurred while getting search suggestions" });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetUserSearchHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized();
                }

                _logger.LogInformation("Get search history for user: {UserId}, Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);

                var history = await _searchService.GetUserSearchHistoryAsync(userId.Value, page, pageSize);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search history");
                return StatusCode(500, new { error = "An error occurred while getting search history" });
            }
        }

        [HttpDelete("history/{id}")]
        public async Task<IActionResult> DeleteSearchHistory(long id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized();
                }

                _logger.LogInformation("Delete search history: {HistoryId} for user: {UserId}", id, userId);

                // 验证权限：只能删除自己的搜索历史
                var history = await _searchService.GetSearchHistoryByIdAsync(id);
                if (history == null)
                {
                    return NotFound(new { error = $"Search history with ID {id} not found" });
                }

                if (history.UserId != userId)
                {
                    return Forbid();
                }

                var result = await _searchService.DeleteSearchHistoryAsync(id);
                if (!result)
                {
                    return NotFound(new { error = $"Search history with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearSearchCache(userId.Value);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting search history: {HistoryId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting search history" });
            }
        }

        [HttpDelete("history")]
        public async Task<IActionResult> ClearUserSearchHistory()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized();
                }

                _logger.LogInformation("Clear search history for user: {UserId}", userId);

                var result = await _searchService.ClearUserSearchHistoryAsync(userId.Value);
                if (!result)
                {
                    return BadRequest(new { error = "Failed to clear search history" });
                }

                // 清除相关缓存
                await ClearSearchCache(userId.Value);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing search history for user");
                return StatusCode(500, new { error = "An error occurred while clearing search history" });
            }
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularSearchTerms([FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Get popular search terms, Limit: {Limit}", limit);

                var cacheKey = $"search:popular:{limit}";
                var cachedPopular = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedPopular))
                {
                    _logger.LogDebug("Cache hit for popular search terms");
                    return Ok(JsonSerializer.Deserialize<object>(cachedPopular));
                }

                var popularTerms = await _searchService.GetPopularSearchTermsAsync(limit);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(popularTerms), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                return Ok(popularTerms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular search terms");
                return StatusCode(500, new { error = "An error occurred while getting popular search terms" });
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetSearchStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                _logger.LogInformation("Get search statistics, StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);

                var statistics = await _searchService.GetSearchStatisticsAsync(startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search statistics");
                return StatusCode(500, new { error = "An error occurred while getting search statistics" });
            }
        }

        [HttpPost("index/writing/{writingId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> IndexWriting(long writingId)
        {
            try
            {
                _logger.LogInformation("Index writing: {WritingId}", writingId);

                var result = await _searchService.IndexWritingAsync(writingId);
                if (!result)
                {
                    return BadRequest(new { error = "Failed to index writing" });
                }

                return Ok(new { message = "Writing indexed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing writing: {WritingId}", writingId);
                return StatusCode(500, new { error = "An error occurred while indexing writing" });
            }
        }

        [HttpPost("index/user/{userId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> IndexUser(long userId)
        {
            try
            {
                _logger.LogInformation("Index user: {UserId}", userId);

                var result = await _searchService.IndexUserAsync(userId);
                if (!result)
                {
                    return BadRequest(new { error = "Failed to index user" });
                }

                return Ok(new { message = "User indexed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing user: {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while indexing user" });
            }
        }

        [HttpPost("rebuild-index")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> RebuildIndex([FromQuery] string indexType = "all")
        {
            try
            {
                _logger.LogInformation("Rebuild index: {IndexType}", indexType);

                var result = await _searchService.RebuildIndexAsync(indexType);
                if (!result)
                {
                    return BadRequest(new { error = "Failed to rebuild index" });
                }

                // 清除所有搜索缓存
                await ClearAllSearchCache();

                return Ok(new { message = "Index rebuild started successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding index: {IndexType}", indexType);
                return StatusCode(500, new { error = "An error occurred while rebuilding index" });
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

        private async Task ClearSearchCache(long userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = $"WritingPlatform:SearchService:search:*:{userId}";

                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cleared {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing search cache for user {UserId}", userId);
            }
        }

        private async Task ClearAllSearchCache()
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = "WritingPlatform:SearchService:search:*";

                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cleared {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all search cache");
            }
        }
    }
}