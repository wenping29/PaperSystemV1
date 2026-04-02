import 'package:flutter/material.dart';

import '../models/chat_model.dart';

/// 聊天列表项组件
class ChatListItem extends StatelessWidget {
  final Chat chat;
  final VoidCallback onTap;

  const ChatListItem({
    super.key,
    required this.chat,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return ListTile(
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      leading: _buildAvatar(context),
      title: _buildTitle(),
      subtitle: _buildSubtitle(context),
      onTap: onTap,
    );
  }

  /// 构建头像
  Widget _buildAvatar(BuildContext context) {
    return Stack(
      children: [
        CircleAvatar(
          radius: 24,
          backgroundImage: chat.otherUserAvatar != null
              ? NetworkImage(chat.otherUserAvatar!)
              : null,
          child: chat.otherUserAvatar == null
              ? Text(
                  chat.displayName.substring(0, 1).toUpperCase(),
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                  ),
                )
              : null,
        ),
        if (chat.hasUnread)
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              width: 12,
              height: 12,
              decoration: BoxDecoration(
                color: Theme.of(context).colorScheme.error,
                shape: BoxShape.circle,
                border: Border.all(
                  color: Theme.of(context).scaffoldBackgroundColor,
                  width: 2,
                ),
              ),
            ),
          ),
      ],
    );
  }

  /// 构建标题行（用户名 + 时间）
  Widget _buildTitle() {
    return Row(
      children: [
        Expanded(
          child: Text(
            chat.displayName,
            style: TextStyle(
              fontWeight: chat.hasUnread ? FontWeight.bold : FontWeight.normal,
              fontSize: 16,
            ),
            overflow: TextOverflow.ellipsis,
            maxLines: 1,
          ),
        ),
        const SizedBox(width: 8),
        Text(
          chat.formattedUpdateTime,
          style: TextStyle(
            fontSize: 12,
            color: Colors.grey.shade600,
          ),
        ),
      ],
    );
  }

  /// 构建副标题（最后消息 + 未读计数）
  Widget _buildSubtitle(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: Text(
            _getMessagePreview(),
            style: TextStyle(
              fontWeight: chat.hasUnread ? FontWeight.w500 : FontWeight.normal,
              color: chat.hasUnread ? Colors.black : Colors.grey.shade700,
              fontSize: 14,
            ),
            overflow: TextOverflow.ellipsis,
            maxLines: 1,
          ),
        ),
        if (chat.hasUnread) _buildUnreadBadge(context),
      ],
    );
  }

  /// 获取消息预览
  String _getMessagePreview() {
    final lastMessage = chat.lastMessage;

    // 显示发送者名字（如果是群聊或者不是自己发送的消息）
    final showSenderName = chat.isGroup ||
        (lastMessage.senderId != chat.otherUserId);

    String prefix = '';
    if (showSenderName) {
      prefix = '${lastMessage.senderName}: ';
    }

    return '$prefix${lastMessage.previewContent}';
  }

  /// 构建未读消息徽章
  Widget _buildUnreadBadge(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.primary,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        '${chat.unreadCount}',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 12,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }
}