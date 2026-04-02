import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/user_model.dart';
import '../../../services/api_service.dart';

class UserState {
  final Map<String, User> users; // userId -> User
  final Set<String> loadingUsers; // 正在加载的用户ID
  final Set<String> failedUsers; // 加载失败的用户ID

  const UserState({
    this.users = const {},
    this.loadingUsers = const {},
    this.failedUsers = const {},
  });

  UserState copyWith({
    Map<String, User>? users,
    Set<String>? loadingUsers,
    Set<String>? failedUsers,
  }) {
    return UserState(
      users: users ?? this.users,
      loadingUsers: loadingUsers ?? this.loadingUsers,
      failedUsers: failedUsers ?? this.failedUsers,
    );
  }

  User? getUser(String userId) => users[userId];
  bool isLoading(String userId) => loadingUsers.contains(userId);
  bool hasFailed(String userId) => failedUsers.contains(userId);
}

class UserNotifier extends StateNotifier<UserState> {
  final ApiService apiService;
  final Map<String, Completer<User>> _pendingRequests = {};

  UserNotifier(this.apiService) : super(const UserState());

  Future<User> getUser(String userId, {bool forceRefresh = false}) async {
    // 如果已经有用户数据且不需要强制刷新，直接返回
    if (!forceRefresh && state.users.containsKey(userId)) {
      return state.users[userId]!;
    }

    // 如果已经在请求中，等待现有请求完成
    if (_pendingRequests.containsKey(userId)) {
      return await _pendingRequests[userId]!.future;
    }

    // 开始新的请求
    final completer = Completer<User>();
    _pendingRequests[userId] = completer;

    // 更新加载状态
    state = state.copyWith(
      loadingUsers: {...state.loadingUsers, userId},
      failedUsers: {...state.failedUsers}..remove(userId),
    );

    try {
      // 调用API获取用户数据
      final response = await apiService.get('/users/$userId');

      if (response.statusCode == 200) {
        final user = User.fromJson(response.data);

        // 更新状态
        final updatedUsers = Map<String, User>.from(state.users);
        updatedUsers[userId] = user;

        state = state.copyWith(
          users: updatedUsers,
          loadingUsers: {...state.loadingUsers}..remove(userId),
        );

        completer.complete(user);
        return user;
      } else {
        throw Exception('Failed to load user: ${response.statusCode}');
      }
    } catch (e) {
      // 更新失败状态
      state = state.copyWith(
        loadingUsers: {...state.loadingUsers}..remove(userId),
        failedUsers: {...state.failedUsers, userId},
      );

      completer.completeError(e);
      rethrow;
    } finally {
      _pendingRequests.remove(userId);
    }
  }

  Future<void> followUser(String userId) async {
    try {
      final response = await apiService.post('/users/$userId/follow');

      if (response.statusCode == 200 || response.statusCode == 201) {
        // 更新本地用户状态
        final user = state.users[userId];
        if (user != null) {
          final updatedUser = user.copyWith(
            isFollowing: true,
            followersCount: user.followersCount + 1,
          );

          final updatedUsers = Map<String, User>.from(state.users);
          updatedUsers[userId] = updatedUser;

          state = state.copyWith(users: updatedUsers);
        }
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> unfollowUser(String userId) async {
    try {
      final response = await apiService.delete('/users/$userId/follow');

      if (response.statusCode == 200 || response.statusCode == 204) {
        // 更新本地用户状态
        final user = state.users[userId];
        if (user != null) {
          final updatedUser = user.copyWith(
            isFollowing: false,
            followersCount: user.followersCount - 1,
          );

          final updatedUsers = Map<String, User>.from(state.users);
          updatedUsers[userId] = updatedUser;

          state = state.copyWith(users: updatedUsers);
        }
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> updateUserProfile({
    required String userId,
    String? username,
    String? email,
    String? bio,
    String? avatarUrl,
  }) async {
    try {
      final data = <String, dynamic>{};
      if (username != null) data['username'] = username;
      if (email != null) data['email'] = email;
      if (bio != null) data['bio'] = bio;
      if (avatarUrl != null) data['avatar_url'] = avatarUrl;

      final response = await apiService.put('/users/$userId', data: data);

      if (response.statusCode == 200) {
        // 更新本地用户数据
        final user = state.users[userId];
        if (user != null) {
          final updatedUser = user.copyWith(
            username: username ?? user.username,
            email: email ?? user.email,
            bio: bio ?? user.bio,
            avatarUrl: avatarUrl ?? user.avatarUrl,
          );

          final updatedUsers = Map<String, User>.from(state.users);
          updatedUsers[userId] = updatedUser;

          state = state.copyWith(users: updatedUsers);
        }
      }
    } catch (e) {
      rethrow;
    }
  }

  void clearCache() {
    state = const UserState();
    _pendingRequests.clear();
  }

  void removeUser(String userId) {
    final updatedUsers = Map<String, User>.from(state.users);
    updatedUsers.remove(userId);

    state = state.copyWith(
      users: updatedUsers,
      loadingUsers: {...state.loadingUsers}..remove(userId),
      failedUsers: {...state.failedUsers}..remove(userId),
    );

    _pendingRequests.remove(userId);
  }
}

final userProvider = StateNotifierProvider<UserNotifier, UserState>((ref) {
  final apiService = ref.watch(apiServiceProvider);
  return UserNotifier(apiService);
});