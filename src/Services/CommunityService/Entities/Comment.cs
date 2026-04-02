using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityService.Entities
{
    public class Comment
    {
        public long Id { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public long PostId { get; set; }

        [Required]
        public long AuthorId { get; set; }

        // 父评论ID（用于嵌套回复）
        public long? ParentId { get; set; }

        public int LikeCount { get; set; } = 0;

        public CommentStatus Status { get; set; } = CommentStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // 导航属性
        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        [ForeignKey("ParentId")]
        public virtual Comment? Parent { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

    }

    public enum CommentStatus
    {
        Active,
        Hidden,
        Deleted,
        Reported
    }
}