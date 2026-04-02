import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../features/writing/models/article_model.dart';
import '../../../features/auth/providers/auth_provider.dart';

class SquarePage extends ConsumerStatefulWidget {
  const SquarePage({super.key});

  @override
  ConsumerState<SquarePage> createState() => _SquarePageState();
}

class _SquarePageState extends ConsumerState<SquarePage> {
  final List<String> _categories = ['全部', '热门', '最新', '关注'];
  int _selectedCategoryIndex = 0;
  final List<String> _hotTags = [
    'AI写作', '科技', '文学', '教程', '心得',
    '创意', '工具', '技巧', '分享', '推荐'
  ];
  String? _selectedTag;

  Future<List<Article>> _loadArticles() async {
    // TODO: 从API加载广场文章，根据分类和标签筛选
    await Future.delayed(const Duration(milliseconds: 500));
    return [
      Article(
        id: '1',
        title: 'AI写作工具全面评测',
        content: '对比市面上主流的AI写作工具，分析各自的优缺点和适用场景。',
        authorId: '10',
        authorName: '科技评测师',
        tags: ['AI写作', '工具', '评测'],
        status: ArticleStatus.published,
        createdAt: DateTime.now().subtract(const Duration(hours: 1)),
        views: 1500,
        likes: 320,
        comments: 56,
        isLiked: false,
      ),
      Article(
        id: '2',
        title: '如何利用AI提升创作效率',
        content: '分享使用AI辅助写作的实战经验和技巧，帮助创作者提升效率。',
        authorId: '11',
        authorName: '效率达人',
        tags: ['AI写作', '效率', '技巧'],
        status: ArticleStatus.published,
        createdAt: DateTime.now().subtract(const Duration(hours: 3)),
        views: 890,
        likes: 210,
        comments: 34,
        isLiked: true,
      ),
      Article(
        id: '3',
        title: '文学创作中的AI应用探索',
        content: '探讨AI在小说、诗歌等文学创作领域的应用可能性和挑战。',
        authorId: '12',
        authorName: '文学探索者',
        tags: ['文学', 'AI写作', '创作'],
        status: ArticleStatus.published,
        createdAt: DateTime.now().subtract(const Duration(days: 1)),
        views: 2300,
        likes: 450,
        comments: 89,
        isLiked: false,
      ),
      Article(
        id: '4',
        title: '从0到1：我的第一篇AI辅助文章',
        content: '记录新手如何使用AI写作工具完成第一篇完整文章的完整过程。',
        authorId: '13',
        authorName: '新手创作者',
        tags: ['新手', '教程', 'AI写作'],
        status: ArticleStatus.published,
        createdAt: DateTime.now().subtract(const Duration(days: 2)),
        views: 1200,
        likes: 280,
        comments: 45,
        isLiked: false,
      ),
      Article(
        id: '5',
        title: 'AI写作的未来发展趋势',
        content: '分析AI写作技术的发展趋势，展望未来的应用场景和可能性。',
        authorId: '14',
        authorName: '未来观察家',
        tags: ['未来', '趋势', 'AI写作'],
        status: ArticleStatus.published,
        createdAt: DateTime.now().subtract(const Duration(days: 3)),
        views: 3100,
        likes: 620,
        comments: 120,
        isLiked: true,
      ),
    ];
  }

  Widget _buildCategoryChips() {
    return SizedBox(
      height: 48,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: _categories.length,
        itemBuilder: (context, index) {
          final isSelected = index == _selectedCategoryIndex;
          return Padding(
            padding: EdgeInsets.only(
              left: index == 0 ? 16 : 8,
              right: index == _categories.length - 1 ? 16 : 8,
            ),
            child: ChoiceChip(
              label: Text(_categories[index]),
              selected: isSelected,
              onSelected: (selected) {
                setState(() {
                  _selectedCategoryIndex = index;
                  // TODO: 根据分类重新加载文章
                });
              },
              selectedColor: Theme.of(context).colorScheme.primary,
              labelStyle: TextStyle(
                color: isSelected ? Colors.white : Colors.grey.shade700,
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildHotTags() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        border: Border(
          top: BorderSide(color: Colors.grey.shade200),
          bottom: BorderSide(color: Colors.grey.shade200),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.local_fire_department, size: 18, color: Colors.orange),
              const SizedBox(width: 6),
              Text(
                '热门标签',
                style: TextStyle(
                  fontWeight: FontWeight.w500,
                  color: Colors.grey.shade700,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: _hotTags.map((tag) {
              final isSelected = tag == _selectedTag;
              return GestureDetector(
                onTap: () {
                  setState(() {
                    _selectedTag = isSelected ? null : tag;
                    // TODO: 根据标签筛选文章
                  });
                },
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  decoration: BoxDecoration(
                    color: isSelected
                        ? Theme.of(context).colorScheme.primary.withAlpha(26)
                        : Colors.grey.shade100,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(
                      color: isSelected
                          ? Theme.of(context).colorScheme.primary
                          : Colors.grey.shade300,
                      width: 1,
                    ),
                  ),
                  child: Text(
                    tag,
                    style: TextStyle(
                      fontSize: 12,
                      color: isSelected
                          ? Theme.of(context).colorScheme.primary
                          : Colors.grey.shade700,
                    ),
                  ),
                ),
              );
            }).toList(),
          ),
          if (_selectedTag != null) ...[
            const SizedBox(height: 12),
            Row(
              children: [
                Text(
                  '已选择: $_selectedTag',
                  style: TextStyle(
                    fontSize: 12,
                    color: Theme.of(context).colorScheme.primary,
                  ),
                ),
                const Spacer(),
                GestureDetector(
                  onTap: () {
                    setState(() {
                      _selectedTag = null;
                    });
                  },
                  child: Text(
                    '清除',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildArticleCard(BuildContext context, Article article) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: InkWell(
        onTap: () {
          // TODO: 跳转到文章详情页
        },
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // 标签和热度
              Row(
                children: [
                  if (article.tags.isNotEmpty)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: Theme.of(context).colorScheme.primary.withAlpha(26),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        article.tags.first,
                        style: TextStyle(
                          fontSize: 10,
                          color: Theme.of(context).colorScheme.primary,
                        ),
                      ),
                    ),
                  const Spacer(),
                  Row(
                    children: [
                      Icon(Icons.local_fire_department,
                          size: 14, color: Colors.orange),
                      const SizedBox(width: 4),
                      Text(
                        '热门',
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.grey.shade600,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
              const SizedBox(height: 12),
              // 标题
              Text(
                article.title,
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
              const SizedBox(height: 8),
              // 内容摘要
              Text(
                article.content.length > 80
                    ? '${article.content.substring(0, 80)}...'
                    : article.content,
                style: Theme.of(context).textTheme.bodyMedium,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
              const SizedBox(height: 12),
              // 作者信息和统计
              Row(
                children: [
                  CircleAvatar(
                    radius: 12,
                    backgroundColor: Colors.grey.shade200,
                    child: const Icon(Icons.person, size: 14),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      article.authorName,
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.grey.shade700,
                      ),
                    ),
                  ),
                  Row(
                    children: [
                      Icon(Icons.remove_red_eye_outlined,
                          size: 12, color: Colors.grey.shade600),
                      const SizedBox(width: 4),
                      Text(
                        '${article.views}',
                        style: TextStyle(
                          fontSize: 11,
                          color: Colors.grey.shade600,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Icon(Icons.favorite_outline,
                          size: 12, color: Colors.grey.shade600),
                      const SizedBox(width: 4),
                      Text(
                        '${article.likes}',
                        style: TextStyle(
                          fontSize: 11,
                          color: Colors.grey.shade600,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
              const SizedBox(height: 8),
              // 发布时间
              Text(
                article.formattedCreateTime,
                style: TextStyle(
                  fontSize: 11,
                  color: Colors.grey.shade500,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('写作广场'),
        actions: [
          IconButton(
            icon: const Icon(Icons.filter_list),
            onPressed: () {
              // TODO: 显示高级筛选
            },
          ),
        ],
      ),
      body: FutureBuilder<List<Article>>(
        future: _loadArticles(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text('加载失败'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () {
                      setState(() {});
                    },
                    child: const Text('重试'),
                  ),
                ],
              ),
            );
          }

          final articles = snapshot.data ?? [];

          return Column(
            children: [
              _buildCategoryChips(),
              _buildHotTags(),
              Expanded(
                child: RefreshIndicator(
                  onRefresh: () async {
                    setState(() {});
                  },
                  child: articles.isEmpty
                      ? Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(
                                Icons.explore_outlined,
                                size: 64,
                                color: Colors.grey.shade400,
                              ),
                              const SizedBox(height: 16),
                              Text(
                                '暂无文章',
                                style: TextStyle(
                                  color: Colors.grey.shade600,
                                ),
                              ),
                            ],
                          ),
                        )
                      : ListView.builder(
                          itemCount: articles.length,
                          itemBuilder: (context, index) {
                            return _buildArticleCard(context, articles[index]);
                          },
                        ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}