using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Chat.Data;
using PaperSystemApi.Chat.Entities;
using PaperSystemApi.Chat.Interfaces;

namespace PaperSystemApi.Chat.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;

        public MessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<Message?> GetByIdAsync(long id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ChatRoom)
                .Include(m => m.ParentMessage)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<IEnumerable<Message>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Messages
                .Where(m => !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ChatRoom)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Message> CreateAsync(Message message)
        {
            message.CreatedAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            message.UpdatedAt = DateTime.UtcNow;
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var message = await GetByIdAsync(id);
            if (message == null) return false;

            message.IsDeleted = true;
            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Message>> GetBySenderIdAsync(long senderId, int page, int pageSize)
        {
            return await _context.Messages
                .Where(m => m.SenderId == senderId && !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ChatRoom)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByReceiverIdAsync(long receiverId, int page, int pageSize)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == receiverId && !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ChatRoom)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByChatRoomIdAsync(long chatRoomId, int page, int pageSize)
        {
            return await _context.Messages
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ChatRoom)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(long user1Id, long user2Id, int page, int pageSize)
        {
            return await _context.Messages
                .Where(m => !m.IsDeleted &&
                    ((m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                     (m.SenderId == user2Id && m.ReceiverId == user1Id)) &&
                    m.ChatRoomId == null) // 私聊消息
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(long userId, long? chatRoomId = null)
        {
            var query = _context.Messages
                .Where(m => !m.IsDeleted && m.Status != "Read" && m.Status != "Failed");

            if (chatRoomId.HasValue)
            {
                query = query.Where(m => m.ChatRoomId == chatRoomId);
            }
            else
            {
                // 获取用户相关的消息（私聊和群聊）
                query = query.Where(m => m.ReceiverId == userId ||
                    (m.ChatRoomId != null && _context.ChatRoomMembers
                        .Any(crm => crm.ChatRoomId == m.ChatRoomId && crm.UserId == userId)));
            }

            return await query
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ChatRoom)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> SearchMessagesAsync(long? chatRoomId, string searchTerm, int page, int pageSize)
        {
            var query = _context.Messages.Where(m => !m.IsDeleted);

            if (chatRoomId.HasValue)
            {
                query = query.Where(m => m.ChatRoomId == chatRoomId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = $"%{searchTerm}%";
                query = query.Where(m => EF.Functions.Like(m.Content, searchTerm));
            }

            return await query
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.ChatRoom)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateMessageStatusAsync(long messageId, string status)
        {
            var message = await GetByIdAsync(messageId);
            if (message == null) return false;

            message.Status = status;
            message.UpdatedAt = DateTime.UtcNow;

            if (status == "Delivered")
            {
                message.DeliveredAt = DateTime.UtcNow;
            }
            else if (status == "Read")
            {
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsReadAsync(long messageId, long userId)
        {
            var message = await GetByIdAsync(messageId);
            if (message == null) return false;

            // 更新消息状态
            await UpdateMessageStatusAsync(messageId, "Read");

            // 创建阅读记录
            var userMessageRead = new UserMessageRead
            {
                UserId = userId,
                MessageId = messageId,
                ReadAt = DateTime.UtcNow
            };

            _context.UserMessageReads.Add(userMessageRead);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsDeliveredAsync(long messageId)
        {
            return await UpdateMessageStatusAsync(messageId, "Delivered");
        }

        public async Task<int> CountMessagesAsync(long? chatRoomId = null, long? senderId = null, long? receiverId = null)
        {
            var query = _context.Messages.Where(m => !m.IsDeleted);

            if (chatRoomId.HasValue)
                query = query.Where(m => m.ChatRoomId == chatRoomId);
            if (senderId.HasValue)
                query = query.Where(m => m.SenderId == senderId);
            if (receiverId.HasValue)
                query = query.Where(m => m.ReceiverId == receiverId);

            return await query.CountAsync();
        }

        public async Task<int> CountUnreadMessagesAsync(long userId, long? chatRoomId = null)
        {
            var query = _context.Messages
                .Where(m => !m.IsDeleted && m.Status != "Read" && m.Status != "Failed");

            if (chatRoomId.HasValue)
            {
                query = query.Where(m => m.ChatRoomId == chatRoomId);
            }
            else
            {
                // 获取用户相关的未读消息
                query = query.Where(m => m.ReceiverId == userId ||
                    (m.ChatRoomId != null && _context.ChatRoomMembers
                        .Any(crm => crm.ChatRoomId == m.ChatRoomId && crm.UserId == userId)));
            }

            return await query.CountAsync();
        }

        public async Task<bool> DeleteMessagesByChatRoomAsync(long chatRoomId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsDeleted = true;
                message.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessagesByUserAsync(long userId)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsDeleted = true;
                message.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}