using System;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.UserServices.Entities
{
    public class UserProfile
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        [StringLength(1000)]
        public string? Biography { get; set; }

        public int WritingCount { get; set; }

        public int LikeCount { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }

        public int TotalWords { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(50)]
        public string? Gender { get; set; }

        // 社交链接
        [StringLength(100)]
        public string? TwitterUrl { get; set; }

        [StringLength(100)]
        public string? GitHubUrl { get; set; }

        [StringLength(100)]
        public string? LinkedInUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual User User { get; set; } = null!;
    }
}