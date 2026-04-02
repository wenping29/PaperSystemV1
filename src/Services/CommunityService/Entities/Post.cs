using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityService.Entities
{
    public class Post
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Summary { get; set; }

        [Required]
        public long AuthorId { get; set; }

        // 关联写作服务的作品ID（可选）
        public long? WorkId { get; set; }

        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [StringLength(1000)]
        public string? Tags { get; set; } // 逗号分隔的标签

        public PostCategory Category { get; set; } = PostCategory.Other;

        public PostStatus Status { get; set; } = PostStatus.Pending;

        public PostVisibility Visibility { get; set; } = PostVisibility.Public;

        public int LikeCount { get; set; } = 0;

        public int CommentCount { get; set; } = 0;

        public int CollectionCount { get; set; } = 0;

        public int ViewCount { get; set; } = 0;

        public decimal HotScore { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // 导航属性
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();
    }

    public enum PostCategory
    {
        Fiction,
        NonFiction,
        Poetry,
        Drama,
        Essay,
        Blog,
        Tutorial,
        Other
    }

    public enum PostStatus
    {
        Draft,
        Pending,
        Published,
        Rejected,
        Archived
    }

    public enum PostVisibility
    {
        Public,
        Private,
        FriendsOnly,
        SubscribersOnly
    }
}