using System;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.DTOs
{
    public class UserResponse
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public UserProfileResponse? Profile { get; set; }
    }

    public class UserProfileResponse
    {
        public string? Biography { get; set; }
        public int WritingCount { get; set; }
        public int LikeCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int TotalWords { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? TwitterUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? LinkedInUrl { get; set; }
    }

    public class CreateUserRequest
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
        public string Password { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }
    }

    public class UpdateUserRequest
    {
        [StringLength(50)]
        public string? Username { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

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
    }

    public class UpdateProfileRequest
    {
        [StringLength(1000)]
        public string? Biography { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(50)]
        public string? Gender { get; set; }

        [StringLength(100)]
        public string? TwitterUrl { get; set; }

        [StringLength(100)]
        public string? GitHubUrl { get; set; }

        [StringLength(100)]
        public string? LinkedInUrl { get; set; }
    }

    public class UserListResponse
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? role { get; set; }
        public int WritingCount { get; set; }
        public int FollowersCount { get; set; }
        public bool IsFollowing { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}