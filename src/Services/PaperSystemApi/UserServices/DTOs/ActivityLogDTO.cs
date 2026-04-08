using System;
using System.ComponentModel.DataAnnotations;
using PaperSystemApi.UserServices.Entities;

namespace PaperSystemApi.UserServices.DTOs
{
    public class CreateActivityLogRequest
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public ActivityType ActivityType { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Details { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(50)]
        public string? ResourceType { get; set; }

        public long? ResourceId { get; set; }
    }

    public class ActivityLogResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ResourceType { get; set; }
        public long? ResourceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserListResponse? User { get; set; }
    }

    public class ActivityLogSearchRequest
    {
        public long? UserId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ActivityLogSearchResponse
    {
        public IEnumerable<ActivityLogResponse> Logs { get; set; } = new List<ActivityLogResponse>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}