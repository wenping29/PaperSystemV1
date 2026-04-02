import '../models/chat_model.dart';
import '../../../services/api_service.dart';

/// 聊天相关API服务
class ChatService {
  final ApiService _apiService;

  ChatService(this._apiService);

  /// 获取聊天列表（分页）
  /// [page] 页码，从1开始
  /// [limit] 每页数量
  Future<List<Chat>> getChats({int page = 1, int limit = 20}) async {
    final response = await _apiService.get('/chats', queryParameters: {
      'page': page,
      'limit': limit,
    });

    // 假设响应格式为 { "data": [], "pagination": {...} }
    final List<dynamic> data = response.data['data'] ?? [];
    return data.map((json) => Chat.fromJson(json)).toList();
  }

  /// 获取聊天详情
  Future<Chat> getChat(String chatId) async {
    final response = await _apiService.get('/chats/$chatId');
    return Chat.fromJson(response.data);
  }

  /// 发送消息
  Future<Message> sendMessage({
    required String chatId,
    required String content,
    MessageType type = MessageType.text,
    String? replyToId,
    Map<String, dynamic>? metadata,
  }) async {
    final response = await _apiService.post('/chats/$chatId/messages', data: {
      'content': content,
      'type': type.value,
      'reply_to_id': replyToId,
      'metadata': metadata,
    }..removeWhere((key, value) => value == null));
    return Message.fromJson(response.data);
  }

  /// 标记聊天为已读
  Future<void> markAsRead(String chatId) async {
    await _apiService.put('/chats/$chatId/read');
  }

  /// 创建新聊天（私聊）
  Future<Chat> createPrivateChat(String otherUserId) async {
    final response = await _apiService.post('/chats', data: {
      'other_user_id': otherUserId,
      'is_group': false,
    });
    return Chat.fromJson(response.data);
  }

  /// 创建群聊
  Future<Chat> createGroupChat({
    required String groupName,
    required List<String> memberIds,
  }) async {
    final response = await _apiService.post('/chats', data: {
      'group_name': groupName,
      'member_ids': memberIds,
      'is_group': true,
    });
    return Chat.fromJson(response.data);
  }

  /// 获取聊天消息列表（分页）
  Future<List<Message>> getMessages({
    required String chatId,
    int page = 1,
    int limit = 50,
  }) async {
    final response = await _apiService.get('/chats/$chatId/messages', queryParameters: {
      'page': page,
      'limit': limit,
    });

    final List<dynamic> data = response.data['data'] ?? [];
    return data.map((json) => Message.fromJson(json)).toList();
  }

  /// 删除聊天
  Future<void> deleteChat(String chatId) async {
    await _apiService.delete('/chats/$chatId');
  }

  /// 删除消息
  Future<void> deleteMessage(String chatId, String messageId) async {
    await _apiService.delete('/chats/$chatId/messages/$messageId');
  }
}