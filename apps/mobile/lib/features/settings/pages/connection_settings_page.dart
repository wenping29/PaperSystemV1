import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/config/app_config.dart';
import '../../../core/config/environment_config.dart';
import '../../../services/api_service.dart';

class ConnectionSettingsPage extends ConsumerStatefulWidget {
  const ConnectionSettingsPage({super.key});

  @override
  ConsumerState<ConnectionSettingsPage> createState() => _ConnectionSettingsPageState();
}

class _ConnectionSettingsPageState extends ConsumerState<ConnectionSettingsPage> {
  final _customUrlController = TextEditingController();
  bool _isTestingConnection = false;
  String? _testResult;
  bool? _testSuccess;

  @override
  void initState() {
    super.initState();
    _customUrlController.text = AppConfig.apiBaseUrl;
  }

  @override
  void dispose() {
    _customUrlController.dispose();
    super.dispose();
  }

  Future<void> _testConnection() async {
    setState(() {
      _isTestingConnection = true;
      _testResult = null;
      _testSuccess = null;
    });

    try {
      final apiService = ref.read(apiServiceProvider);
      // 刷新API地址配置
      apiService.refreshBaseUrl();

      final success = await apiService.testConnection();

      setState(() {
        _testSuccess = success;
        _testResult = success
            ? '✅ 连接成功！'
            : '❌ 连接失败，请检查地址和网络';
      });
    } catch (e) {
      setState(() {
        _testSuccess = false;
        _testResult = '❌ 连接异常: $e';
      });
    } finally {
      setState(() {
        _isTestingConnection = false;
      });
    }
  }

  Future<void> _setCustomUrl() async {
    final url = _customUrlController.text.trim();
    if (url.isEmpty) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('请输入有效的URL')),
        );
      }
      return;
    }

    await AppConfig.setApiBaseUrl(url);
    // 刷新API服务
    ref.read(apiServiceProvider).refreshBaseUrl();

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('API地址已更新')),
      );
    }
  }

  Future<void> _selectEnvironment(String envName, String url) async {
    await AppConfig.setApiBaseUrl(url);
    _customUrlController.text = url;
    // 刷新API服务
    ref.read(apiServiceProvider).refreshBaseUrl();

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('已切换到: $envName')),
      );
    }
  }

  Future<void> _resetToDefault() async {
    await AppConfig.resetApiBaseUrl();
    _customUrlController.text = AppConfig.apiBaseUrl;
    // 刷新API服务
    ref.read(apiServiceProvider).refreshBaseUrl();

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('已恢复默认设置')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final currentEnvironment = AppConfig.currentEnvironmentName;

    return Scaffold(
      appBar: AppBar(
        title: const Text('连接设置'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.pop(),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // 当前状态
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      '当前配置',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    const SizedBox(height: 8),
                    if (currentEnvironment != null)
                      Chip(
                        label: Text(currentEnvironment),
                        backgroundColor: Theme.of(context).colorScheme.primaryContainer,
                      ),
                    const SizedBox(height: 8),
                    Text(
                      AppConfig.apiBaseUrl,
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                        fontFamily: 'monospace',
                      ),
                    ),
                  ],
                ),
              ),
            ),

            const SizedBox(height: 24),

            // 快速选择环境
            Text(
              '快速选择',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            ...EnvironmentConfig.environments.entries.map((entry) {
              final isSelected = AppConfig.apiBaseUrl == entry.value;
              return Card(
                color: isSelected
                    ? Theme.of(context).colorScheme.primaryContainer.withAlpha(50)
                    : null,
                child: ListTile(
                  title: Text(entry.key),
                  subtitle: Text(entry.value),
                  trailing: isSelected ? const Icon(Icons.check_circle, color: Colors.green) : null,
                  onTap: () => _selectEnvironment(entry.key, entry.value),
                ),
              );
            }),

            const SizedBox(height: 24),

            // 自定义URL
            Text(
              '自定义地址',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    TextField(
                      controller: _customUrlController,
                      decoration: const InputDecoration(
                        labelText: 'API Base URL',
                        hintText: 'http://192.168.1.100:5000/api/v1',
                        border: OutlineInputBorder(),
                      ),
                    ),
                    const SizedBox(height: 16),
                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton(
                            onPressed: _resetToDefault,
                            child: const Text('恢复默认'),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: ElevatedButton(
                            onPressed: _setCustomUrl,
                            child: const Text('应用'),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),

            const SizedBox(height: 24),

            // 连接测试
            Text(
              '连接测试',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    if (_testResult != null)
                      Padding(
                        padding: const EdgeInsets.only(bottom: 16),
                        child: Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: (_testSuccess ?? false)
                                ? Colors.green.withAlpha(20)
                                : Colors.red.withAlpha(20),
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(
                              color: (_testSuccess ?? false)
                                  ? Colors.green.withAlpha(100)
                                  : Colors.red.withAlpha(100),
                            ),
                          ),
                          child: Text(_testResult!),
                        ),
                      ),
                    ElevatedButton.icon(
                      onPressed: _isTestingConnection ? null : _testConnection,
                      icon: _isTestingConnection
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(Icons.wifi_tethering),
                      label: Text(_isTestingConnection ? '测试中...' : '测试连接'),
                    ),
                  ],
                ),
              ),
            ),

            const SizedBox(height: 24),

            // 使用说明
            Card(
              color: Colors.blue.withAlpha(10),
              child: const Padding(
                padding: EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      '使用说明',
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                    SizedBox(height: 12),
                    Text('• Android模拟器: 自动使用 10.0.2.2 访问主机'),
                    Text('• iOS模拟器: 使用 localhost 即可'),
                    Text('• 真机测试: 请使用电脑的局域网IP地址'),
                    Text('• 确保后端服务已启动在端口 5000'),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
