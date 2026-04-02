using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class Comment : BaseEntity
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        public CommentStatus Status { get; set; } = CommentStatus.Active;

        public int LikeCount { get; set; }

        public int ReplyCount { get; set; }

        public long AuthorId { get; set; }

        public long WritingId { get; set; }

        public long? ParentCommentId { get; set; }

        // 导航属性
        public virtual User Author { get; set; } = null!;
        public virtual Writing Writing { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }

    public enum CommentStatus
    {
        Active,
        Deleted,
        Hidden,
        Reported
    }

    public class Like : BaseEntity
    {
        public long UserId { get; set; }

        public long WritingId { get; set; }

        public LikeType Type { get; set; } = LikeType.Like;

        // 导航属性
        public virtual User User { get; set; } = null!;
        public virtual Writing Writing { get; set; } = null!;
    }

    public class CommentLike : BaseEntity
    {
        public long UserId { get; set; }

        public long CommentId { get; set; }

        // 导航属性
        public virtual User User { get; set; } = null!;
        public virtual Comment Comment { get; set; } = null!;
    }

    public enum LikeType
    {
        Like,
        Love,
        Celebrate,
        Insightful,
        Funny
    }

    public class UserFollow : BaseEntity
    {
        public long FollowerId { get; set; }

        public long FollowingId { get; set; }

        // 导航属性
        public virtual User Follower { get; set; } = null!;
        public virtual User Following { get; set; } = null!;
    }
}