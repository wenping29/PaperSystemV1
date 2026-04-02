using Microsoft.EntityFrameworkCore;
using FileService.Entities;

namespace FileService.Data
{
    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
        {
        }

        public DbSet<FileMetadata> FileMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置FileMetadata实体
            modelBuilder.Entity<FileMetadata>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.FileId).IsUnique();
                entity.HasIndex(e => e.UploadedByUserId);
                entity.HasIndex(e => e.FileType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => e.FileHash);
                entity.HasIndex(e => e.LastAccessedAt);

                entity.Property(e => e.FileId).IsRequired().HasMaxLength(36);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FileType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.MetadataJson).HasMaxLength(1000);
                entity.Property(e => e.FileHash).HasMaxLength(64);
                entity.Property(e => e.Tags).HasMaxLength(500);
            });
        }
    }
}