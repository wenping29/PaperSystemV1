using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityService.DTOs
{
    public class CommentResponse
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public long PostId { get; set; }
        public long AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatar { get; set; }
        public long? ParentId { get; set; }
        public int LikeCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLiked { get; set; }
        public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
    }

    public class CreateCommentRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public long PostId { get; set; }

        public long? ParentId { get; set; }
    }

    public class UpdateCommentRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class CommentQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public long? PostId { get; set; }
        public long? AuthorId { get; set; }
        public long? ParentId { get; set; } // null表示只获取顶级评论
        public string? SortBy { get; set; } // latest, popular
    }
}