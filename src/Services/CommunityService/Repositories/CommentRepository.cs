using Microsoft.EntityFrameworkCore;
using CommunityService.Data;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CommunityDbContext _context;

        public CommentRepository(CommunityDbContext context)
        {
            _context = context;
        }

        public async Task<Comment?> GetByIdAsync(long id)
        {
            return await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<IEnumerable<Comment>> GetAllAsync(int page, int pageSize, long? postId, long? authorId, long? parentId, string? sortBy)
        {
            var query = _context.Comments.AsQueryable();

            query = query.Where(c => !c.IsDeleted);

            if (postId.HasValue)
            {
                query = query.Where(c => c.PostId == postId.Value);
            }

            if (authorId.HasValue)
            {
                query = query.Where(c => c.AuthorId == authorId.Value);
            }

            if (parentId.HasValue)
            {
                query = query.Where(c => c.ParentId == parentId.Value);
            }
            else
            {
                // 默认只获取顶级评论（ParentId为null）
                query = query.Where(c => c.ParentId == null);
            }

            query = sortBy?.ToLower() switch
            {
                "popular" => query.OrderByDescending(c => c.LikeCount),
                "latest" => query.OrderByDescending(c => c.CreatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            return await query
                .Include(c => c.Replies)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var comment = await GetByIdAsync(id);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(long id)
        {
            var comment = await GetByIdAsync(id);
            if (comment == null) return false;

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Comments.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<int> GetCountAsync(long? postId, long? authorId, long? parentId)
        {
            var query = _context.Comments.AsQueryable();

            query = query.Where(c => !c.IsDeleted);

            if (postId.HasValue)
            {
                query = query.Where(c => c.PostId == postId.Value);
            }

            if (authorId.HasValue)
            {
                query = query.Where(c => c.AuthorId == authorId.Value);
            }

            if (parentId.HasValue)
            {
                query = query.Where(c => c.ParentId == parentId.Value);
            }
            else
            {
                query = query.Where(c => c.ParentId == null);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(long postId, int page, int pageSize, bool includeReplies)
        {
            var query = _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted && c.ParentId == null);

            if (includeReplies)
            {
                query = query.Include(c => c.Replies.Where(r => !r.IsDeleted));
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetRepliesAsync(long parentId, int page, int pageSize)
        {
            return await _context.Comments
                .Where(c => c.ParentId == parentId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IncrementLikeCountAsync(long id, int delta)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (comment == null) return false;

            comment.LikeCount += delta;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(long id, CommentStatus status)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (comment == null) return false;

            comment.Status = status;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}