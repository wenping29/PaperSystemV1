using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Data;
using PaperSystemApi.Models;
using PaperSystemApi.Interfaces;

namespace PaperSystemApi.Repositories
{
    public class UserMessageReadRepository : IUserMessageReadRepository
    {
        private readonly ChatDbContext _context;

        public UserMessageReadRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<UserMessageRead?> GetByIdAsync(long id)
        {
            return await _context.UserMessageReads
                .Include(umr => umr.User)
                .Include(umr => umr.Message)
                .FirstOrDefaultAsync(umr => umr.Id == id);
        }

        public async Task<UserMessageRead> CreateAsync(UserMessageRead userMessageRead)
        {
            userMessageRead.CreatedAt = DateTime.UtcNow;
            userMessageRead.UpdatedAt = DateTime.UtcNow;
            _context.UserMessageReads.Add(userMessageRead);
            await _context.SaveChangesAsync();
            return userMessageRead;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var userMessageRead = await GetByIdAsync(id);
            if (userMessageRead == null) return false;

            _context.UserMessageReads.Remove(userMessageRead);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserMessageRead?> GetByUserAndMessageAsync(long userId, long messageId)
        {
            return await _context.UserMessageReads
                .Include(umr => umr.User)
                .Include(umr => umr.Message)
                .FirstOrDefaultAsync(umr => umr.UserId == userId && umr.MessageId == messageId);
        }

        public async Task<IEnumerable<UserMessageRead>> GetByMessageIdAsync(long messageId, int page, int pageSize)
        {
            return await _context.UserMessageReads
                .Where(umr => umr.MessageId == messageId)
                .Include(umr => umr.User)
                .Include(umr => umr.Message)
                .OrderByDescending(umr => umr.ReadAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserMessageRead>> GetByUserIdAsync(long userId, int page, int pageSize)
        {
            return await _context.UserMessageReads
                .Where(umr => umr.UserId == userId)
                .Include(umr => umr.User)
                .Include(umr => umr.Message)
                .OrderByDescending(umr => umr.ReadAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserMessageRead>> GetByChatRoomIdAsync(long chatRoomId, long userId, int page, int pageSize)
        {
            return await _context.UserMessageReads
                .Where(umr => umr.UserId == userId &&
                    _context.Messages.Any(m => m.Id == umr.MessageId && m.ChatRoomId == chatRoomId))
                .Include(umr => umr.User)
                .Include(umr => umr.Message)
                .OrderByDescending(umr => umr.ReadAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountReadersAsync(long messageId)
        {
            return await _context.UserMessageReads
                .CountAsync(umr => umr.MessageId == messageId);
        }

        public async Task<bool> HasUserReadMessageAsync(long userId, long messageId)
        {
            return await _context.UserMessageReads
                .AnyAsync(umr => umr.UserId == userId && umr.MessageId == messageId);
        }

        public async Task<bool> MarkMessagesAsReadAsync(long userId, IEnumerable<long> messageIds)
        {
            foreach (var messageId in messageIds)
            {
                if (!await HasUserReadMessageAsync(userId, messageId))
                {
                    var userMessageRead = new UserMessageRead
                    {
                        UserId = userId,
                        MessageId = messageId,
                        ReadAt = DateTime.UtcNow
                    };
                    await CreateAsync(userMessageRead);
                }
            }
            return true;
        }

        public async Task<bool> DeleteReadRecordsByUserAsync(long userId)
        {
            var records = await _context.UserMessageReads
                .Where(umr => umr.UserId == userId)
                .ToListAsync();

            _context.UserMessageReads.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReadRecordsByMessageAsync(long messageId)
        {
            var records = await _context.UserMessageReads
                .Where(umr => umr.MessageId == messageId)
                .ToListAsync();

            _context.UserMessageReads.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}