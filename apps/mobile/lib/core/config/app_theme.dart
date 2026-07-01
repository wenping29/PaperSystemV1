import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

class AppTheme {
  // 浅色主题
  static final ThemeData lightTheme = ThemeData(
    useMaterial3: true,
    // Web 平台不指定 fontFamily，让浏览器使用系统字体
    fontFamily: kIsWeb ? 'NotoSansSC' : 'PingFang SC',
    textTheme: TextTheme(
      displayLarge: TextStyle(
        fontSize: 28,
        fontWeight: FontWeight.w700,
        color: Colors.grey.shade900,
      ),
      displayMedium: TextStyle(
        fontSize: 24,
        fontWeight: FontWeight.w600,
        color: Colors.grey.shade900,
      ),
      displaySmall: TextStyle(
        fontSize: 20,
        fontWeight: FontWeight.w600,
        color: Colors.grey.shade900,
      ),
      bodyLarge: TextStyle(
        fontSize: 16,
        fontWeight: FontWeight.w400,
        color: Colors.grey.shade800,
      ),
      bodyMedium: TextStyle(
        fontSize: 14,
        fontWeight: FontWeight.w400,
        color: Colors.grey.shade700,
      ),
      bodySmall: TextStyle(
        fontSize: 12,
        fontWeight: FontWeight.w400,
        color: Colors.grey.shade600,
      ),
    ),
  );

  // 深色主题
  static final ThemeData darkTheme = ThemeData(
    useMaterial3: true,
    // Web 平台不指定 fontFamily，让浏览器使用系统字体
    fontFamily: kIsWeb ? 'NotoSansSC' : 'PingFang SC',
    textTheme: TextTheme(
      displayLarge: TextStyle(
        fontSize: 28,
        fontWeight: FontWeight.w700,
        color: Colors.white,
      ),
      displayMedium: TextStyle(
        fontSize: 24,
        fontWeight: FontWeight.w600,
        color: Colors.white,
      ),
      displaySmall: TextStyle(
        fontSize: 20,
        fontWeight: FontWeight.w600,
        color: Colors.white,
      ),
      bodyLarge: TextStyle(
        fontSize: 16,
        fontWeight: FontWeight.w400,
        color: Colors.grey.shade300,
      ),
      bodyMedium: TextStyle(
        fontSize: 14,
        fontWeight: FontWeight.w400,
        color: Colors.grey.shade400,
      ),
      bodySmall: TextStyle(
        fontSize: 12,
        fontWeight: FontWeight.w400,
        color: Colors.grey.shade500,
      ),
    ),
  );
}
