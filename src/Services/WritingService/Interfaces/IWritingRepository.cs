using WritingService.Entities;

namespace WritingService.Interfaces
{
    public interface IWritingRepository
    {
        // Work 相关方法
        Task<Work?> GetWorkByIdAsync(long id);
        Task<Work?> GetWorkBySlugAsync(string slug);
        Task<IEnumerable<Work>> GetWorksAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null);
        Task<Work> CreateWorkAsync(Work work);
        Task<Work> UpdateWorkAsync(Work work);
        Task<bool> DeleteWorkAsync(long id);
        Task<int> GetWorksCountAsync(string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null);

        // 作者相关
        Task<IEnumerable<Work>> GetWorksByAuthorIdAsync(long authorId, int page, int pageSize, bool? isPublished = null);
        Task<int> GetWorksCountByAuthorIdAsync(long authorId, bool? isPublished = null);

        // 分类相关
        Task<Category?> GetCategoryByIdAsync(long id);
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetCategoriesAsync(bool includeInactive = false);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(long id);

        // 模板相关
        Task<Template?> GetTemplateByIdAsync(long id);
        Task<IEnumerable<Template>> GetTemplatesAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublic = null);
        Task<Template> CreateTemplateAsync(Template template);
        Task<Template> UpdateTemplateAsync(Template template);
        Task<bool> DeleteTemplateAsync(long id);
        Task<int> IncrementTemplateUsageAsync(long templateId);

        // 版本控制
        Task<WorkVersion?> GetWorkVersionAsync(long workId, int versionNumber);
        Task<IEnumerable<WorkVersion>> GetWorkVersionsAsync(long workId, int page, int pageSize);
        Task<WorkVersion> CreateWorkVersionAsync(WorkVersion version);
        Task<int> GetNextVersionNumberAsync(long workId);

        // 协作
        Task<WorkCollaborator?> GetCollaboratorAsync(long workId, long userId);
        Task<IEnumerable<WorkCollaborator>> GetCollaboratorsAsync(long workId);
        Task<WorkCollaborator> AddCollaboratorAsync(WorkCollaborator collaborator);
        Task<WorkCollaborator> UpdateCollaboratorAsync(WorkCollaborator collaborator);
        Task<bool> RemoveCollaboratorAsync(long workId, long userId);
        Task<bool> IsUserCollaboratorAsync(long workId, long userId);
    }
}