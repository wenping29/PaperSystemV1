using Microsoft.EntityFrameworkCore;
using CommunityService.Data;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly CommunityDbContext _context;

        public LikeRepository(CommunityDbContext context)
        {
            _context = context;
        }

        public async Task<Like?> GetByIdAsync(long id)
        {
            return await _context.Likes.FindAsync(id);
        }

        public async Task<Like?> GetByUserAndTargetAsync(long userId, LikeTargetType targetType, long targetId)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TargetType == targetType && l.TargetId == targetId);
        }

        public async Task<IEnumerable<Like>> GetAllAsync(int page, int pageSize, long? userId, string? targetType, long? targetId)
        {
            var query = _context.Likes.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(l => l.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(targetType) && Enum.TryParse<LikeTargetType>(targetType, true, out var targetTypeEnum))
            {
                query = query.Where(l => l.TargetType == targetTypeEnum);
            }

            if (targetId.HasValue)
            {
                query = query.Where(l => l.TargetId == targetId.Value);
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Like> CreateAsync(Like like)
        {
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var like = await GetByIdAsync(id);
            if (like == null) return false;

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByUserAndTargetAsync(long userId, LikeTargetType targetType, long targetId)
        {
            var like = await GetByUserAndTargetAsync(userId, targetType, targetId);
            if (like == null) return false;

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Likes.AnyAsync(l => l.Id == id);
        }

        public async Task<bool> ExistsByUserAndTargetAsync(long userId, LikeTargetType targetType, long targetId)
        {
            return await _context.Likes
                .AnyAsync(l => l.UserId == userId && l.TargetType == targetType && l.TargetId == targetId);
        }

        public async Task<int> GetCountByTargetAsync(LikeTargetType targetType, long targetId)
        {
            return await _context.Likes
                .CountAsync(l => l.TargetType == targetType && l.TargetId == targetId);
        }

        public async Task<int> GetCountByUserAsync(long userId, LikeTargetType? targetType)
        {
            var query = _context.Likes.Where(l => l.UserId == userId);

            if (targetType.HasValue)
            {
                query = query.Where(l => l.TargetType == targetType.Value);
            }

            return await query.CountAsync();
        }
    }
}