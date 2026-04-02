using Microsoft.EntityFrameworkCore;
using SearchService.Entities;

namespace SearchService.Data
{
    public class SearchDbContext : DbContext
    {
        public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
        {
        }

        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<SearchIndex> SearchIndices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置SearchHistory实体
            modelBuilder.Entity<SearchHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Query);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SearchType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });

                entity.Property(e => e.Query).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SearchType).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.DeviceType).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.MetadataJson).HasMaxLength(1000);
            });

            // 配置SearchIndex实体
            modelBuilder.Entity<SearchIndex>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IndexType);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IndexedAt);
                entity.HasIndex(e => new { e.IndexType, e.EntityId }).IsUnique();
                entity.HasIndex(e => new { e.IndexType, e.Status });
                entity.HasIndex(e => e.LastUpdatedAt);
                entity.HasIndex(e => e.LastAccessedAt);

                entity.Property(e => e.IndexType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.MetadataJson).HasMaxLength(1000);
                entity.Property(e => e.Tags).HasMaxLength(500);
            });
        }
    }
}