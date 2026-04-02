import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'app/app.dart';
import 'core/config/app_config.dart';

void main() async {
  // 确保WidgetsBinding初始化
  WidgetsFlutterBinding.ensureInitialized();

  // 初始化应用配置
  await AppConfig.initialize();

  // 运行应用
  runApp(
    ProviderScope(
      child: ScreenUtilInit(
        designSize: const Size(390, 844), // iPhone 14尺寸
        minTextAdapt: true,
        splitScreenMode: true,
        builder: (context, child) {
          return const MyApp();
        },
      ),
    ),
  );
}