using Microsoft.EntityFrameworkCore;
using PaperSystemApi.File.Data;
using PaperSystemApi.File.Entities;
using PaperSystemApi.File.Interfaces;

namespace PaperSystemApi.File.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly FileDbContext _context;

        public FileRepository(FileDbContext context)
        {
            _context = context;
        }

        public async Task<FileMetadata?> GetByIdAsync(long id)
        {
            return await _context.FileMetadata
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
        }

        public async Task<FileMetadata?> GetByFileIdAsync(string fileId)
        {
            return await _context.FileMetadata
                .FirstOrDefaultAsync(f => f.FileId == fileId && !f.IsDeleted);
        }

        public async Task<IEnumerable<FileMetadata>> GetAllAsync(int page, int pageSize)
        {
            return await _context.FileMetadata
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<FileMetadata>> GetByUserIdAsync(long userId, int page, int pageSize)
        {
            return await _context.FileMetadata
                .Where(f => f.UploadedByUserId == userId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<FileMetadata>> GetByTypeAsync(string fileType, int page, int pageSize)
        {
            return await _context.FileMetadata
                .Where(f => f.FileType == fileType && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<FileMetadata>> SearchAsync(string? searchTerm, string? fileType, long? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            var query = _context.FileMetadata.Where(f => !f.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = $"%{searchTerm}%";
                query = query.Where(f => EF.Functions.Like(f.OriginalFileName, searchTerm) ||
                                         EF.Functions.Like(f.Description, searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(fileType))
            {
                query = query.Where(f => f.FileType == fileType);
            }

            if (userId.HasValue)
            {
                query = query.Where(f => f.UploadedByUserId == userId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<FileMetadata> CreateAsync(FileMetadata file)
        {
            _context.FileMetadata.Add(file);
            await _context.SaveChangesAsync();
            return file;
        }

        public async Task<FileMetadata> UpdateAsync(FileMetadata file)
        {
            file.UpdatedAt = DateTime.UtcNow;
            _context.FileMetadata.Update(file);
            await _context.SaveChangesAsync();
            return file;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var file = await GetByIdAsync(id);
            if (file == null) return false;

            _context.FileMetadata.Remove(file);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(long id)
        {
            var file = await GetByIdAsync(id);
            if (file == null) return false;

            file.IsDeleted = true;
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FileIdExistsAsync(string fileId)
        {
            return await _context.FileMetadata.AnyAsync(f => f.FileId == fileId && !f.IsDeleted);
        }

        public async Task<int> CountFilesAsync(string? fileType = null, long? userId = null)
        {
            var query = _context.FileMetadata.Where(f => !f.IsDeleted);

            if (!string.IsNullOrWhiteSpace(fileType))
            {
                query = query.Where(f => f.FileType == fileType);
            }

            if (userId.HasValue)
            {
                query = query.Where(f => f.UploadedByUserId == userId);
            }

            return await query.CountAsync();
        }

        public async Task<bool> UpdateStatusAsync(long id, string status)
        {
            var file = await GetByIdAsync(id);
            if (file == null) return false;

            file.Status = status;
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateExpirationAsync(long id, DateTime? expiresAt)
        {
            var file = await GetByIdAsync(id);
            if (file == null) return false;

            file.ExpiresAt = expiresAt;
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FileMetadata>> GetExpiredFilesAsync(DateTime cutoffDate)
        {
            return await _context.FileMetadata
                .Where(f => !f.IsDeleted && f.ExpiresAt.HasValue && f.ExpiresAt <= cutoffDate)
                .ToListAsync();
        }

        public async Task<bool> CleanupExpiredFilesAsync(DateTime cutoffDate)
        {
            var expiredFiles = await GetExpiredFilesAsync(cutoffDate);
            foreach (var file in expiredFiles)
            {
                file.IsDeleted = true;
                file.UpdatedAt = DateTime.UtcNow;
            }

            if (expiredFiles.Any())
            {
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<FileMetadata?> GetByFileHashAsync(string fileHash)
        {
            return await _context.FileMetadata
                .FirstOrDefaultAsync(f => f.FileHash == fileHash && !f.IsDeleted);
        }

        public async Task<bool> UpdateAccessInfoAsync(long id, bool incrementCount = true)
        {
            var file = await GetByIdAsync(id);
            if (file == null) return false;

            if (incrementCount)
            {
                file.AccessCount++;
            }
            file.LastAccessedAt = DateTime.UtcNow;
            file.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FileMetadata>> GetByTagsAsync(string tags, int page, int pageSize)
        {
            // 简单实现：检查Tags字段是否包含指定的标签（逗号分隔）
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();

            var query = _context.FileMetadata.Where(f => !f.IsDeleted);

            foreach (var tag in tagList)
            {
                query = query.Where(f => f.Tags != null && f.Tags.Contains(tag));
            }

            return await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}