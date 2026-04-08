using PaperSystemApi.Chat.Entities;

namespace PaperSystemApi.Chat.Interfaces
{
    public interface IUserMessageReadRepository
    {
        Task<UserMessageRead?> GetByIdAsync(long id);
        Task<UserMessageRead> CreateAsync(UserMessageRead userMessageRead);
        Task<bool> DeleteAsync(long id);

        // 查询阅读记录
        Task<UserMessageRead?> GetByUserAndMessageAsync(long userId, long messageId);
        Task<IEnumerable<UserMessageRead>> GetByMessageIdAsync(long messageId, int page, int pageSize);
        Task<IEnumerable<UserMessageRead>> GetByUserIdAsync(long userId, int page, int pageSize);
        Task<IEnumerable<UserMessageRead>> GetByChatRoomIdAsync(long chatRoomId, long userId, int page, int pageSize);

        // 统计
        Task<int> CountReadersAsync(long messageId);
        Task<bool> HasUserReadMessageAsync(long userId, long messageId);

        // 批量操作
        Task<bool> MarkMessagesAsReadAsync(long userId, IEnumerable<long> messageIds);
        Task<bool> DeleteReadRecordsByUserAsync(long userId);
        Task<bool> DeleteReadRecordsByMessageAsync(long messageId);
    }
}