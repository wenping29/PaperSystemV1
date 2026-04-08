using PaperSystemApi.DTOs;

namespace PaperSystemApi.Services
{
    public interface IWritingServiceS
    {
        // Work 相关方法
        Task<WorkResponse?> GetWorkByIdAsync(long id);
        Task<WorkResponse?> GetWorkBySlugAsync(string slug);
        Task<IEnumerable<WorkListResponse>> GetWorksAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null);
        Task<WorkResponse> CreateWorkAsync(CreateWorkRequest request);
        Task<WorkResponse?> UpdateWorkAsync(long id, UpdateWorkRequest request);
        Task<bool> DeleteWorkAsync(long id);
        Task<bool> PublishWorkAsync(long id, bool publish = true);
        Task<bool> LikeWorkAsync(long id, long userId);
        Task<WorkContentResponse> GetWorkContentAsync(long id);
        Task<IEnumerable<WorkListResponse>> GetWorksByAuthorIdAsync(long authorId, int page, int pageSize, bool? isPublished = null);
        Task<int> GetWorksCountAsync(string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null);
        Task<int> GetWorksCountByAuthorIdAsync(long authorId, bool? isPublished = null);

        // Category 相关方法
        Task<CategoryResponse?> GetCategoryByIdAsync(long id);
        Task<CategoryResponse?> GetCategoryBySlugAsync(string slug);
        Task<IEnumerable<CategoryListResponse>> GetCategoriesAsync(bool includeInactive = false);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryResponse?> UpdateCategoryAsync(long id, UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(long id);

        // Template 相关方法
        Task<TemplateResponse?> GetTemplateByIdAsync(long id);
        Task<IEnumerable<TemplateListResponse>> GetTemplatesAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublic = null);
        Task<TemplateResponse> CreateTemplateAsync(CreateTemplateRequest request);
        Task<TemplateResponse?> UpdateTemplateAsync(long id, UpdateTemplateRequest request);
        Task<bool> DeleteTemplateAsync(long id);
        Task<TemplateResponse> UseTemplateAsync(UseTemplateRequest request, long userId);

        // 版本控制
        Task<WorkVersionResponse?> GetWorkVersionAsync(long workId, int versionNumber);
        Task<IEnumerable<WorkVersionResponse>> GetWorkVersionsAsync(long workId, int page, int pageSize);
        Task<WorkVersionResponse> CreateWorkVersionAsync(long workId, CreateWorkVersionRequest request, long userId);

        // 协作
        Task<CollaboratorResponse?> GetCollaboratorAsync(long workId, long userId);
        Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(long workId);
        Task<CollaboratorResponse> AddCollaboratorAsync(long workId, AddCollaboratorRequest request);
        Task<CollaboratorResponse?> UpdateCollaboratorAsync(long workId, long userId, UpdateCollaboratorRequest request);
        Task<bool> RemoveCollaboratorAsync(long workId, long userId);
        Task<bool> IsUserCollaboratorAsync(long workId, long userId);
    }

    // 版本控制DTO
    public class WorkVersionResponse
    {
        public long Id { get; set; }
        public long WorkId { get; set; }
        public int VersionNumber { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Excerpt { get; set; }
        public string? ChangeDescription { get; set; }
        public int WordCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
    }

    public class CreateWorkVersionRequest
    {
        public string? Content { get; set; }
        public string? ChangeDescription { get; set; }
    }

    // 协作DTO
    public class CollaboratorResponse
    {
        public long Id { get; set; }
        public long WorkId { get; set; }
        public long UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime InvitedAt { get; set; }
        public DateTime? JoinedAt { get; set; }
    }

    public class AddCollaboratorRequest
    {
        public long UserId { get; set; }
        public string Role { get; set; } = "Collaborator";
    }

    public class UpdateCollaboratorRequest
    {
        public string? Role { get; set; }
        public string? Status { get; set; }
    }
}