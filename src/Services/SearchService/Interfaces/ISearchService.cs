using SearchService.DTOs;

namespace SearchService.Interfaces
{
    public interface IISearchService
    {
        Task<SearchResult<WritingSearchResult>> SearchWritingsAsync(SearchRequest request, long? userId = null);
        Task<SearchResult<UserSearchResult>> SearchUsersAsync(SearchRequest request, long? userId = null);
        Task<SearchResult<TagSearchResult>> SearchTagsAsync(SearchRequest request, long? userId = null);
        Task<SearchResult<object>> SearchAllAsync(SearchRequest request, long? userId = null);
        Task<SearchHistoryDTO?> GetSearchHistoryByIdAsync(long id);
        Task<IEnumerable<SearchHistoryDTO>> GetUserSearchHistoryAsync(long userId, int page, int pageSize);
        Task<bool> DeleteSearchHistoryAsync(long id);
        Task<bool> ClearUserSearchHistoryAsync(long userId);
        Task<IEnumerable<SearchService.DTOs.PopularSearchTerm>> GetPopularSearchTermsAsync(int limit = 10);
        Task<IEnumerable<SearchSuggestion>> GetSearchSuggestionsAsync(string query, int limit = 10);
        Task<bool> IndexWritingAsync(long writingId);
        Task<bool> IndexUserAsync(long userId);
        Task<bool> RemoveWritingFromIndexAsync(long writingId);
        Task<bool> RemoveUserFromIndexAsync(long userId);
        Task<bool> RebuildIndexAsync(string indexType);
        Task<SearchStatistics> GetSearchStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public class SearchStatistics
    {
        public int TotalSearches { get; set; }
        public int SuccessfulSearches { get; set; }
        public int FailedSearches { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public Dictionary<string, int> SearchesByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TopQueries { get; set; } = new Dictionary<string, int>();
        public int UniqueUsers { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}