using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaperSystemApi.Data
{
    public class FriendshipDbContextFactory : IDesignTimeDbContextFactory<FriendshipDbContext>
    {
        public FriendshipDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FriendshipDbContext>();
            optionsBuilder.UseSqlite("Data Source=Data/writing_platform_friendship.db");

            return new FriendshipDbContext(optionsBuilder.Options);
        }
    }
}
