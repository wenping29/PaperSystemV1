using System.ComponentModel.DataAnnotations;

namespace SearchService.Entities
{
    public class PopularSearchTerm
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Term { get; set; } = string.Empty;

        public int SearchCount { get; set; }

        public DateTime FirstSearchedAt { get; set; }

        public DateTime LastSearchedAt { get; set; }

        public DateTime LastSearched { get; set; }

        public bool IsTrending { get; set; }

        public string? Category { get; set; }

        public int Rank { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}