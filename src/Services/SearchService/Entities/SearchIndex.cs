using System;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Entities
{
    public class SearchIndex
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string IndexType { get; set; } = string.Empty; // writing, user, tag, comment, community

        [Required]
        public long EntityId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = IndexStatus.Indexed;

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public int Version { get; set; } = 1;

        public DateTime IndexedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedAt { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        public int AccessCount { get; set; } = 0;

        [StringLength(1000)]
        public string? MetadataJson { get; set; } // 索引元数据，如标题、内容摘要等

        [StringLength(500)]
        public string? Tags { get; set; } // 逗号分隔的标签

        public double RelevanceScore { get; set; } = 1.0; // 相关性评分
    }

    public static class IndexStatus
    {
        public const string Pending = "pending";
        public const string Indexed = "indexed";
        public const string Updating = "updating";
        public const string Error = "error";
        public const string Deleted = "deleted";
    }

    public static class IndexType
    {
        public const string Writing = "writing";
        public const string User = "user";
        public const string Tag = "tag";
        public const string Comment = "comment";
        public const string Community = "community";
    }
}