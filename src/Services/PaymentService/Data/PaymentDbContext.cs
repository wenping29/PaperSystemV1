using Microsoft.EntityFrameworkCore;
using PaymentService.Entities;

namespace PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<RefundTransaction> RefundTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置PaymentTransaction实体
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionNo).IsUnique();
                entity.HasIndex(e => e.GatewayTransactionNo);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => new { e.PaymentGateway, e.Status, e.CreatedAt });
                entity.HasIndex(e => new { e.PaymentType, e.Status, e.CreatedAt });

                entity.Property(e => e.TransactionNo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GatewayTransactionNo).HasMaxLength(100);
                entity.Property(e => e.PaymentGateway).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PaymentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ProductId).HasMaxLength(100);
                entity.Property(e => e.ProductName).HasMaxLength(200);
                entity.Property(e => e.ProductDescription).HasMaxLength(500);
                entity.Property(e => e.TargetUserId).HasMaxLength(100);
                entity.Property(e => e.TargetUserName).HasMaxLength(200);
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.Anonymous).HasMaxLength(50);
                entity.Property(e => e.ClientIp).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.DeviceInfo).HasMaxLength(100);
                entity.Property(e => e.Channel).HasMaxLength(100);
                entity.Property(e => e.ReturnUrl).HasMaxLength(100);
                entity.Property(e => e.NotifyUrl).HasMaxLength(100);
                entity.Property(e => e.Attach).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.ErrorCode).HasMaxLength(50);

                // JSON列配置
                entity.Property(e => e.GatewayRequestData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                entity.Property(e => e.GatewayResponseData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                entity.Property(e => e.GatewayCallbackData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                // 金额字段配置
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Fee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RefundedAmount).HasColumnType("decimal(18,2)");
            });

            // 配置RefundTransaction实体
            modelBuilder.Entity<RefundTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RefundNo).IsUnique();
                entity.HasIndex(e => e.GatewayRefundNo);
                entity.HasIndex(e => e.PaymentTransactionId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.PaymentGateway, e.Status, e.CreatedAt });

                entity.Property(e => e.RefundNo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GatewayRefundNo).HasMaxLength(100);
                entity.Property(e => e.PaymentGateway).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserEmail).HasMaxLength(100);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ReviewerId).HasMaxLength(100);
                entity.Property(e => e.ReviewerName).HasMaxLength(100);
                entity.Property(e => e.ReviewComment).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.ErrorCode).HasMaxLength(50);

                // JSON列配置
                entity.Property(e => e.GatewayRequestData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                entity.Property(e => e.GatewayResponseData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                entity.Property(e => e.GatewayCallbackData)
                    .HasColumnType("json")
                    .HasConversion(
                        v => v ?? string.Empty,
                        v => v);

                // 金额字段配置
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Fee).HasColumnType("decimal(18,2)");

                // 外键关系
                entity.HasOne(e => e.PaymentTransaction)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentTransactionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 种子数据（可选）
            // 可以添加一些测试数据
        }
    }
}