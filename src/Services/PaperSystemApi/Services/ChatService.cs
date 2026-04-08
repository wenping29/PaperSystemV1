using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PaperSystemApi.Models;
using PaperSystemApi.Interfaces;

namespace PaperSystemApi.Services
{
    public class ChatService : IChatService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserMessageReadRepository _userMessageReadRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IMessageRepository messageRepository,
            IChatRoomRepository chatRoomRepository,
            IUserMessageReadRepository userMessageReadRepository,
            IDistributedCache cache,
            ILogger<ChatService> logger)
        {
            _messageRepository = messageRepository;
            _chatRoomRepository = chatRoomRepository;
            _userMessageReadRepository = userMessageReadRepository;
            _cache = cache;
            _logger = logger;
        }

        #region 消息相关方法

        public async Task<Message?> GetMessageByIdAsync(long id)
        {
            return await _messageRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync(int page, int pageSize)
        {
            return await _messageRepository.GetAllAsync(page, pageSize);
        }

        public async Task<IEnumerable<Message>> GetMessagesByChatRoomAsync(long chatRoomId, int page, int pageSize)
        {
            // 验证用户是否有权限访问该聊天室
            // 注意：这里需要从上下文获取当前用户ID，暂时省略
            return await _messageRepository.GetByChatRoomIdAsync(chatRoomId, page, pageSize);
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(long user1Id, long user2Id, int page, int pageSize)
        {
            return await _messageRepository.GetConversationAsync(user1Id, user2Id, page, pageSize);
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(long userId, long? chatRoomId = null)
        {
            return await _messageRepository.GetUnreadMessagesAsync(userId, chatRoomId);
        }

        public async Task<Message> SendMessageAsync(long senderId, long? receiverId, long? chatRoomId, string content, string messageType = "Text", string? fileUrl = null, long? parentMessageId = null)
        {
            // 验证参数
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Message content cannot be empty");

            if (receiverId == null && chatRoomId == null)
                throw new ArgumentException("Either receiverId or chatRoomId must be specified");

            if (receiverId != null && chatRoomId != null)
                throw new ArgumentException("Cannot specify both receiverId and chatRoomId");

            // 如果是私聊，确保接收者存在且不是自己
            if (receiverId.HasValue)
            {
                if (receiverId.Value == senderId)
                    throw new ArgumentException("Cannot send message to yourself");

                // 在实际应用中，这里应该验证接收者用户是否存在
                // 可以通过调用UserService或检查用户数据库来验证
            }

            // 如果是群聊，确保用户是聊天室成员
            if (chatRoomId.HasValue)
            {
                var isMember = await _chatRoomRepository.IsMemberAsync(chatRoomId.Value, senderId);
                if (!isMember)
                    throw new UnauthorizedAccessException("You are not a member of this chat room");
            }

            var message = new Message
            {
                Content = content,
                MessageType = messageType,
                SenderId = senderId,
                ReceiverId = receiverId,
                ChatRoomId = chatRoomId,
                ParentMessageId = parentMessageId,
                Status = "Sent", // 初始状态为已发送
                SentAt = DateTime.UtcNow,
                FileUrl = fileUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdMessage = await _messageRepository.CreateAsync(message);

            // 如果是群聊，更新聊天室最后活动时间
            if (chatRoomId.HasValue)
            {
                await _chatRoomRepository.UpdateLastActivityAsync(chatRoomId.Value);
            }

            _logger.LogInformation("Message sent: ID={MessageId}, Sender={SenderId}, Receiver={ReceiverId}, ChatRoom={ChatRoomId}",
                createdMessage.Id, senderId, receiverId, chatRoomId);

            return createdMessage;
        }

        public async Task<Message> UpdateMessageAsync(long messageId, string content)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                throw new ArgumentException("Message not found");

            // 验证用户是否有权限编辑消息（只有发送者可以编辑）
            // 注意：这里需要从上下文获取当前用户ID，暂时省略

            message.Content = content;
            message.UpdatedAt = DateTime.UtcNow;

            return await _messageRepository.UpdateAsync(message);
        }

        public async Task<bool> DeleteMessageAsync(long messageId, long userId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                return false;

            // 验证用户是否有权限删除消息（发送者或聊天室管理员）
            if (message.SenderId != userId)
            {
                // 如果是群聊消息，检查用户是否是聊天室管理员
                if (message.ChatRoomId.HasValue)
                {
                    var hasPermission = await _chatRoomRepository.HasPermissionAsync(message.ChatRoomId.Value, userId, "Admin");
                    if (!hasPermission)
                        return false;
                }
                else
                {
                    // 私聊消息只有发送者可以删除
                    return false;
                }
            }

            return await _messageRepository.DeleteAsync(messageId);
        }

        public async Task<bool> MarkMessageAsReadAsync(long messageId, long userId)
        {
            // 检查用户是否有权限标记消息为已读
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                return false;

            // 如果是私聊消息，只有接收者可以标记为已读
            if (message.ReceiverId.HasValue && message.ReceiverId.Value != userId)
                return false;

            // 如果是群聊消息，检查用户是否是聊天室成员
            if (message.ChatRoomId.HasValue)
            {
                var isMember = await _chatRoomRepository.IsMemberAsync(message.ChatRoomId.Value, userId);
                if (!isMember)
                    return false;
            }

            return await _messageRepository.MarkAsReadAsync(messageId, userId);
        }

        public async Task<bool> MarkMessageAsDeliveredAsync(long messageId)
        {
            return await _messageRepository.MarkAsDeliveredAsync(messageId);
        }

        public async Task<int> GetUnreadMessageCountAsync(long userId, long? chatRoomId = null)
        {
            return await _messageRepository.CountUnreadMessagesAsync(userId, chatRoomId);
        }

        public async Task<IEnumerable<Message>> SearchMessagesAsync(long? chatRoomId, string searchTerm, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetMessagesByChatRoomAsync(chatRoomId ?? 0, page, pageSize);

            return await _messageRepository.SearchMessagesAsync(chatRoomId, searchTerm, page, pageSize);
        }

        #endregion

        #region 聊天室相关方法

        public async Task<ChatRoom?> GetChatRoomByIdAsync(long id)
        {
            return await _chatRoomRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(int page, int pageSize)
        {
            return await _chatRoomRepository.GetAllAsync(page, pageSize);
        }

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsByUserAsync(long userId, int page, int pageSize)
        {
            return await _chatRoomRepository.GetByUserIdAsync(userId, page, pageSize);
        }

        public async Task<IEnumerable<ChatRoom>> GetPublicChatRoomsAsync(int page, int pageSize)
        {
            return await _chatRoomRepository.GetPublicChatRoomsAsync(page, pageSize);
        }

        public async Task<ChatRoom> CreateChatRoomAsync(long creatorId, string name, string roomType = "Group", string? description = null, bool isPublic = false, int maxMembers = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Chat room name cannot be empty");

            var chatRoom = new ChatRoom
            {
                Name = name,
                Description = description,
                RoomType = roomType,
                CreatorId = creatorId,
                IsPublic = isPublic,
                MaxMembers = maxMembers,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            return await _chatRoomRepository.CreateAsync(chatRoom);
        }

        public async Task<ChatRoom?> UpdateChatRoomAsync(long chatRoomId, long userId, string? name = null, string? description = null, bool? isPublic = null)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
            if (chatRoom == null)
                return null;

            // 验证用户是否有权限更新聊天室（创建者或管理员）
            var hasPermission = await _chatRoomRepository.HasPermissionAsync(chatRoomId, userId, "Admin");
            if (!hasPermission)
                return null;

            if (!string.IsNullOrWhiteSpace(name))
                chatRoom.Name = name;

            if (description != null)
                chatRoom.Description = description;

            if (isPublic.HasValue)
                chatRoom.IsPublic = isPublic.Value;

            chatRoom.UpdatedAt = DateTime.UtcNow;

            return await _chatRoomRepository.UpdateAsync(chatRoom);
        }

        public async Task<bool> DeleteChatRoomAsync(long chatRoomId, long userId)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
            if (chatRoom == null)
                return false;

            // 只有创建者可以删除聊天室
            if (chatRoom.CreatorId != userId)
                return false;

            return await _chatRoomRepository.DeleteAsync(chatRoomId);
        }

        public async Task<ChatRoom?> GetChatRoomByInviteCodeAsync(string inviteCode)
        {
            return await _chatRoomRepository.GetByInviteCodeAsync(inviteCode);
        }

        public async Task<string?> GenerateInviteCodeAsync(long chatRoomId, long userId, DateTime? expiresAt = null)
        {
            // 验证用户是否有权限生成邀请码（管理员以上）
            var hasPermission = await _chatRoomRepository.HasPermissionAsync(chatRoomId, userId, "Admin");
            if (!hasPermission)
                return null;

            return await _chatRoomRepository.GenerateInviteCodeAsync(chatRoomId, expiresAt);
        }

        public async Task<bool> RevokeInviteCodeAsync(long chatRoomId, long userId)
        {
            // 验证用户是否有权限撤销邀请码（管理员以上）
            var hasPermission = await _chatRoomRepository.HasPermissionAsync(chatRoomId, userId, "Admin");
            if (!hasPermission)
                return false;

            return await _chatRoomRepository.RevokeInviteCodeAsync(chatRoomId);
        }

        public async Task<IEnumerable<ChatRoom>> SearchChatRoomsAsync(string searchTerm, int page, int pageSize)
        {
            return await _chatRoomRepository.SearchChatRoomsAsync(searchTerm, page, pageSize);
        }

        #endregion

        #region 聊天室成员管理

        public async Task<bool> JoinChatRoomAsync(long chatRoomId, long userId, string? inviteCode = null)
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
            if (chatRoom == null)
                return false;

            // 检查聊天室是否公开或需要邀请码
            if (!chatRoom.IsPublic)
            {
                if (string.IsNullOrEmpty(inviteCode))
                    return false;

                if (chatRoom.InviteCode != inviteCode)
                    return false;

                // 检查邀请码是否过期
                if (chatRoom.InviteExpiresAt.HasValue && chatRoom.InviteExpiresAt.Value < DateTime.UtcNow)
                    return false;
            }

            // 检查成员数量限制
            if (chatRoom.MaxMembers > 0)
            {
                var memberCount = await _chatRoomRepository.CountMembersAsync(chatRoomId);
                if (memberCount >= chatRoom.MaxMembers)
                    return false;
            }

            return await _chatRoomRepository.AddMemberAsync(chatRoomId, userId);
        }

        public async Task<bool> LeaveChatRoomAsync(long chatRoomId, long userId)
        {
            var member = await _chatRoomRepository.GetMemberAsync(chatRoomId, userId);
            if (member == null)
                return false;

            // 创建者不能离开，只能删除聊天室
            if (member.Role == "Owner")
                return false;

            return await _chatRoomRepository.RemoveMemberAsync(chatRoomId, userId);
        }

        public async Task<bool> AddMemberAsync(long chatRoomId, long adminUserId, long userId, string role = "Member")
        {
            // 验证管理员权限
            var hasPermission = await _chatRoomRepository.HasPermissionAsync(chatRoomId, adminUserId, "Admin");
            if (!hasPermission)
                return false;

            return await _chatRoomRepository.AddMemberAsync(chatRoomId, userId, role);
        }

        public async Task<bool> RemoveMemberAsync(long chatRoomId, long adminUserId, long userId)
        {
            // 验证管理员权限
            var hasPermission = await _chatRoomRepository.HasPermissionAsync(chatRoomId, adminUserId, "Admin");
            if (!hasPermission)
                return false;

            // 不能移除自己（使用LeaveChatRoomAsync）
            if (adminUserId == userId)
                return false;

            return await _chatRoomRepository.RemoveMemberAsync(chatRoomId, userId);
        }

        public async Task<bool> UpdateMemberRoleAsync(long chatRoomId, long adminUserId, long userId, string role)
        {
            // 验证管理员权限
            var hasPermission = await _chatRoomRepository.HasPermissionAsync(chatRoomId, adminUserId, "Admin");
            if (!hasPermission)
                return false;

            // 不能修改自己的角色（除了创建者）
            if (adminUserId == userId)
            {
                var member = await _chatRoomRepository.GetMemberAsync(chatRoomId, adminUserId);
                if (member?.Role != "Owner")
                    return false;
            }

            return await _chatRoomRepository.UpdateMemberRoleAsync(chatRoomId, userId, role);
        }

        public async Task<IEnumerable<ChatRoomMember>> GetChatRoomMembersAsync(long chatRoomId, int page, int pageSize)
        {
            return await _chatRoomRepository.GetMembersAsync(chatRoomId, page, pageSize);
        }

        public async Task<ChatRoomMember?> GetChatRoomMemberAsync(long chatRoomId, long userId)
        {
            return await _chatRoomRepository.GetMemberAsync(chatRoomId, userId);
        }

        public async Task<bool> IsChatRoomMemberAsync(long chatRoomId, long userId)
        {
            return await _chatRoomRepository.IsMemberAsync(chatRoomId, userId);
        }

        public async Task<bool> HasChatRoomPermissionAsync(long chatRoomId, long userId, string requiredRole)
        {
            return await _chatRoomRepository.HasPermissionAsync(chatRoomId, userId, requiredRole);
        }

        public async Task<int> GetChatRoomMemberCountAsync(long chatRoomId)
        {
            return await _chatRoomRepository.CountMembersAsync(chatRoomId);
        }

        #endregion

        #region 实时通信

        public async Task<bool> UpdateUserPresenceAsync(long userId, bool isOnline, string? connectionId = null)
        {
            var cacheKey = $"user:presence:{userId}";
            var cacheValue = isOnline ? (connectionId ?? "online") : "offline";

            await _cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // 5分钟无活动自动离线
            });

            _logger.LogInformation("User presence updated: UserId={UserId}, Online={IsOnline}, ConnectionId={ConnectionId}",
                userId, isOnline, connectionId);

            return true;
        }

        public async Task<IEnumerable<long>> GetOnlineUsersAsync(long? chatRoomId = null)
        {
            var onlineUsers = new List<long>();

            if (chatRoomId.HasValue)
            {
                // 获取聊天室所有成员
                var memberIds = await _chatRoomRepository.GetMemberIdsAsync(chatRoomId.Value);
                foreach (var userId in memberIds)
                {
                    var cacheKey = $"user:presence:{userId}";
                    var status = await _cache.GetStringAsync(cacheKey);
                    if (status != null && status != "offline")
                        onlineUsers.Add(userId);
                }
            }
            else
            {
                // 获取所有在线用户（在实际应用中，这可能需要更高效的实现）
                // 这里简化处理，仅返回示例
                // 实际实现可能需要使用Redis集合或发布/订阅
            }

            return onlineUsers;
        }

        public async Task<bool> IsUserOnlineAsync(long userId)
        {
            var cacheKey = $"user:presence:{userId}";
            var status = await _cache.GetStringAsync(cacheKey);
            return status != null && status != "offline";
        }

        #endregion
    }
}