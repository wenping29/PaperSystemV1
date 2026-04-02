class Chat {
  final String id;
  final String otherUserId;
  final String otherUserName;
  final String? otherUserAvatar;
  final Message lastMessage;
  final int unreadCount;
  final DateTime updatedAt;
  final bool isGroup;
  final List<String>? memberIds;

  const Chat({
    required this.id,
    required this.otherUserId,
    required this.otherUserName,
    this.otherUserAvatar,
    required this.lastMessage,
    this.unreadCount = 0,
    required this.updatedAt,
    this.isGroup = false,
    this.memberIds,
  });

  factory Chat.fromJson(Map<String, dynamic> json) {
    return Chat(
      id: json['id'] ?? '',
      otherUserId: json['other_user_id'] ?? json['otherUserId'] ?? '',
      otherUserName: json['other_user_name'] ?? json['otherUserName'] ?? '',
      otherUserAvatar: json['other_user_avatar'] ?? json['otherUserAvatar'],
      lastMessage: Message.fromJson(json['last_message'] ?? json['lastMessage'] ?? {}),
      unreadCount: json['unread_count'] ?? json['unreadCount'] ?? 0,
      updatedAt: json['updated_at'] != null
          ? DateTime.tryParse(json['updated_at']) ?? DateTime.now()
          : DateTime.now(),
      isGroup: json['is_group'] ?? json['isGroup'] ?? false,
      memberIds: (json['member_ids'] as List<dynamic>?)?.map((e) => e.toString()).toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'other_user_id': otherUserId,
      'other_user_name': otherUserName,
      'other_user_avatar': otherUserAvatar,
      'last_message': lastMessage.toJson(),
      'unread_count': unreadCount,
      'updated_at': updatedAt.toIso8601String(),
      'is_group': isGroup,
      'member_ids': memberIds,
    };
  }

  Chat copyWith({
    String? id,
    String? otherUserId,
    String? otherUserName,
    String? otherUserAvatar,
    Message? lastMessage,
    int? unreadCount,
    DateTime? updatedAt,
    bool? isGroup,
    List<String>? memberIds,
  }) {
    return Chat(
      id: id ?? this.id,
      otherUserId: otherUserId ?? this.otherUserId,
      otherUserName: otherUserName ?? this.otherUserName,
      otherUserAvatar: otherUserAvatar ?? this.otherUserAvatar,
      lastMessage: lastMessage ?? this.lastMessage,
      unreadCount: unreadCount ?? this.unreadCount,
      updatedAt: updatedAt ?? this.updatedAt,
      isGroup: isGroup ?? this.isGroup,
      memberIds: memberIds ?? this.memberIds,
    );
  }

  bool get hasUnread => unreadCount > 0;

  String get displayName => otherUserName;

  String get formattedUpdateTime {
    final now = DateTime.now();
    final difference = now.difference(updatedAt);

    if (difference.inDays > 365) {
      return '${(difference.inDays / 365).floor()}年前';
    } else if (difference.inDays > 30) {
      return '${(difference.inDays / 30).floor()}个月前';
    } else if (difference.inDays > 0) {
      return '${difference.inDays}天前';
    } else if (difference.inHours > 0) {
      return '${difference.inHours}小时前';
    } else if (difference.inMinutes > 0) {
      return '${difference.inMinutes}分钟前';
    } else {
      return '刚刚';
    }
  }
}

class Message {
  final String id;
  final String chatId;
  final String senderId;
  final String senderName;
  final MessageType type;
  final String content;
  final DateTime sentAt;
  final bool isRead;
  final String? replyToId;
  final Map<String, dynamic>? metadata;

  const Message({
    required this.id,
    required this.chatId,
    required this.senderId,
    required this.senderName,
    required this.type,
    required this.content,
    required this.sentAt,
    this.isRead = false,
    this.replyToId,
    this.metadata,
  });

  factory Message.fromJson(Map<String, dynamic> json) {
    return Message(
      id: json['id'] ?? '',
      chatId: json['chat_id'] ?? json['chatId'] ?? '',
      senderId: json['sender_id'] ?? json['senderId'] ?? '',
      senderName: json['sender_name'] ?? json['senderName'] ?? '',
      type: MessageType.fromString(json['type'] ?? 'text'),
      content: json['content'] ?? '',
      sentAt: json['sent_at'] != null
          ? DateTime.tryParse(json['sent_at']) ?? DateTime.now()
          : DateTime.now(),
      isRead: json['is_read'] ?? json['isRead'] ?? false,
      replyToId: json['reply_to_id'] ?? json['replyToId'],
      metadata: json['metadata'] != null ? Map<String, dynamic>.from(json['metadata']) : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'chat_id': chatId,
      'sender_id': senderId,
      'sender_name': senderName,
      'type': type.value,
      'content': content,
      'sent_at': sentAt.toIso8601String(),
      'is_read': isRead,
      'reply_to_id': replyToId,
      'metadata': metadata,
    };
  }

  Message copyWith({
    String? id,
    String? chatId,
    String? senderId,
    String? senderName,
    MessageType? type,
    String? content,
    DateTime? sentAt,
    bool? isRead,
    String? replyToId,
    Map<String, dynamic>? metadata,
  }) {
    return Message(
      id: id ?? this.id,
      chatId: chatId ?? this.chatId,
      senderId: senderId ?? this.senderId,
      senderName: senderName ?? this.senderName,
      type: type ?? this.type,
      content: content ?? this.content,
      sentAt: sentAt ?? this.sentAt,
      isRead: isRead ?? this.isRead,
      replyToId: replyToId ?? this.replyToId,
      metadata: metadata ?? this.metadata,
    );
  }

  bool get isText => type == MessageType.text;
  bool get isImage => type == MessageType.image;
  bool get isFile => type == MessageType.file;

  String get formattedTime {
    final now = DateTime.now();
    if (sentAt.year == now.year && sentAt.month == now.month && sentAt.day == now.day) {
      return '${sentAt.hour.toString().padLeft(2, '0')}:${sentAt.minute.toString().padLeft(2, '0')}';
    } else {
      return '${sentAt.month}月${sentAt.day}日';
    }
  }

  String get previewContent {
    if (type == MessageType.text) {
      return content.length > 30 ? '${content.substring(0, 30)}...' : content;
    } else if (type == MessageType.image) {
      return '[图片]';
    } else if (type == MessageType.file) {
      return '[文件]';
    }
    return content;
  }
}

enum MessageType {
  text('text'),
  image('image'),
  file('file');

  final String value;

  const MessageType(this.value);

  factory MessageType.fromString(String value) {
    switch (value) {
      case 'image':
        return MessageType.image;
      case 'file':
        return MessageType.file;
      case 'text':
      default:
        return MessageType.text;
    }
  }

  String get displayName {
    switch (this) {
      case MessageType.text:
        return '文本';
      case MessageType.image:
        return '图片';
      case MessageType.file:
        return '文件';
    }
  }
}