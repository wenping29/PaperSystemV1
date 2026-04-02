using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityService.Entities
{
    public class Like
    {
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public LikeTargetType TargetType { get; set; }

        [Required]
        public long TargetId { get; set; } // PostId 或 CommentId

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 复合唯一键：UserId, TargetType, TargetId 在DbContext中配置
    }

    public enum LikeTargetType
    {
        Post,
        Comment
    }
}