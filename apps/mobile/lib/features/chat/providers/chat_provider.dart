import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/chat_model.dart';
import '../services/chat_service.dart';
import '../../../services/api_service.dart';

/// 聊天状态
class ChatState {
  /// 聊天会话映射：chatId -> Chat
  final Map<String, Chat> chats;

  /// 正在加载聊天列表
  final bool isLoadingList;

  /// 是否有更多数据可加载
  final bool hasMore;

  /// 当前页码
  final int currentPage;

  /// 列表加载错误信息
  final String? listError;

  /// 正在加载的聊天ID集合
  final Set<String> loadingChats;

  /// 加载失败的聊天ID集合
  final Set<String> failedChats;

  const ChatState({
    this.chats = const {},
    this.isLoadingList = false,
    this.hasMore = true,
    this.currentPage = 1,
    this.listError,
    this.loadingChats = const {},
    this.failedChats = const {},
  });

  /// 获取排序后的聊天列表（按更新时间倒序）
  List<Chat> get chatList => chats.values.toList()
    ..sort((a, b) => b.updatedAt.compareTo(a.updatedAt));

  /// 获取未读消息总数
  int get totalUnreadCount => chats.values.fold(
        0,
        (sum, chat) => sum + chat.unreadCount,
      );

  /// 是否有聊天列表数据
  bool get hasChats => chats.isNotEmpty;

  /// 是否正在加载指定聊天
  bool isLoadingChat(String chatId) => loadingChats.contains(chatId);

  /// 指定聊天是否加载失败
  bool hasChatFailed(String chatId) => failedChats.contains(chatId);

  /// 复制状态（参考auth_provider.dart模式）
  ChatState copyWith({
    Map<String, Chat>? chats,
    bool? isLoadingList,
    bool? hasMore,
    int? currentPage,
    String? listError,
    Set<String>? loadingChats,
    Set<String>? failedChats,
  }) {
    return ChatState(
      chats: chats ?? this.chats,
      isLoadingList: isLoadingList ?? this.isLoadingList,
      hasMore: hasMore ?? this.hasMore,
      currentPage: currentPage ?? this.currentPage,
      listError: listError ?? this.listError,
      loadingChats: loadingChats ?? this.loadingChats,
      failedChats: failedChats ?? this.failedChats,
    );
  }

  /// 添加或更新聊天
  ChatState withChat(Chat chat) {
    final updatedChats = Map<String, Chat>.from(chats);
    updatedChats[chat.id] = chat;
    return copyWith(chats: updatedChats);
  }

  /// 移除聊天
  ChatState withoutChat(String chatId) {
    final updatedChats = Map<String, Chat>.from(chats);
    updatedChats.remove(chatId);
    return copyWith(chats: updatedChats);
  }
}

/// 聊天状态管理器
class ChatNotifier extends StateNotifier<ChatState> {
  final ChatService _chatService;

  ChatNotifier(ApiService apiService)
      : _chatService = ChatService(apiService),
        super(const ChatState());

  /// 加载聊天列表
  /// [refresh] 是否刷新（重置分页）
  Future<void> loadChats({bool refresh = false}) async {
    if (state.isLoadingList) return;

    final page = refresh ? 1 : state.currentPage;
    if (!refresh && !state.hasMore) return;

    state = state.copyWith(
      isLoadingList: true,
      listError: null,
    );

    try {
      final chats = await _chatService.getChats(page: page, limit: 20);

      // 如果是刷新，清空现有聊天；否则合并
      final updatedChats = refresh
          ? <String, Chat>{}
          : Map<String, Chat>.from(state.chats);

      for (final chat in chats) {
        updatedChats[chat.id] = chat;
      }

      state = state.copyWith(
        chats: updatedChats,
        isLoadingList: false,
        hasMore: chats.length >= 20, // 假设每页20条
        currentPage: page + 1,
      );
    } catch (e) {
      state = state.copyWith(
        isLoadingList: false,
        listError: e.toString(),
      );
    }
  }

  /// 加载单个聊天详情
  Future<void> loadChat(String chatId) async {
    if (state.loadingChats.contains(chatId)) return;

    state = state.copyWith(
      loadingChats: {...state.loadingChats, chatId},
      failedChats: {...state.failedChats}..remove(chatId),
    );

    try {
      final chat = await _chatService.getChat(chatId);
      state = state.withChat(chat).copyWith(
            loadingChats: {...state.loadingChats}..remove(chatId),
          );
    } catch (e) {
      state = state.copyWith(
        loadingChats: {...state.loadingChats}..remove(chatId),
        failedChats: {...state.failedChats, chatId},
      );
    }
  }

  /// 更新聊天信息（收到新消息时调用）
  void updateChat(Chat chat) {
    state = state.withChat(chat);
  }

  /// 标记聊天为已读
  Future<void> markChatAsRead(String chatId) async {
    final chat = state.chats[chatId];
    if (chat == null || chat.unreadCount == 0) return;

    try {
      await _chatService.markAsRead(chatId);
      final updatedChat = chat.copyWith(unreadCount: 0);
      state = state.withChat(updatedChat);
    } catch (e) {
      // 静默失败，不更新状态
      // print('标记聊天为已读失败: $e');
    }
  }

  /// 发送消息
  Future<Message> sendMessage({
    required String chatId,
    required String content,
    MessageType type = MessageType.text,
    String? replyToId,
    Map<String, dynamic>? metadata,
  }) async {
    try {
      final message = await _chatService.sendMessage(
        chatId: chatId,
        content: content,
        type: type,
        replyToId: replyToId,
        metadata: metadata,
      );

      // 更新聊天最后一条消息
      final chat = state.chats[chatId];
      if (chat != null) {
        final updatedChat = chat.copyWith(
          lastMessage: message,
          updatedAt: DateTime.now(),
        );
        state = state.withChat(updatedChat);
      }

      return message;
    } catch (e) {
      rethrow;
    }
  }

  /// 创建私聊
  Future<Chat> createPrivateChat(String otherUserId) async {
    try {
      final chat = await _chatService.createPrivateChat(otherUserId);
      state = state.withChat(chat);
      return chat;
    } catch (e) {
      rethrow;
    }
  }

  /// 创建群聊
  Future<Chat> createGroupChat({
    required String groupName,
    required List<String> memberIds,
  }) async {
    try {
      final chat = await _chatService.createGroupChat(
        groupName: groupName,
        memberIds: memberIds,
      );
      state = state.withChat(chat);
      return chat;
    } catch (e) {
      rethrow;
    }
  }

  /// 删除聊天
  Future<void> deleteChat(String chatId) async {
    try {
      await _chatService.deleteChat(chatId);
      state = state.withoutChat(chatId);
    } catch (e) {
      rethrow;
    }
  }

  /// 删除消息
  Future<void> deleteMessage(String chatId, String messageId) async {
    try {
      await _chatService.deleteMessage(chatId, messageId);
    } catch (e) {
      rethrow;
    }
  }

  /// 清空错误状态
  void clearError() {
    state = state.copyWith(listError: null);
  }

  /// 重置状态
  void reset() {
    state = const ChatState();
  }
}

/// 聊天状态Provider
final chatProvider = StateNotifierProvider<ChatNotifier, ChatState>((ref) {
  final apiService = ref.watch(apiServiceProvider);
  return ChatNotifier(apiService);
});