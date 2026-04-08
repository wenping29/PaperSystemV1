using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PaperSystemApi.UserServices.DTOs;
using PaperSystemApi.UserServices.Entities;
using PaperSystemApi.UserServices.Helpers;
using PaperSystemApi.UserServices.Interfaces;

namespace PaperSystemApi.UserServices.Services
{
    public class UserServiceS : IUserServiceS
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserServiceS> _logger;

        public UserServiceS(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IMapper mapper,
            IDistributedCache cache,
            ILogger<UserServiceS> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        public async Task<UserResponse?> GetUserByIdAsync(long id)
        {
            var user = await _userRepository.GetUserWithProfileAsync(id);
            return user == null ? null : MapToUserResponse(user);
        }

        public async Task<UserResponse?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user == null ? null : MapToUserResponse(user);
        }

        public async Task<IEnumerable<UserListResponse>> GetUsersAsync(int page, int pageSize, string? search = null)
        {
            var users = await _userRepository.SearchUsersAsync(search ?? string.Empty, page, pageSize);
            var userList = new List<UserListResponse>();

            foreach (var user in users)
            {
                var followersCount = await _userRepository.GetFollowersCountAsync(user.Id);
                var isFollowing = false; // 当前登录用户的关注状态，需要从上下文获取

                userList.Add(new UserListResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    AvatarUrl = user.AvatarUrl,
                    Bio = user.Bio,
                    WritingCount = user.Profile?.WritingCount ?? 0,
                    FollowersCount = followersCount,
                    IsFollowing = isFollowing,
                    role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                });
            }

            return userList;
        }

        public async Task<int> GetUsersCountAsync(string? search = null)
        {
            return await _userRepository.CountUsersAsync(search);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            // 验证用户名和邮箱是否已存在
            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                throw new ArgumentException("Username already exists");
            }

            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Phone = request.Phone,
                Role = UserRole.User,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // 创建用户资料
            var profile = new UserProfile
            {
                UserId = createdUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            createdUser.Profile = profile;
            await _userRepository.UpdateAsync(createdUser);

            return MapToUserResponse(createdUser);
        }

        public async Task<UserResponse?> UpdateUserAsync(long id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            // 如果更新用户名，验证是否已存在
            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    throw new ArgumentException("Username already exists");
                }
                user.Username = request.Username;
            }

            // 如果更新邮箱，验证是否已存在
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    throw new ArgumentException("Email already exists");
                }
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Phone)) user.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.Bio)) user.Bio = request.Bio;
            if (!string.IsNullOrEmpty(request.AvatarUrl)) user.AvatarUrl = request.AvatarUrl;
            if (!string.IsNullOrEmpty(request.Location)) user.Location = request.Location;
            if (!string.IsNullOrEmpty(request.Website)) user.Website = request.Website;

            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToUserResponse(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<UserProfileResponse?> GetUserProfileAsync(long userId)
        {
            var profile = await _userRepository.GetProfileByUserIdAsync(userId);
            return profile == null ? null : _mapper.Map<UserProfileResponse>(profile);
        }

        public async Task<UserProfileResponse> UpdateUserProfileAsync(long userId, UpdateProfileRequest request)
        {
            var profile = new UserProfile
            {
                UserId = userId,
                Biography = request.Biography,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                TwitterUrl = request.TwitterUrl,
                GitHubUrl = request.GitHubUrl,
                LinkedInUrl = request.LinkedInUrl,
                UpdatedAt = DateTime.UtcNow
            };

            var updatedProfile = await _userRepository.CreateOrUpdateProfileAsync(profile);
            return _mapper.Map<UserProfileResponse>(updatedProfile);
        }

        public async Task<AuthResponse> AuthenticateAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username) ??
                       await _userRepository.GetByEmailAsync(request.Username);

            if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            if (user.Status != UserStatus.Active)
            {
                throw new UnauthorizedAccessException("User account is not active");
            }

            // 更新最后登录时间
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Role.ToString());
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            // 存储刷新令牌到缓存，有效期7天
            var cacheKey = $"refresh_token:{refreshToken}";
            await _cache.SetStringAsync(cacheKey, user.Id.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // 假设token有效期60分钟
                User = MapToUserResponse(user)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var createUserRequest = new CreateUserRequest
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone
            };

            var userResponse = await CreateUserAsync(createUserRequest);

            // 自动登录新注册用户
            var loginRequest = new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            };

            return await AuthenticateAsync(loginRequest);
        }

        public async Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> FollowUserAsync(long followerId, long followingId)
        {
            return await _userRepository.FollowUserAsync(followerId, followingId);
        }

        public async Task<bool> UnfollowUserAsync(long followerId, long followingId)
        {
            return await _userRepository.UnfollowUserAsync(followerId, followingId);
        }

        public async Task<bool> IsFollowingAsync(long followerId, long followingId)
        {
            return await _userRepository.IsFollowingAsync(followerId, followingId);
        }

        public async Task<IEnumerable<UserListResponse>> GetFollowersAsync(long userId, int page, int pageSize)
        {
            var followers = await _userRepository.GetFollowersAsync(userId, page, pageSize);
            return followers.Select(f => new UserListResponse
            {
                Id = f.Id,
                Username = f.Username,
                AvatarUrl = f.AvatarUrl,
                Bio = f.Bio,
                WritingCount = f.Profile?.WritingCount ?? 0,
                FollowersCount = 0, // 需要额外查询
                IsFollowing = false, // 需要从当前用户上下文判断
                CreatedAt = f.CreatedAt
            });
        }

        public async Task<IEnumerable<UserListResponse>> GetFollowingAsync(long userId, int page, int pageSize)
        {
            var following = await _userRepository.GetFollowingAsync(userId, page, pageSize);
            return following.Select(f => new UserListResponse
            {
                Id = f.Id,
                Username = f.Username,
                AvatarUrl = f.AvatarUrl,
                Bio = f.Bio,
                WritingCount = f.Profile?.WritingCount ?? 0,
                FollowersCount = 0, // 需要额外查询
                IsFollowing = false, // 需要从当前用户上下文判断
                CreatedAt = f.CreatedAt
            });
        }

        public async Task<UserStatsResponse> GetUserStatsAsync(long userId)
        {
            var user = await _userRepository.GetUserWithProfileAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var followersCount = await _userRepository.GetFollowersCountAsync(userId);
            var followingCount = await _userRepository.GetFollowingCountAsync(userId);

            return new UserStatsResponse
            {
                WritingsCount = user.Profile?.WritingCount ?? 0,
                LikesCount = user.Profile?.LikeCount ?? 0,
                CommentsCount = 0, // 需要从评论服务获取
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                TotalWords = user.Profile?.TotalWords ?? 0,
                LastActive = user.LastLoginAt
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // 从缓存中获取用户ID
            var cacheKey = $"refresh_token:{request.RefreshToken}";
            var userIdString = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // 获取用户
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // 生成新令牌
            var newToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Role.ToString());
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            // 删除旧的刷新令牌
            await _cache.RemoveAsync(cacheKey);

            // 存储新的刷新令牌
            var newCacheKey = $"refresh_token:{newRefreshToken}";
            await _cache.SetStringAsync(newCacheKey, user.Id.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });

            return new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // 从配置获取
                User = MapToUserResponse(user)
            };
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            // 查找用户
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.Status != UserStatus.Active)
            {
                // 出于安全考虑，即使找不到用户也返回true，避免邮箱枚举攻击
                return true;
            }

            // 生成密码重置令牌
            var resetToken = Guid.NewGuid().ToString("N");
            var cacheKey = $"password_reset:{resetToken}";

            // 存储令牌到缓存，有效期1小时
            await _cache.SetStringAsync(cacheKey, user.Id.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            // TODO: 在实际应用中，这里应该发送包含重置链接的电子邮件
            // 例如: https://yourapp.com/reset-password?token={resetToken}
            // 出于演示目的，我们只记录令牌
            _logger.LogInformation("Password reset token for user {UserId}: {ResetToken}", user.Id, resetToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // 验证令牌
            var cacheKey = $"password_reset:{request.Token}";
            var userIdString = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or expired reset token");
            }

            // 获取用户
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // 验证新密码和确认密码是否匹配（DTO中已有Compare属性验证）
            // 更新密码
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // 删除重置令牌
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("Password reset successfully for user {UserId}", userId);
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(long userId, UpdateUserRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // 验证角色值
            if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
            {
                throw new ArgumentException($"Invalid role value: {request.Role}. Valid values: {string.Join(", ", Enum.GetNames<UserRole>())}");
            }

            // 记录角色变更
            var oldRole = user.Role;
            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Updated role for user {UserId}: {OldRole} -> {NewRole}. Reason: {Reason}",
                userId, oldRole, newRole, request.Reason);

            // 清除用户缓存
            var cacheKey = $"user:{userId}";
            await _cache.RemoveAsync(cacheKey);

            return true;
        }

        private UserResponse MapToUserResponse(User user)
        {
            var response = _mapper.Map<UserResponse>(user);

            if (user.Profile != null)
            {
                response.Profile = _mapper.Map<UserProfileResponse>(user.Profile);
            }

            return response;
        }
    }
}