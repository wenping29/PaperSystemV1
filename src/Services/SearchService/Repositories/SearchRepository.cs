using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Entities;
using SearchService.Interfaces;

namespace SearchService.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly SearchDbContext _context;

        public SearchRepository(SearchDbContext context)
        {
            _context = context;
        }

        public async Task<SearchHistory?> GetByIdAsync(long id)
        {
            return await _context.SearchHistories.FindAsync(id);
        }

        public async Task<IEnumerable<SearchHistory>> GetByUserIdAsync(long userId, int page, int pageSize)
        {
            return await _context.SearchHistories
                .Where(sh => sh.UserId == userId)
                .OrderByDescending(sh => sh.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<SearchHistory>> GetRecentAsync(int limit)
        {
            return await _context.SearchHistories
                .OrderByDescending(sh => sh.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<SearchHistory> CreateAsync(SearchHistory history)
        {
            _context.SearchHistories.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var history = await GetByIdAsync(id);
            if (history == null) return false;

            _context.SearchHistories.Remove(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearUserHistoryAsync(long userId)
        {
            var histories = await _context.SearchHistories
                .Where(sh => sh.UserId == userId)
                .ToListAsync();

            if (!histories.Any()) return true;

            _context.SearchHistories.RemoveRange(histories);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PopularSearchTerm>> GetPopularSearchTermsAsync(int limit, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SearchHistories.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(sh => sh.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(sh => sh.CreatedAt <= endDate.Value);
            }

            var popularTerms = await query
                .Where(sh => !string.IsNullOrEmpty(sh.Query))
                .GroupBy(sh => sh.Query)
                .Select(g => new PopularSearchTerm
                {
                    Term = g.Key,
                    SearchCount = g.Count(),
                    LastSearched = g.Max(sh => sh.CreatedAt)
                })
                .OrderByDescending(t => t.SearchCount)
                .ThenByDescending(t => t.LastSearched)
                .Take(limit)
                .ToListAsync();

            return popularTerms;
        }

        public async Task<IEnumerable<SearchHistory>> SearchHistoriesAsync(string? query, string? searchType, long? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            var searchQuery = _context.SearchHistories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                searchQuery = searchQuery.Where(sh => sh.Query.Contains(query));
            }

            if (!string.IsNullOrWhiteSpace(searchType))
            {
                searchQuery = searchQuery.Where(sh => sh.SearchType == searchType);
            }

            if (userId.HasValue)
            {
                searchQuery = searchQuery.Where(sh => sh.UserId == userId);
            }

            if (startDate.HasValue)
            {
                searchQuery = searchQuery.Where(sh => sh.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                searchQuery = searchQuery.Where(sh => sh.CreatedAt <= endDate.Value);
            }

            return await searchQuery
                .OrderByDescending(sh => sh.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountSearchHistoriesAsync(string? query, string? searchType, long? userId, DateTime? startDate, DateTime? endDate)
        {
            var searchQuery = _context.SearchHistories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                searchQuery = searchQuery.Where(sh => sh.Query.Contains(query));
            }

            if (!string.IsNullOrWhiteSpace(searchType))
            {
                searchQuery = searchQuery.Where(sh => sh.SearchType == searchType);
            }

            if (userId.HasValue)
            {
                searchQuery = searchQuery.Where(sh => sh.UserId == userId);
            }

            if (startDate.HasValue)
            {
                searchQuery = searchQuery.Where(sh => sh.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                searchQuery = searchQuery.Where(sh => sh.CreatedAt <= endDate.Value);
            }

            return await searchQuery.CountAsync();
        }

        // 搜索索引相关方法实现
        public async Task<SearchIndex?> GetIndexAsync(string indexType, long entityId)
        {
            return await _context.SearchIndices
                .FirstOrDefaultAsync(i => i.IndexType == indexType && i.EntityId == entityId);
        }

        public async Task<SearchIndex?> GetIndexByIdAsync(long id)
        {
            return await _context.SearchIndices.FindAsync(id);
        }

        public async Task<SearchIndex> CreateOrUpdateIndexAsync(SearchIndex index)
        {
            var existing = await GetIndexAsync(index.IndexType, index.EntityId);
            if (existing == null)
            {
                index.IndexedAt = DateTime.UtcNow;
                _context.SearchIndices.Add(index);
            }
            else
            {
                existing.Status = index.Status;
                existing.ErrorMessage = index.ErrorMessage;
                existing.Version = index.Version;
                existing.LastUpdatedAt = DateTime.UtcNow;
                existing.MetadataJson = index.MetadataJson;
                existing.Tags = index.Tags;
                existing.RelevanceScore = index.RelevanceScore;
                _context.SearchIndices.Update(existing);
                index = existing;
            }

            await _context.SaveChangesAsync();
            return index;
        }

        public async Task<bool> DeleteIndexAsync(string indexType, long entityId)
        {
            var index = await GetIndexAsync(indexType, entityId);
            if (index == null) return false;

            _context.SearchIndices.Remove(index);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteIndexByIdAsync(long id)
        {
            var index = await GetIndexByIdAsync(id);
            if (index == null) return false;

            _context.SearchIndices.Remove(index);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SearchIndex>> GetIndicesByTypeAsync(string indexType, int page, int pageSize)
        {
            return await _context.SearchIndices
                .Where(i => i.IndexType == indexType)
                .OrderByDescending(i => i.IndexedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<SearchIndex>> SearchIndicesAsync(string? indexType, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            var query = _context.SearchIndices.AsQueryable();

            if (!string.IsNullOrWhiteSpace(indexType))
            {
                query = query.Where(i => i.IndexType == indexType);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(i => i.Status == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(i => i.IndexedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.IndexedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(i => i.IndexedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountIndicesAsync(string? indexType, string? status)
        {
            var query = _context.SearchIndices.AsQueryable();

            if (!string.IsNullOrWhiteSpace(indexType))
            {
                query = query.Where(i => i.IndexType == indexType);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(i => i.Status == status);
            }

            return await query.CountAsync();
        }

        public async Task<bool> UpdateIndexStatusAsync(string indexType, long entityId, string status, string? errorMessage = null)
        {
            var index = await GetIndexAsync(indexType, entityId);
            if (index == null) return false;

            index.Status = status;
            index.ErrorMessage = errorMessage;
            index.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SearchIndex>> GetIndicesNeedingUpdateAsync(string indexType, int limit)
        {
            return await _context.SearchIndices
                .Where(i => i.IndexType == indexType && i.Status == IndexStatus.Pending)
                .OrderBy(i => i.IndexedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}