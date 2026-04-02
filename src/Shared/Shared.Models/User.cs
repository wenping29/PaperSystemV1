using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(200)]
        public string? AvatarUrl { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime? LastLoginAt { get; set; }

        // 导航属性
        public virtual ICollection<Writing> Writings { get; set; } = new List<Writing>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
        public virtual ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
        public virtual UserProfile? Profile { get; set; }
    }

    public enum UserRole
    {
        User,
        Author,
        Editor,
        Admin,
        SuperAdmin
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        Banned
    }

    public class UserProfile : BaseEntity
    {
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

        // 导航属性
        public virtual User User { get; set; } = null!;
    }
}