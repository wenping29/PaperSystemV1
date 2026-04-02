using System;

namespace SearchService.DTOs
{
    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public string? SearchType { get; set; } // writing, user, tag, all
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "relevance"; // relevance, date, popularity
        public bool SortDescending { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Language { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; } // 逗号分隔的标签
        public long? AuthorId { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }
        public bool? IncludeDeleted { get; set; } = false;
    }

    public class SearchResult<T>
    {
        public bool Success { get; set; }
        public string Query { get; set; } = string.Empty;
        public string SearchType { get; set; } = string.Empty;
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Results { get; set; } = new List<T>();
        public Dictionary<string, object>? Facets { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public string? SearchId { get; set; }
    }

    public class WritingSearchResult
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public long AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatarUrl { get; set; }
        public string? Category { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public decimal? Rating { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Language { get; set; }
        public string? Status { get; set; }
        public double RelevanceScore { get; set; }
    }

    public class UserSearchResult
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Location { get; set; }
        public int WritingCount { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActive { get; set; }
        public bool IsVerified { get; set; }
        public double RelevanceScore { get; set; }
    }

    public class TagSearchResult
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public double RelevanceScore { get; set; }
    }

    public class SearchHistoryDTO
    {
        public long Id { get; set; }
        public string Query { get; set; } = string.Empty;
        public string? SearchType { get; set; }
        public long? UserId { get; set; }
        public int ResultCount { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime CreatedAt { get; set; }
        public TimeSpan ResponseTime { get; set; }
    }

    public class PopularSearchTerm
    {
        public string Term { get; set; } = string.Empty;
        public int SearchCount { get; set; }
        public DateTime LastSearched { get; set; }
    }

    public class SearchSuggestion
    {
        public string Suggestion { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // query, user, tag, writing
        public long? Id { get; set; }
        public double Score { get; set; }
    }
}