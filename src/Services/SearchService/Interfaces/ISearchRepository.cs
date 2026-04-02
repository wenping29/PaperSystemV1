using SearchService.Entities;

namespace SearchService.Interfaces
{
    public interface ISearchRepository
    {
        Task<SearchHistory?> GetByIdAsync(long id);
        Task<IEnumerable<SearchHistory>> GetByUserIdAsync(long userId, int page, int pageSize);
        Task<IEnumerable<SearchHistory>> GetRecentAsync(int limit);
        Task<SearchHistory> CreateAsync(SearchHistory history);
        Task<bool> DeleteAsync(long id);
        Task<bool> ClearUserHistoryAsync(long userId);
        Task<IEnumerable<PopularSearchTerm>> GetPopularSearchTermsAsync(int limit, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<SearchHistory>> SearchHistoriesAsync(string? query, string? searchType, long? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<int> CountSearchHistoriesAsync(string? query, string? searchType, long? userId, DateTime? startDate, DateTime? endDate);

        // 搜索索引相关方法
        Task<SearchIndex?> GetIndexAsync(string indexType, long entityId);
        Task<SearchIndex?> GetIndexByIdAsync(long id);
        Task<SearchIndex> CreateOrUpdateIndexAsync(SearchIndex index);
        Task<bool> DeleteIndexAsync(string indexType, long entityId);
        Task<bool> DeleteIndexByIdAsync(long id);
        Task<IEnumerable<SearchIndex>> GetIndicesByTypeAsync(string indexType, int page, int pageSize);
        Task<IEnumerable<SearchIndex>> SearchIndicesAsync(string? indexType, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<int> CountIndicesAsync(string? indexType, string? status);
        Task<bool> UpdateIndexStatusAsync(string indexType, long entityId, string status, string? errorMessage = null);
        Task<IEnumerable<SearchIndex>> GetIndicesNeedingUpdateAsync(string indexType, int limit);
    }
}