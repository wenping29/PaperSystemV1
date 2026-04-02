using Microsoft.EntityFrameworkCore;
using CommunityService.Data;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly CommunityDbContext _context;

        public CollectionRepository(CommunityDbContext context)
        {
            _context = context;
        }

        public async Task<Collection?> GetByIdAsync(long id)
        {
            return await _context.Collections
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Collection?> GetByUserAndPostAsync(long userId, long postId)
        {
            return await _context.Collections
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.PostId == postId && !c.IsDeleted);
        }

        public async Task<IEnumerable<Collection>> GetAllAsync(int page, int pageSize, long? userId, long? postId)
        {
            var query = _context.Collections.AsQueryable();

            query = query.Where(c => !c.IsDeleted);

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserId == userId.Value);
            }

            if (postId.HasValue)
            {
                query = query.Where(c => c.PostId == postId.Value);
            }

            return await query
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Collection> CreateAsync(Collection collection)
        {
            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();
            return collection;
        }

        public async Task<Collection> UpdateAsync(Collection collection)
        {
            collection.UpdatedAt = DateTime.UtcNow;
            _context.Collections.Update(collection);
            await _context.SaveChangesAsync();
            return collection;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var collection = await GetByIdAsync(id);
            if (collection == null) return false;

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByUserAndPostAsync(long userId, long postId)
        {
            var collection = await GetByUserAndPostAsync(userId, postId);
            if (collection == null) return false;

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Collections.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> ExistsByUserAndPostAsync(long userId, long postId)
        {
            return await _context.Collections
                .AnyAsync(c => c.UserId == userId && c.PostId == postId && !c.IsDeleted);
        }

        public async Task<int> GetCountByUserAsync(long userId)
        {
            return await _context.Collections
                .CountAsync(c => c.UserId == userId && !c.IsDeleted);
        }

        public async Task<int> GetCountByPostAsync(long postId)
        {
            return await _context.Collections
                .CountAsync(c => c.PostId == postId && !c.IsDeleted);
        }

        public async Task<IEnumerable<Collection>> GetByUserIdAsync(long userId, int page, int pageSize)
        {
            return await _context.Collections
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Collection>> GetByPostIdAsync(long postId, int page, int pageSize)
        {
            return await _context.Collections
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}