using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaperSystemApi.Data
{
    public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();
            optionsBuilder.UseSqlite("Data Source=Data/writing_platform_chat.db");

            return new ChatDbContext(optionsBuilder.Options);
        }
    }
}
