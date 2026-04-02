using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Notification实体
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 索引配置
                entity.HasIndex(e => e.UserId); // 按用户查询
                entity.HasIndex(e => new { e.UserId, e.Status }); // 按用户和状态查询
                entity.HasIndex(e => new { e.UserId, e.Type }); // 按用户和类型查询
                entity.HasIndex(e => e.Type); // 按类型查询
                entity.HasIndex(e => e.Status); // 按状态查询
                entity.HasIndex(e => e.CreatedAt); // 按创建时间查询
                entity.HasIndex(e => e.ExpiresAt); // 按过期时间查询
                entity.HasIndex(e => new { e.UserId, e.IsImportant }); // 按用户和重要性查询
                entity.HasIndex(e => new { e.UserId, e.Priority }); // 按用户和优先级查询
                entity.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId }); // 按相关实体查询

                // 字段配置
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MetadataJson).HasMaxLength(1000);
                entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
                entity.Property(e => e.ActionUrl).HasMaxLength(500);
                entity.Property(e => e.Icon).HasMaxLength(100);
                entity.Property(e => e.Tags).HasMaxLength(500);

                // 默认值
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.IsImportant).HasDefaultValue(false);
                entity.Property(e => e.Priority).HasDefaultValue(0);
            });

            // 配置NotificationTemplate实体
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 索引配置
                entity.HasIndex(e => e.Name).IsUnique(); // 模板名称唯一
                entity.HasIndex(e => e.Type); // 按类型查询
                entity.HasIndex(e => e.IsActive); // 按启用状态查询
                entity.HasIndex(e => e.CreatedAt); // 按创建时间查询

                // 字段配置
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TitleTemplate).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContentTemplate).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.VariablesJson).HasMaxLength(1000);
                entity.Property(e => e.Description).HasMaxLength(500);

                // 默认值
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // 配置NotificationSettings实体
            modelBuilder.Entity<NotificationSettings>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 复合唯一索引：用户+通知类型+渠道的唯一设置
                entity.HasIndex(e => new { e.UserId, e.NotificationType, e.Channel }).IsUnique();
                entity.HasIndex(e => e.UserId); // 按用户查询
                entity.HasIndex(e => e.NotificationType); // 按通知类型查询
                entity.HasIndex(e => e.Channel); // 按渠道查询
                entity.HasIndex(e => e.IsEnabled); // 按启用状态查询

                // 字段配置
                entity.Property(e => e.NotificationType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Channel).IsRequired().HasMaxLength(20);

                // 默认值
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            });
        }
    }
}