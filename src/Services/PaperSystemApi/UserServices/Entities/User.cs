using System;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.UserServices.Entities
{
    public class User
    {
        public long Id { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // 导航属性
        public virtual UserProfile? Profile { get; set; }
        public virtual ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
        public virtual ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
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
}