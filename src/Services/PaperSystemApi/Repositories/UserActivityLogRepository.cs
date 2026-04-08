using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Interfaces;

namespace PaperSystemApi.Repositories
{
    public class UserActivityLogRepository : IUserActivityLogRepository
    {
        private readonly UserDbContext _context;

        public UserActivityLogRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<UserActivityLog> CreateAsync(UserActivityLog activityLog)
        {
            activityLog.CreatedAt = DateTime.UtcNow;
            _context.UserActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();
            return activityLog;
        }

        public async Task<UserActivityLog?> GetByIdAsync(long id)
        {
            return await _context.UserActivityLogs
                .Include(log => log.User)
                .FirstOrDefaultAsync(log => log.Id == id);
        }

        public async Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(long userId, int page, int pageSize)
        {
            return await _context.UserActivityLogs
                .Where(log => log.UserId == userId)
                .Include(log => log.User)
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> GetByActivityTypeAsync(ActivityType activityType, int page, int pageSize)
        {
            return await _context.UserActivityLogs
                .Where(log => log.ActivityType == activityType)
                .Include(log => log.User)
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> SearchAsync(
            long? userId,
            ActivityType? activityType,
            string? searchTerm,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize)
        {
            var query = _context.UserActivityLogs.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(log => log.UserId == userId.Value);
            }

            if (activityType.HasValue)
            {
                query = query.Where(log => log.ActivityType == activityType.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = $"%{searchTerm}%";
                query = query.Where(log =>
                    EF.Functions.Like(log.Description, searchTerm) ||
                    EF.Functions.Like(log.Details, searchTerm) ||
                    EF.Functions.Like(log.IpAddress, searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.CreatedAt <= endDate.Value);
            }

            return await query
                .Include(log => log.User)
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(
            long? userId,
            ActivityType? activityType,
            string? searchTerm,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.UserActivityLogs.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(log => log.UserId == userId.Value);
            }

            if (activityType.HasValue)
            {
                query = query.Where(log => log.ActivityType == activityType.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = $"%{searchTerm}%";
                query = query.Where(log =>
                    EF.Functions.Like(log.Description, searchTerm) ||
                    EF.Functions.Like(log.Details, searchTerm) ||
                    EF.Functions.Like(log.IpAddress, searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.CreatedAt <= endDate.Value);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> GetRecentActivitiesAsync(int limit = 50)
        {
            return await _context.UserActivityLogs
                .Include(log => log.User)
                .OrderByDescending(log => log.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> DeleteOldLogsAsync(DateTime cutoffDate)
        {
            var oldLogs = await _context.UserActivityLogs
                .Where(log => log.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.UserActivityLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}