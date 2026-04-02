using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityService.Entities
{
    public class Collection
    {
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public long PostId { get; set; }

        [StringLength(200)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // 导航属性
        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }
    }
}