using Microsoft.EntityFrameworkCore;
using WritingService.Entities;

namespace WritingService.Data
{
    public class WritingDbContext : DbContext
    {
        public WritingDbContext(DbContextOptions<WritingDbContext> options) : base(options) { }

        public DbSet<Work> Works { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<WorkVersion> WorkVersions { get; set; }
        public DbSet<WorkCollaborator> WorkCollaborators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Work 配置
            modelBuilder.Entity<Work>(entity =>
            {
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.TagsJson).HasMaxLength(1000);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Works)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Category 配置
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.ParentCategoryId);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.SortOrder);

                entity.HasOne(e => e.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(e => e.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Template 配置
            modelBuilder.Entity<Template>(entity =>
            {
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsPublic);
                entity.HasIndex(e => e.UsageCount);

                entity.Property(e => e.TagsJson).HasMaxLength(1000);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // WorkVersion 配置
            modelBuilder.Entity<WorkVersion>(entity =>
            {
                entity.HasIndex(e => e.WorkId);
                entity.HasIndex(e => e.VersionNumber);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Work)
                      .WithMany(w => w.Versions)
                      .HasForeignKey(e => e.WorkId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 唯一约束：每个作品的版本号必须唯一
                entity.HasIndex(e => new { e.WorkId, e.VersionNumber }).IsUnique();
            });

            // WorkCollaborator 配置
            modelBuilder.Entity<WorkCollaborator>(entity =>
            {
                entity.HasIndex(e => e.WorkId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.WorkId, e.UserId }).IsUnique();

                entity.HasOne(e => e.Work)
                      .WithMany(w => w.Collaborators)
                      .HasForeignKey(e => e.WorkId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}