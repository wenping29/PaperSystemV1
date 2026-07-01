using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaperSystemApi.DTOs;
using PaperSystemApi.Models;
using PaperSystemApi.Interfaces;
using IOFile = System.IO.File;

namespace PaperSystemApi.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadPath;

        public FileService(
            IFileRepository fileRepository,
            IMapper mapper,
            ILogger<FileService> logger,
            IConfiguration configuration)
        {
            _fileRepository = fileRepository;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;

            // 获取上传路径配置，默认为 wwwroot/uploads
            _uploadPath = _configuration["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // 确保上传目录存在
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<FileDTO?> GetFileByIdAsync(long id)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            return file == null ? null : MapToFileDTO(file);
        }

        public async Task<FileDTO?> GetFileByFileIdAsync(string fileId)
        {
            var file = await _fileRepository.GetByFileIdAsync(fileId);
            return file == null ? null : MapToFileDTO(file);
        }

        public async Task<IEnumerable<FileDTO>> GetFilesAsync(FileQueryParams queryParams)
        {
            var files = await _fileRepository.SearchAsync(
                null, // searchTerm - 可以根据需要扩�?
                queryParams.FileType,
                queryParams.UploadedByUserId,
                queryParams.StartDate,
                queryParams.EndDate,
                queryParams.Page,
                queryParams.PageSize);

            return files.Select(MapToFileDTO);
        }

        public async Task<int> GetFilesCountAsync(FileQueryParams queryParams)
        {
            // 注意：这里简化了，假设只按类型和用户筛�?
            return await _fileRepository.CountFilesAsync(queryParams.FileType, queryParams.UploadedByUserId);
        }

        public async Task<FileUploadResult> UploadFileAsync(UploadFileRequest request, long? userId = null)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "No file provided"
                };
            }

            try
            {
                // 验证文件大小
                var maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSize", 100 * 1024 * 1024); // 默认100MB
                if (request.File.Length > maxFileSize)
                {
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = $"File size exceeds limit. Max: {maxFileSize / (1024 * 1024)}MB"
                    };
                }

                // 生成唯一文件�?
                var fileId = Guid.NewGuid().ToString();
                var fileExtension = Path.GetExtension(request.File.FileName);
                var safeFileName = $"{fileId}{fileExtension}";
                var filePath = Path.Combine(_uploadPath, safeFileName);

                // 确定文件类型
                var fileType = request.FileType ?? DetermineFileType(request.File.ContentType, fileExtension);

                // 保存文件到磁�?
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // 创建文件元数据记�?
                var fileMetadata = new FileMetadata
                {
                    FileId = fileId,
                    OriginalFileName = request.File.FileName,
                    StoragePath = filePath,
                    ContentType = request.File.ContentType,
                    SizeInBytes = request.File.Length,
                    FileType = fileType,
                    UploadedByUserId = userId,
                    Description = request.Description,
                    Status = FileStatus.Ready,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = request.ExpiresAt
                };

                var createdFile = await _fileRepository.CreateAsync(fileMetadata);
                var fileDto = MapToFileDTO(createdFile);

                // 生成下载和预览URL
                fileDto.DownloadUrl = GenerateFileUrl(fileId, "download");
                fileDto.PreviewUrl = GenerateFileUrl(fileId, "preview");

                return new FileUploadResult
                {
                    Success = true,
                    File = fileDto,
                    FilePath = filePath,
                    FileSize = request.File.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", request.File?.FileName);
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Upload failed: {ex.Message}"
                };
            }
        }

        public async Task<FileDTO?> UpdateFileAsync(long id, UpdateFileRequest request)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null) return null;

            if (!string.IsNullOrEmpty(request.Description))
            {
                file.Description = request.Description;
            }

            if (request.ExpiresAt.HasValue)
            {
                file.ExpiresAt = request.ExpiresAt;
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                file.Status = request.Status;
            }

            var updatedFile = await _fileRepository.UpdateAsync(file);
            return MapToFileDTO(updatedFile);
        }

        public async Task<bool> DeleteFileAsync(long id)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null) return false;

            try
            {
                // 删除物理文件
                if (IOFile.Exists(file.StoragePath))
                {
                    IOFile.Delete(file.StoragePath);
                }

                // 删除数据库记�?
                return await _fileRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", file.FileId);
                return false;
            }
        }

        public async Task<bool> SoftDeleteFileAsync(long id)
        {
            return await _fileRepository.SoftDeleteAsync(id);
        }

        public async Task<FileDownloadInfo> GetFileDownloadInfoAsync(string fileId)
        {
            var file = await _fileRepository.GetByFileIdAsync(fileId);
            if (file == null)
            {
                return new FileDownloadInfo
                {
                    FileName = string.Empty,
                    ContentType = string.Empty,
                    FileSize = 0,
                    LastModified = DateTime.MinValue
                };
            }

            var fileInfo = new FileInfo(file.StoragePath);
            return new FileDownloadInfo
            {
                FileName = file.OriginalFileName,
                ContentType = file.ContentType,
                FileSize = file.SizeInBytes,
                LastModified = fileInfo.LastWriteTimeUtc,
                DownloadUrl = GenerateFileUrl(fileId, "download")
            };
        }

        public async Task<Stream> GetFileStreamAsync(string fileId)
        {
            var file = await _fileRepository.GetByFileIdAsync(fileId);
            if (file == null || !IOFile.Exists(file.StoragePath))
            {
                throw new FileNotFoundException($"File not found: {fileId}");
            }

            return new FileStream(file.StoragePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public async Task<bool> UpdateFileStatusAsync(long id, string status)
        {
            return await _fileRepository.UpdateStatusAsync(id, status);
        }

        public async Task<bool> UpdateFileExpirationAsync(long id, DateTime? expiresAt)
        {
            return await _fileRepository.UpdateExpirationAsync(id, expiresAt);
        }

        public async Task<IEnumerable<FileDTO>> GetExpiredFilesAsync(DateTime cutoffDate)
        {
            var files = await _fileRepository.GetExpiredFilesAsync(cutoffDate);
            return files.Select(MapToFileDTO);
        }

        public async Task<bool> CleanupExpiredFilesAsync(DateTime cutoffDate)
        {
            var expiredFiles = await _fileRepository.GetExpiredFilesAsync(cutoffDate);
            var success = true;

            foreach (var file in expiredFiles)
            {
                try
                {
                    // 删除物理文件
                    if (IOFile.Exists(file.StoragePath))
                    {
                        IOFile.Delete(file.StoragePath);
                    }

                    // 软删除数据库记录
                    await _fileRepository.SoftDeleteAsync(file.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up expired file {FileId}", file.FileId);
                    success = false;
                }
            }

            return success;
        }

        public async Task<FileDTO?> GenerateUploadUrlAsync(string fileName, string contentType, long? userId = null, DateTime? expiresAt = null)
        {
            // 生成预签名URL逻辑（对于云存储�?
            // 这里简化实现，仅创建文件元数据记录
            var fileId = Guid.NewGuid().ToString();
            var fileExtension = Path.GetExtension(fileName);
            var safeFileName = $"{fileId}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, safeFileName);
            var fileType = DetermineFileType(contentType, fileExtension);

            var fileMetadata = new FileMetadata
            {
                FileId = fileId,
                OriginalFileName = fileName,
                StoragePath = filePath,
                ContentType = contentType,
                SizeInBytes = 0, // 未知
                FileType = fileType,
                UploadedByUserId = userId,
                Status = FileStatus.Uploaded, // 标记为已上传但未完成
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            var createdFile = await _fileRepository.CreateAsync(fileMetadata);
            var fileDto = MapToFileDTO(createdFile);

            // 生成上传URL（这里简化）
            fileDto.DownloadUrl = GenerateFileUrl(fileId, "upload");

            return fileDto;
        }

        public async Task<bool> ValidateFileAsync(string fileId, long? userId = null)
        {
            var file = await _fileRepository.GetByFileIdAsync(fileId);
            if (file == null) return false;

            // 检查文件是否存�?
            if (!IOFile.Exists(file.StoragePath)) return false;

            // 如果提供了用户ID，检查权�?
            if (userId.HasValue && file.UploadedByUserId.HasValue)
            {
                return file.UploadedByUserId.Value == userId.Value;
            }

            return true;
        }

        private FileDTO MapToFileDTO(FileMetadata file)
        {
            var dto = _mapper.Map<FileDTO>(file);
            dto.DownloadUrl = GenerateFileUrl(file.FileId, "download");
            dto.PreviewUrl = GenerateFileUrl(file.FileId, "preview");
            return dto;
        }

        private string GenerateFileUrl(string fileId, string action)
        {
            var baseUrl = _configuration["FileStorage:BaseUrl"] ?? _configuration["BaseUrl"] ?? "http://localhost:5006";
            return $"{baseUrl}/api/files/{fileId}/{action}";
        }

        private string DetermineFileType(string contentType, string fileExtension)
        {
            if (contentType.StartsWith("image/")) return FileType.Image;
            if (contentType.StartsWith("video/")) return FileType.Video;
            if (contentType.StartsWith("audio/")) return FileType.Audio;
            if (contentType.Contains("pdf") || contentType.Contains("document") || contentType.Contains("text"))
                return FileType.Document;
            if (fileExtension == ".zip" || fileExtension == ".rar" || fileExtension == ".7z" || fileExtension == ".tar" || fileExtension == ".gz")
                return FileType.Archive;

            return FileType.Other;
        }

        public async Task<FileDTO?> GetFileByHashAsync(string fileHash)
        {
            var file = await _fileRepository.GetByFileHashAsync(fileHash);
            return file == null ? null : MapToFileDTO(file);
        }

        public async Task<bool> UpdateFileAccessInfoAsync(long id, bool incrementCount = true)
        {
            return await _fileRepository.UpdateAccessInfoAsync(id, incrementCount);
        }

        public async Task<IEnumerable<FileDTO>> GetFilesByTagsAsync(string tags, int page = 1, int pageSize = 20)
        {
            var files = await _fileRepository.GetByTagsAsync(tags, page, pageSize);
            return files.Select(MapToFileDTO);
        }
    }
}
