/// 环境配置 - 支持不同运行环境的API地址
class EnvironmentConfig {
  /// Android模拟器访问主机的地址
  static const String androidEmulator = 'http://10.0.2.2:5000/api/v1';

  /// iOS模拟器访问主机的地址
  static const String iosSimulator = 'http://localhost:5000/api/v1';

  /// 局域网开发环境（需要替换为实际IP）
  static const String localNetwork = 'http://192.168.1.100:5000/api/v1';

  /// 远程服务器（如果有）
  static const String remoteServer = 'http://8.136.202.27:5000/api/v1';

  /// 预定义的环境选项
  static const Map<String, String> environments = {
    'Android模拟器': androidEmulator,
    'iOS模拟器': iosSimulator,
    '局域网设备': localNetwork,
    '远程服务器': remoteServer,
  };

  /// 获取当前推荐的默认地址
  /// 注意：实际项目中应该使用条件编译或平台检测
  static String get recommendedBaseUrl {
    // 默认使用Android模拟器地址，因为这是最常见的开发场景
    return androidEmulator;
  }
}
