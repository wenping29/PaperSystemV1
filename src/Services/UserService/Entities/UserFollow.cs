using System;

namespace UserService.Entities
{
    public class UserFollow
    {
        public long Id { get; set; }

        public long FollowerId { get; set; }

        public long FollowingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual User Follower { get; set; } = null!;
        public virtual User Following { get; set; } = null!;
    }
}