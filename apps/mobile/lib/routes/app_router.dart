import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../features/auth/pages/login_page.dart';
import '../features/auth/pages/register_page.dart';
import '../features/home/pages/home_page.dart';
import '../features/writing/pages/writing_page.dart';
import '../features/profile/pages/profile_page.dart';
import '../features/square/pages/square_page.dart';
import '../features/chat/pages/chat_list_page.dart';
import '../features/chat/pages/chat_detail_page.dart';

class AppRouter {
  static final GoRouter router = GoRouter(
    initialLocation: '/login',
    routes: [
      // 登录页
      GoRoute(
        path: '/login',
        name: 'login',
        pageBuilder: (context, state) => MaterialPage(
          key: state.pageKey,
          child: const LoginPage(),
        ),
      ),

      // 注册页
      GoRoute(
        path: '/register',
        name: 'register',
        pageBuilder: (context, state) => MaterialPage(
          key: state.pageKey,
          child: const RegisterPage(),
        ),
      ),

      // 主页（底部导航）
      GoRoute(
        path: '/',
        name: 'home',
        pageBuilder: (context, state) => MaterialPage(
          key: state.pageKey,
          child: const HomePage(),
        ),
        routes: [
          // 写作页
          GoRoute(
            path: 'writing',
            name: 'writing',
            pageBuilder: (context, state) => MaterialPage(
              key: state.pageKey,
              child: WritingPage(
                articleId: state.uri.queryParameters['articleId'],
              ),
            ),
          ),

          // 个人主页
          GoRoute(
            path: 'profile/:userId',
            name: 'profile',
            pageBuilder: (context, state) => MaterialPage(
              key: state.pageKey,
              child: ProfilePage(userId: state.pathParameters['userId']!),
            ),
          ),

          // 写作广场
          GoRoute(
            path: 'square',
            name: 'square',
            pageBuilder: (context, state) => MaterialPage(
              key: state.pageKey,
              child: const SquarePage(),
            ),
          ),

          // 聊天列表
          GoRoute(
            path: 'chats',
            name: 'chats',
            pageBuilder: (context, state) => MaterialPage(
              key: state.pageKey,
              child: const ChatListPage(),
            ),
          ),

          // 聊天详情
          GoRoute(
            path: 'chat/:chatId',
            name: 'chat',
            pageBuilder: (context, state) => MaterialPage(
              key: state.pageKey,
              child: ChatDetailPage(
                chatId: state.pathParameters['chatId']!,
              ),
            ),
          ),
        ],
      ),
    ],

    // 路由监听
    observers: [],

    // 重定向逻辑
    redirect: (context, state) {
      // TODO: 添加认证重定向逻辑
      // final isLoggedIn = context.read(authProvider).isLoggedIn;
      // final isLoginPage = state.matchedLocation == '/login';
      // final isRegisterPage = state.matchedLocation == '/register';

      // if (!isLoggedIn && !isLoginPage && !isRegisterPage) {
      //   return '/login';
      // }

      // if (isLoggedIn && (isLoginPage || isRegisterPage)) {
      //   return '/';
      // }

      return null;
    },

    // 错误页面
    errorPageBuilder: (context, state) => MaterialPage(
      key: state.pageKey,
      child: Scaffold(
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Text(
                '页面不存在',
                style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              Text(
                state.error.toString(),
                style: const TextStyle(color: Colors.grey),
              ),
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: () => context.go('/'),
                child: const Text('返回首页'),
              ),
            ],
          ),
        ),
      ),
    ),
  );
}