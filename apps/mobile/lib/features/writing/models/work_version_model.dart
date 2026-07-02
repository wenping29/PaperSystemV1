class WorkVersion {
  final int id;
  final int workId;
  final int versionNumber;
  final String content;
  final String? title;
  final String? excerpt;
  final String? changeDescription;
  final int wordCount;
  final DateTime createdAt;
  final int createdBy;

  const WorkVersion({
    required this.id,
    required this.workId,
    required this.versionNumber,
    this.content = '',
    this.title,
    this.excerpt,
    this.changeDescription,
    this.wordCount = 0,
    required this.createdAt,
    this.createdBy = 0,
  });

  factory WorkVersion.fromJson(Map<String, dynamic> json) {
    return WorkVersion(
      id: json['id'] ?? 0,
      workId: json['workId'] ?? 0,
      versionNumber: json['versionNumber'] ?? 0,
      content: json['content'] ?? '',
      title: json['title'],
      excerpt: json['excerpt'],
      changeDescription: json['changeDescription'],
      wordCount: json['wordCount'] ?? 0,
      createdAt: json['createdAt'] != null
          ? DateTime.tryParse(json['createdAt']) ?? DateTime.now()
          : DateTime.now(),
      createdBy: json['createdBy'] ?? 0,
    );
  }
}
