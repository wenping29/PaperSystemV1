using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperSystemApi.Models
{
    public class Template
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
        public string? Description { get; set; }

        public long AuthorId { get; set; }

        public long? CategoryId { get; set; }

        [StringLength(1000)] // 存储JSON数组
        public string? TagsJson { get; set; }

        public TemplateType Type { get; set; } = TemplateType.Custom;

        public bool IsPublic { get; set; } = false;

        public int UsageCount { get; set; } = 0;

        public int Likes { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual Category? Category { get; set; }

        // 计算属性：标签列表
        [NotMapped]
        public string[]? Tags
        {
            get => string.IsNullOrEmpty(TagsJson) ? null : System.Text.Json.JsonSerializer.Deserialize<string[]>(TagsJson);
            set => TagsJson = value == null ? null : System.Text.Json.JsonSerializer.Serialize(value);
        }
        public object Name { get; internal set; }

    }

    public enum TemplateType
    {
        Custom,
        System,
        Community
    }
}