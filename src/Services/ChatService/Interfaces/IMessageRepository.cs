using ChatService.Entities;

namespace ChatService.Interfaces
{
    public interface IMessageRepository
    {
        // 基本CRUD操作
        Task<Message?> GetByIdAsync(long id);
        Task<IEnumerable<Message>> GetAllAsync(int page, int pageSize);
        Task<Message> CreateAsync(Message message);
        Task<Message> UpdateAsync(Message message);
        Task<bool> DeleteAsync(long id);

        // 消息查询
        Task<IEnumerable<Message>> GetBySenderIdAsync(long senderId, int page, int pageSize);
        Task<IEnumerable<Message>> GetByReceiverIdAsync(long receiverId, int page, int pageSize);
        Task<IEnumerable<Message>> GetByChatRoomIdAsync(long chatRoomId, int page, int pageSize);
        Task<IEnumerable<Message>> GetConversationAsync(long user1Id, long user2Id, int page, int pageSize);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(long userId, long? chatRoomId = null);
        Task<IEnumerable<Message>> SearchMessagesAsync(long? chatRoomId, string searchTerm, int page, int pageSize);

        // 消息状态更新
        Task<bool> UpdateMessageStatusAsync(long messageId, string status);
        Task<bool> MarkAsReadAsync(long messageId, long userId);
        Task<bool> MarkAsDeliveredAsync(long messageId);

        // 统计
        Task<int> CountMessagesAsync(long? chatRoomId = null, long? senderId = null, long? receiverId = null);
        Task<int> CountUnreadMessagesAsync(long userId, long? chatRoomId = null);

        // 批量操作
        Task<bool> DeleteMessagesByChatRoomAsync(long chatRoomId);
        Task<bool> DeleteMessagesByUserAsync(long userId);
    }
}