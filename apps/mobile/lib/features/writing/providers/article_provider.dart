import 'dart:async';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/article_model.dart';
import '../../../services/api_service.dart';

class ArticleState {
  final Map<String, Article> articles; // articleId -> Article
  final Set<String> loadingArticles; // 正在加载的文章ID
  final Set<String> failedArticles; // 加载失败的文章ID
  final String? currentArticleId; // 当前正在编辑的文章ID
  final bool isSaving;
  final bool isPublishing;
  final String? saveError;
  final String? publishError;

  const ArticleState({
    this.articles = const {},
    this.loadingArticles = const {},
    this.failedArticles = const {},
    this.currentArticleId,
    this.isSaving = false,
    this.isPublishing = false,
    this.saveError,
    this.publishError,
  });

  ArticleState copyWith({
    Map<String, Article>? articles,
    Set<String>? loadingArticles,
    Set<String>? failedArticles,
    String? currentArticleId,
    bool? isSaving,
    bool? isPublishing,
    String? saveError,
    String? publishError,
  }) {
    return ArticleState(
      articles: articles ?? this.articles,
      loadingArticles: loadingArticles ?? this.loadingArticles,
      failedArticles: failedArticles ?? this.failedArticles,
      currentArticleId: currentArticleId ?? this.currentArticleId,
      isSaving: isSaving ?? this.isSaving,
      isPublishing: isPublishing ?? this.isPublishing,
      saveError: saveError ?? this.saveError,
      publishError: publishError ?? this.publishError,
    );
  }

  Article? getArticle(String articleId) => articles[articleId];
  Article? get currentArticle => currentArticleId != null ? articles[currentArticleId] : null;
  bool isLoading(String articleId) => loadingArticles.contains(articleId);
  bool hasFailed(String articleId) => failedArticles.contains(articleId);
}

class ArticleNotifier extends StateNotifier<ArticleState> {
  final ApiService apiService;
  final Map<String, Completer<Article>> _pendingRequests = {};

  ArticleNotifier(this.apiService) : super(const ArticleState());

  Future<Article> getArticle(String articleId, {bool forceRefresh = false}) async {
    // 如果已经有文章数据且不需要强制刷新，直接返回
    if (!forceRefresh && state.articles.containsKey(articleId)) {
      return state.articles[articleId]!;
    }

    // 如果已经在请求中，等待现有请求完成
    if (_pendingRequests.containsKey(articleId)) {
      return await _pendingRequests[articleId]!.future;
    }

    // 开始新的请求
    final completer = Completer<Article>();
    _pendingRequests[articleId] = completer;

    // 更新加载状态
    state = state.copyWith(
      loadingArticles: {...state.loadingArticles, articleId},
      failedArticles: {...state.failedArticles}..remove(articleId),
    );

    try {
      // 调用API获取文章数据
      final response = await apiService.get('/articles/$articleId');

      if (response.statusCode == 200) {
        final article = Article.fromJson(response.data);

        // 更新状态
        final updatedArticles = Map<String, Article>.from(state.articles);
        updatedArticles[articleId] = article;

        state = state.copyWith(
          articles: updatedArticles,
          loadingArticles: {...state.loadingArticles}..remove(articleId),
        );

        completer.complete(article);
        return article;
      } else {
        throw Exception('Failed to load article: ${response.statusCode}');
      }
    } catch (e) {
      // 更新失败状态
      state = state.copyWith(
        loadingArticles: {...state.loadingArticles}..remove(articleId),
        failedArticles: {...state.failedArticles, articleId},
      );

      completer.completeError(e);
      rethrow;
    } finally {
      _pendingRequests.remove(articleId);
    }
  }

  Future<void> saveDraft({
    String? articleId,
    required String title,
    required String content,
    required String authorId,
    required String authorName,
    List<String> tags = const [],
  }) async {
    state = state.copyWith(isSaving: true, saveError: null);

    try {
      final data = {
        'title': title,
        'content': content,
        'author_id': authorId,
        'author_name': authorName,
        'tags': tags,
        'status': ArticleStatus.draft.value,
      };

      Response response;
      if (articleId != null && articleId.isNotEmpty) {
        // 更新现有草稿
        response = await apiService.put('/articles/$articleId', data: data);
      } else {
        // 创建新草稿
        response = await apiService.post('/articles', data: data);
      }

      if (response.statusCode == 200 || response.statusCode == 201) {
        final article = Article.fromJson(response.data);

        // 更新状态
        final updatedArticles = Map<String, Article>.from(state.articles);
        updatedArticles[article.id] = article;

        state = state.copyWith(
          articles: updatedArticles,
          currentArticleId: article.id,
          isSaving: false,
        );
      } else {
        throw Exception('保存失败: ${response.statusCode}');
      }
    } catch (e) {
      state = state.copyWith(
        isSaving: false,
        saveError: e.toString(),
      );
      rethrow;
    }
  }

  Future<void> publishArticle({
    required String articleId,
    required String title,
    required String content,
    required String authorId,
    required String authorName,
    List<String> tags = const [],
  }) async {
    state = state.copyWith(isPublishing: true, publishError: null);

    try {
      final data = {
        'title': title,
        'content': content,
        'author_id': authorId,
        'author_name': authorName,
        'tags': tags,
        'status': ArticleStatus.published.value,
        'published_at': DateTime.now().toIso8601String(),
      };

      final response = await apiService.put('/articles/$articleId', data: data);

      if (response.statusCode == 200) {
        final article = Article.fromJson(response.data);

        // 更新状态
        final updatedArticles = Map<String, Article>.from(state.articles);
        updatedArticles[article.id] = article;

        state = state.copyWith(
          articles: updatedArticles,
          currentArticleId: article.id,
          isPublishing: false,
        );
      } else {
        throw Exception('发布失败: ${response.statusCode}');
      }
    } catch (e) {
      state = state.copyWith(
        isPublishing: false,
        publishError: e.toString(),
      );
      rethrow;
    }
  }

  Future<void> deleteArticle(String articleId) async {
    try {
      final response = await apiService.delete('/articles/$articleId');

      if (response.statusCode == 200 || response.statusCode == 204) {
        // 从状态中移除文章
        final updatedArticles = Map<String, Article>.from(state.articles);
        updatedArticles.remove(articleId);

        state = state.copyWith(
          articles: updatedArticles,
          currentArticleId: state.currentArticleId == articleId ? null : state.currentArticleId,
          loadingArticles: {...state.loadingArticles}..remove(articleId),
          failedArticles: {...state.failedArticles}..remove(articleId),
        );
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> likeArticle(String articleId) async {
    try {
      final response = await apiService.post('/articles/$articleId/like');

      if (response.statusCode == 200 || response.statusCode == 201) {
        // 更新本地文章状态
        final article = state.articles[articleId];
        if (article != null) {
          final updatedArticle = article.copyWith(
            isLiked: true,
            likes: article.likes + 1,
          );

          final updatedArticles = Map<String, Article>.from(state.articles);
          updatedArticles[articleId] = updatedArticle;

          state = state.copyWith(articles: updatedArticles);
        }
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> unlikeArticle(String articleId) async {
    try {
      final response = await apiService.delete('/articles/$articleId/like');

      if (response.statusCode == 200 || response.statusCode == 204) {
        // 更新本地文章状态
        final article = state.articles[articleId];
        if (article != null) {
          final updatedArticle = article.copyWith(
            isLiked: false,
            likes: article.likes - 1,
          );

          final updatedArticles = Map<String, Article>.from(state.articles);
          updatedArticles[articleId] = updatedArticle;

          state = state.copyWith(articles: updatedArticles);
        }
      }
    } catch (e) {
      rethrow;
    }
  }

  void setCurrentArticle(String? articleId) {
    state = state.copyWith(currentArticleId: articleId);
  }

  void clearErrors() {
    state = state.copyWith(
      saveError: null,
      publishError: null,
    );
  }

  void clearCache() {
    state = const ArticleState();
    _pendingRequests.clear();
  }
}

final articleProvider = StateNotifierProvider<ArticleNotifier, ArticleState>((ref) {
  final apiService = ref.watch(apiServiceProvider);
  return ArticleNotifier(apiService);
});