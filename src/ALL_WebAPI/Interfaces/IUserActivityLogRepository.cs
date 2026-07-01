using PaperSystemApi.Models;

namespace PaperSystemApi.Interfaces
{
    public interface IUserActivityLogRepository
    {
        Task<UserActivityLog> CreateAsync(UserActivityLog activityLog);
        Task<UserActivityLog?> GetByIdAsync(long id);
        Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(long userId, int page, int pageSize);
        Task<IEnumerable<UserActivityLog>> GetByActivityTypeAsync(ActivityType activityType, int page, int pageSize);
        Task<IEnumerable<UserActivityLog>> SearchAsync(long? userId, ActivityType? activityType, string? searchTerm, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<int> CountAsync(long? userId, ActivityType? activityType, string? searchTerm, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<UserActivityLog>> GetRecentActivitiesAsync(int limit = 50);
        Task<bool> DeleteOldLogsAsync(DateTime cutoffDate);
    }
}