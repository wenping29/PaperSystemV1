using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WritingService.DTOs;
using WritingService.Entities;
using WritingService.Interfaces;

namespace WritingService.Services
{
    public class WritingServiceS : IWritingServiceS
    {
        private readonly IWritingRepository _writingRepository;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<WritingServiceS> _logger;

        public WritingServiceS(
            IWritingRepository writingRepository,
            IMapper mapper,
            IDistributedCache cache,
            ILogger<WritingServiceS> logger)
        {
            _writingRepository = writingRepository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        #region Work 方法

        public async Task<WorkResponse?> GetWorkByIdAsync(long id)
        {
            var work = await _writingRepository.GetWorkByIdAsync(id);
            if (work == null) return null;

            var response = _mapper.Map<WorkResponse>(work);
            // TODO: 从用户服务获取作者信息
            response.AuthorName = $"User{work.AuthorId}";
            if (work.Category != null)
            {
                response.CategoryName = work.Category.Name;
            }

            return response;
        }

        public async Task<WorkResponse?> GetWorkBySlugAsync(string slug)
        {
            var work = await _writingRepository.GetWorkBySlugAsync(slug);
            if (work == null) return null;

            var response = _mapper.Map<WorkResponse>(work);
            // TODO: 从用户服务获取作者信息
            response.AuthorName = $"User{work.AuthorId}";
            if (work.Category != null)
            {
                response.CategoryName = work.Category.Name;
            }

            return response;
        }

        public async Task<IEnumerable<WorkListResponse>> GetWorksAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null)
        {
            var works = await _writingRepository.GetWorksAsync(page, pageSize, search, authorId, categoryId, isPublished);
            var responses = new List<WorkListResponse>();

            foreach (var work in works)
            {
                var response = _mapper.Map<WorkListResponse>(work);
                // TODO: 从用户服务获取作者信息
                response.AuthorName = $"User{work.AuthorId}";
                if (work.Category != null)
                {
                    response.CategoryName = work.Category.Name;
                }
                responses.Add(response);
            }

            return responses;
        }

        public async Task<WorkResponse> CreateWorkAsync(CreateWorkRequest request)
        {
            var work = _mapper.Map<Work>(request);
            work.CreatedAt = DateTime.UtcNow;
            work.UpdatedAt = DateTime.UtcNow;
            work.Status = WorkStatus.Draft;
            work.IsPublished = false;
            work.WordCount = work.Content?.Length ?? 0;

            var createdWork = await _writingRepository.CreateWorkAsync(work);
            return _mapper.Map<WorkResponse>(createdWork);
        }

        public async Task<WorkResponse?> UpdateWorkAsync(long id, UpdateWorkRequest request)
        {
            var work = await _writingRepository.GetWorkByIdAsync(id);
            if (work == null) return null;

            _mapper.Map(request, work);
            work.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.Content))
            {
                work.WordCount = request.Content.Length;
            }

            var updatedWork = await _writingRepository.UpdateWorkAsync(work);
            return _mapper.Map<WorkResponse>(updatedWork);
        }

        public async Task<bool> DeleteWorkAsync(long id)
        {
            return await _writingRepository.DeleteWorkAsync(id);
        }

        public async Task<bool> PublishWorkAsync(long id, bool publish = true)
        {
            var work = await _writingRepository.GetWorkByIdAsync(id);
            if (work == null) return false;

            work.IsPublished = publish;
            work.Status = publish ? WorkStatus.Published : WorkStatus.Draft;
            if (publish && work.PublishedAt == null)
            {
                work.PublishedAt = DateTime.UtcNow;
            }
            work.UpdatedAt = DateTime.UtcNow;

            await _writingRepository.UpdateWorkAsync(work);
            return true;
        }

        public async Task<bool> LikeWorkAsync(long id, long userId)
        {
            var work = await _writingRepository.GetWorkByIdAsync(id);
            if (work == null) return false;

            // TODO: 实现点赞逻辑，需要检查用户是否已经点赞过
            work.Likes++;
            work.UpdatedAt = DateTime.UtcNow;

            await _writingRepository.UpdateWorkAsync(work);
            return true;
        }

        public async Task<WorkContentResponse> GetWorkContentAsync(long id)
        {
            var work = await _writingRepository.GetWorkByIdAsync(id);
            if (work == null) return null;

            return new WorkContentResponse
            {
                WorkId = work.Id,
                Content = work.Content,
                WordCount = work.WordCount,
                ReadingTimeMinutes = (int)Math.Ceiling(work.WordCount / 200.0) // 假设每分钟阅读200字
            };
        }

        public async Task<IEnumerable<WorkListResponse>> GetWorksByAuthorIdAsync(long authorId, int page, int pageSize, bool? isPublished = null)
        {
            var works = await _writingRepository.GetWorksAsync(page, pageSize, null, authorId, null, isPublished);
            return _mapper.Map<IEnumerable<WorkListResponse>>(works);
        }

        public async Task<int> GetWorksCountAsync(string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null)
        {
            return await _writingRepository.GetWorksCountAsync(search, authorId, categoryId, isPublished);
        }

        public async Task<int> GetWorksCountByAuthorIdAsync(long authorId, bool? isPublished = null)
        {
            return await _writingRepository.GetWorksCountAsync(null, authorId, null, isPublished);
        }

        #endregion

        #region Category 方法

        public async Task<CategoryResponse?> GetCategoryByIdAsync(long id)
        {
            var category = await _writingRepository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse?> GetCategoryBySlugAsync(string slug)
        {
            var category = await _writingRepository.GetCategoryBySlugAsync(slug);
            if (category == null) return null;

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<IEnumerable<CategoryListResponse>> GetCategoriesAsync(bool includeInactive = false)
        {
            var categories = await _writingRepository.GetCategoriesAsync(includeInactive);
            return _mapper.Map<IEnumerable<CategoryListResponse>>(categories);
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var category = _mapper.Map<Category>(request);
            var createdCategory = await _writingRepository.CreateCategoryAsync(category);
            return _mapper.Map<CategoryResponse>(createdCategory);
        }

        public async Task<CategoryResponse?> UpdateCategoryAsync(long id, UpdateCategoryRequest request)
        {
            var category = await _writingRepository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            _mapper.Map(request, category);
            var updatedCategory = await _writingRepository.UpdateCategoryAsync(category);
            return _mapper.Map<CategoryResponse>(updatedCategory);
        }

        public async Task<bool> DeleteCategoryAsync(long id)
        {
            return await _writingRepository.DeleteCategoryAsync(id);
        }

        #endregion

        #region Template 方法

        public async Task<TemplateResponse?> GetTemplateByIdAsync(long id)
        {
            var template = await _writingRepository.GetTemplateByIdAsync(id);
            if (template == null) return null;

            return _mapper.Map<TemplateResponse>(template);
        }

        public async Task<IEnumerable<TemplateListResponse>> GetTemplatesAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublic = null)
        {
            var templates = await _writingRepository.GetTemplatesAsync(page, pageSize, search, authorId, categoryId, isPublic);
            return _mapper.Map<IEnumerable<TemplateListResponse>>(templates);
        }

        public async Task<TemplateResponse> CreateTemplateAsync(CreateTemplateRequest request)
        {
            var template = _mapper.Map<Template>(request);
            var createdTemplate = await _writingRepository.CreateTemplateAsync(template);
            return _mapper.Map<TemplateResponse>(createdTemplate);
        }

        public async Task<TemplateResponse?> UpdateTemplateAsync(long id, UpdateTemplateRequest request)
        {
            var template = await _writingRepository.GetTemplateByIdAsync(id);
            if (template == null) return null;

            _mapper.Map(request, template);
            var updatedTemplate = await _writingRepository.UpdateTemplateAsync(template);
            return _mapper.Map<TemplateResponse>(updatedTemplate);
        }

        public async Task<bool> DeleteTemplateAsync(long id)
        {
            return await _writingRepository.DeleteTemplateAsync(id);
        }

        public async Task<TemplateResponse> UseTemplateAsync(UseTemplateRequest request, long userId)
        {
            var template = await _writingRepository.GetTemplateByIdAsync(request.TemplateId);
            if (template == null) throw new ArgumentException("Template not found");

            // 创建基于模板的新作品
            var work = new Work
            {
                Title = (string)(request.Title ?? template.Name),
                Content = template.Content,
                AuthorId = userId,
                CategoryId = template.CategoryId,
                Status = WorkStatus.Draft,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                WordCount = template.Content.Length
            };

            var createdWork = await _writingRepository.CreateWorkAsync(work);

            // 返回模板响应，而不是作品响应
            return _mapper.Map<TemplateResponse>(template);
        }

        #endregion

        #region 版本控制

        public async Task<WorkVersionResponse?> GetWorkVersionAsync(long workId, int versionNumber)
        {
            var version = await _writingRepository.GetWorkVersionAsync(workId, versionNumber);
            if (version == null) return null;

            return _mapper.Map<WorkVersionResponse>(version);
        }

        public async Task<IEnumerable<WorkVersionResponse>> GetWorkVersionsAsync(long workId, int page, int pageSize)
        {
            var versions = await _writingRepository.GetWorkVersionsAsync(workId, page, pageSize);
            return _mapper.Map<IEnumerable<WorkVersionResponse>>(versions);
        }

        public async Task<WorkVersionResponse> CreateWorkVersionAsync(long workId, CreateWorkVersionRequest request, long userId)
        {
            var work = await _writingRepository.GetWorkByIdAsync(workId);
            if (work == null) throw new ArgumentException("Work not found");

            // 获取下一个版本号
            var versionNumber = await _writingRepository.GetNextVersionNumberAsync(workId);

            var version = new WorkVersion
            {
                WorkId = workId,
                VersionNumber = versionNumber,
                Content = request.Content ?? work.Content,
                Title = work.Title,
                Excerpt = work.Excerpt,
                ChangeDescription = request.ChangeDescription,
                WordCount = (request.Content ?? work.Content).Length,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            var createdVersion = await _writingRepository.CreateWorkVersionAsync(version);
            return _mapper.Map<WorkVersionResponse>(createdVersion);
        }

        #endregion

        #region 协作

        public async Task<CollaboratorResponse?> GetCollaboratorAsync(long workId, long userId)
        {
            var collaborator = await _writingRepository.GetCollaboratorAsync(workId, userId);
            if (collaborator == null) return null;

            return _mapper.Map<CollaboratorResponse>(collaborator);
        }

        public async Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(long workId)
        {
            var collaborators = await _writingRepository.GetCollaboratorsAsync(workId);
            return _mapper.Map<IEnumerable<CollaboratorResponse>>(collaborators);
        }

        public async Task<CollaboratorResponse> AddCollaboratorAsync(long workId, AddCollaboratorRequest request)
        {
            var work = await _writingRepository.GetWorkByIdAsync(workId);
            if (work == null) throw new ArgumentException("Work not found");

            var collaborator = new WorkCollaborator
            {
                WorkId = workId,
                UserId = request.UserId,
                Role = request.Role,
                Status =CollaboratorStatus.Pending,
                InvitedAt = DateTime.UtcNow
            };

            var createdCollaborator = await _writingRepository.AddCollaboratorAsync(collaborator);
            return _mapper.Map<CollaboratorResponse>(createdCollaborator);
        }

        public async Task<CollaboratorResponse?> UpdateCollaboratorAsync(long workId, long userId, UpdateCollaboratorRequest request)
        {
            var collaborator = await _writingRepository.GetCollaboratorAsync(workId, userId);
            if (collaborator == null) return null;

            _mapper.Map(request, collaborator);
            var updatedCollaborator = await _writingRepository.UpdateCollaboratorAsync(collaborator);
            return _mapper.Map<CollaboratorResponse>(updatedCollaborator);
        }

        public async Task<bool> RemoveCollaboratorAsync(long workId, long userId)
        {
            return await _writingRepository.RemoveCollaboratorAsync(workId, userId);
        }

        public async Task<bool> IsUserCollaboratorAsync(long workId, long userId)
        {
            var collaborator = await _writingRepository.GetCollaboratorAsync(workId, userId);
            return collaborator != null;
        }

        #endregion
    }
}