using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WritingService.Entities
{
    public class WorkVersion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long WorkId { get; set; }

        [Required]
        public int VersionNumber { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Excerpt { get; set; }

        [StringLength(1000)]
        public string? ChangeDescription { get; set; }

        public int WordCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public long CreatedBy { get; set; }

        // 导航属性
        public virtual Work? Work { get; set; }
    }
}