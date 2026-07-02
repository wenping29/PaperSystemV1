import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/config/app_config.dart';

class ApiService {
  late Dio _dio;
  String? _token;

  ApiService() {
    _initDio();
  }

  void _initDio() {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppConfig.apiBaseUrl,
        connectTimeout: const Duration(seconds: 30), // 连接超时
        receiveTimeout: const Duration(seconds: 60), // 接收超时
        sendTimeout: const Duration(seconds: 60), // 发送超时
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );

    // 添加请求拦截器
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          // 添加认证token
          if (_token != null) {
            options.headers['Authorization'] = 'Bearer $_token';
          }
          return handler.next(options);
        },
        onError: (error, handler) async {
          // 处理连接超时等常见错误
          String errorMessage = _getErrorMessage(error);
          print('API Error: $errorMessage');

          if (error.response?.statusCode == 401) {
            // 处理401未授权
            print('认证失败，需要重新登录');
          }

          return handler.next(error);
        },
      ),
    );
  }

  /// 重新初始化Dio（当API地址改变时调用）
  void refreshBaseUrl() {
    _dio.options.baseUrl = AppConfig.apiBaseUrl;
  }

  String _getErrorMessage(DioException error) {
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
        return '连接超时，请检查网络设置或服务器地址';
      case DioExceptionType.sendTimeout:
        return '发送超时，请检查网络连接';
      case DioExceptionType.receiveTimeout:
        return '接收超时，请检查服务器是否正常运行';
      case DioExceptionType.badResponse:
        return '服务器响应错误: ${error.response?.statusCode}';
      case DioExceptionType.cancel:
        return '请求已取消';
      case DioExceptionType.unknown:
        return '网络连接失败，请检查网络设置';
      case DioExceptionType.connectionError:
        return '无法连接到服务器，请确认服务器地址和端口正确';
      case DioExceptionType.badCertificate:
        return '证书验证失败';
    }
  }

  void setToken(String token) {
    _token = token;
  }

  void clearToken() {
    _token = null;
  }

  Future<Response> get(
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.get(
        path,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      _handleError(e);
      rethrow;
    }
  }

  Future<Response> post(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.post(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      _handleError(e);
      rethrow;
    }
  }

  Future<Response> put(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.put(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      _handleError(e);
      rethrow;
    }
  }

  Future<Response> patch(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.patch(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      _handleError(e);
      rethrow;
    }
  }

  Future<Response> delete(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.delete(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      _handleError(e);
      rethrow;
    }
  }

  Future<Response> uploadFile(
    String path,
    String filePath, {
    String fieldName = 'file',
    Map<String, dynamic>? data,
    Options? options,
    void Function(int, int)? onSendProgress,
  }) async {
    try {
      final formData = FormData.fromMap({
        ...?data,
        fieldName: await MultipartFile.fromFile(filePath),
      });

      return await _dio.post(
        path,
        data: formData,
        options: options,
        onSendProgress: onSendProgress,
      );
    } on DioException catch (e) {
      _handleError(e);
      rethrow;
    }
  }

  void _handleError(DioException e) {
    if (e.response != null) {
      print('API错误响应: ${e.response?.statusCode} - ${e.response?.data}');
    } else {
      print('网络错误: ${e.message}');
    }
  }

  /// 测试连接是否正常
  Future<bool> testConnection() async {
    try {
      // 尝试访问根路径或健康检查端点
      await _dio.get('/');
      return true;
    } catch (e) {
      print('连接测试失败: $e');
      return false;
    }
  }
}

final apiServiceProvider = Provider<ApiService>((ref) {
  return ApiService();
});
