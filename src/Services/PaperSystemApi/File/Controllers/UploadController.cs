using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using PaperSystemApi.File.DTOs;
using PaperSystemApi.File.Interfaces;

namespace PaperSystemApi.File.Controllers
{
    [ApiController]
    [Route("api/v1/upload")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IFileService _fileService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public UploadController(
            ILogger<UploadController> logger,
            IFileService fileService,
            IDistributedCache cache,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _fileService = fileService;
            _cache = cache;
            _redis = redis;
        }

        [HttpPost]
        [AllowAnonymous] // 可以根据需要设置为需要认�?
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            try
            {
                _logger.LogInformation("Uploading file: {FileName}", request.File?.FileName);

                // 从JWT令牌获取用户ID（如果存在）
                long? userId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                var result = await _fileService.UploadFileAsync(request, userId);
                if (!result.Success)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }

                // 清除文件列表缓存
                await ClearFilesCache();

                return CreatedAtAction(nameof(FilesController.GetFileByFileId), "Files", new { fileId = result.File?.FileId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = $"An error occurred while uploading file: {ex.Message}" });
            }
        }

        [HttpPost("generate-url")]
        public async Task<IActionResult> GenerateUploadUrl([FromBody] GenerateUploadUrlRequest request)
        {
            try
            {
                _logger.LogInformation("Generating upload URL for file: {FileName}", request.FileName);

                // 从JWT令牌获取用户ID
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized();
                }

                var file = await _fileService.GenerateUploadUrlAsync(request.FileName, request.ContentType, userId, request.ExpiresAt);
                if (file == null)
                {
                    return BadRequest(new { error = "Failed to generate upload URL" });
                }

                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating upload URL");
                return StatusCode(500, new { error = "An error occurred while generating upload URL" });
            }
        }

        [HttpPost("chunk")]
        [AllowAnonymous]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> UploadChunk([FromForm] UploadChunkRequest request)
        {
            try
            {
                _logger.LogInformation("Uploading chunk {ChunkNumber} for file: {FileId}", request.ChunkNumber, request.FileId);

                // 简单的分片上传实现
                // 这里可以添加分片上传的逻辑，如验证、合并等
                return Ok(new { message = "Chunk uploaded successfully", chunkNumber = request.ChunkNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading chunk");
                return StatusCode(500, new { error = "An error occurred while uploading chunk" });
            }
        }

        [HttpGet("status/{fileId}")]
        public async Task<IActionResult> GetUploadStatus(string fileId)
        {
            try
            {
                _logger.LogInformation("Getting upload status for file: {FileId}", fileId);

                var file = await _fileService.GetFileByFileIdAsync(fileId);
                if (file == null)
                {
                    return NotFound(new { error = $"File with fileId '{fileId}' not found" });
                }

                return Ok(new { fileId = file.FileId, status = file.Status, uploadedAt = file.CreatedAt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upload status");
                return StatusCode(500, new { error = "An error occurred while getting upload status" });
            }
        }

        private async Task ClearFilesCache()
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = "WritingPlatform:FileService:files:*";

                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cleared {Count} cache keys with pattern: {Pattern}", keys.Length, pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing files cache");
                // 不抛出异常，避免影响主要业务逻辑
            }
        }
    }

    // 分片上传请求DTO
    public class UploadChunkRequest
    {
        public string FileId { get; set; } = string.Empty;
        public int ChunkNumber { get; set; }
        public int TotalChunks { get; set; }
        public IFormFile File { get; set; } = null!;
    }

    // 生成上传URL的请求DTO（重用FilesController中的定义�?
    // public class GenerateUploadUrlRequest
    // {
    //     public string FileName { get; set; } = string.Empty;
    //     public string ContentType { get; set; } = string.Empty;
    //     public DateTime? ExpiresAt { get; set; }
    // }
}
