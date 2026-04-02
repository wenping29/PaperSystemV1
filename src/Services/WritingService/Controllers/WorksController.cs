using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WritingService.Controllers
{
    [ApiController]
    [Route("api/v1/works")]
    public class WorksController : ControllerBase
    {
        private readonly ILogger<WorksController> _logger;
        private readonly IDistributedCache _cache;

        public WorksController(ILogger<WorksController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetWorks([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Getting works - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            // 模拟分页数据
            var works = Enumerable.Range(1, 5).Select(i => new
            {
                Id = i,
                Title = $"Sample Work {i}",
                Author = $"Author {i}",
                Excerpt = $"This is a sample excerpt for work {i}...",
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                WordCount = 1000 + i * 100,
                Likes = 10 + i
            }).ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalCount = 100,
                totalPages = 5,
                data = works
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkById(long id)
        {
            _logger.LogInformation("Getting work with ID: {WorkId}", id);

            // 缓存示例
            var cacheKey = $"work:{id}";
            var cachedWork = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedWork))
            {
                _logger.LogDebug("Cache hit for work {WorkId}", id);
                return Ok(JsonSerializer.Deserialize<object>(cachedWork));
            }

            _logger.LogDebug("Cache miss for work {WorkId}", id);

            // 模拟作品数据
            var work = new
            {
                Id = id,
                Title = $"Work Title {id}",
                AuthorId = 1,
                AuthorName = $"Author {id}",
                Content = $"This is the full content of work {id}. It contains detailed information...",
                Excerpt = $"This is an excerpt for work {id}...",
                WordCount = 1500,
                Category = "Fiction",
                Tags = new[] { "sample", "fiction", "writing" },
                CreatedAt = DateTime.UtcNow.AddDays(-id),
                UpdatedAt = DateTime.UtcNow,
                Likes = 25,
                Views = 100,
                IsPublished = true
            };

            // 写入缓存
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(work), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return Ok(work);
        }

        [HttpPost]
        public IActionResult CreateWork([FromBody] CreateWorkRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Request body cannot be empty" });
            }

            _logger.LogInformation("Creating new work: {Title}", request.Title);

            // 模拟创建作品
            var newWork = new
            {
                Id = new Random().Next(1000, 9999),
                Title = request.Title,
                AuthorId = request.AuthorId,
                Content = request.Content,
                Category = request.Category,
                Tags = request.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                WordCount = request.Content?.Length / 5 ?? 0,
                IsPublished = false
            };

            return CreatedAtAction(nameof(GetWorkById), new { id = newWork.Id }, newWork);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWork(long id, [FromBody] UpdateWorkRequest request)
        {
            _logger.LogInformation("Updating work with ID: {WorkId}", id);

            // 清除缓存
            var cacheKey = $"work:{id}";
            await _cache.RemoveAsync(cacheKey);

            return Ok(new { message = $"Work {id} updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWork(long id)
        {
            _logger.LogInformation("Deleting work with ID: {WorkId}", id);

            // 清除缓存
            var cacheKey = $"work:{id}";
            await _cache.RemoveAsync(cacheKey);

            return NoContent();
        }

        [HttpGet("{id}/content")]
        public IActionResult GetWorkContent(long id)
        {
            return Ok(new
            {
                workId = id,
                content = $"This is the full content of work {id}. It contains detailed information...",
                format = "markdown",
                wordCount = 1500,
                readingTimeMinutes = 5
            });
        }

        [HttpPost("{id}/publish")]
        public IActionResult PublishWork(long id)
        {
            _logger.LogInformation("Publishing work with ID: {WorkId}", id);
            return Ok(new { message = $"Work {id} published successfully", publishedAt = DateTime.UtcNow });
        }

        [HttpPost("{id}/like")]
        public IActionResult LikeWork(long id)
        {
            _logger.LogInformation("Liking work with ID: {WorkId}", id);
            return Ok(new { message = $"Work {id} liked successfully", likes = new Random().Next(50, 100) });
        }
    }

    public class CreateWorkRequest
    {
        public string Title { get; set; } = string.Empty;
        public long AuthorId { get; set; }
        public string? Content { get; set; }
        public string? Category { get; set; }
        public string[]? Tags { get; set; }
    }

    public class UpdateWorkRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Category { get; set; }
        public string[]? Tags { get; set; }
        public bool? IsPublished { get; set; }
    }
}