import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../models/user_model.dart';
import '../../auth/providers/auth_provider.dart';
import '../providers/user_provider.dart';

class ProfilePage extends ConsumerWidget {
  final String userId;

  const ProfilePage({
    super.key,
    required this.userId,
  });

  bool _isCurrentUser(WidgetRef ref) {
    final authState = ref.watch(authProvider);
    return authState.userId == userId;
  }

  Future<void> _handleFollow(WidgetRef ref, User user) async {
    try {
      if (user.isFollowing) {
        await ref.read(userProvider.notifier).unfollowUser(userId);
      } else {
        await ref.read(userProvider.notifier).followUser(userId);
      }
    } catch (e) {
      // 错误处理可以在UI层显示SnackBar
      // print('关注操作失败: $e');
    }
  }

  Future<void> _handleEditProfile(BuildContext context) async {
    // TODO: 跳转到编辑资料页面
    if (context.mounted) {
      context.push('/profile/edit');
    }
  }

  Future<void> _handleSendMessage(BuildContext context) async {
    // TODO: 跳转到聊天页面
    if (context.mounted) {
      context.push('/chats/new?userId=$userId');
    }
  }

  Future<void> _handleRefresh(WidgetRef ref) async {
    try {
      await ref.read(userProvider.notifier).getUser(userId, forceRefresh: true);
    } catch (e) {
      // print('刷新失败: $e');
    }
  }

  Widget _buildHeader(BuildContext context, User user) {
    return Column(
      children: [
        // 头像
        CircleAvatar(
          radius: 50,
          backgroundColor: Colors.grey.shade200,
          backgroundImage: user.hasAvatar
              ? NetworkImage(user.avatarUrl!)
              : const AssetImage('assets/images/default_avatar.png')
                  as ImageProvider,
          child: !user.hasAvatar
              ? const Icon(
                  Icons.person,
                  size: 60,
                  color: Colors.grey,
                )
              : null,
        ),
        const SizedBox(height: 16),

        // 用户名
        Text(
          user.displayName,
          style: Theme.of(context).textTheme.headlineMedium,
        ),
        const SizedBox(height: 8),

        // 用户简介
        if (user.hasBio)
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 32),
            child: Text(
              user.bio!,
              style: Theme.of(context).textTheme.bodyMedium,
              textAlign: TextAlign.center,
            ),
          ),
        const SizedBox(height: 16),

        // 加入时间
        if (user.formattedJoinDate != null)
          Text(
            user.formattedJoinDate!,
            style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: Colors.grey.shade600,
                ),
          ),
      ],
    );
  }

  Widget _buildStats(BuildContext context, User user) {
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 20),
      padding: const EdgeInsets.symmetric(vertical: 16),
      decoration: BoxDecoration(
        border: Border(
          top: BorderSide(color: Colors.grey.shade300),
          bottom: BorderSide(color: Colors.grey.shade300),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _buildStatItem(context, '文章', user.postsCount.toString()),
          _buildStatItem(context, '关注', user.followingCount.toString()),
          _buildStatItem(context, '粉丝', user.followersCount.toString()),
        ],
      ),
    );
  }

  Widget _buildStatItem(BuildContext context, String label, String value) {
    return Column(
      children: [
        Text(
          value,
          style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                fontWeight: FontWeight.bold,
              ),
        ),
        const SizedBox(height: 4),
        Text(
          label,
          style: Theme.of(context).textTheme.bodySmall?.copyWith(
                color: Colors.grey.shade600,
              ),
        ),
      ],
    );
  }

  Widget _buildActionButtons(
      BuildContext context, WidgetRef ref, User user, bool isCurrentUser) {
    if (isCurrentUser) {
      return Padding(
        padding: const EdgeInsets.symmetric(horizontal: 24),
        child: SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: () => _handleEditProfile(context),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 12),
            ),
            child: const Text('编辑资料'),
          ),
        ),
      );
    } else {
      return Padding(
        padding: const EdgeInsets.symmetric(horizontal: 24),
        child: Row(
          children: [
            Expanded(
              child: ElevatedButton(
                onPressed: () => _handleFollow(ref, user),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  backgroundColor: user.isFollowing
                      ? Colors.grey.shade300
                      : Theme.of(context).colorScheme.primary,
                  foregroundColor: user.isFollowing
                      ? Colors.grey.shade800
                      : Colors.white,
                ),
                child: Text(user.isFollowing ? '已关注' : '关注'),
              ),
            ),
            const SizedBox(width: 12),
            ElevatedButton(
              onPressed: () => _handleSendMessage(context),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 20),
              ),
              child: const Icon(Icons.message_outlined),
            ),
          ],
        ),
      );
    }
  }

  Widget _buildContentTabs(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Column(
        children: [
          TabBar(
            tabs: const [
              Tab(text: '文章'),
              Tab(text: '收藏'),
            ],
            indicatorColor: Theme.of(context).colorScheme.primary,
            labelColor: Theme.of(context).colorScheme.primary,
            unselectedLabelColor: Colors.grey.shade600,
          ),
          SizedBox(
            height: 300,
            child: TabBarView(
              children: [
                // 文章列表
                Center(
                  child: Text(
                    '文章列表',
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: Colors.grey.shade600,
                        ),
                  ),
                ),
                // 收藏列表
                Center(
                  child: Text(
                    '收藏列表',
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: Colors.grey.shade600,
                        ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLoading() {
    return Scaffold(
      appBar: AppBar(
        title: const Text('个人主页'),
      ),
      body: const Center(
        child: CircularProgressIndicator(),
      ),
    );
  }

  Widget _buildError(String error, WidgetRef ref) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('个人主页'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              '加载失败: $error',
              style: const TextStyle(color: Colors.red),
            ),
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: () => _handleRefresh(ref),
              child: const Text('重试'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildProfile(
      BuildContext context, WidgetRef ref, User user, bool isCurrentUser) {
    return Scaffold(
      appBar: AppBar(
        title: Text(isCurrentUser ? '我的主页' : '个人主页'),
        actions: [
          if (!isCurrentUser)
            IconButton(
              icon: const Icon(Icons.more_vert),
              onPressed: () {
                // TODO: 显示更多选项（举报、屏蔽等）
              },
            ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () => _handleRefresh(ref),
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 24),
              _buildHeader(context, user),
              _buildStats(context, user),
              const SizedBox(height: 24),
              _buildActionButtons(context, ref, user, isCurrentUser),
              const SizedBox(height: 32),
              _buildContentTabs(context),
              const SizedBox(height: 40),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // 监听用户数据状态
    final userState = ref.watch(userProvider);
    final user = userState.getUser(userId);
    final isLoading = userState.isLoading(userId);
    final hasFailed = userState.hasFailed(userId);
    final isCurrentUser = _isCurrentUser(ref);

    // 首次加载数据
    if (user == null && !isLoading && !hasFailed) {
      Future.microtask(() {
        ref.read(userProvider.notifier).getUser(userId);
      });
    }

    if (isLoading) {
      return _buildLoading();
    }

    if (hasFailed) {
      return _buildError('加载用户数据失败', ref);
    }

    if (user == null) {
      return _buildError('用户不存在', ref);
    }

    return _buildProfile(context, ref, user, isCurrentUser);
  }
}