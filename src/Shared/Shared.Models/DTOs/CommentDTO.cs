using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models.DTOs
{
    public class CommentResponse
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public UserResponse Author { get; set; } = null!;
        public long WritingId { get; set; }
        public long? ParentCommentId { get; set; }
        public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
        public bool IsLiked { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCommentRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public long WritingId { get; set; }

        public long? ParentCommentId { get; set; }
    }

    public class UpdateCommentRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class CommentListResponse
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public UserResponse Author { get; set; } = null!;
        public long WritingId { get; set; }
        public long? ParentCommentId { get; set; }
        public bool IsLiked { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LikeResponse
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public UserResponse User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLikeRequest
    {
        [Required]
        public long WritingId { get; set; }

        public string Type { get; set; } = "Like";
    }

    public class CreateCommentLikeRequest
    {
        [Required]
        public long CommentId { get; set; }
    }
}