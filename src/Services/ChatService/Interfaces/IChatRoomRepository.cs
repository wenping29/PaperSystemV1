using ChatService.Entities;

namespace ChatService.Interfaces
{
    public interface IChatRoomRepository
    {
        // 基本CRUD操作
        Task<ChatRoom?> GetByIdAsync(long id);
        Task<IEnumerable<ChatRoom>> GetAllAsync(int page, int pageSize);
        Task<ChatRoom> CreateAsync(ChatRoom chatRoom);
        Task<ChatRoom> UpdateAsync(ChatRoom chatRoom);
        Task<bool> DeleteAsync(long id);

        // 聊天室查询
        Task<ChatRoom?> GetByInviteCodeAsync(string inviteCode);
        Task<IEnumerable<ChatRoom>> GetByCreatorIdAsync(long creatorId, int page, int pageSize);
        Task<IEnumerable<ChatRoom>> GetByUserIdAsync(long userId, int page, int pageSize);
        Task<IEnumerable<ChatRoom>> GetPublicChatRoomsAsync(int page, int pageSize);
        Task<IEnumerable<ChatRoom>> SearchChatRoomsAsync(string searchTerm, int page, int pageSize);

        // 聊天室成员管理
        Task<bool> AddMemberAsync(long chatRoomId, long userId, string role = "Member");
        Task<bool> RemoveMemberAsync(long chatRoomId, long userId);
        Task<bool> UpdateMemberRoleAsync(long chatRoomId, long userId, string role);
        Task<ChatRoomMember?> GetMemberAsync(long chatRoomId, long userId);
        Task<IEnumerable<ChatRoomMember>> GetMembersAsync(long chatRoomId, int page, int pageSize);
        Task<IEnumerable<long>> GetMemberIdsAsync(long chatRoomId);
        Task<bool> IsMemberAsync(long chatRoomId, long userId);
        Task<bool> IsCreatorAsync(long chatRoomId, long userId);
        Task<bool> HasPermissionAsync(long chatRoomId, long userId, string requiredRole);

        // 聊天室状态更新
        Task<bool> UpdateLastActivityAsync(long chatRoomId);
        Task<bool> UpdateMemberLastReadAsync(long chatRoomId, long userId, long messageId);

        // 统计
        Task<int> CountChatRoomsAsync(long? creatorId = null, bool? isPublic = null);
        Task<int> CountMembersAsync(long chatRoomId);

        // 邀请管理
        Task<string?> GenerateInviteCodeAsync(long chatRoomId, DateTime? expiresAt = null);
        Task<bool> RevokeInviteCodeAsync(long chatRoomId);
    }
}