using System;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Entities
{
    public class SearchHistory
    {
        public long Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Query { get; set; } = string.Empty;

        [StringLength(50)]
        public string? SearchType { get; set; } // 例如: writing, user, tag, all

        public long? UserId { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? DeviceType { get; set; }

        public int ResultCount { get; set; }

        public bool IsSuccessful { get; set; } = true;

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TimeSpan ResponseTime { get; set; }

        [StringLength(1000)]
        public string? MetadataJson { get; set; } // 额外的元数据，如过滤器、排序等
    }

    public static class SearchType
    {
        public const string Writing = "writing";
        public const string User = "user";
        public const string Tag = "tag";
        public const string All = "all";
        public const string Comment = "comment";
        public const string Community = "community";
    }
}