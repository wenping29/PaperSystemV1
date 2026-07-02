import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../services/api_service.dart';
import '../models/article_model.dart';
import '../models/work_version_model.dart';

class ArticleDetailState {
  final AsyncValue<Article> article;
  final AsyncValue<List<WorkVersion>> versions;

  const ArticleDetailState({
    this.article = const AsyncValue.loading(),
    this.versions = const AsyncValue.loading(),
  });

  ArticleDetailState copyWith({
    AsyncValue<Article>? article,
    AsyncValue<List<WorkVersion>>? versions,
  }) {
    return ArticleDetailState(
      article: article ?? this.article,
      versions: versions ?? this.versions,
    );
  }
}

class ArticleDetailNotifier extends StateNotifier<ArticleDetailState> {
  final ApiService _apiService;

  ArticleDetailNotifier(this._apiService) : super(const ArticleDetailState());

  Future<void> loadArticle(String articleId) async {
    state = state.copyWith(article: const AsyncValue.loading());
    try {
      final response = await _apiService.get('/works/$articleId');
      state = state.copyWith(
        article: AsyncValue.data(Article.fromJson(response.data)),
      );
    } catch (e, st) {
      state = state.copyWith(article: AsyncValue.error(e, st));
    }
  }

  Future<void> loadVersions(String articleId) async {
    state = state.copyWith(versions: const AsyncValue.loading());
    try {
      final response = await _apiService.get('/works/$articleId/versions');
      final list = (response.data['data'] as List<dynamic>)
          .map((e) => WorkVersion.fromJson(e as Map<String, dynamic>))
          .toList();
      state = state.copyWith(versions: AsyncValue.data(list));
    } catch (e, st) {
      state = state.copyWith(versions: AsyncValue.error(e, st));
    }
  }
}

final articleDetailProvider = StateNotifierProvider<ArticleDetailNotifier, ArticleDetailState>((ref) {
  final apiService = ref.watch(apiServiceProvider);
  return ArticleDetailNotifier(apiService);
});
