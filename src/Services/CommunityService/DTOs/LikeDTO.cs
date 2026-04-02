using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityService.DTOs
{
    public class LikeResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string TargetType { get; set; } = string.Empty; // Post, Comment
        public long TargetId { get; set; }
        public DateTime CreatedAt { get; set; }
        // 目标信息
        public string? TargetTitle { get; set; }
        public string? TargetContent { get; set; }
    }

    public class CreateLikeRequest
    {
        [Required]
        public string TargetType { get; set; } = string.Empty; // Post, Comment

        [Required]
        [Range(1, long.MaxValue)]
        public long TargetId { get; set; }
    }

    public class DeleteLikeRequest
    {
        [Required]
        public string TargetType { get; set; } = string.Empty;

        [Required]
        public long TargetId { get; set; }
    }

    public class LikeQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public long? UserId { get; set; }
        public string? TargetType { get; set; }
        public long? TargetId { get; set; }
    }
}