using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models.DTOs
{
    public class CategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; }
        public long? ParentCategoryId { get; set; }
        public CategoryResponse? ParentCategory { get; set; }
        public List<CategoryResponse> SubCategories { get; set; } = new List<CategoryResponse>();
        public int WritingCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCategoryRequest
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
    }

    public class UpdateCategoryRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? IconUrl { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public int? DisplayOrder { get; set; }

        public long? ParentCategoryId { get; set; }

        public bool? IsActive { get; set; }
    }

    public class TagResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int UsageCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTagRequest
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }
    }

    public class UpdateTagRequest
    {
        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public bool? IsFeatured { get; set; }
    }

    public class PopularTagResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int UsageCount { get; set; }
        public int RecentUsageCount { get; set; }
    }
}