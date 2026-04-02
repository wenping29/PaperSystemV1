using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class Writing : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Summary { get; set; }

        public string? CoverImageUrl { get; set; }

        public WritingStatus Status { get; set; } = WritingStatus.Draft;

        public WritingVisibility Visibility { get; set; } = WritingVisibility.Public;

        public WritingType Type { get; set; } = WritingType.Article;

        public int ReadTimeMinutes { get; set; }

        public int WordCount { get; set; }

        public int LikeCount { get; set; }

        public int CommentCount { get; set; }

        public int ViewCount { get; set; }

        public long AuthorId { get; set; }

        public long? CategoryId { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime? LastEditedAt { get; set; }

        // 导航属性
        public virtual User Author { get; set; } = null!;
        public virtual Category? Category { get; set; }
        public virtual ICollection<WritingTag> WritingTags { get; set; } = new List<WritingTag>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual WritingMetadata? Metadata { get; set; }
    }

    public enum WritingStatus
    {
        Draft,
        Published,
        Archived,
        Deleted
    }

    public enum WritingVisibility
    {
        Public,
        Private,
        Unlisted,
        Protected
    }

    public enum WritingType
    {
        Article,
        Story,
        Poetry,
        Essay,
        Report,
        Blog,
        Novel,
        ShortStory
    }

    public class WritingMetadata : BaseEntity
    {
        public long WritingId { get; set; }

        public string? Keywords { get; set; }

        public string? SeoDescription { get; set; }

        public string? SeoTitle { get; set; }

        public string? CanonicalUrl { get; set; }

        public bool AllowComments { get; set; } = true;

        public bool AllowLikes { get; set; } = true;

        public bool AllowSharing { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public bool IsPromoted { get; set; } = false;

        public decimal? ReadingProgressScore { get; set; }

        public int? AverageReadingTime { get; set; }

        // 导航属性
        public virtual Writing Writing { get; set; } = null!;
    }
}