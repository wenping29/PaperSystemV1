using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Models;

namespace PaperSystemApi.Data
{
    public class FriendshipDbContext : DbContext
    {
        public FriendshipDbContext(DbContextOptions<FriendshipDbContext> options) : base(options)
        {
        }

        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Friendship实体
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 复合唯一索引：确保同一对用户只有一个好友关系
                entity.HasIndex(e => new { e.UserId, e.FriendId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.FriendId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.LastInteractedAt);
                entity.HasIndex(e => new { e.UserId, e.IsFavorite });

                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Alias).HasMaxLength(100);
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.Property(e => e.MetadataJson).HasMaxLength(1000);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.PrivacySettings).HasMaxLength(500);
            });

            // 配置FriendRequest实体
            modelBuilder.Entity<FriendRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 复合唯一索引：防止重复请求
                entity.HasIndex(e => new { e.RequesterId, e.ReceiverId, e.Status }).IsUnique();
                entity.HasIndex(e => e.RequesterId);
                entity.HasIndex(e => e.ReceiverId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ExpiresAt);

                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.ResponseMessage).HasMaxLength(1000);
            });
        }
    }
}