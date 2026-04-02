using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using SearchService.DTOs;
using SearchService.Entities;
using SearchService.Interfaces;

namespace SearchService.Services
{
    public class ISearchService : IISearchService
    {
        private readonly ISearchRepository _searchRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ISearchService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _userServiceUrl;
        private readonly string _writingServiceUrl;

        public ISearchService(
            ISearchRepository searchRepository,
            IMapper mapper,
            ILogger<ISearchService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _searchRepository = searchRepository;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;

            _userServiceUrl = _configuration["ServiceUrls:UserService"] ?? "http://localhost:5000";
            _writingServiceUrl = _configuration["ServiceUrls:WritingService"] ?? "http://localhost:5001";
        }

        public async Task<SearchResult<WritingSearchResult>> SearchWritingsAsync(SearchRequest request, long? userId = null)
        {
            var startTime = DateTime.UtcNow;
            var searchId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Searching writings: {Query}, Type: {SearchType}, User: {UserId}", request.Query, request.SearchType, userId);

                // 这里应该调用WritingService的搜索API
                // 暂时返回空结果
                var result = new SearchResult<WritingSearchResult>
                {
                    Success = true,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "writing",
                    TotalResults = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = 0,
                    Results = new List<WritingSearchResult>(),
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId
                };

                // 记录搜索历史
                await RecordSearchHistory(request, userId, result.TotalResults, true, null, startTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching writings: {Query}", request.Query);
                await RecordSearchHistory(request, userId, 0, false, ex.Message, startTime);

                return new SearchResult<WritingSearchResult>
                {
                    Success = false,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "writing",
                    ErrorMessage = $"Search failed: {ex.Message}",
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId,
                    Results = new List<WritingSearchResult>()
                };
            }
        }

        public async Task<SearchResult<UserSearchResult>> SearchUsersAsync(SearchRequest request, long? userId = null)
        {
            var startTime = DateTime.UtcNow;
            var searchId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Searching users: {Query}, Type: {SearchType}, User: {UserId}", request.Query, request.SearchType, userId);

                // 这里应该调用UserService的搜索API
                // 暂时返回空结果
                var result = new SearchResult<UserSearchResult>
                {
                    Success = true,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "user",
                    TotalResults = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = 0,
                    Results = new List<UserSearchResult>(),
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId
                };

                await RecordSearchHistory(request, userId, result.TotalResults, true, null, startTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users: {Query}", request.Query);
                await RecordSearchHistory(request, userId, 0, false, ex.Message, startTime);

                return new SearchResult<UserSearchResult>
                {
                    Success = false,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "user",
                    ErrorMessage = $"Search failed: {ex.Message}",
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId,
                    Results = new List<UserSearchResult>()
                };
            }
        }

        public async Task<SearchResult<TagSearchResult>> SearchTagsAsync(SearchRequest request, long? userId = null)
        {
            var startTime = DateTime.UtcNow;
            var searchId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Searching tags: {Query}, Type: {SearchType}, User: {UserId}", request.Query, request.SearchType, userId);

                // 暂时返回空结果
                var result = new SearchResult<TagSearchResult>
                {
                    Success = true,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "tag",
                    TotalResults = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = 0,
                    Results = new List<TagSearchResult>(),
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId
                };

                await RecordSearchHistory(request, userId, result.TotalResults, true, null, startTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tags: {Query}", request.Query);
                await RecordSearchHistory(request, userId, 0, false, ex.Message, startTime);

                return new SearchResult<TagSearchResult>
                {
                    Success = false,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "tag",
                    ErrorMessage = $"Search failed: {ex.Message}",
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId,
                    Results = new List<TagSearchResult>()
                };
            }
        }

        public async Task<SearchResult<object>> SearchAllAsync(SearchRequest request, long? userId = null)
        {
            var startTime = DateTime.UtcNow;
            var searchId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Searching all: {Query}, Type: {SearchType}, User: {UserId}", request.Query, request.SearchType, userId);

                // 并行搜索不同类型
                var writingsTask = SearchWritingsAsync(request, userId);
                var usersTask = SearchUsersAsync(request, userId);
                var tagsTask = SearchTagsAsync(request, userId);

                await Task.WhenAll(writingsTask, usersTask, tagsTask);

                var writingsResult = await writingsTask;
                var usersResult = await usersTask;
                var tagsResult = await tagsTask;

                // 合并结果
                var combinedResults = new List<object>();
                combinedResults.AddRange(writingsResult.Results);
                combinedResults.AddRange(usersResult.Results);
                combinedResults.AddRange(tagsResult.Results);

                var totalResults = writingsResult.TotalResults + usersResult.TotalResults + tagsResult.TotalResults;

                var result = new SearchResult<object>
                {
                    Success = true,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "all",
                    TotalResults = totalResults,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalResults / (double)request.PageSize),
                    Results = combinedResults,
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId
                };

                await RecordSearchHistory(request, userId, totalResults, true, null, startTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching all: {Query}", request.Query);
                await RecordSearchHistory(request, userId, 0, false, ex.Message, startTime);

                return new SearchResult<object>
                {
                    Success = false,
                    Query = request.Query,
                    SearchType = request.SearchType ?? "all",
                    ErrorMessage = $"Search failed: {ex.Message}",
                    ResponseTime = DateTime.UtcNow - startTime,
                    SearchId = searchId,
                    Results = new List<object>()
                };
            }
        }

        public async Task<SearchHistoryDTO?> GetSearchHistoryByIdAsync(long id)
        {
            var history = await _searchRepository.GetByIdAsync(id);
            return history == null ? null : _mapper.Map<SearchHistoryDTO>(history);
        }

        public async Task<IEnumerable<SearchHistoryDTO>> GetUserSearchHistoryAsync(long userId, int page, int pageSize)
        {
            var histories = await _searchRepository.GetByUserIdAsync(userId, page, pageSize);
            return _mapper.Map<IEnumerable<SearchHistoryDTO>>(histories);
        }

        public async Task<bool> DeleteSearchHistoryAsync(long id)
        {
            return await _searchRepository.DeleteAsync(id);
        }

        public async Task<bool> ClearUserSearchHistoryAsync(long userId)
        {
            return await _searchRepository.ClearUserHistoryAsync(userId);
        }

        public async Task<IEnumerable<SearchService.DTOs.PopularSearchTerm>> GetPopularSearchTermsAsync(int limit = 10)
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            return (IEnumerable<DTOs.PopularSearchTerm>)await _searchRepository.GetPopularSearchTermsAsync(limit, lastMonth, DateTime.UtcNow);
        }

        public async Task<IEnumerable<SearchSuggestion>> GetSearchSuggestionsAsync(string query, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return new List<SearchSuggestion>();
            }

            // 从搜索历史中获取建议
            var popularTerms = await _searchRepository.GetPopularSearchTermsAsync(limit * 2);
            var suggestions = popularTerms
                .Where(t => t.Term.ToLower().Contains(query.ToLower()))
                .Take(limit)
                .Select(t => new SearchSuggestion
                {
                    Suggestion = t.Term,
                    Type = "query",
                    Score = t.SearchCount / 100.0
                })
                .ToList();

            return suggestions;
        }

        public async Task<bool> IndexWritingAsync(long writingId)
        {
            try
            {
                _logger.LogInformation("Indexing writing: {WritingId}", writingId);

                var index = new SearchIndex
                {
                    IndexType = IndexType.Writing,
                    EntityId = writingId,
                    Status = IndexStatus.Pending,
                    IndexedAt = DateTime.UtcNow,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        IndexedAt = DateTime.UtcNow
                    })
                };

                await _searchRepository.CreateOrUpdateIndexAsync(index);

                // 这里可以添加实际索引逻辑（例如调用Elasticsearch API）
                // 模拟索引成功
                await _searchRepository.UpdateIndexStatusAsync(IndexType.Writing, writingId, IndexStatus.Indexed);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing writing {WritingId}", writingId);
                await _searchRepository.UpdateIndexStatusAsync(IndexType.Writing, writingId, IndexStatus.Error, ex.Message);
                return false;
            }
        }

        public async Task<bool> IndexUserAsync(long userId)
        {
            try
            {
                _logger.LogInformation("Indexing user: {UserId}", userId);

                var index = new SearchIndex
                {
                    IndexType = IndexType.User,
                    EntityId = userId,
                    Status = IndexStatus.Pending,
                    IndexedAt = DateTime.UtcNow,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        IndexedAt = DateTime.UtcNow
                    })
                };

                await _searchRepository.CreateOrUpdateIndexAsync(index);

                // 模拟索引成功
                await _searchRepository.UpdateIndexStatusAsync(IndexType.User, userId, IndexStatus.Indexed);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing user {UserId}", userId);
                await _searchRepository.UpdateIndexStatusAsync(IndexType.User, userId, IndexStatus.Error, ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveWritingFromIndexAsync(long writingId)
        {
            try
            {
                _logger.LogInformation("Removing writing from index: {WritingId}", writingId);

                var result = await _searchRepository.DeleteIndexAsync(IndexType.Writing, writingId);
                if (!result)
                {
                    _logger.LogWarning("Writing not found in index: {WritingId}", writingId);
                }

                // 这里可以添加从实际搜索索引中删除的逻辑
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing writing from index {WritingId}", writingId);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromIndexAsync(long userId)
        {
            try
            {
                _logger.LogInformation("Removing user from index: {UserId}", userId);

                var result = await _searchRepository.DeleteIndexAsync(IndexType.User, userId);
                if (!result)
                {
                    _logger.LogWarning("User not found in index: {UserId}", userId);
                }

                // 这里可以添加从实际搜索索引中删除的逻辑
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from index {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RebuildIndexAsync(string indexType)
        {
            try
            {
                _logger.LogInformation("Rebuilding index: {IndexType}", indexType);

                // 获取所有需要重建的索引
                var indices = await _searchRepository.GetIndicesByTypeAsync(indexType, 1, int.MaxValue);
                foreach (var index in indices)
                {
                    // 标记为待更新
                    await _searchRepository.UpdateIndexStatusAsync(indexType, index.EntityId, IndexStatus.Pending);
                    _logger.LogDebug("Marked index for rebuild: {IndexType} - {EntityId}", indexType, index.EntityId);
                }

                _logger.LogInformation("Started rebuild of {Count} {IndexType} indices", indices.Count(), indexType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding index {IndexType}", indexType);
                return false;
            }
        }

        public async Task<SearchStatistics> GetSearchStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var histories = await _searchRepository.SearchHistoriesAsync(null, null, null, startDate, endDate, 1, int.MaxValue);
            var popularTerms = await _searchRepository.GetPopularSearchTermsAsync(10, startDate, endDate);

            var statistics = new SearchStatistics
            {
                TotalSearches = histories.Count(),
                SuccessfulSearches = histories.Count(h => h.IsSuccessful),
                FailedSearches = histories.Count(h => !h.IsSuccessful),
                AverageResponseTimeMs = histories.Any() ? histories.Average(h => h.ResponseTime.TotalMilliseconds) : 0,
                PeriodStart = startDate.Value,
                PeriodEnd = endDate.Value,
                UniqueUsers = histories.Select(h => h.UserId).Distinct().Count()
            };

            // 按类型分组
            statistics.SearchesByType = histories
                .Where(h => !string.IsNullOrEmpty(h.SearchType))
                .GroupBy(h => h.SearchType!)
                .ToDictionary(g => g.Key, g => g.Count());

            // 热门查询
            statistics.TopQueries = popularTerms
                .Take(10)
                .ToDictionary(t => t.Term, t => t.SearchCount);

            return statistics;
        }

        private async Task RecordSearchHistory(SearchRequest request, long? userId, int resultCount, bool isSuccessful, string? errorMessage, DateTime startTime)
        {
            try
            {
                var responseTime = DateTime.UtcNow - startTime;

                var history = new SearchHistory
                {
                    Query = request.Query,
                    SearchType = request.SearchType,
                    UserId = userId,
                    ResultCount = resultCount,
                    IsSuccessful = isSuccessful,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow,
                    ResponseTime = responseTime,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        request.Page,
                        request.PageSize,
                        request.SortBy,
                        request.SortDescending,
                        request.Language,
                        request.Category,
                        request.Tags,
                        request.AuthorId
                    })
                };

                await _searchRepository.CreateAsync(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording search history for query: {Query}", request.Query);
                // 不抛出异常，避免影响主要搜索功能
            }
        }
    }
}