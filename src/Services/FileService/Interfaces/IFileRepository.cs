using FileService.Entities;

namespace FileService.Interfaces
{
    public interface IFileRepository
    {
        Task<FileMetadata?> GetByIdAsync(long id);
        Task<FileMetadata?> GetByFileIdAsync(string fileId);
        Task<IEnumerable<FileMetadata>> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<FileMetadata>> GetByUserIdAsync(long userId, int page, int pageSize);
        Task<IEnumerable<FileMetadata>> GetByTypeAsync(string fileType, int page, int pageSize);
        Task<IEnumerable<FileMetadata>> SearchAsync(string? searchTerm, string? fileType, long? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<FileMetadata> CreateAsync(FileMetadata file);
        Task<FileMetadata> UpdateAsync(FileMetadata file);
        Task<bool> DeleteAsync(long id);
        Task<bool> SoftDeleteAsync(long id);
        Task<bool> FileIdExistsAsync(string fileId);
        Task<int> CountFilesAsync(string? fileType = null, long? userId = null);
        Task<bool> UpdateStatusAsync(long id, string status);
        Task<bool> UpdateExpirationAsync(long id, DateTime? expiresAt);
        Task<IEnumerable<FileMetadata>> GetExpiredFilesAsync(DateTime cutoffDate);
        Task<bool> CleanupExpiredFilesAsync(DateTime cutoffDate);
        Task<FileMetadata?> GetByFileHashAsync(string fileHash);
        Task<bool> UpdateAccessInfoAsync(long id, bool incrementCount = true);
        Task<IEnumerable<FileMetadata>> GetByTagsAsync(string tags, int page, int pageSize);
    }
}