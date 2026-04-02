using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Entities;
using UserService.Interfaces;

namespace UserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(long id)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .Include(u => u.Profile)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<UserProfile?> GetProfileByUserIdAsync(long userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<UserProfile> CreateOrUpdateProfileAsync(UserProfile profile)
        {
            var existingProfile = await GetProfileByUserIdAsync(profile.UserId);
            if (existingProfile != null)
            {
                existingProfile.Biography = profile.Biography;
                existingProfile.BirthDate = profile.BirthDate;
                existingProfile.Gender = profile.Gender;
                existingProfile.TwitterUrl = profile.TwitterUrl;
                existingProfile.GitHubUrl = profile.GitHubUrl;
                existingProfile.LinkedInUrl = profile.LinkedInUrl;
                existingProfile.UpdatedAt = DateTime.UtcNow;
                _context.UserProfiles.Update(existingProfile);
            }
            else
            {
                _context.UserProfiles.Add(profile);
            }

            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<bool> FollowUserAsync(long followerId, long followingId)
        {
            if (followerId == followingId) return false;

            var existing = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);

            if (existing != null) return false;

            var userFollow = new UserFollow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFollows.Add(userFollow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowUserAsync(long followerId, long followingId)
        {
            var userFollow = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);

            if (userFollow == null) return false;

            _context.UserFollows.Remove(userFollow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(long followerId, long followingId)
        {
            return await _context.UserFollows
                .AnyAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
        }

        public async Task<IEnumerable<User>> GetFollowersAsync(long userId, int page, int pageSize)
        {
            return await _context.UserFollows
                .Where(uf => uf.FollowingId == userId)
                .Include(uf => uf.Follower)
                .ThenInclude(u => u.Profile)
                .Select(uf => uf.Follower)
                .Where(u => !u.IsDeleted)
                .OrderByDescending(uf => uf.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetFollowingAsync(long userId, int page, int pageSize)
        {
            return await _context.UserFollows
                .Where(uf => uf.FollowerId == userId)
                .Include(uf => uf.Following)
                .ThenInclude(u => u.Profile)
                .Select(uf => uf.Following)
                .Where(u => !u.IsDeleted)
                .OrderByDescending(uf => uf.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFollowersCountAsync(long userId)
        {
            return await _context.UserFollows
                .CountAsync(uf => uf.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(long userId)
        {
            return await _context.UserFollows
                .CountAsync(uf => uf.FollowerId == userId);
        }

        public async Task<User?> GetUserWithProfileAsync(long id)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync(page, pageSize);
            }

            searchTerm = $"%{searchTerm}%";
            return await _context.Users
                .Where(u => !u.IsDeleted &&
                    (EF.Functions.Like(u.Username, searchTerm) ||
                     EF.Functions.Like(u.Email, searchTerm) ||
                     EF.Functions.Like(u.Bio, searchTerm)))
                .Include(u => u.Profile)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountUsersAsync(string? searchTerm = null)
        {
            var query = _context.Users.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = $"%{searchTerm}%";
                query = query.Where(u => EF.Functions.Like(u.Username, searchTerm) ||
                                         EF.Functions.Like(u.Email, searchTerm) ||
                                         EF.Functions.Like(u.Bio, searchTerm));
            }

            return await query.CountAsync();
        }
    }
}