using UserService.DTOs;
using UserService.Entities;

namespace UserService.Interfaces
{
    public interface IUserActivityLogService
    {
        Task<ActivityLogResponse> CreateActivityLogAsync(CreateActivityLogRequest request);
        Task<ActivityLogResponse?> GetActivityLogByIdAsync(long id);
        Task<ActivityLogSearchResponse> SearchActivityLogsAsync(ActivityLogSearchRequest request);
        Task<IEnumerable<ActivityLogResponse>> GetUserActivityLogsAsync(long userId, int page, int pageSize);
        Task<IEnumerable<ActivityLogResponse>> GetRecentActivityLogsAsync(int limit = 50);
        Task<bool> CleanupOldLogsAsync(DateTime cutoffDate);
    }
}