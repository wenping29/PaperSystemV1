using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaperSystemApi.Data
{
    public class WritingDbContextFactory : IDesignTimeDbContextFactory<WritingDbContext>
    {
        public WritingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WritingDbContext>();
            optionsBuilder.UseSqlite("Data Source=Data/writing_platform_writing.db");

            return new WritingDbContext(optionsBuilder.Options);
        }
    }
}
