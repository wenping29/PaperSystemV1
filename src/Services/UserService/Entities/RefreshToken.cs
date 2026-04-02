using System;
using System.ComponentModel.DataAnnotations;

namespace UserService.Entities
{
    public class RefreshToken
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        [StringLength(50)]
        public string? RevokedByIp { get; set; }

        [StringLength(200)]
        public string? ReplacedByToken { get; set; }

        // 导航属性
        public virtual User User { get; set; } = null!;
    }
}