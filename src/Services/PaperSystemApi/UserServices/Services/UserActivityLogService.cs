using AutoMapper;
using Microsoft.Extensions.Logging;
using PaperSystemApi.UserServices.DTOs;
using PaperSystemApi.UserServices.Entities;
using PaperSystemApi.UserServices.Interfaces;

namespace PaperSystemApi.UserServices.Services
{
    public class UserActivityLogService : IUserActivityLogService
    {
        private readonly IUserActivityLogRepository _activityLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserActivityLogService> _logger;

        public UserActivityLogService(
            IUserActivityLogRepository activityLogRepository,
            IMapper mapper,
            ILogger<UserActivityLogService> logger)
        {
            _activityLogRepository = activityLogRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ActivityLogResponse> CreateActivityLogAsync(CreateActivityLogRequest request)
        {
            var activityLog = _mapper.Map<UserActivityLog>(request);
            var createdLog = await _activityLogRepository.CreateAsync(activityLog);

            _logger.LogDebug("Created activity log for user {UserId}: {ActivityType}",
                request.UserId, request.ActivityType);

            return await MapToActivityLogResponse(createdLog);
        }

        public async Task<ActivityLogResponse?> GetActivityLogByIdAsync(long id)
        {
            var activityLog = await _activityLogRepository.GetByIdAsync(id);
            if (activityLog == null) return null;

            return await MapToActivityLogResponse(activityLog);
        }

        public async Task<ActivityLogSearchResponse> SearchActivityLogsAsync(ActivityLogSearchRequest request)
        {
            var logs = await _activityLogRepository.SearchAsync(
                request.UserId,
                request.ActivityType,
                request.SearchTerm,
                request.StartDate,
                request.EndDate,
                request.Page,
                request.PageSize);

            var totalCount = await _activityLogRepository.CountAsync(
                request.UserId,
                request.ActivityType,
                request.SearchTerm,
                request.StartDate,
                request.EndDate);

            var logResponses = new List<ActivityLogResponse>();
            foreach (var log in logs)
            {
                logResponses.Add(await MapToActivityLogResponse(log));
            }

            return new ActivityLogSearchResponse
            {
                Logs = logResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }

        public async Task<IEnumerable<ActivityLogResponse>> GetUserActivityLogsAsync(long userId, int page, int pageSize)
        {
            var logs = await _activityLogRepository.GetByUserIdAsync(userId, page, pageSize);
            var logResponses = new List<ActivityLogResponse>();

            foreach (var log in logs)
            {
                logResponses.Add(await MapToActivityLogResponse(log));
            }

            return logResponses;
        }

        public async Task<IEnumerable<ActivityLogResponse>> GetRecentActivityLogsAsync(int limit = 50)
        {
            var logs = await _activityLogRepository.GetRecentActivitiesAsync(limit);
            var logResponses = new List<ActivityLogResponse>();

            foreach (var log in logs)
            {
                logResponses.Add(await MapToActivityLogResponse(log));
            }

            return logResponses;
        }

        public async Task<bool> CleanupOldLogsAsync(DateTime cutoffDate)
        {
            try
            {
                var result = await _activityLogRepository.DeleteOldLogsAsync(cutoffDate);
                _logger.LogInformation("Cleaned up activity logs older than {CutoffDate}. Result: {Result}",
                    cutoffDate, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old activity logs");
                return false;
            }
        }

        private async Task<ActivityLogResponse> MapToActivityLogResponse(UserActivityLog activityLog)
        {
            var response = _mapper.Map<ActivityLogResponse>(activityLog);
            response.ActivityType = activityLog.ActivityType.ToString();

            // 如果包含用户信息，映射用户简要信息
            if (activityLog.User != null)
            {
                response.User = new UserListResponse
                {
                    Id = activityLog.User.Id,
                    Username = activityLog.User.Username,
                    AvatarUrl = activityLog.User.AvatarUrl,
                    Bio = activityLog.User.Bio,
                    CreatedAt = activityLog.User.CreatedAt
                };
            }

            return response;
        }
    }
}