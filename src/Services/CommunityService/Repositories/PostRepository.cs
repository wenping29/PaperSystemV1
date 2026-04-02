using Microsoft.EntityFrameworkCore;
using CommunityService.Data;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly CommunityDbContext _context;

        public PostRepository(CommunityDbContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetByIdAsync(long id)
        {
            return await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Collections)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<Post>> GetAllAsync(int page, int pageSize, string? category, string? tag, string? keyword, string? sortBy, long? authorId, string? status, string? visibility)
        {
            var query = _context.Posts.AsQueryable();

            query = query.Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<PostCategory>(category, true, out var categoryEnum))
            {
                query = query.Where(p => p.Category == categoryEnum);
            }

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(p => p.Tags != null && p.Tags.Contains(tag));
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword) || (p.Summary != null && p.Summary.Contains(keyword)));
            }

            if (authorId.HasValue)
            {
                query = query.Where(p => p.AuthorId == authorId.Value);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PostStatus>(status, true, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(visibility) && Enum.TryParse<PostVisibility>(visibility, true, out var visibilityEnum))
            {
                query = query.Where(p => p.Visibility == visibilityEnum);
            }

            // 排序
            query = sortBy?.ToLower() switch
            {
                "hot" => query.OrderByDescending(p => p.HotScore),
                "popular" => query.OrderByDescending(p => p.LikeCount + p.CommentCount * 2 + p.CollectionCount * 3),
                "latest" => query.OrderByDescending(p => p.CreatedAt),
                "view" => query.OrderByDescending(p => p.ViewCount),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Post> CreateAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            post.UpdatedAt = DateTime.UtcNow;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var post = await GetByIdAsync(id);
            if (post == null) return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(long id)
        {
            var post = await GetByIdAsync(id);
            if (post == null) return false;

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Posts.AnyAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<int> GetCountAsync(string? category, string? tag, string? keyword, long? authorId, string? status, string? visibility)
        {
            var query = _context.Posts.AsQueryable();

            query = query.Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<PostCategory>(category, true, out var categoryEnum))
            {
                query = query.Where(p => p.Category == categoryEnum);
            }

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(p => p.Tags != null && p.Tags.Contains(tag));
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword) || (p.Summary != null && p.Summary.Contains(keyword)));
            }

            if (authorId.HasValue)
            {
                query = query.Where(p => p.AuthorId == authorId.Value);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PostStatus>(status, true, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(visibility) && Enum.TryParse<PostVisibility>(visibility, true, out var visibilityEnum))
            {
                query = query.Where(p => p.Visibility == visibilityEnum);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Post>> GetHotPostsAsync(int limit)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && p.Status == PostStatus.Published && p.Visibility == PostVisibility.Public)
                .OrderByDescending(p => p.HotScore)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByAuthorIdAsync(long authorId, int page, int pageSize, string? status)
        {
            var query = _context.Posts.Where(p => p.AuthorId == authorId && !p.IsDeleted);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PostStatus>(status, true, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IncrementViewCountAsync(long id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (post == null) return false;

            post.ViewCount++;
            post.HotScore += 0.1m; // 浏览增加少量热度
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementLikeCountAsync(long id, int delta)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (post == null) return false;

            post.LikeCount += delta;
            post.HotScore += delta * 1.0m; // 点赞增加热度
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementCommentCountAsync(long id, int delta)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (post == null) return false;

            post.CommentCount += delta;
            post.HotScore += delta * 1.5m; // 评论增加更多热度
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementCollectionCountAsync(long id, int delta)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (post == null) return false;

            post.CollectionCount += delta;
            post.HotScore += delta * 2.0m; // 收藏增加更多热度
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateHotScoreAsync(long id, decimal hotScore)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (post == null) return false;

            post.HotScore = hotScore;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(long id, PostStatus status)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (post == null) return false;

            post.Status = status;
            post.UpdatedAt = DateTime.UtcNow;
            if (status == PostStatus.Published && post.PublishedAt == null)
            {
                post.PublishedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Post>> SearchAsync(string keyword, int page, int pageSize)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && (p.Title.Contains(keyword) || p.Content.Contains(keyword) || (p.Summary != null && p.Summary.Contains(keyword))))
                .OrderByDescending(p => p.HotScore)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}