using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using PaperSystemApi.File.DTOs;
using PaperSystemApi.File.Interfaces;

namespace PaperSystemApi.Controllers
{
    [ApiController]
    [Route("api/v1/files")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly IFileService _fileService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public FilesController(
            ILogger<FilesController> logger,
            IFileService fileService,
            IDistributedCache cache,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _fileService = fileService;
            _cache = cache;
            _redis = redis;
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles([FromQuery] FileQueryParams queryParams)
        {
            try
            {
                _logger.LogInformation("Getting files - Page: {Page}, PageSize: {PageSize}, Type: {FileType}", queryParams.Page, queryParams.PageSize, queryParams.FileType);

                // 检查缓�?
                var cacheKey = $"files:page:{queryParams.Page}:size:{queryParams.PageSize}:type:{queryParams.FileType}:user:{queryParams.UploadedByUserId}";
                var cachedFiles = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedFiles))
                {
                    _logger.LogDebug("Cache hit for files");
                    return Ok(JsonSerializer.Deserialize<object>(cachedFiles));
                }

                var files = await _fileService.GetFilesAsync(queryParams);

                // 缓存结果
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(files), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files");
                return StatusCode(500, new { error = "An error occurred while getting files" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileById(long id)
        {
            try
            {
                _logger.LogInformation("Getting file with ID: {FileId}", id);

                // 检查缓�?
                var cacheKey = $"file:{id}";
                var cachedFile = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedFile))
                {
                    _logger.LogDebug("Cache hit for file {FileId}", id);
                    return Ok(JsonSerializer.Deserialize<object>(cachedFile));
                }

                var file = await _fileService.GetFileByIdAsync(id);
                if (file == null)
                {
                    return NotFound(new { error = $"File with ID {id} not found" });
                }

                // 缓存结果
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(file), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file with ID: {FileId}", id);
                return StatusCode(500, new { error = "An error occurred while getting file" });
            }
        }

        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetFileByFileId(string fileId)
        {
            try
            {
                _logger.LogInformation("Getting file with fileId: {FileId}", fileId);

                var file = await _fileService.GetFileByFileIdAsync(fileId);
                if (file == null)
                {
                    return NotFound(new { error = $"File with fileId '{fileId}' not found" });
                }

                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file with fileId: {FileId}", fileId);
                return StatusCode(500, new { error = "An error occurred while getting file" });
            }
        }

        [HttpPost("upload")]
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

                return CreatedAtAction(nameof(GetFileByFileId), new { fileId = result.File?.FileId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = $"An error occurred while uploading file: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFile(long id, [FromBody] UpdateFileRequest request)
        {
            try
            {
                _logger.LogInformation("Updating file with ID: {FileId}", id);

                // 验证权限：只有上传者或管理员可以更新文�?
                var file = await _fileService.GetFileByIdAsync(id);
                if (file == null)
                {
                    return NotFound(new { error = $"File with ID {id} not found" });
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized();
                }

                // 检查权限：上传者或管理�?
                if (file.UploadedByUserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                var updatedFile = await _fileService.UpdateFileAsync(id, request);
                if (updatedFile == null)
                {
                    return NotFound(new { error = $"File with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearFileCache(id);
                await ClearFilesCache();

                return Ok(updatedFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating file with ID: {FileId}", id);
                return StatusCode(500, new { error = "An error occurred while updating file" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(long id)
        {
            try
            {
                _logger.LogInformation("Deleting file with ID: {FileId}", id);

                // 验证权限
                var file = await _fileService.GetFileByIdAsync(id);
                if (file == null)
                {
                    return NotFound(new { error = $"File with ID {id} not found" });
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized();
                }

                // 检查权限：上传者或管理�?
                if (file.UploadedByUserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                var result = await _fileService.DeleteFileAsync(id);
                if (!result)
                {
                    return NotFound(new { error = $"File with ID {id} not found" });
                }

                // 清除相关缓存
                await ClearFileCache(id);
                await ClearFilesCache();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file with ID: {FileId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting file" });
            }
        }

        [HttpGet("{fileId}/download")]
        [AllowAnonymous] // 可以根据需要设置为需要认�?
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            try
            {
                _logger.LogInformation("Downloading file: {FileId}", fileId);

                var file = await _fileService.GetFileByFileIdAsync(fileId);
                if (file == null)
                {
                    return NotFound(new { error = $"File with fileId '{fileId}' not found" });
                }

                // 检查文件是否过�?
                if (file.ExpiresAt.HasValue && file.ExpiresAt < DateTime.UtcNow)
                {
                    return NotFound(new { error = "File has expired" });
                }

                // 检查文件状�?
                if (file.Status != "ready" && file.Status != "uploaded")
                {
                    return BadRequest(new { error = $"File is not available for download. Status: {file.Status}" });
                }

                var stream = await _fileService.GetFileStreamAsync(fileId);
                return File(stream, file.ContentType, file.OriginalFileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found: {FileId}", fileId);
                return NotFound(new { error = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileId}", fileId);
                return StatusCode(500, new { error = "An error occurred while downloading file" });
            }
        }

        [HttpGet("{fileId}/info")]
        public async Task<IActionResult> GetFileInfo(string fileId)
        {
            try
            {
                _logger.LogInformation("Getting file info: {FileId}", fileId);

                var info = await _fileService.GetFileDownloadInfoAsync(fileId);
                if (string.IsNullOrEmpty(info.FileName))
                {
                    return NotFound(new { error = $"File with fileId '{fileId}' not found" });
                }

                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info: {FileId}", fileId);
                return StatusCode(500, new { error = "An error occurred while getting file info" });
            }
        }

        [HttpPost("cleanup-expired")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CleanupExpiredFiles()
        {
            try
            {
                _logger.LogInformation("Cleaning up expired files");

                var cutoffDate = DateTime.UtcNow;
                var result = await _fileService.CleanupExpiredFilesAsync(cutoffDate);

                // 清除文件列表缓存
                await ClearFilesCache();

                return Ok(new { message = "Expired files cleanup completed", timestamp = cutoffDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired files");
                return StatusCode(500, new { error = "An error occurred while cleaning up expired files" });
            }
        }

        [HttpPost("generate-upload-url")]
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

        private async Task ClearFileCache(long fileId)
        {
            var cacheKey = $"file:{fileId}";
            await _cache.RemoveAsync(cacheKey);

            // 直接清除Redis中的键以确保一致�?
            try
            {
                var db = _redis.GetDatabase();
                var fullKey = $"WritingPlatform:FileService:{cacheKey}";
                await db.KeyDeleteAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing file cache directly for file {FileId}", fileId);
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

        [HttpGet("hash/{fileHash}")]
        public async Task<IActionResult> GetFileByHash(string fileHash)
        {
            try
            {
                _logger.LogInformation("Getting file by hash: {FileHash}", fileHash);

                var file = await _fileService.GetFileByHashAsync(fileHash);
                if (file == null)
                {
                    return NotFound(new { error = $"File with hash '{fileHash}' not found" });
                }

                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file by hash: {FileHash}", fileHash);
                return StatusCode(500, new { error = "An error occurred while getting file" });
            }
        }

        [HttpGet("tags/{tags}")]
        public async Task<IActionResult> GetFilesByTags(string tags, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting files by tags: {Tags}, Page: {Page}, PageSize: {PageSize}", tags, page, pageSize);

                var files = await _fileService.GetFilesByTagsAsync(tags, page, pageSize);
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files by tags: {Tags}", tags);
                return StatusCode(500, new { error = "An error occurred while getting files" });
            }
        }

        [HttpPost("{id}/access")]
        public async Task<IActionResult> RecordFileAccess(long id)
        {
            try
            {
                _logger.LogInformation("Recording access for file ID: {FileId}", id);

                var result = await _fileService.UpdateFileAccessInfoAsync(id, true);
                if (!result)
                {
                    return NotFound(new { error = $"File with ID {id} not found" });
                }

                return Ok(new { message = "File access recorded", fileId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording file access for ID: {FileId}", id);
                return StatusCode(500, new { error = "An error occurred while recording file access" });
            }
        }
    }

    // 用于生成上传URL的请求DTO
    public class GenerateUploadUrlRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}
