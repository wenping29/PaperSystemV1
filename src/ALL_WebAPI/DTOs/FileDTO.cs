using System;
using Microsoft.AspNetCore.Http;

namespace PaperSystemApi.DTOs
{
    public class FileDTO
    {
        public long Id { get; set; }
        public string FileId { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public string FileType { get; set; } = string.Empty;
        public long? UploadedByUserId { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? MetadataJson { get; set; }
        public string? FileHash { get; set; }
        public int AccessCount { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public string? Tags { get; set; }
        public string? DownloadUrl { get; set; }
        public string? PreviewUrl { get; set; }
    }

    public class UploadFileRequest
    {
        public IFormFile File { get; set; } = null!;
        public string? Description { get; set; }
        public string? FileType { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateFileRequest
    {
        public string? Description { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Status { get; set; }
    }

    public class FileQueryParams
    {
        public long? UploadedByUserId { get; set; }
        public string? FileType { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public FileDTO? File { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
    }

    public class FileDownloadInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public string? DownloadUrl { get; set; }
    }
}