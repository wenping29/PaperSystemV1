using System;
using System.ComponentModel.DataAnnotations;

namespace CommunityService.DTOs
{
    public class CollectionResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long PostId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // 帖子信息
        public string? PostTitle { get; set; }
        public string? PostSummary { get; set; }
        public string? PostCoverImageUrl { get; set; }
        public string? PostTags { get; set; }
        public string? PostCategory { get; set; }
        public long? PostAuthorId { get; set; }
        public string? PostAuthorName { get; set; }
    }

    public class CreateCollectionRequest
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long PostId { get; set; }

        [StringLength(200)]
        public string? Note { get; set; }
    }

    public class UpdateCollectionRequest
    {
        [StringLength(200)]
        public string? Note { get; set; }
    }

    public class CollectionQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public long? UserId { get; set; }
        public long? PostId { get; set; }
    }
}