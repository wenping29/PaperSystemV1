using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityService.DTOs
{
    public class PostResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public long AuthorId { get; set; }
        public long? WorkId { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Tags { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int CollectionCount { get; set; }
        public int ViewCount { get; set; }
        public decimal HotScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsDeleted { get; set; }
        // 作者信息（可能需要从用户服务获取）
        public string? AuthorName { get; set; }
        public string? AuthorAvatar { get; set; }
        // 用户交互状态
        public bool IsLiked { get; set; }
        public bool IsCollected { get; set; }
    }

    public class PostListResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public long AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatar { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Tags { get; set; }
        public string Category { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int CollectionCount { get; set; }
        public int ViewCount { get; set; }
        public decimal HotScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class CreatePostRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public long? WorkId { get; set; }

        [Url]
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [StringLength(1000)]
        public string? Tags { get; set; }

        public string Category { get; set; } = "Other";

        public string Visibility { get; set; } = "Public";
    }

    public class UpdatePostRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        public string? Summary { get; set; }

        [Url]
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [StringLength(1000)]
        public string? Tags { get; set; }

        public string? Category { get; set; }

        public string? Visibility { get; set; }

        public string? Status { get; set; }
    }

    public class PostQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Category { get; set; }
        public string? Tag { get; set; }
        public string? Keyword { get; set; }
        public string? SortBy { get; set; } // hot, latest, popular
        public long? AuthorId { get; set; }
        public string? Status { get; set; }
        public string? Visibility { get; set; }
    }
}