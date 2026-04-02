import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../auth/providers/auth_provider.dart';
import '../models/article_model.dart';
import '../providers/article_provider.dart';

class WritingPage extends ConsumerStatefulWidget {
  final String? articleId; // 可选：编辑现有文章

  const WritingPage({
    super.key,
    this.articleId,
  });

  @override
  ConsumerState<WritingPage> createState() => _WritingPageState();
}

class _WritingPageState extends ConsumerState<WritingPage> {
  final _titleController = TextEditingController();
  final _contentController = TextEditingController();
  final _tagController = TextEditingController();
  final _titleFocusNode = FocusNode();
  final _contentFocusNode = FocusNode();
  final _tagFocusNode = FocusNode();

  List<String> _tags = [];
  bool _isLoading = true;
  Article? _currentArticle;

  @override
  void initState() {
    super.initState();
    _loadArticleIfNeeded();
  }

  @override
  void dispose() {
    _titleController.dispose();
    _contentController.dispose();
    _tagController.dispose();
    _titleFocusNode.dispose();
    _contentFocusNode.dispose();
    _tagFocusNode.dispose();
    super.dispose();
  }

  Future<void> _loadArticleIfNeeded() async {
    if (widget.articleId != null && widget.articleId!.isNotEmpty) {
      try {
        final article = await ref.read(articleProvider.notifier).getArticle(widget.articleId!);
        setState(() {
          _currentArticle = article;
          _titleController.text = article.title;
          _contentController.text = article.content;
          _tags = article.tags;
          _isLoading = false;
        });
      } catch (e) {
        // 加载失败，创建新文章
        setState(() {
          _isLoading = false;
        });
      }
    } else {
      setState(() {
        _isLoading = false;
      });
    }
  }

  Future<void> _saveDraft() async {
    final authState = ref.read(authProvider);
    if (authState.userId == null || authState.username == null) {
      _showError('请先登录');
      return;
    }

    if (_titleController.text.trim().isEmpty) {
      _showError('请输入标题');
      return;
    }

    try {
      await ref.read(articleProvider.notifier).saveDraft(
        articleId: _currentArticle?.id,
        title: _titleController.text.trim(),
        content: _contentController.text,
        authorId: authState.userId!,
        authorName: authState.username!,
        tags: _tags,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('草稿保存成功'),
            backgroundColor: Colors.green,
          ),
        );
      }
    } catch (e) {
      _showError('保存失败: $e');
    }
  }

  Future<void> _publishArticle() async {
    final authState = ref.read(authProvider);
    if (authState.userId == null || authState.username == null) {
      _showError('请先登录');
      return;
    }

    if (_titleController.text.trim().isEmpty) {
      _showError('请输入标题');
      return;
    }

    if (_contentController.text.trim().isEmpty) {
      _showError('请输入内容');
      return;
    }

    try {
      final articleId = _currentArticle?.id ?? '';
      await ref.read(articleProvider.notifier).publishArticle(
        articleId: articleId.isNotEmpty ? articleId : 'new',
        title: _titleController.text.trim(),
        content: _contentController.text,
        authorId: authState.userId!,
        authorName: authState.username!,
        tags: _tags,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('文章发布成功'),
            backgroundColor: Colors.green,
          ),
        );
        context.pop();
      }
    } catch (e) {
      _showError('发布失败: $e');
    }
  }

  void _showError(String message) {
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(message),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _addTag() {
    final tag = _tagController.text.trim();
    if (tag.isNotEmpty && !_tags.contains(tag)) {
      setState(() {
        _tags.add(tag);
        _tagController.clear();
      });
    }
  }

  void _removeTag(String tag) {
    setState(() {
      _tags.remove(tag);
    });
  }

  void _handleAIToolSelect(String tool) {
    // TODO: 调用AI工具
    _showAIToolDialog(tool);
  }

  void _showAIToolDialog(String tool) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('AI $tool'),
        content: const Text('AI功能正在开发中...'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('关闭'),
          ),
        ],
      ),
    );
  }

  Widget _buildAITools() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'AI写作助手',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 16,
            ),
          ),
          const SizedBox(height: 12),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              _buildAIToolButton('续写'),
              _buildAIToolButton('改写'),
              _buildAIToolButton('润色'),
              _buildAIToolButton('总结'),
              _buildAIToolButton('翻译'),
              _buildAIToolButton('生成标题'),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildAIToolButton(String tool) {
    return OutlinedButton(
      onPressed: () => _handleAIToolSelect(tool),
      style: OutlinedButton.styleFrom(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
        ),
      ),
      child: Text(tool),
    );
  }

  Widget _buildTagInput() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            '标签',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 16,
            ),
          ),
          const SizedBox(height: 8),
          TextField(
            controller: _tagController,
            focusNode: _tagFocusNode,
            decoration: InputDecoration(
              hintText: '输入标签，按回车添加',
              border: const OutlineInputBorder(),
              suffixIcon: IconButton(
                icon: const Icon(Icons.add),
                onPressed: _addTag,
              ),
            ),
            onSubmitted: (_) => _addTag(),
          ),
          const SizedBox(height: 12),
          if (_tags.isNotEmpty)
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: _tags.map((tag) => Chip(
                label: Text(tag),
                deleteIcon: const Icon(Icons.close, size: 16),
                onDeleted: () => _removeTag(tag),
              )).toList(),
            ),
        ],
      ),
    );
  }

  Widget _buildContentEditor() {
    return Expanded(
      child: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const SizedBox(height: 16),
            TextField(
              controller: _titleController,
              focusNode: _titleFocusNode,
              style: const TextStyle(
                fontSize: 24,
                fontWeight: FontWeight.bold,
              ),
              decoration: const InputDecoration(
                hintText: '请输入标题...',
                border: InputBorder.none,
              ),
              maxLines: 2,
            ),
            const SizedBox(height: 24),
            TextField(
              controller: _contentController,
              focusNode: _contentFocusNode,
              style: const TextStyle(fontSize: 16),
              decoration: const InputDecoration(
                hintText: '开始写作...',
                border: InputBorder.none,
              ),
              maxLines: null,
              keyboardType: TextInputType.multiline,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildActionButtons() {
    final articleState = ref.watch(articleProvider);
    final isSaving = articleState.isSaving;
    final isPublishing = articleState.isPublishing;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border(top: BorderSide(color: Colors.grey.shade300)),
      ),
      child: Row(
        children: [
          Expanded(
            child: ElevatedButton(
              onPressed: isSaving ? null : _saveDraft,
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 12),
                backgroundColor: Colors.grey.shade200,
                foregroundColor: Colors.grey.shade800,
              ),
              child: isSaving
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : Text(_currentArticle?.isDraft == true ? '更新草稿' : '保存草稿'),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: ElevatedButton(
              onPressed: isPublishing ? null : _publishArticle,
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 12),
              ),
              child: isPublishing
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : const Text('发布文章'),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(
          title: const Text('写作'),
        ),
        body: const Center(
          child: CircularProgressIndicator(),
        ),
      );
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('写作'),
        actions: [
          IconButton(
            icon: const Icon(Icons.help_outline),
            onPressed: () {
              // TODO: 显示帮助
            },
          ),
        ],
      ),
      body: Column(
        children: [
          _buildContentEditor(),
          const SizedBox(height: 16),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: _buildAITools(),
          ),
          const SizedBox(height: 16),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: _buildTagInput(),
          ),
          _buildActionButtons(),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _saveDraft,
        tooltip: '快速保存',
        child: const Icon(Icons.save),
      ),
    );
  }
}