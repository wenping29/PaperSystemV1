class Article {
  final String id;
  final String title;
  final String content;
  final String authorId;
  final String authorName;
  final List<String> tags;
  final ArticleStatus status;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final DateTime? publishedAt;
  final int views;
  final int likes;
  final int comments;
  final bool isLiked;

  const Article({
    required this.id,
    required this.title,
    required this.content,
    required this.authorId,
    required this.authorName,
    this.tags = const [],
    this.status = ArticleStatus.draft,
    required this.createdAt,
    this.updatedAt,
    this.publishedAt,
    this.views = 0,
    this.likes = 0,
    this.comments = 0,
    this.isLiked = false,
  });

  factory Article.fromJson(Map<String, dynamic> json) {
    return Article(
      id: json['id'] ?? '',
      title: json['title'] ?? '',
      content: json['content'] ?? '',
      authorId: json['author_id'] ?? json['authorId'] ?? '',
      authorName: json['author_name'] ?? json['authorName'] ?? '',
      tags: (json['tags'] as List<dynamic>?)?.map((e) => e.toString()).toList() ?? [],
      status: ArticleStatus.fromString(json['status'] ?? 'draft'),
      createdAt: json['created_at'] != null
          ? DateTime.tryParse(json['created_at']) ?? DateTime.now()
          : DateTime.now(),
      updatedAt: json['updated_at'] != null
          ? DateTime.tryParse(json['updated_at'])
          : null,
      publishedAt: json['published_at'] != null
          ? DateTime.tryParse(json['published_at'])
          : null,
      views: json['views'] ?? json['view_count'] ?? 0,
      likes: json['likes'] ?? json['like_count'] ?? 0,
      comments: json['comments'] ?? json['comment_count'] ?? 0,
      isLiked: json['is_liked'] ?? json['isLiked'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'title': title,
      'content': content,
      'author_id': authorId,
      'author_name': authorName,
      'tags': tags,
      'status': status.value,
      'created_at': createdAt.toIso8601String(),
      'updated_at': updatedAt?.toIso8601String(),
      'published_at': publishedAt?.toIso8601String(),
      'views': views,
      'likes': likes,
      'comments': comments,
      'is_liked': isLiked,
    };
  }

  Article copyWith({
    String? id,
    String? title,
    String? content,
    String? authorId,
    String? authorName,
    List<String>? tags,
    ArticleStatus? status,
    DateTime? createdAt,
    DateTime? updatedAt,
    DateTime? publishedAt,
    int? views,
    int? likes,
    int? comments,
    bool? isLiked,
  }) {
    return Article(
      id: id ?? this.id,
      title: title ?? this.title,
      content: content ?? this.content,
      authorId: authorId ?? this.authorId,
      authorName: authorName ?? this.authorName,
      tags: tags ?? this.tags,
      status: status ?? this.status,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
      publishedAt: publishedAt ?? this.publishedAt,
      views: views ?? this.views,
      likes: likes ?? this.likes,
      comments: comments ?? this.comments,
      isLiked: isLiked ?? this.isLiked,
    );
  }

  bool get isDraft => status == ArticleStatus.draft;
  bool get isPublished => status == ArticleStatus.published;

  String get formattedCreateTime {
    final now = DateTime.now();
    final difference = now.difference(createdAt);

    if (difference.inDays > 365) {
      return '${(difference.inDays / 365).floor()}年前';
    } else if (difference.inDays > 30) {
      return '${(difference.inDays / 30).floor()}个月前';
    } else if (difference.inDays > 0) {
      return '${difference.inDays}天前';
    } else if (difference.inHours > 0) {
      return '${difference.inHours}小时前';
    } else if (difference.inMinutes > 0) {
      return '${difference.inMinutes}分钟前';
    } else {
      return '刚刚';
    }
  }

  String get formattedUpdateTime {
    if (updatedAt == null) return '';
    final now = DateTime.now();
    final difference = now.difference(updatedAt!);

    if (difference.inDays > 365) {
      return '${(difference.inDays / 365).floor()}年前更新';
    } else if (difference.inDays > 30) {
      return '${(difference.inDays / 30).floor()}个月前更新';
    } else if (difference.inDays > 0) {
      return '${difference.inDays}天前更新';
    } else if (difference.inHours > 0) {
      return '${difference.inHours}小时前更新';
    } else if (difference.inMinutes > 0) {
      return '${difference.inMinutes}分钟前更新';
    } else {
      return '刚刚更新';
    }
  }
}

enum ArticleStatus {
  draft('draft'),
  published('published'),
  archived('archived');

  final String value;

  const ArticleStatus(this.value);

  factory ArticleStatus.fromString(String value) {
    switch (value) {
      case 'published':
        return ArticleStatus.published;
      case 'archived':
        return ArticleStatus.archived;
      case 'draft':
      default:
        return ArticleStatus.draft;
    }
  }

  String get displayName {
    switch (this) {
      case ArticleStatus.draft:
        return '草稿';
      case ArticleStatus.published:
        return '已发布';
      case ArticleStatus.archived:
        return '已归档';
    }
  }
}