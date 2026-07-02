# Java.net.ConnectException 连接超时问题解决方案

## 问题描述
`java.net.ConnectException: Connection timed out: connect` 错误通常表示无法建立网络连接。

## 可能的原因及解决方案

### 1. Flutter 移动端无法连接到本地服务器（最常见）

#### 原因
- 在 Android 模拟器上，`localhost` 指向模拟器本身，而非开发电脑
- 需要使用特殊的IP地址 `10.0.2.2` 来访问开发电脑

#### 已修复
- ✅ 已将默认API地址改为 `http://10.0.2.2:5000/api/v1`
- ✅ 已添加网络安全配置
- ✅ 已添加必要的网络权限

#### 不同环境的API地址配置

| 运行环境 | API Base URL | 说明 |
|---------|-------------|------|
| Android模拟器 | `http://10.0.2.2:5000/api/v1` | 模拟器访问主机 |
| Android真机（同WiFi） | `http://[你电脑的IP]:5000/api/v1` | 需要在同一网络 |
| iOS模拟器 | `http://localhost:5000/api/v1` 或 `http://127.0.0.1:5000/api/v1` | 可以直接访问 |
| iOS真机 | `http://[你电脑的IP]:5000/api/v1` | 需要在同一网络 |

#### 如何在应用内切换API地址
1. 可以通过 SharedPreferences 修改
2. 或者创建一个设置页面
3. 当前可以通过清除应用数据并重新设置来修改

### 2. 后端服务器未启动

#### 检查步骤
```bash
# 进入后端API目录
cd src/ALL_WebAPI

# 使用开发模式启动
dotnet run

# 或者编译后运行
dotnet build
dotnet bin/Debug/net8.0/PaperSystemApi.dll
```

#### 确认服务运行
打开浏览器访问：`http://localhost:5000/swagger`
应该能看到Swagger UI页面

### 3. 防火墙阻止连接

#### Windows
1. 打开"Windows Defender 防火墙"
2. 点击"允许应用通过防火墙"
3. 确保 `dotnet` 或你的应用在列表中

#### 临时关闭防火墙测试（仅用于诊断）
```powershell
# 管理员权限运行
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled False
```

### 4. Kestrel 未监听所有网络接口

#### 确认配置
在 `appsettings.json` 中：
```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://0.0.0.0:5000"
    }
  }
}
```
`0.0.0.0` 表示监听所有网络接口

### 5. 使用真实设备测试时的网络配置

#### 获取电脑的局域网IP地址
Windows:
```bash
ipconfig
# 找到 "IPv4 Address"，通常是 192.168.x.x 或 10.x.x.x
```

Mac/Linux:
```bash
ifconfig
# 或
ip addr
```

#### 更新应用中的API地址
将 `10.0.2.2` 替换为你的电脑实际IP地址

### 6. 数据库连接超时

#### 当前配置（SQLite）
项目默认使用SQLite，应该不会有网络连接问题

#### 如果使用MySQL
检查 `ConnectionStrings` 配置中的服务器地址是否可达

## 快速启动指南

### 1. 启动后端API
```bash
cd D:\Project\PaperSystemV1\src\ALL_WebAPI
dotnet run
```
等待看到类似消息：
```
Now listening on: http://0.0.0.0:5000
Application started. Press Ctrl+C to shut down.
```

### 2. 验证后端运行
访问：http://localhost:5000/swagger

### 3. 启动Flutter应用
```bash
cd D:\Project\PaperSystemV1\apps\mobile
flutter run
```

### 4. 检查连接
如果使用Android模拟器，应该能正常连接到 `10.0.2.2:5000`

## 测试网络连接

### 从Android模拟器测试
```bash
# 进入模拟器shell
adb shell

# 测试连接（在模拟器shell内）
ping 10.0.2.2
```

### 从电脑测试
```bash
# 测试端口是否监听
netstat -ano | findstr :5000

# 或使用PowerShell
Test-NetConnection -ComputerName localhost -Port 5000
```

## 常见错误及解决方案

| 错误信息 | 原因 | 解决方案 |
|---------|------|---------|
| Connection refused | 服务未启动或端口错误 | 启动后端服务，检查端口5000 |
| Connection timed out | 网络不通，防火墙 | 检查防火墙，确认IP地址 |
| SocketException: Failed host lookup | DNS错误 | 使用IP地址而非域名 |
| SSL/TLS error | HTTPS配置问题 | 在开发环境使用HTTP |

## 开发环境配置建议

### 针对不同设备的API地址配置文件

创建 `lib/core/config/environment_config.dart`:

```dart
class EnvironmentConfig {
  static const String androidEmulator = 'http://10.0.2.2:5000/api/v1';
  static const String iosSimulator = 'http://localhost:5000/api/v1';
  static const String localNetwork = 'http://192.168.1.100:5000/api/v1'; // 替换为实际IP
  
  static String get baseUrl {
    // 根据平台自动选择
    if (const bool.fromEnvironment('dart.library.io')) {
      // 可以使用Platform来检测
      return androidEmulator; // 默认值
    }
    return iosSimulator;
  }
}
```

## 获取帮助

如果以上方法都无法解决问题：
1. 检查后端日志输出
2. 使用抓包工具（如Charles、Wireshark）检查网络请求
3. 尝试在浏览器中直接访问API端点
