using Microsoft.AspNetCore.Http;
using FileService.DTOs;

namespace FileService.Interfaces
{
    public interface IIFileService
    {
        Task<FileDTO?> GetFileByIdAsync(long id);
        Task<FileDTO?> GetFileByFileIdAsync(string fileId);
        Task<IEnumerable<FileDTO>> GetFilesAsync(FileQueryParams queryParams);
        Task<int> GetFilesCountAsync(FileQueryParams queryParams);
        Task<FileUploadResult> UploadFileAsync(UploadFileRequest request, long? userId = null);
        Task<FileDTO?> UpdateFileAsync(long id, UpdateFileRequest request);
        Task<bool> DeleteFileAsync(long id);
        Task<bool> SoftDeleteFileAsync(long id);
        Task<FileDownloadInfo> GetFileDownloadInfoAsync(string fileId);
        Task<Stream> GetFileStreamAsync(string fileId);
        Task<bool> UpdateFileStatusAsync(long id, string status);
        Task<bool> UpdateFileExpirationAsync(long id, DateTime? expiresAt);
        Task<IEnumerable<FileDTO>> GetExpiredFilesAsync(DateTime cutoffDate);
        Task<bool> CleanupExpiredFilesAsync(DateTime cutoffDate);
        Task<FileDTO?> GenerateUploadUrlAsync(string fileName, string contentType, long? userId = null, DateTime? expiresAt = null);
        Task<bool> ValidateFileAsync(string fileId, long? userId = null);
        Task<FileDTO?> GetFileByHashAsync(string fileHash);
        Task<bool> UpdateFileAccessInfoAsync(long id, bool incrementCount = true);
        Task<IEnumerable<FileDTO>> GetFilesByTagsAsync(string tags, int page = 1, int pageSize = 20);
    }
}