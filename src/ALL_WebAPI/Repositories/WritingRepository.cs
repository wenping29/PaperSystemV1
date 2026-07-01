using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Interfaces;

namespace PaperSystemApi.Repositories
{
    public class WritingRepository : IWritingRepository
    {
        private readonly WritingDbContext _context;
        private readonly ILogger<WritingRepository> _logger;

        public WritingRepository(WritingDbContext context, ILogger<WritingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Work 方法

        public async Task<Work?> GetWorkByIdAsync(long id)
        {
            return await _context.Works
                .Include(w => w.Category)
                .FirstOrDefaultAsync(w => w.Id == id && w.Status != WorkStatus.Deleted);
        }

        public async Task<Work?> GetWorkBySlugAsync(string slug)
        {
            return await _context.Works
                .Include(w => w.Category)
                .FirstOrDefaultAsync(w => w.Slug == slug && w.Status != WorkStatus.Deleted);
        }

        public async Task<IEnumerable<Work>> GetWorksAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null)
        {
            var query = _context.Works
                .Include(w => w.Category)
                .Where(w => w.Status != WorkStatus.Deleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(w => w.Title.Contains(search) || w.Content.Contains(search));
            }

            if (authorId.HasValue)
            {
                query = query.Where(w => w.AuthorId == authorId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(w => w.CategoryId == categoryId.Value);
            }

            if (isPublished.HasValue)
            {
                query = query.Where(w => w.IsPublished == isPublished.Value);
            }

            query = query.OrderByDescending(w => w.CreatedAt);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Work> CreateWorkAsync(Work work)
        {
            work.CreatedAt = DateTime.UtcNow;
            work.UpdatedAt = DateTime.UtcNow;

            _context.Works.Add(work);
            await _context.SaveChangesAsync();
            return work;
        }

        public async Task<Work> UpdateWorkAsync(Work work)
        {
            work.UpdatedAt = DateTime.UtcNow;
            _context.Works.Update(work);
            await _context.SaveChangesAsync();
            return work;
        }

        public async Task<bool> DeleteWorkAsync(long id)
        {
            var work = await GetWorkByIdAsync(id);
            if (work == null) return false;

            work.Status = WorkStatus.Deleted;
            work.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetWorksCountAsync(string? search = null, long? authorId = null, long? categoryId = null, bool? isPublished = null)
        {
            var query = _context.Works.Where(w => w.Status != WorkStatus.Deleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(w => w.Title.Contains(search) || w.Content.Contains(search));
            }

            if (authorId.HasValue)
            {
                query = query.Where(w => w.AuthorId == authorId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(w => w.CategoryId == categoryId.Value);
            }

            if (isPublished.HasValue)
            {
                query = query.Where(w => w.IsPublished == isPublished.Value);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Work>> GetWorksByAuthorIdAsync(long authorId, int page, int pageSize, bool? isPublished = null)
        {
            var query = _context.Works
                .Include(w => w.Category)
                .Where(w => w.AuthorId == authorId && w.Status != WorkStatus.Deleted);

            if (isPublished.HasValue)
            {
                query = query.Where(w => w.IsPublished == isPublished.Value);
            }

            query = query.OrderByDescending(w => w.CreatedAt);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetWorksCountByAuthorIdAsync(long authorId, bool? isPublished = null)
        {
            var query = _context.Works.Where(w => w.AuthorId == authorId && w.Status != WorkStatus.Deleted);

            if (isPublished.HasValue)
            {
                query = query.Where(w => w.IsPublished == isPublished.Value);
            }

            return await query.CountAsync();
        }

        #endregion

        #region Category 方法

        public async Task<Category?> GetCategoryByIdAsync(long id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync(bool includeInactive = false)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(long id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category == null) return false;

            // 检查是否有子分类
            var hasSubCategories = await _context.Categories.AnyAsync(c => c.ParentCategoryId == id);
            if (hasSubCategories)
            {
                throw new InvalidOperationException("Cannot delete category with subcategories");
            }

            // 检查是否有作品使用此分类
            var hasWorks = await _context.Works.AnyAsync(w => w.CategoryId == id);
            if (hasWorks)
            {
                throw new InvalidOperationException("Cannot delete category that is being used by works");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Template 方法

        public async Task<Template?> GetTemplateByIdAsync(long id)
        {
            return await _context.Templates
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Template>> GetTemplatesAsync(int page, int pageSize, string? search = null, long? authorId = null, long? categoryId = null, bool? isPublic = null)
        {
            var query = _context.Templates
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));
            }

            if (authorId.HasValue)
            {
                query = query.Where(t => t.AuthorId == authorId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            if (isPublic.HasValue)
            {
                query = query.Where(t => t.IsPublic == isPublic.Value);
            }

            query = query.OrderByDescending(t => t.UsageCount)
                        .ThenByDescending(t => t.CreatedAt);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Template> CreateTemplateAsync(Template template)
        {
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<Template> UpdateTemplateAsync(Template template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            _context.Templates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteTemplateAsync(long id)
        {
            var template = await GetTemplateByIdAsync(id);
            if (template == null) return false;

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> IncrementTemplateUsageAsync(long templateId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null) return 0;

            template.UsageCount++;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return template.UsageCount;
        }

        #endregion

        #region WorkVersion 方法

        public async Task<WorkVersion?> GetWorkVersionAsync(long workId, int versionNumber)
        {
            return await _context.WorkVersions
                .FirstOrDefaultAsync(v => v.WorkId == workId && v.VersionNumber == versionNumber);
        }

        public async Task<IEnumerable<WorkVersion>> GetWorkVersionsAsync(long workId, int page, int pageSize)
        {
            return await _context.WorkVersions
                .Where(v => v.WorkId == workId)
                .OrderByDescending(v => v.VersionNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<WorkVersion> CreateWorkVersionAsync(WorkVersion version)
        {
            version.CreatedAt = DateTime.UtcNow;
            _context.WorkVersions.Add(version);
            await _context.SaveChangesAsync();
            return version;
        }

        public async Task<int> GetNextVersionNumberAsync(long workId)
        {
            var maxVersion = await _context.WorkVersions
                .Where(v => v.WorkId == workId)
                .MaxAsync(v => (int?)v.VersionNumber) ?? 0;
            return maxVersion + 1;
        }

        #endregion

        #region WorkCollaborator 方法

        public async Task<WorkCollaborator?> GetCollaboratorAsync(long workId, long userId)
        {
            return await _context.WorkCollaborators
                .FirstOrDefaultAsync(c => c.WorkId == workId && c.UserId == userId);
        }

        public async Task<IEnumerable<WorkCollaborator>> GetCollaboratorsAsync(long workId)
        {
            return await _context.WorkCollaborators
                .Where(c => c.WorkId == workId)
                .OrderByDescending(c => c.JoinedAt ?? c.InvitedAt)
                .ToListAsync();
        }

        public async Task<WorkCollaborator> AddCollaboratorAsync(WorkCollaborator collaborator)
        {
            collaborator.InvitedAt = DateTime.UtcNow;
            _context.WorkCollaborators.Add(collaborator);
            await _context.SaveChangesAsync();
            return collaborator;
        }

        public async Task<WorkCollaborator> UpdateCollaboratorAsync(WorkCollaborator collaborator)
        {
            _context.WorkCollaborators.Update(collaborator);
            await _context.SaveChangesAsync();
            return collaborator;
        }

        public async Task<bool> RemoveCollaboratorAsync(long workId, long userId)
        {
            var collaborator = await GetCollaboratorAsync(workId, userId);
            if (collaborator == null) return false;

            _context.WorkCollaborators.Remove(collaborator);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserCollaboratorAsync(long workId, long userId)
        {
            return await _context.WorkCollaborators
                .AnyAsync(c => c.WorkId == workId && c.UserId == userId && c.Status == CollaboratorStatus.Active);
        }

        #endregion
    }
}