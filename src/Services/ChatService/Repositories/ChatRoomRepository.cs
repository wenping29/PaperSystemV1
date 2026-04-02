using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ChatService.Data;
using ChatService.Entities;
using ChatService.Interfaces;

namespace ChatService.Repositories
{
    public class ChatRoomRepository : IChatRoomRepository
    {
        private readonly ChatDbContext _context;

        public ChatRoomRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ChatRoom?> GetByIdAsync(long id)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Creator)
                .Include(cr => cr.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(cr => cr.Id == id && !cr.IsDeleted);
        }

        public async Task<IEnumerable<ChatRoom>> GetAllAsync(int page, int pageSize)
        {
            return await _context.ChatRooms
                .Where(cr => !cr.IsDeleted)
                .Include(cr => cr.Creator)
                .Include(cr => cr.Members)
                .OrderByDescending(cr => cr.LastActivityAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<ChatRoom> CreateAsync(ChatRoom chatRoom)
        {
            chatRoom.CreatedAt = DateTime.UtcNow;
            chatRoom.UpdatedAt = DateTime.UtcNow;
            chatRoom.LastActivityAt = DateTime.UtcNow;

            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // 创建者自动成为Owner
            await AddMemberAsync(chatRoom.Id, chatRoom.CreatorId, "Owner");

            return chatRoom;
        }

        public async Task<ChatRoom> UpdateAsync(ChatRoom chatRoom)
        {
            chatRoom.UpdatedAt = DateTime.UtcNow;
            _context.ChatRooms.Update(chatRoom);
            await _context.SaveChangesAsync();
            return chatRoom;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var chatRoom = await GetByIdAsync(id);
            if (chatRoom == null) return false;

            chatRoom.IsDeleted = true;
            chatRoom.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatRoom?> GetByInviteCodeAsync(string inviteCode)
        {
            return await _context.ChatRooms
                .Include(cr => cr.Creator)
                .Include(cr => cr.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(cr => cr.InviteCode == inviteCode && !cr.IsDeleted);
        }

        public async Task<IEnumerable<ChatRoom>> GetByCreatorIdAsync(long creatorId, int page, int pageSize)
        {
            return await _context.ChatRooms
                .Where(cr => cr.CreatorId == creatorId && !cr.IsDeleted)
                .Include(cr => cr.Creator)
                .Include(cr => cr.Members)
                .OrderByDescending(cr => cr.LastActivityAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetByUserIdAsync(long userId, int page, int pageSize)
        {
            return await _context.ChatRooms
                .Where(cr => !cr.IsDeleted &&
                    (cr.CreatorId == userId ||
                     cr.Members.Any(m => m.UserId == userId && !m.IsDeleted)))
                .Include(cr => cr.Creator)
                .Include(cr => cr.Members)
                .OrderByDescending(cr => cr.LastActivityAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetPublicChatRoomsAsync(int page, int pageSize)
        {
            return await _context.ChatRooms
                .Where(cr => cr.IsPublic && !cr.IsDeleted)
                .Include(cr => cr.Creator)
                .Include(cr => cr.Members)
                .OrderByDescending(cr => cr.LastActivityAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> SearchChatRoomsAsync(string searchTerm, int page, int pageSize)
        {
            var query = _context.ChatRooms.Where(cr => !cr.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = $"%{searchTerm}%";
                query = query.Where(cr => EF.Functions.Like(cr.Name, searchTerm) ||
                                          EF.Functions.Like(cr.Description, searchTerm));
            }

            return await query
                .Include(cr => cr.Creator)
                .Include(cr => cr.Members)
                .OrderByDescending(cr => cr.LastActivityAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> AddMemberAsync(long chatRoomId, long userId, string role = "Member")
        {
            if (await IsMemberAsync(chatRoomId, userId))
                return false;

            var member = new ChatRoomMember
            {
                UserId = userId,
                ChatRoomId = chatRoomId,
                Role = role,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ChatRoomMembers.Add(member);
            await UpdateLastActivityAsync(chatRoomId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(long chatRoomId, long userId)
        {
            var member = await GetMemberAsync(chatRoomId, userId);
            if (member == null) return false;

            // 如果是创建者，不能移除，除非删除聊天室
            if (member.Role == "Owner")
                return false;

            member.IsDeleted = true;
            member.UpdatedAt = DateTime.UtcNow;
            await UpdateLastActivityAsync(chatRoomId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMemberRoleAsync(long chatRoomId, long userId, string role)
        {
            var member = await GetMemberAsync(chatRoomId, userId);
            if (member == null) return false;

            // 不能修改创建者的角色
            if (member.Role == "Owner")
                return false;

            member.Role = role;
            member.UpdatedAt = DateTime.UtcNow;
            await UpdateLastActivityAsync(chatRoomId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatRoomMember?> GetMemberAsync(long chatRoomId, long userId)
        {
            return await _context.ChatRoomMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.ChatRoomId == chatRoomId &&
                                          m.UserId == userId &&
                                          !m.IsDeleted);
        }

        public async Task<IEnumerable<ChatRoomMember>> GetMembersAsync(long chatRoomId, int page, int pageSize)
        {
            return await _context.ChatRoomMembers
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .Include(m => m.User)
                .OrderBy(m => m.JoinedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<long>> GetMemberIdsAsync(long chatRoomId)
        {
            return await _context.ChatRoomMembers
                .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                .Select(m => m.UserId)
                .ToListAsync();
        }

        public async Task<bool> IsMemberAsync(long chatRoomId, long userId)
        {
            return await _context.ChatRoomMembers
                .AnyAsync(m => m.ChatRoomId == chatRoomId &&
                               m.UserId == userId &&
                               !m.IsDeleted);
        }

        public async Task<bool> IsCreatorAsync(long chatRoomId, long userId)
        {
            var chatRoom = await GetByIdAsync(chatRoomId);
            return chatRoom?.CreatorId == userId;
        }

        public async Task<bool> HasPermissionAsync(long chatRoomId, long userId, string requiredRole)
        {
            var member = await GetMemberAsync(chatRoomId, userId);
            if (member == null) return false;

            // 角色权限等级：Owner > Admin > Member > Guest
            var roleHierarchy = new Dictionary<string, int>
            {
                { "Owner", 4 },
                { "Admin", 3 },
                { "Member", 2 },
                { "Guest", 1 }
            };

            if (!roleHierarchy.ContainsKey(member.Role) || !roleHierarchy.ContainsKey(requiredRole))
                return false;

            return roleHierarchy[member.Role] >= roleHierarchy[requiredRole];
        }

        public async Task<bool> UpdateLastActivityAsync(long chatRoomId)
        {
            var chatRoom = await GetByIdAsync(chatRoomId);
            if (chatRoom == null) return false;

            chatRoom.LastActivityAt = DateTime.UtcNow;
            chatRoom.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMemberLastReadAsync(long chatRoomId, long userId, long messageId)
        {
            var member = await GetMemberAsync(chatRoomId, userId);
            if (member == null) return false;

            member.LastReadMessageId = messageId;
            member.LastReadAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountChatRoomsAsync(long? creatorId = null, bool? isPublic = null)
        {
            var query = _context.ChatRooms.Where(cr => !cr.IsDeleted);

            if (creatorId.HasValue)
                query = query.Where(cr => cr.CreatorId == creatorId);
            if (isPublic.HasValue)
                query = query.Where(cr => cr.IsPublic == isPublic.Value);

            return await query.CountAsync();
        }

        public async Task<int> CountMembersAsync(long chatRoomId)
        {
            return await _context.ChatRoomMembers
                .CountAsync(m => m.ChatRoomId == chatRoomId && !m.IsDeleted);
        }

        public async Task<string?> GenerateInviteCodeAsync(long chatRoomId, DateTime? expiresAt = null)
        {
            var chatRoom = await GetByIdAsync(chatRoomId);
            if (chatRoom == null) return null;

            // 生成随机邀请码
            var randomBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var inviteCode = Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 16);

            chatRoom.InviteCode = inviteCode;
            chatRoom.InviteExpiresAt = expiresAt;
            chatRoom.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return inviteCode;
        }

        public async Task<bool> RevokeInviteCodeAsync(long chatRoomId)
        {
            var chatRoom = await GetByIdAsync(chatRoomId);
            if (chatRoom == null) return false;

            chatRoom.InviteCode = null;
            chatRoom.InviteExpiresAt = null;
            chatRoom.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}