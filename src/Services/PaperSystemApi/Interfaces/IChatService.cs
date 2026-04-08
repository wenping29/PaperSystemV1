using PaperSystemApi.Models;

namespace PaperSystemApi.Services
{
    public interface IChatService
    {
        // 消息相关方法
        Task<Message?> GetMessageByIdAsync(long id);
        Task<IEnumerable<Message>> GetMessagesAsync(int page, int pageSize);
        Task<IEnumerable<Message>> GetMessagesByChatRoomAsync(long chatRoomId, int page, int pageSize);
        Task<IEnumerable<Message>> GetConversationAsync(long user1Id, long user2Id, int page, int pageSize);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(long userId, long? chatRoomId = null);
        Task<Message> SendMessageAsync(long senderId, long? receiverId, long? chatRoomId, string content, string messageType = "Text", string? fileUrl = null, long? parentMessageId = null);
        Task<Message> UpdateMessageAsync(long messageId, string content);
        Task<bool> DeleteMessageAsync(long messageId, long userId);
        Task<bool> MarkMessageAsReadAsync(long messageId, long userId);
        Task<bool> MarkMessageAsDeliveredAsync(long messageId);
        Task<int> GetUnreadMessageCountAsync(long userId, long? chatRoomId = null);
        Task<IEnumerable<Message>> SearchMessagesAsync(long? chatRoomId, string searchTerm, int page, int pageSize);

        // 聊天室相关方法
        Task<ChatRoom?> GetChatRoomByIdAsync(long id);
        Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(int page, int pageSize);
        Task<IEnumerable<ChatRoom>> GetChatRoomsByUserAsync(long userId, int page, int pageSize);
        Task<IEnumerable<ChatRoom>> GetPublicChatRoomsAsync(int page, int pageSize);
        Task<ChatRoom> CreateChatRoomAsync(long creatorId, string name, string roomType = "Group", string? description = null, bool isPublic = false, int maxMembers = 0);
        Task<ChatRoom?> UpdateChatRoomAsync(long chatRoomId, long userId, string? name = null, string? description = null, bool? isPublic = null);
        Task<bool> DeleteChatRoomAsync(long chatRoomId, long userId);
        Task<ChatRoom?> GetChatRoomByInviteCodeAsync(string inviteCode);
        Task<string?> GenerateInviteCodeAsync(long chatRoomId, long userId, DateTime? expiresAt = null);
        Task<bool> RevokeInviteCodeAsync(long chatRoomId, long userId);
        Task<IEnumerable<ChatRoom>> SearchChatRoomsAsync(string searchTerm, int page, int pageSize);

        // 聊天室成员管理
        Task<bool> JoinChatRoomAsync(long chatRoomId, long userId, string? inviteCode = null);
        Task<bool> LeaveChatRoomAsync(long chatRoomId, long userId);
        Task<bool> AddMemberAsync(long chatRoomId, long adminUserId, long userId, string role = "Member");
        Task<bool> RemoveMemberAsync(long chatRoomId, long adminUserId, long userId);
        Task<bool> UpdateMemberRoleAsync(long chatRoomId, long adminUserId, long userId, string role);
        Task<IEnumerable<ChatRoomMember>> GetChatRoomMembersAsync(long chatRoomId, int page, int pageSize);
        Task<ChatRoomMember?> GetChatRoomMemberAsync(long chatRoomId, long userId);
        Task<bool> IsChatRoomMemberAsync(long chatRoomId, long userId);
        Task<bool> HasChatRoomPermissionAsync(long chatRoomId, long userId, string requiredRole);
        Task<int> GetChatRoomMemberCountAsync(long chatRoomId);

        // 实时通信
        Task<bool> UpdateUserPresenceAsync(long userId, bool isOnline, string? connectionId = null);
        Task<IEnumerable<long>> GetOnlineUsersAsync(long? chatRoomId = null);
        Task<bool> IsUserOnlineAsync(long userId);
    }
}