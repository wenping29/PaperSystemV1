using System;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.File.Entities
{
    public class FileMetadata
    {
        public long Id { get; set; }

        [Required]
        [StringLength(36)]
        public string FileId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string StoragePath { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty;

        public long SizeInBytes { get; set; }

        [Required]
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty; // 例如: image, document, video, audio

        public long? UploadedByUserId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = FileStatus.Uploaded;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        [StringLength(1000)]
        public string? MetadataJson { get; set; } // 额外的元数据，如尺寸、时长等

        [StringLength(64)]
        public string? FileHash { get; set; } // 文件哈希值，用于去重和验证

        public int AccessCount { get; set; } = 0; // 文件访问次数

        public DateTime? LastAccessedAt { get; set; } // 最后访问时间

        public string? Tags { get; set; } // 文件标签，逗号分隔
    }

    public static class FileStatus
    {
        public const string Uploaded = "uploaded";
        public const string Processing = "processing";
        public const string Ready = "ready";
        public const string Failed = "failed";
        public const string Deleted = "deleted";
    }

    public static class FileType
    {
        public const string Image = "image";
        public const string Document = "document";
        public const string Video = "video";
        public const string Audio = "audio";
        public const string Archive = "archive";
        public const string Other = "other";
    }
}