using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Interfaces;

namespace PaperSystemApi.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly FriendshipDbContext _context;

        public FriendshipRepository(FriendshipDbContext context)
        {
            _context = context;
        }

        // Friendship 操作实现
        public async Task<Friendship?> GetFriendshipByIdAsync(long id)
        {
            return await _context.Friendships
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
        }

        public async Task<Friendship?> GetFriendshipAsync(long userId, long friendId)
        {
            return await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId && !f.IsDeleted);
        }

        public async Task<IEnumerable<Friendship>> GetFriendshipsByUserIdAsync(long userId, string? status, bool? isFavorite, int page, int pageSize)
        {
            var query = _context.Friendships
                .Where(f => f.UserId == userId && !f.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(f => f.Status == status);
            }

            if (isFavorite.HasValue)
            {
                query = query.Where(f => f.IsFavorite == isFavorite.Value);
            }

            return await query
                .OrderByDescending(f => f.LastInteractedAt ?? f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> SearchFriendshipsAsync(long userId, string? searchTerm, int page, int pageSize)
        {
            // 注意：这里需要结合用户服务搜索好友信息
            // 暂时只返回所有好友
            return await GetFriendshipsByUserIdAsync(userId, null, null, page, pageSize);
        }

        public async Task<Friendship> CreateFriendshipAsync(Friendship friendship)
        {
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }

        public async Task<Friendship> UpdateFriendshipAsync(Friendship friendship)
        {
            friendship.UpdatedAt = DateTime.UtcNow;
            _context.Friendships.Update(friendship);
            await _context.SaveChangesAsync();
            return friendship;
        }

        public async Task<bool> DeleteFriendshipAsync(long id)
        {
            var friendship = await GetFriendshipByIdAsync(id);
            if (friendship == null) return false;

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteFriendshipAsync(long id)
        {
            var friendship = await GetFriendshipByIdAsync(id);
            if (friendship == null) return false;

            friendship.IsDeleted = true;
            friendship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFriendshipStatusAsync(long userId, long friendId, string status)
        {
            var friendship = await GetFriendshipAsync(userId, friendId);
            if (friendship == null) return false;

            friendship.Status = status;
            friendship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateInteractionAsync(long userId, long friendId)
        {
            var friendship = await GetFriendshipAsync(userId, friendId);
            if (friendship == null) return false;

            friendship.LastInteractedAt = DateTime.UtcNow;
            friendship.InteractionScore += 1;
            friendship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountFriendshipsAsync(long userId, string? status = null)
        {
            var query = _context.Friendships
                .Where(f => f.UserId == userId && !f.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(f => f.Status == status);
            }

            return await query.CountAsync();
        }

        public async Task<bool> FriendshipExistsAsync(long userId, long friendId)
        {
            return await _context.Friendships
                .AnyAsync(f => f.UserId == userId && f.FriendId == friendId && !f.IsDeleted);
        }

        public async Task<IEnumerable<long>> GetFriendIdsAsync(long userId, string? status = null)
        {
            var query = _context.Friendships
                .Where(f => f.UserId == userId && !f.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(f => f.Status == status);
            }

            return await query
                .Select(f => f.FriendId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetMutualFriendshipsAsync(long user1Id, long user2Id)
        {
            var user1Friends = await GetFriendIdsAsync(user1Id, "active");
            var user2Friends = await GetFriendIdsAsync(user2Id, "active");

            var mutualFriendIds = user1Friends.Intersect(user2Friends);
            return await _context.Friendships
                .Where(f => f.UserId == user1Id && mutualFriendIds.Contains(f.FriendId) && !f.IsDeleted)
                .ToListAsync();
        }

        // FriendRequest 操作实现
        public async Task<FriendRequest?> GetFriendRequestByIdAsync(long id)
        {
            return await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == id && !fr.IsDeleted);
        }

        public async Task<FriendRequest?> GetFriendRequestAsync(long requesterId, long receiverId, string? status = null)
        {
            var query = _context.FriendRequests
                .Where(fr => fr.RequesterId == requesterId && fr.ReceiverId == receiverId && !fr.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(fr => fr.Status == status);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FriendRequest>> GetFriendRequestsByUserIdAsync(long userId, string? type, string? status, int page, int pageSize)
        {
            var query = _context.FriendRequests
                .Where(fr => !fr.IsDeleted);

            if (type == "sent")
            {
                query = query.Where(fr => fr.RequesterId == userId);
            }
            else if (type == "received")
            {
                query = query.Where(fr => fr.ReceiverId == userId);
            }
            else
            {
                query = query.Where(fr => fr.RequesterId == userId || fr.ReceiverId == userId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(fr => fr.Status == status);
            }

            return await query
                .OrderByDescending(fr => fr.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<FriendRequest>> GetPendingFriendRequestsAsync(long userId)
        {
            return await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == "pending" && !fr.IsDeleted)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }

        public async Task<FriendRequest> CreateFriendRequestAsync(FriendRequest request)
        {
            _context.FriendRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<FriendRequest> UpdateFriendRequestAsync(FriendRequest request)
        {
            request.UpdatedAt = DateTime.UtcNow;
            _context.FriendRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> DeleteFriendRequestAsync(long id)
        {
            var request = await GetFriendRequestByIdAsync(id);
            if (request == null) return false;

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FriendRequestExistsAsync(long requesterId, long receiverId, string? status = null)
        {
            var query = _context.FriendRequests
                .Where(fr => fr.RequesterId == requesterId && fr.ReceiverId == receiverId && !fr.IsDeleted);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(fr => fr.Status == status);
            }

            return await query.AnyAsync();
        }

        public async Task<int> CountFriendRequestsAsync(long userId, string? type = null, string? status = null)
        {
            var query = _context.FriendRequests
                .Where(fr => !fr.IsDeleted);

            if (type == "sent")
            {
                query = query.Where(fr => fr.RequesterId == userId);
            }
            else if (type == "received")
            {
                query = query.Where(fr => fr.ReceiverId == userId);
            }
            else
            {
                query = query.Where(fr => fr.RequesterId == userId || fr.ReceiverId == userId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(fr => fr.Status == status);
            }

            return await query.CountAsync();
        }

        public async Task<bool> CancelFriendRequestAsync(long requesterId, long receiverId)
        {
            var request = await GetFriendRequestAsync(requesterId, receiverId, "pending");
            if (request == null) return false;

            request.Status = "cancelled";
            request.UpdatedAt = DateTime.UtcNow;
            request.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptFriendRequestAsync(long requestId, string? responseMessage = null)
        {
            var request = await GetFriendRequestByIdAsync(requestId);
            if (request == null || request.Status != "pending") return false;

            request.Status = "accepted";
            request.UpdatedAt = DateTime.UtcNow;
            request.RespondedAt = DateTime.UtcNow;
            request.ResponseMessage = responseMessage;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectFriendRequestAsync(long requestId, string? responseMessage = null)
        {
            var request = await GetFriendRequestByIdAsync(requestId);
            if (request == null || request.Status != "pending") return false;

            request.Status = "rejected";
            request.UpdatedAt = DateTime.UtcNow;
            request.RespondedAt = DateTime.UtcNow;
            request.ResponseMessage = responseMessage;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FriendRequest>> GetExpiredFriendRequestsAsync(DateTime cutoffDate)
        {
            return await _context.FriendRequests
                .Where(r => r.Status == FriendRequestStatus.Pending &&
                           r.ExpiresAt.HasValue &&
                           r.ExpiresAt <= cutoffDate &&
                           !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> CleanupExpiredFriendRequestsAsync(DateTime cutoffDate)
        {
            var expiredRequests = await GetExpiredFriendRequestsAsync(cutoffDate);
            if (!expiredRequests.Any()) return true;

            foreach (var request in expiredRequests)
            {
                request.Status = FriendRequestStatus.Expired;
                request.UpdatedAt = DateTime.UtcNow;
                request.RespondedAt = DateTime.UtcNow;
                request.ResponseMessage = "Request expired";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Friendship>> GetFriendshipsByTagsAsync(long userId, string tags, int page, int pageSize)
        {
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();

            var query = _context.Friendships
                .Where(f => f.UserId == userId && !f.IsDeleted);

            foreach (var tag in tagList)
            {
                query = query.Where(f => f.Tags != null && f.Tags.Contains(tag));
            }

            return await query
                .OrderByDescending(f => f.LastInteractedAt ?? f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // 统计操作
        public async Task<FriendshipStats> GetFriendshipStatsAsync(long userId)
        {
            var totalFriends = await CountFriendshipsAsync(userId);
            var activeFriends = await CountFriendshipsAsync(userId, "active");
            var favoriteFriends = await _context.Friendships
                .CountAsync(f => f.UserId == userId && f.IsFavorite && !f.IsDeleted);
            var pendingRequests = await CountFriendRequestsAsync(userId, "received", "pending");
            var sentRequests = await CountFriendRequestsAsync(userId, "sent", "pending");

            var lastFriendAdded = await _context.Friendships
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.CreatedAt)
                .FirstOrDefaultAsync();

            return new FriendshipStats
            {
                TotalFriends = totalFriends,
                ActiveFriends = activeFriends,
                FavoriteFriends = favoriteFriends,
                PendingRequests = pendingRequests,
                SentRequests = sentRequests,
                LastFriendAdded = lastFriendAdded
            };
        }
    }
}