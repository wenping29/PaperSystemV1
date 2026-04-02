import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../core/config/app_theme.dart';
import '../routes/app_router.dart';

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    final GoRouter router = AppRouter.router;

    return MaterialApp.router(
      title: 'AI写作平台',
      theme: AppTheme.lightTheme,
      darkTheme: AppTheme.darkTheme,
      themeMode: ThemeMode.light,
      routerConfig: router,
      debugShowCheckedModeBanner: false,
      builder: (context, child) {
        // 可以在这里添加全局Overlay或Toast
        return child ?? const SizedBox.shrink();
      },
    );
  }
}