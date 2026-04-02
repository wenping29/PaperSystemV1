using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models.DTOs
{
    public class WritingResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int ReadTimeMinutes { get; set; }
        public int WordCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public UserResponse Author { get; set; } = null!;
        public CategoryResponse? Category { get; set; }
        public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? LastEditedAt { get; set; }
        public bool IsLiked { get; set; }
        public bool IsBookmarked { get; set; }
        public WritingMetadataResponse? Metadata { get; set; }
    }

    public class WritingListResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int ReadTimeMinutes { get; set; }
        public int WordCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public UserResponse Author { get; set; } = null!;
        public CategoryResponse? Category { get; set; }
        public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPromoted { get; set; }
    }

    public class CreateWritingRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Summary { get; set; }

        public string? CoverImageUrl { get; set; }

        public string Visibility { get; set; } = "Public";

        public string Type { get; set; } = "Article";

        public long? CategoryId { get; set; }

        public List<long> TagIds { get; set; } = new List<long>();

        // 元数据
        public string? Keywords { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoTitle { get; set; }
        public bool AllowComments { get; set; } = true;
        public bool AllowLikes { get; set; } = true;
        public bool AllowSharing { get; set; } = true;
    }

    public class UpdateWritingRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [StringLength(1000)]
        public string? Summary { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? Status { get; set; }

        public string? Visibility { get; set; }

        public string? Type { get; set; }

        public long? CategoryId { get; set; }

        public List<long>? TagIds { get; set; }
    }

    public class WritingMetadataResponse
    {
        public string? Keywords { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoTitle { get; set; }
        public string? CanonicalUrl { get; set; }
        public bool AllowComments { get; set; }
        public bool AllowLikes { get; set; }
        public bool AllowSharing { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPromoted { get; set; }
        public decimal? ReadingProgressScore { get; set; }
        public int? AverageReadingTime { get; set; }
    }

    public class WritingStatsResponse
    {
        public int TotalWritings { get; set; }
        public int PublishedWritings { get; set; }
        public int DraftWritings { get; set; }
        public int TotalWords { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalViews { get; set; }
        public Dictionary<string, int> WritingsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> WritingsByMonth { get; set; } = new Dictionary<string, int>();
    }
}