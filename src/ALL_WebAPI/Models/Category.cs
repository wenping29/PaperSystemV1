using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperSystemApi.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category>? SubCategories { get; set; }
        public virtual ICollection<Work>? Works { get; set; }
    }
}