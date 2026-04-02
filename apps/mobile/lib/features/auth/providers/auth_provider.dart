import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../services/api_service.dart';

class AuthState {
  final bool isLoggedIn;
  final String? token;
  final String? username;
  final String? userId;

  const AuthState({
    required this.isLoggedIn,
    this.token,
    this.username,
    this.userId,
  });

  AuthState copyWith({
    bool? isLoggedIn,
    String? token,
    String? username,
    String? userId,
  }) {
    return AuthState(
      isLoggedIn: isLoggedIn ?? this.isLoggedIn,
      token: token ?? this.token,
      username: username ?? this.username,
      userId: userId ?? this.userId,
    );
  }

  static const AuthState initial = AuthState(isLoggedIn: false);
}

class AuthNotifier extends StateNotifier<AuthState> {
  final ApiService apiService;
  late SharedPreferences prefs;

  AuthNotifier(this.apiService) : super(AuthState.initial) {
    _loadAuthState();
  }

  Future<void> _loadAuthState() async {
    prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('auth_token');
    final username = prefs.getString('auth_username');
    final userId = prefs.getString('auth_user_id');

    if (token != null && username != null) {
      state = state.copyWith(
        isLoggedIn: true,
        token: token,
        username: username,
        userId: userId,
      );
      // 设置API服务token
      apiService.setToken(token);
    }
  }

  Future<bool> login({
    required String username,
    required String password,
  }) async {
    try {
      // 调用API登录
      final response = await apiService.post('/auth/login', data: {
        'username': username,
        'password': password,
      });

      if (response.statusCode == 200) {
        final token = response.data['access_token'];
        final userId = response.data['user_id'] ?? '';

        // 保存到本地
        await prefs.setString('auth_token', token);
        await prefs.setString('auth_username', username);
        await prefs.setString('auth_user_id', userId);

        // 更新状态
        state = state.copyWith(
          isLoggedIn: true,
          token: token,
          username: username,
          userId: userId,
        );

        // 设置API服务token
        apiService.setToken(token);

        return true;
      }
    } catch (e) {
      print('登录失败: $e');
    }

    return false;
  }

  Future<bool> register({
    required String username,
    required String email,
    required String password,
  }) async {
    try {
      final response = await apiService.post('/auth/register', data: {
        'username': username,
        'email': email,
        'password': password,
      });

      if (response.statusCode == 200 || response.statusCode == 201) {
        // 注册成功后自动登录
        return await login(username: username, password: password);
      }
    } catch (e) {
      print('注册失败: $e');
    }

    return false;
  }

  Future<void> logout() async {
    // 清除本地存储
    await prefs.remove('auth_token');
    await prefs.remove('auth_username');
    await prefs.remove('auth_user_id');

    // 清除API服务token
    apiService.clearToken();

    // 重置状态
    state = AuthState.initial;
  }

  Future<void> updateProfile({
    String? email,
    String? avatarUrl,
  }) async {
    // TODO: 实现更新资料
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  final apiService = ref.watch(apiServiceProvider);
  return AuthNotifier(apiService);
});