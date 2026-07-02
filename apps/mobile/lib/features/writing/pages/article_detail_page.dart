import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../providers/article_detail_provider.dart';
import '../models/article_model.dart';
import '../models/work_version_model.dart';

class ArticleDetailPage extends ConsumerStatefulWidget {
  final String articleId;

  const ArticleDetailPage({super.key, required this.articleId});

  @override
  ConsumerState<ArticleDetailPage> createState() => _ArticleDetailPageState();
}

class _ArticleDetailPageState extends ConsumerState<ArticleDetailPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(articleDetailProvider.notifier).loadArticle(widget.articleId);
      ref.read(articleDetailProvider.notifier).loadVersions(widget.articleId);
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(articleDetailProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('文章详情'),
      ),
      body: state.article.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Text('加载失败'),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () {
                  ref
                      .read(articleDetailProvider.notifier)
                      .loadArticle(widget.articleId);
                },
                child: const Text('重试'),
              ),
            ],
          ),
        ),
        data: (article) => _buildContent(article, state.versions),
      ),
    );
  }

  Widget _buildContent(Article article, AsyncValue<List<WorkVersion>> versions) {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildHeader(article),
          const Divider(),
          _buildContentBody(article),
          const Divider(),
          _buildVersionsSection(versions),
        ],
      ),
    );
  }

  Widget _buildHeader(Article article) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              CircleAvatar(
                radius: 20,
                backgroundColor: Colors.grey.shade200,
                child: const Icon(Icons.person, size: 20),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      article.authorName,
                      style: const TextStyle(
                        fontWeight: FontWeight.w500,
                        fontSize: 15,
                      ),
                    ),
                    Text(
                      article.formattedCreateTime,
                      style: TextStyle(
                        fontSize: 13,
                        color: Colors.grey.shade600,
                      ),
                    ),
                  ],
                ),
              ),
              Row(
                children: [
                  Icon(Icons.remove_red_eye_outlined,
                      size: 16, color: Colors.grey.shade600),
                  const SizedBox(width: 4),
                  Text(
                    '${article.views}',
                    style: TextStyle(fontSize: 13, color: Colors.grey.shade600),
                  ),
                  const SizedBox(width: 16),
                  Icon(Icons.favorite_outline,
                      size: 16, color: Colors.grey.shade600),
                  const SizedBox(width: 4),
                  Text(
                    '${article.likes}',
                    style: TextStyle(fontSize: 13, color: Colors.grey.shade600),
                  ),
                ],
              ),
            ],
          ),
          const SizedBox(height: 16),
          Text(
            article.title,
            style: const TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 8),
          if (article.tags.isNotEmpty)
            Wrap(
              spacing: 6,
              children: article.tags.map((tag) => Chip(
                label: Text(
                  tag,
                  style: TextStyle(
                    fontSize: 11,
                    color: Theme.of(context).colorScheme.primary,
                  ),
                ),
                backgroundColor:
                    Theme.of(context).colorScheme.primary.withAlpha(26),
                visualDensity: VisualDensity.compact,
                materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                padding: const EdgeInsets.symmetric(horizontal: 4),
              )).toList(),
            ),
          if (article.publishedAt != null) ...[
            const SizedBox(height: 8),
            Text(
              '发布于 ${article.publishedAt!.year}-${article.publishedAt!.month.toString().padLeft(2, '0')}-${article.publishedAt!.day.toString().padLeft(2, '0')}',
              style: TextStyle(fontSize: 13, color: Colors.grey.shade500),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildContentBody(Article article) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            '正文',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 12),
          Text(
            article.content,
            style: const TextStyle(
              fontSize: 16,
              height: 1.8,
            ),
          ),
          const SizedBox(height: 24),
          if (article.updatedAt != null)
            Text(
              '最后编辑于 ${article.updatedAt!.year}-${article.updatedAt!.month.toString().padLeft(2, '0')}-${article.updatedAt!.day.toString().padLeft(2, '0')} ${article.updatedAt!.hour.toString().padLeft(2, '0')}:${article.updatedAt!.minute.toString().padLeft(2, '0')}',
              style: TextStyle(fontSize: 13, color: Colors.grey.shade400),
            ),
        ],
      ),
    );
  }

  Widget _buildVersionsSection(AsyncValue<List<WorkVersion>> versions) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Text(
                '修改记录',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(width: 8),
              versions.when(
                loading: () =>
                    const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2)),
                data: (list) => Text(
                  '共 ${list.length} 条',
                  style: TextStyle(fontSize: 13, color: Colors.grey.shade500),
                ),
                error: (_, __) => const SizedBox.shrink(),
              ),
            ],
          ),
          const SizedBox(height: 12),
          versions.when(
            loading: () => const Center(
              child: Padding(
                padding: EdgeInsets.all(24),
                child: CircularProgressIndicator(),
              ),
            ),
            error: (e, _) => Center(
              child: Text(
                '加载失败',
                style: TextStyle(color: Colors.grey.shade500),
              ),
            ),
            data: (list) {
              if (list.isEmpty) {
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Text(
                      '暂无修改记录',
                      style: TextStyle(color: Colors.grey.shade500),
                    ),
                  ),
                );
              }
              return ListView.separated(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: list.length,
                separatorBuilder: (_, __) => const Divider(height: 1),
                itemBuilder: (context, index) {
                  final version = list[index];
                  return ListTile(
                    contentPadding: EdgeInsets.zero,
                    leading: CircleAvatar(
                      radius: 16,
                      backgroundColor: version.versionNumber == 1
                          ? Colors.green.shade100
                          : Colors.blue.shade100,
                      child: Text(
                        'v${version.versionNumber}',
                        style: TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                          color: version.versionNumber == 1
                              ? Colors.green.shade700
                              : Colors.blue.shade700,
                        ),
                      ),
                    ),
                    title: Text(
                      version.changeDescription ?? (version.versionNumber == 1 ? '创建作品' : '更新作品'),
                      style: const TextStyle(fontSize: 14),
                    ),
                    subtitle: Text(
                      version.createdAt.year.toString() +
                          '-' +
                          version.createdAt.month.toString().padLeft(2, '0') +
                          '-' +
                          version.createdAt.day.toString().padLeft(2, '0') +
                          ' ' +
                          version.createdAt.hour.toString().padLeft(2, '0') +
                          ':' +
                          version.createdAt.minute.toString().padLeft(2, '0'),
                      style: TextStyle(fontSize: 12, color: Colors.grey.shade500),
                    ),
                    trailing: Text(
                      '${version.wordCount}字',
                      style: TextStyle(fontSize: 12, color: Colors.grey.shade500),
                    ),
                  );
                },
              );
            },
          ),
        ],
      ),
    );
  }
}
