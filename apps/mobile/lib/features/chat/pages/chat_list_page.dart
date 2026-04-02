import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../auth/providers/auth_provider.dart';
import '../providers/chat_provider.dart';
import '../widgets/chat_list_item.dart';

/// 聊天列表页面
class ChatListPage extends ConsumerStatefulWidget {
  const ChatListPage({super.key});

  @override
  ConsumerState<ChatListPage> createState() => _ChatListPageState();
}

class _ChatListPageState extends ConsumerState<ChatListPage> {
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
    // 在页面构建后加载数据
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadInitialData();
    });
  }

  /// 加载初始数据
  void _loadInitialData() {
    final authState = ref.read(authProvider);
    if (authState.isLoggedIn) {
      ref.read(chatProvider.notifier).loadChats(refresh: true);
    }
  }

  /// 滚动监听 - 实现加载更多
  void _onScroll() {
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent - 200) {
      final authState = ref.read(authProvider);
      if (authState.isLoggedIn) {
        ref.read(chatProvider.notifier).loadChats();
      }
    }
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);
    final chatState = ref.watch(chatProvider);

    // 检查登录状态
    if (!authState.isLoggedIn) {
      return _buildLoginRequired(context);
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('消息'),
        actions: [
          IconButton(
            icon: const Icon(Icons.search),
            onPressed: () {
              // TODO: 搜索聊天
            },
          ),
          IconButton(
            icon: const Icon(Icons.add_comment),
            onPressed: () {
              // TODO: 新建聊天
            },
          ),
        ],
      ),
      body: _buildBody(context, chatState),
    );
  }

  /// 构建页面主体
  Widget _buildBody(BuildContext context, ChatState chatState) {
    // 初始加载中
    if (chatState.isLoadingList && chatState.chats.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }

    // 加载失败
    if (chatState.listError != null && chatState.chats.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.error_outline,
              size: 64,
              color: Colors.grey,
            ),
            const SizedBox(height: 16),
            Text(
              '加载失败',
              style: TextStyle(
                fontSize: 16,
                color: Colors.grey.shade700,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              chatState.listError!,
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey.shade600,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: () => ref
                  .read(chatProvider.notifier)
                  .loadChats(refresh: true),
              child: const Text('重试'),
            ),
          ],
        ),
      );
    }

    // 空状态
    if (chatState.chats.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.chat_outlined,
              size: 64,
              color: Colors.grey.shade400,
            ),
            const SizedBox(height: 16),
            Text(
              '暂无聊天',
              style: TextStyle(
                fontSize: 16,
                color: Colors.grey.shade600,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              '开始与作者或其他读者交流吧',
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey.shade500,
              ),
            ),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: () async {
        await ref.read(chatProvider.notifier).loadChats(refresh: true);
      },
      child: ListView.builder(
        controller: _scrollController,
        itemCount: chatState.chatList.length + 1, // +1 for loading indicator
        itemBuilder: (context, index) {
          if (index < chatState.chatList.length) {
            final chat = chatState.chatList[index];
            return ChatListItem(
              chat: chat,
              onTap: () {
                // 标记为已读
                ref.read(chatProvider.notifier).markChatAsRead(chat.id);
                // 跳转到聊天详情页（后续实现）
                context.push('/chat/${chat.id}');
              },
            );
          } else {
            return _buildLoadingMore(chatState);
          }
        },
      ),
    );
  }

  /// 构建加载更多指示器
  Widget _buildLoadingMore(ChatState chatState) {
    if (chatState.hasMore) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 16),
        child: Center(child: CircularProgressIndicator()),
      );
    } else if (chatState.chatList.isNotEmpty) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 16),
        child: Center(
          child: Text(
            '没有更多聊天了',
            style: TextStyle(color: Colors.grey),
          ),
        ),
      );
    }
    return const SizedBox();
  }

  /// 构建未登录提示
  Widget _buildLoginRequired(BuildContext context) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.chat_outlined,
            size: 64,
            color: Colors.grey.shade400,
          ),
          const SizedBox(height: 16),
          Text(
            '登录后查看消息',
            style: TextStyle(
              fontSize: 16,
              color: Colors.grey.shade600,
            ),
          ),
          const SizedBox(height: 24),
          ElevatedButton(
            onPressed: () => context.push('/login'),
            child: const Text('去登录'),
          ),
        ],
      ),
    );
  }
}