using PaperSystemApi.UserServices.DTOs;
using PaperSystemApi.UserServices.Entities;

namespace PaperSystemApi.UserServices.Interfaces
{
    public interface IUserServiceS
    {
        Task<UserResponse?> GetUserByIdAsync(long id);
        Task<UserResponse?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserListResponse>> GetUsersAsync(int page, int pageSize, string? search = null);
        Task<int> GetUsersCountAsync(string? search = null);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse?> UpdateUserAsync(long id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(long id);
        Task<UserProfileResponse?> GetUserProfileAsync(long userId);
        Task<UserProfileResponse> UpdateUserProfileAsync(long userId, UpdateProfileRequest request);
        Task<AuthResponse> AuthenticateAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequest request);
        Task<bool> FollowUserAsync(long followerId, long followingId);
        Task<bool> UnfollowUserAsync(long followerId, long followingId);
        Task<bool> IsFollowingAsync(long followerId, long followingId);
        Task<IEnumerable<UserListResponse>> GetFollowersAsync(long userId, int page, int pageSize);
        Task<IEnumerable<UserListResponse>> GetFollowingAsync(long userId, int page, int pageSize);
        Task<UserStatsResponse> GetUserStatsAsync(long userId);

        // 新增的认证相关方法
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        // 用户管理方法
        Task<bool> UpdateUserRoleAsync(long userId, UpdateUserRoleRequest request);
    }

    public class UserStatsResponse
    {
        public int WritingsCount { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int TotalWords { get; set; }
        public DateTime? LastActive { get; set; }
    }
}