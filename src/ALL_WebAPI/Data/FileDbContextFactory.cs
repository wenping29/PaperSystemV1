using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaperSystemApi.Data
{
    public class FileDbContextFactory : IDesignTimeDbContextFactory<FileDbContext>
    {
        public FileDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FileDbContext>();
            optionsBuilder.UseSqlite("Data Source=Data/writing_platform_file.db");

            return new FileDbContext(optionsBuilder.Options);
        }
    }
}
