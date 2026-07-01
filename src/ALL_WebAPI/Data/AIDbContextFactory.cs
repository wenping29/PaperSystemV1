using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaperSystemApi.Data
{
    public class AIDbContextFactory : IDesignTimeDbContextFactory<AIDbContext>
    {
        public AIDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AIDbContext>();
            optionsBuilder.UseSqlite("Data Source=Data/writing_platform_ai.db");

            return new AIDbContext(optionsBuilder.Options);
        }
    }
}
