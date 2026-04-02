class User {
  final String id;
  final String username;
  final String? email;
  final String? avatarUrl;
  final String? bio;
  final DateTime? createdAt;
  final int followersCount;
  final int followingCount;
  final int postsCount;
  final bool isFollowing;

  const User({
    required this.id,
    required this.username,
    this.email,
    this.avatarUrl,
    this.bio,
    this.createdAt,
    this.followersCount = 0,
    this.followingCount = 0,
    this.postsCount = 0,
    this.isFollowing = false,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] ?? json['user_id'] ?? '',
      username: json['username'] ?? '',
      email: json['email'],
      avatarUrl: json['avatar_url'] ?? json['avatarUrl'],
      bio: json['bio'],
      createdAt: json['created_at'] != null
          ? DateTime.tryParse(json['created_at'])
          : null,
      followersCount: json['followers_count'] ?? json['followersCount'] ?? 0,
      followingCount: json['following_count'] ?? json['followingCount'] ?? 0,
      postsCount: json['posts_count'] ?? json['postsCount'] ?? 0,
      isFollowing: json['is_following'] ?? json['isFollowing'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'username': username,
      'email': email,
      'avatar_url': avatarUrl,
      'bio': bio,
      'created_at': createdAt?.toIso8601String(),
      'followers_count': followersCount,
      'following_count': followingCount,
      'posts_count': postsCount,
      'is_following': isFollowing,
    };
  }

  User copyWith({
    String? id,
    String? username,
    String? email,
    String? avatarUrl,
    String? bio,
    DateTime? createdAt,
    int? followersCount,
    int? followingCount,
    int? postsCount,
    bool? isFollowing,
  }) {
    return User(
      id: id ?? this.id,
      username: username ?? this.username,
      email: email ?? this.email,
      avatarUrl: avatarUrl ?? this.avatarUrl,
      bio: bio ?? this.bio,
      createdAt: createdAt ?? this.createdAt,
      followersCount: followersCount ?? this.followersCount,
      followingCount: followingCount ?? this.followingCount,
      postsCount: postsCount ?? this.postsCount,
      isFollowing: isFollowing ?? this.isFollowing,
    );
  }

  bool get hasAvatar => avatarUrl != null && avatarUrl!.isNotEmpty;

  bool get hasBio => bio != null && bio!.isNotEmpty;

  String get displayName => username;

  String? get formattedJoinDate {
    if (createdAt == null) return null;
    return '${createdAt!.year}年${createdAt!.month}月加入';
  }
}