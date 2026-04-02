using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatService.Interfaces;

namespace ChatService.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatService _chatService;
        private static readonly Dictionary<long, string> _userConnections = new Dictionary<long, string>();

        public ChatHub(ILogger<ChatHub> logger, IChatService chatService)
        {
            _logger = logger;
            _chatService = chatService;
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        private long GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

        /// <summary>
        /// 客户端连接时调用
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;

            // 更新用户在线状态
            await _chatService.UpdateUserPresenceAsync(userId, true, connectionId);

            // 存储用户连接
            lock (_userConnections)
            {
                _userConnections[userId] = connectionId;
            }

            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, connectionId);

            // 通知用户已连接的所有聊天室
            var userChatRooms = await _chatService.GetChatRoomsByUserAsync(userId, 1, 100);
            foreach (var chatRoom in userChatRooms)
            {
                await Groups.AddToGroupAsync(connectionId, $"chatroom_{chatRoom.Id}");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 客户端断开连接时调用
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;

            // 更新用户在线状态
            await _chatService.UpdateUserPresenceAsync(userId, false, connectionId);

            // 移除用户连接
            lock (_userConnections)
            {
                _userConnections.Remove(userId);
            }

            _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, connectionId);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 发送消息到聊天室
        /// </summary>
        public async Task SendToChatRoom(long chatRoomId, string content, string messageType = "Text", string? fileUrl = null, long? parentMessageId = null)
        {
            try
            {
                var userId = GetCurrentUserId();

                // 验证用户是否是聊天室成员
                if (!await _chatService.IsChatRoomMemberAsync(chatRoomId, userId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not a member of this chat room");
                    return;
                }

                // 创建消息
                var message = await _chatService.SendMessageAsync(
                    userId, null, chatRoomId, content, messageType, fileUrl, parentMessageId);

                // 广播消息到聊天室
                await Clients.Group($"chatroom_{chatRoomId}").SendAsync("ReceiveMessage", new
                {
                    MessageId = message.Id,
                    ChatRoomId = chatRoomId,
                    SenderId = userId,
                    Content = content,
                    MessageType = messageType,
                    FileUrl = fileUrl,
                    ParentMessageId = parentMessageId,
                    SentAt = message.SentAt
                });

                _logger.LogInformation("Message sent to chat room {ChatRoomId} by user {UserId}", chatRoomId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        public async Task SendPrivateMessage(long receiverId, string content, string messageType = "Text", string? fileUrl = null)
        {
            try
            {
                var senderId = GetCurrentUserId();

                // 不能给自己发消息
                if (senderId == receiverId)
                {
                    await Clients.Caller.SendAsync("Error", "Cannot send message to yourself");
                    return;
                }

                // 创建消息
                var message = await _chatService.SendMessageAsync(
                    senderId, receiverId, null, content, messageType, fileUrl, null);

                // 获取接收者的连接ID
                string? receiverConnectionId = null;
                lock (_userConnections)
                {
                    if (_userConnections.TryGetValue(receiverId, out var connId))
                    {
                        receiverConnectionId = connId;
                    }
                }

                // 发送给接收者（如果在线）
                if (!string.IsNullOrEmpty(receiverConnectionId))
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", new
                    {
                        MessageId = message.Id,
                        SenderId = senderId,
                        Content = content,
                        MessageType = messageType,
                        FileUrl = fileUrl,
                        SentAt = message.SentAt
                    });
                }

                // 也发送给发送者（用于确认）
                await Clients.Caller.SendAsync("MessageSent", new
                {
                    MessageId = message.Id,
                    ReceiverId = receiverId,
                    SentAt = message.SentAt
                });

                _logger.LogInformation("Private message sent from {SenderId} to {ReceiverId}", senderId, receiverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending private message from {SenderId} to {ReceiverId}", GetCurrentUserId(), receiverId);
                await Clients.Caller.SendAsync("Error", "Failed to send private message");
            }
        }

        /// <summary>
        /// 加入聊天室
        /// </summary>
        public async Task JoinChatRoom(long chatRoomId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var connectionId = Context.ConnectionId;

                // 验证用户是否是聊天室成员
                if (!await _chatService.IsChatRoomMemberAsync(chatRoomId, userId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not a member of this chat room");
                    return;
                }

                // 添加到聊天室组
                await Groups.AddToGroupAsync(connectionId, $"chatroom_{chatRoomId}");

                // 通知聊天室其他成员
                await Clients.Group($"chatroom_{chatRoomId}").SendAsync("UserJoined", new
                {
                    UserId = userId,
                    ChatRoomId = chatRoomId,
                    JoinedAt = DateTime.UtcNow
                });

                await Clients.Caller.SendAsync("JoinedChatRoom", new
                {
                    ChatRoomId = chatRoomId,
                    Success = true
                });

                _logger.LogInformation("User {UserId} joined chat room {ChatRoomId}", userId, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to join chat room");
            }
        }

        /// <summary>
        /// 离开聊天室
        /// </summary>
        public async Task LeaveChatRoom(long chatRoomId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var connectionId = Context.ConnectionId;

                // 从聊天室组移除
                await Groups.RemoveFromGroupAsync(connectionId, $"chatroom_{chatRoomId}");

                // 通知聊天室其他成员
                await Clients.Group($"chatroom_{chatRoomId}").SendAsync("UserLeft", new
                {
                    UserId = userId,
                    ChatRoomId = chatRoomId,
                    LeftAt = DateTime.UtcNow
                });

                await Clients.Caller.SendAsync("LeftChatRoom", new
                {
                    ChatRoomId = chatRoomId,
                    Success = true
                });

                _logger.LogInformation("User {UserId} left chat room {ChatRoomId}", userId, chatRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to leave chat room");
            }
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public async Task MarkMessageAsRead(long messageId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.MarkMessageAsReadAsync(messageId, userId);

                if (result)
                {
                    await Clients.Caller.SendAsync("MessageRead", new
                    {
                        MessageId = messageId,
                        ReadAt = DateTime.UtcNow
                    });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to mark message as read");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
                await Clients.Caller.SendAsync("Error", "Failed to mark message as read");
            }
        }

        /// <summary>
        /// 获取在线用户
        /// </summary>
        public async Task GetOnlineUsers(long? chatRoomId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var onlineUsers = await _chatService.GetOnlineUsersAsync(chatRoomId);

                await Clients.Caller.SendAsync("OnlineUsers", new
                {
                    ChatRoomId = chatRoomId,
                    Users = onlineUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                await Clients.Caller.SendAsync("Error", "Failed to get online users");
            }
        }

        /// <summary>
        /// 用户输入状态
        /// </summary>
        public async Task Typing(long chatRoomId, bool isTyping)
        {
            try
            {
                var userId = GetCurrentUserId();

                // 验证用户是否是聊天室成员
                if (!await _chatService.IsChatRoomMemberAsync(chatRoomId, userId))
                    return;

                // 通知聊天室其他成员（除了自己）
                await Clients.OthersInGroup($"chatroom_{chatRoomId}").SendAsync("UserTyping", new
                {
                    UserId = userId,
                    ChatRoomId = chatRoomId,
                    IsTyping = isTyping
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing status for chat room {ChatRoomId}", chatRoomId);
            }
        }
    }
}