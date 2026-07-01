import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../services/api_service.dart';
import '../../writing/models/article_model.dart';

class HomeData {
  final List<Article> featured;
  final List<Article> recent;
  final List<CategoryInfo> categories;
  final HomeStats stats;

  const HomeData({
    this.featured = const [],
    this.recent = const [],
    this.categories = const [],
    this.stats = const HomeStats(),
  });

  factory HomeData.fromJson(Map<String, dynamic> json) {
    return HomeData(
      featured: (json['featured'] as List<dynamic>?)
              ?.map((e) => Article.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      recent: (json['recent'] as List<dynamic>?)
              ?.map((e) => Article.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      categories: (json['categories'] as List<dynamic>?)
              ?.map((e) => CategoryInfo.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      stats: HomeStats.fromJson(json['stats'] as Map<String, dynamic>? ?? {}),
    );
  }
}

class CategoryInfo {
  final int id;
  final String name;
  final String? description;
  final int worksCount;

  const CategoryInfo({
    required this.id,
    required this.name,
    this.description,
    this.worksCount = 0,
  });

  factory CategoryInfo.fromJson(Map<String, dynamic> json) {
    return CategoryInfo(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      description: json['description'],
      worksCount: json['worksCount'] ?? 0,
    );
  }
}

class HomeStats {
  final int totalWorks;
  final int totalUsers;
  final int totalCategories;

  const HomeStats({
    this.totalWorks = 0,
    this.totalUsers = 0,
    this.totalCategories = 0,
  });

  factory HomeStats.fromJson(Map<String, dynamic> json) {
    return HomeStats(
      totalWorks: json['totalWorks'] ?? 0,
      totalUsers: json['totalUsers'] ?? 0,
      totalCategories: json['totalCategories'] ?? 0,
    );
  }
}

class HomeNotifier extends StateNotifier<AsyncValue<HomeData>> {
  final ApiService _apiService;

  HomeNotifier(this._apiService) : super(const AsyncValue.loading());

  Future<void> loadHomeData() async {
    try {
      state = const AsyncValue.loading();
      final response = await _apiService.get('/home');
      state = AsyncValue.data(HomeData.fromJson(response.data));
    } catch (e, st) {
      state = AsyncValue.error(e, st);
    }
  }
}

final homeProvider =
    StateNotifierProvider<HomeNotifier, AsyncValue<HomeData>>((ref) {
  final apiService = ref.watch(apiServiceProvider);
  return HomeNotifier(apiService);
});
