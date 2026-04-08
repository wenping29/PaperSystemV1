using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.DTOs
{
    public class WorkResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public long AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string[]? Tags { get; set; }
        public int WordCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public string? Slug { get; set; }
    }

    public class WorkListResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public long AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int WordCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public bool IsPublished { get; set; }
    }

    public class CreateWorkRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [StringLength(500)]
        public string? Excerpt { get; set; }

        [Required]
        public long AuthorId { get; set; }

        public long? CategoryId { get; set; }

        public string[]? Tags { get; set; }
    }

    public class UpdateWorkRequest
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [StringLength(500)]
        public string? Excerpt { get; set; }

        public long? CategoryId { get; set; }

        public string[]? Tags { get; set; }

        public bool? IsPublished { get; set; }

        public string? Status { get; set; }
    }

    public class WorkContentResponse
    {
        public long WorkId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Format { get; set; } = "markdown";
        public int WordCount { get; set; }
        public int ReadingTimeMinutes { get; set; }
    }

    public class PublishWorkRequest
    {
        public bool Publish { get; set; } = true;
    }
}