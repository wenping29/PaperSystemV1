using System.ComponentModel.DataAnnotations;

namespace WritingService.DTOs
{
    public class CategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string? Slug { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int WorksCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CategoryListResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int WorksCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCategoryRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public long? ParentCategoryId { get; set; }

        [StringLength(200)]
        public string? Slug { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;
    }

    public class UpdateCategoryRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public long? ParentCategoryId { get; set; }

        [StringLength(200)]
        public string? Slug { get; set; }

        public bool? IsActive { get; set; }

        public int? SortOrder { get; set; }
    }
}