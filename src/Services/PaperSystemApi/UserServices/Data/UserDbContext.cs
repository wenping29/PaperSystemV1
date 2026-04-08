using Microsoft.EntityFrameworkCore;
using PaperSystemApi.UserServices.Entities;

namespace PaperSystemApi.UserServices.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置User实体
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Bio).HasMaxLength(500);
                entity.Property(e => e.AvatarUrl).HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                // 一对一关系：User -> UserProfile
                entity.HasOne(e => e.Profile)
                      .WithOne(e => e.User)
                      .HasForeignKey<UserProfile>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置UserProfile实体
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Biography).HasMaxLength(1000);
                entity.Property(e => e.Gender).HasMaxLength(50);
                entity.Property(e => e.TwitterUrl).HasMaxLength(100);
                entity.Property(e => e.GitHubUrl).HasMaxLength(100);
                entity.Property(e => e.LinkedInUrl).HasMaxLength(100);
            });

            // 配置UserFollow实体（用户关注关系）
            modelBuilder.Entity<UserFollow>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 复合唯一索引：不能重复关注
                entity.HasIndex(e => new { e.FollowerId, e.FollowingId }).IsUnique();

                // 外键关系：关注者
                entity.HasOne(e => e.Follower)
                      .WithMany(e => e.Following)
                      .HasForeignKey(e => e.FollowerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 外键关系：被关注者
                entity.HasOne(e => e.Following)
                      .WithMany(e => e.Followers)
                      .HasForeignKey(e => e.FollowingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置RefreshToken实体
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RevokedByIp).HasMaxLength(50);
                entity.Property(e => e.ReplacedByToken).HasMaxLength(200);

                // 外键关系：RefreshToken -> User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置UserActivityLog实体
            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ActivityType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.ResourceType, e.ResourceId });

                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Details).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ResourceType).HasMaxLength(50);
                entity.Property(e => e.ActivityType).HasConversion<string>().HasMaxLength(50);

                // 外键关系：UserActivityLog -> User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 添加种子数据（可选）
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@writingplatform.com",
                    PasswordHash = "AQAAAAIAAYagAAAAENpBpO6J7ROv7LzmGQ7wKHfT4Nl1V8hGjH0yXpYpMk0=", // 密码：Admin@123
                    Role = UserRole.Admin,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}