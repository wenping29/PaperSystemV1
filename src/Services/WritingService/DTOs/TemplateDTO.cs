using System.ComponentModel.DataAnnotations;

namespace WritingService.DTOs
{
    public class TemplateResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string[]? Tags { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int UsageCount { get; set; }
        public int Likes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TemplateListResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int UsageCount { get; set; }
        public int Likes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTemplateRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public long AuthorId { get; set; }

        public long? CategoryId { get; set; }

        public string[]? Tags { get; set; }

        public string Type { get; set; } = "Custom";

        public bool IsPublic { get; set; } = false;
    }

    public class UpdateTemplateRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public long? CategoryId { get; set; }

        public string[]? Tags { get; set; }

        public string? Type { get; set; }

        public bool? IsPublic { get; set; }
    }

    public class UseTemplateRequest
    {
        public long TemplateId { get; set; }
        public string? CustomContent { get; set; }
        public object? Title { get; internal set; }

    }
}