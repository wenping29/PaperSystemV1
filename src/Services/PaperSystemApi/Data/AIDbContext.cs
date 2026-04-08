using Microsoft.EntityFrameworkCore;
using PaperSystemApi.Models;

namespace PaperSystemApi.Data
{
    public class AIDbContext : DbContext
    {
        public AIDbContext(DbContextOptions<AIDbContext> options) : base(options)
        {
        }

        public DbSet<AIAuditLog> AIAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置AIAuditLog实体
            modelBuilder.Entity<AIAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ServiceType);
                entity.HasIndex(e => e.RequestType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => new { e.ServiceType, e.RequestType, e.CreatedAt });

                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequestType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.ModelUsed).HasMaxLength(50);
                entity.Property(e => e.Provider).HasMaxLength(100);
                entity.Property(e => e.ClientIp).HasMaxLength(100);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.CorrelationId).HasMaxLength(100);
                entity.Property(e => e.SessionId).HasMaxLength(100);

                // JSON列配置（MySQL的JSON类型）
                entity.Property(e => e.RequestData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                entity.Property(e => e.ResponseData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                // 计算列示例（如果需要）
                // entity.Property(e => e.RequestDate)
                //     .HasComputedColumnSql("DATE(CreatedAt)");
            });

            // 种子数据（可选）
            // 可以添加一些初始数据，例如默认的审计日志条目
        }
    }
}