import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:hive/hive.dart';
import 'package:path_provider/path_provider.dart';

class AppConfig {
  static late SharedPreferences prefs;
  static late Box appBox;

  static Future<void> initialize() async {
    // 初始化SharedPreferences
    prefs = await SharedPreferences.getInstance();

    // 初始化Hive
    if (!kIsWeb) {
      final appDocumentDir = await getApplicationDocumentsDirectory();
      Hive.init(appDocumentDir.path);
    }

    // 打开默认Box
    appBox = await Hive.openBox('app_box');

    // 其他初始化（如Firebase、推送通知等）
    // await Firebase.initializeApp();
    // await FirebaseMessaging.instance.requestPermission();

    // 设置默认配置
    await _setDefaultConfig();
  }

  static Future<void> _setDefaultConfig() async {
    // 设置默认主题
    if (!prefs.containsKey('theme_mode')) {
      await prefs.setString('theme_mode', 'light');
    }

    // 设置默认语言
    if (!prefs.containsKey('language')) {
      await prefs.setString('language', 'zh');
    }

    // 设置API基础URL
    if (!prefs.containsKey('api_base_url')) {
      await prefs.setString('api_base_url', 'http://localhost:8000/api/v1');
    }

    // 设置是否首次启动
    if (!prefs.containsKey('first_launch')) {
      await prefs.setBool('first_launch', true);
    }
  }

  // 获取API基础URL
  static String get apiBaseUrl {
    return prefs.getString('api_base_url') ?? 'http://localhost:8000/api/v1';
  }

  // 设置API基础URL
  static Future<void> setApiBaseUrl(String url) async {
    await prefs.setString('api_base_url', url);
  }

  // 获取主题模式
  static ThemeMode get themeMode {
    final mode = prefs.getString('theme_mode') ?? 'light';
    switch (mode) {
      case 'dark':
        return ThemeMode.dark;
      case 'system':
        return ThemeMode.system;
      default:
        return ThemeMode.light;
    }
  }

  // 设置主题模式
  static Future<void> setThemeMode(ThemeMode mode) async {
    String modeStr;
    switch (mode) {
      case ThemeMode.dark:
        modeStr = 'dark';
        break;
      case ThemeMode.system:
        modeStr = 'system';
        break;
      default:
        modeStr = 'light';
    }
    await prefs.setString('theme_mode', modeStr);
  }

  // 检查是否首次启动
  static bool get isFirstLaunch {
    return prefs.getBool('first_launch') ?? true;
  }

  // 标记已启动
  static Future<void> markLaunched() async {
    await prefs.setBool('first_launch', false);
  }

  // 清除所有配置（用于注销）
  static Future<void> clearAll() async {
    await prefs.clear();
    await appBox.clear();
  }
}