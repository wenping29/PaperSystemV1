using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? IconUrl { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public int DisplayOrder { get; set; }

        public long? ParentCategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        // 导航属性
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Writing> Writings { get; set; } = new List<Writing>();
    }

    public class Tag : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public int UsageCount { get; set; }

        public bool IsFeatured { get; set; } = false;

        // 导航属性
        public virtual ICollection<WritingTag> WritingTags { get; set; } = new List<WritingTag>();
    }

    public class WritingTag : BaseEntity
    {
        public long WritingId { get; set; }

        public long TagId { get; set; }

        // 导航属性
        public virtual Writing Writing { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}