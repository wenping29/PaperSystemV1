using Microsoft.EntityFrameworkCore;
using CommunityService.Entities;

namespace CommunityService.Data
{
    public class CommunityDbContext : DbContext
    {
        public CommunityDbContext(DbContextOptions<CommunityDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Collection> Collections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Post实体
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.WorkId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Visibility);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.HotScore).IsDescending();

                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Summary);
                entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(1000);
                entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.Visibility).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.LikeCount).HasDefaultValue(0);
                entity.Property(e => e.CommentCount).HasDefaultValue(0);
                entity.Property(e => e.CollectionCount).HasDefaultValue(0);
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.HotScore).HasDefaultValue(0).HasPrecision(18, 6);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.PublishedAt);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // 一对多关系：Post -> Comments
                entity.HasMany(e => e.Comments)
                      .WithOne(e => e.Post)
                      .HasForeignKey(e => e.PostId)
                      .OnDelete(DeleteBehavior.Cascade);


                // 一对多关系：Post -> Collections
                entity.HasMany(e => e.Collections)
                      .WithOne(e => e.Post)
                      .HasForeignKey(e => e.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置Comment实体
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PostId);
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.LikeCount).HasDefaultValue(0);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // 自引用关系：Comment -> Replies
                entity.HasMany(e => e.Replies)
                      .WithOne(e => e.Parent)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置Like实体
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => e.Id);
                // 复合唯一索引：用户对同一目标只能点赞一次
                entity.HasIndex(e => new { e.UserId, e.TargetType, e.TargetId }).IsUnique();
                entity.HasIndex(e => new { e.TargetType, e.TargetId }); // 用于统计点赞数

                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.TargetType).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.TargetId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // 配置Collection实体
            modelBuilder.Entity<Collection>(entity =>
            {
                entity.HasKey(e => e.Id);
                // 复合唯一索引：用户不能重复收藏同一帖子
                entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PostId);

                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.PostId).IsRequired();
                entity.Property(e => e.Note).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            // 添加种子数据（可选）
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Title = "欢迎来到写作平台社区",
                    Content = "这是第一篇社区帖子，欢迎大家分享自己的作品和想法！",
                    Summary = "社区欢迎帖",
                    AuthorId = 1,
                    WorkId = null,
                    CoverImageUrl = null,
                    Tags = "欢迎,社区,公告",
                    Category = PostCategory.Other,
                    Status = PostStatus.Published,
                    Visibility = PostVisibility.Public,
                    LikeCount = 10,
                    CommentCount = 5,
                    CollectionCount = 3,
                    ViewCount = 100,
                    HotScore = 50.5m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PublishedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            );
        }
    }
}