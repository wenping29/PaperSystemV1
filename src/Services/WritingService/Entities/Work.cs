using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WritingService.Entities
{
    public class Work
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Excerpt { get; set; }

        [Required]
        public long AuthorId { get; set; }

        public long? CategoryId { get; set; }

        [StringLength(1000)] // 存储JSON数组
        public string? TagsJson { get; set; }

        public int WordCount { get; set; }

        public WorkStatus Status { get; set; } = WorkStatus.Draft;

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedAt { get; set; }

        public int Views { get; set; } = 0;

        public int Likes { get; set; } = 0;

        [StringLength(200)]
        public string? Slug { get; set; }

        // 导航属性
        public virtual Category? Category { get; set; }
        public virtual ICollection<WorkVersion>? Versions { get; set; }
        public virtual ICollection<WorkCollaborator>? Collaborators { get; set; }

        // 计算属性：标签列表
        [NotMapped]
        public string[]? Tags
        {
            get => string.IsNullOrEmpty(TagsJson) ? null : System.Text.Json.JsonSerializer.Deserialize<string[]>(TagsJson);
            set => TagsJson = value == null ? null : System.Text.Json.JsonSerializer.Serialize(value);
        }
    }

    public enum WorkStatus
    {
        Draft,
        Published,
        Private,
        Archived,
        Deleted
    }
}