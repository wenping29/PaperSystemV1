using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificationService.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private static readonly Dictionary<long, string> _userConnections = new Dictionary<long, string>();

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                _userConnections[userId.Value] = Context.ConnectionId;
                _logger.LogInformation("User {UserId} connected to NotificationHub with connection {ConnectionId}", userId, Context.ConnectionId);

                // 加入用户特定的组
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

                // 通知客户端连接成功
                await Clients.Caller.SendAsync("Connected", new { userId, connectionId = Context.ConnectionId, timestamp = DateTime.UtcNow });
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue && _userConnections.ContainsKey(userId.Value))
            {
                _userConnections.Remove(userId.Value);
                _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);

                // 离开用户组
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // 客户端可以调用此方法来确认收到通知
        public async Task AckNotification(string notificationId)
        {
            var userId = GetUserId();
            _logger.LogDebug("User {UserId} acknowledged notification {NotificationId}", userId, notificationId);
            await Clients.Caller.SendAsync("NotificationAcked", new { notificationId, timestamp = DateTime.UtcNow });
        }

        // 获取连接状态
        public async Task<object> GetConnectionStatus()
        {
            var userId = GetUserId();
            var isConnected = userId.HasValue && _userConnections.ContainsKey(userId.Value);

            return new
            {
                userId,
                isConnected,
                connectionId = Context.ConnectionId,
                connectedAt = Context.Items["ConnectedAt"] ?? DateTime.UtcNow,
                groups = Context.Items["Groups"] ?? new List<string>()
            };
        }

        // 发送测试消息（仅开发环境）
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task SendTestMessage(string message)
        {
            var userId = GetUserId();
            _logger.LogInformation("Admin user {UserId} sending test message: {Message}", userId, message);

            await Clients.Caller.SendAsync("ReceiveTestMessage", new
            {
                message,
                sender = userId,
                timestamp = DateTime.UtcNow
            });
        }

        // 订阅特定类型的通知
        public async Task SubscribeToNotificationType(string notificationType)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            var groupName = $"type-{notificationType}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogDebug("User {UserId} subscribed to notification type {NotificationType}", userId, notificationType);
            await Clients.Caller.SendAsync("Subscribed", new { notificationType, groupName, timestamp = DateTime.UtcNow });
        }

        // 取消订阅特定类型的通知
        public async Task UnsubscribeFromNotificationType(string notificationType)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            var groupName = $"type-{notificationType}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogDebug("User {UserId} unsubscribed from notification type {NotificationType}", userId, notificationType);
            await Clients.Caller.SendAsync("Unsubscribed", new { notificationType, groupName, timestamp = DateTime.UtcNow });
        }

        // 获取在线用户列表（仅管理员）
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<object> GetOnlineUsers()
        {
            return new
            {
                totalUsers = _userConnections.Count,
                users = _userConnections.Keys.ToList(),
                timestamp = DateTime.UtcNow
            };
        }

        private long? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    // 实时通知消息类
    public class RealTimeNotificationMessage
    {
        public string Type { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Sender { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    // 通知事件类型
    public static class NotificationEventTypes
    {
        public const string NewNotification = "new_notification";
        public const string NotificationRead = "notification_read";
        public const string NotificationDeleted = "notification_deleted";
        public const string NotificationArchived = "notification_archived";
        public const string NotificationImportantToggled = "notification_important_toggled";
        public const string UnreadCountUpdated = "unread_count_updated";
        public const string BatchOperationCompleted = "batch_operation_completed";
        public const string ConnectionStatusChanged = "connection_status_changed";
    }
}