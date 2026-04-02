using UserService.Entities;

namespace UserService.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync(int page, int pageSize);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(long id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<UserProfile?> GetProfileByUserIdAsync(long userId);
        Task<UserProfile> CreateOrUpdateProfileAsync(UserProfile profile);
        Task<bool> FollowUserAsync(long followerId, long followingId);
        Task<bool> UnfollowUserAsync(long followerId, long followingId);
        Task<bool> IsFollowingAsync(long followerId, long followingId);
        Task<IEnumerable<User>> GetFollowersAsync(long userId, int page, int pageSize);
        Task<IEnumerable<User>> GetFollowingAsync(long userId, int page, int pageSize);
        Task<int> GetFollowersCountAsync(long userId);
        Task<int> GetFollowingCountAsync(long userId);
        Task<User?> GetUserWithProfileAsync(long id);
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int page, int pageSize);
        Task<int> CountUsersAsync(string? searchTerm = null);
    }
}