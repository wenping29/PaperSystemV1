using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using FriendshipService.DTOs;
using FriendshipService.Entities;
using FriendshipService.Interfaces;

namespace FriendshipService.Services
{
    public class IFriendshipService : IIFriendshipService
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<IFriendshipService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _userServiceUrl;

        public IFriendshipService(
            IFriendshipRepository friendshipRepository,
            IMapper mapper,
            ILogger<IFriendshipService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;

            _userServiceUrl = _configuration["ServiceUrls:UserService"];
        }

        public async Task<FriendshipDTO?> GetFriendshipByIdAsync(long id)
        {
            var friendship = await _friendshipRepository.GetFriendshipByIdAsync(id);
            if (friendship == null) return null;

            var dto = _mapper.Map<FriendshipDTO>(friendship);
            await EnrichFriendInfoAsync(dto);
            return dto;
        }

        public async Task<FriendshipDTO?> GetFriendshipAsync(long userId, long friendId)
        {
            var friendship = await _friendshipRepository.GetFriendshipAsync(userId, friendId);
            if (friendship == null) return null;

            var dto = _mapper.Map<FriendshipDTO>(friendship);
            await EnrichFriendInfoAsync(dto);
            return dto;
        }

        public async Task<IEnumerable<FriendshipDTO>> GetFriendshipsAsync(FriendshipQueryParams queryParams)
        {
            var userId = queryParams.UserId ?? throw new ArgumentException("UserId is required");

            var friendships = await _friendshipRepository.GetFriendshipsByUserIdAsync(
                userId, queryParams.Status, queryParams.IsFavorite, queryParams.Page, queryParams.PageSize);

            var dtos = new List<FriendshipDTO>();
            foreach (var friendship in friendships)
            {
                var dto = _mapper.Map<FriendshipDTO>(friendship);
                await EnrichFriendInfoAsync(dto);
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<int> GetFriendshipsCountAsync(FriendshipQueryParams queryParams)
        {
            var userId = queryParams.UserId ?? throw new ArgumentException("UserId is required");
            return await _friendshipRepository.CountFriendshipsAsync(userId, queryParams.Status);
        }

        public async Task<FriendshipDTO> SendFriendRequestAsync(CreateFriendRequest request, long requesterId)
        {
            // 验证不能向自己发送好友请求
            if (requesterId == request.ReceiverId)
            {
                throw new ArgumentException("Cannot send friend request to yourself");
            }

            // 检查是否已经是好友
            if (await _friendshipRepository.FriendshipExistsAsync(requesterId, request.ReceiverId))
            {
                throw new InvalidOperationException("Users are already friends");
            }

            // 检查是否已有待处理的请求
            if (await _friendshipRepository.FriendRequestExistsAsync(requesterId, request.ReceiverId, "pending"))
            {
                throw new InvalidOperationException("Friend request already pending");
            }

            var friendRequest = new FriendRequest
            {
                RequesterId = requesterId,
                ReceiverId = request.ReceiverId,
                Message = request.Message,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            var createdRequest = await _friendshipRepository.CreateFriendRequestAsync(friendRequest);
            _logger.LogInformation("Friend request sent from {RequesterId} to {ReceiverId}", requesterId, request.ReceiverId);

            // 返回一个临时的FriendshipDTO（实际上还没有建立好友关系）
            var dto = new FriendshipDTO
            {
                UserId = requesterId,
                FriendId = request.ReceiverId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await EnrichFriendInfoAsync(dto);
            return dto;
        }

        public async Task<FriendRequestDTO?> RespondToFriendRequestAsync(long requestId, RespondToFriendRequest response, long userId)
        {
            var request = await _friendshipRepository.GetFriendRequestByIdAsync(requestId);
            if (request == null || request.ReceiverId != userId)
            {
                return null;
            }

            if (request.Status != "pending")
            {
                throw new InvalidOperationException("Friend request is not pending");
            }

            if (response.Accept)
            {
                await _friendshipRepository.AcceptFriendRequestAsync(requestId, response.ResponseMessage);

                // 创建双向好友关系
                var friendship1 = new Friendship
                {
                    UserId = request.RequesterId,
                    FriendId = request.ReceiverId,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var friendship2 = new Friendship
                {
                    UserId = request.ReceiverId,
                    FriendId = request.RequesterId,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _friendshipRepository.CreateFriendshipAsync(friendship1);
                await _friendshipRepository.CreateFriendshipAsync(friendship2);

                _logger.LogInformation("Friend request {RequestId} accepted, friendship created between {User1Id} and {User2Id}",
                    requestId, request.RequesterId, request.ReceiverId);
            }
            else
            {
                await _friendshipRepository.RejectFriendRequestAsync(requestId, response.ResponseMessage);
                _logger.LogInformation("Friend request {RequestId} rejected", requestId);
            }

            var updatedRequest = await _friendshipRepository.GetFriendRequestByIdAsync(requestId);
            return _mapper.Map<FriendRequestDTO>(updatedRequest);
        }

        public async Task<FriendshipDTO?> UpdateFriendshipAsync(long friendshipId, UpdateFriendshipRequest request, long userId)
        {
            var friendship = await _friendshipRepository.GetFriendshipByIdAsync(friendshipId);
            if (friendship == null || friendship.UserId != userId)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(request.Alias))
            {
                friendship.Alias = request.Alias;
            }

            if (!string.IsNullOrEmpty(request.Note))
            {
                friendship.Note = request.Note;
            }

            if (request.IsFavorite.HasValue)
            {
                friendship.IsFavorite = request.IsFavorite.Value;
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                friendship.Status = request.Status;
            }

            var updatedFriendship = await _friendshipRepository.UpdateFriendshipAsync(friendship);
            var dto = _mapper.Map<FriendshipDTO>(updatedFriendship);
            await EnrichFriendInfoAsync(dto);
            return dto;
        }

        public async Task<bool> DeleteFriendshipAsync(long friendshipId, long userId)
        {
            var friendship = await _friendshipRepository.GetFriendshipByIdAsync(friendshipId);
            if (friendship == null || friendship.UserId != userId)
            {
                return false;
            }

            // 同时删除双向关系
            var reverseFriendship = await _friendshipRepository.GetFriendshipAsync(friendship.FriendId, friendship.UserId);
            if (reverseFriendship != null)
            {
                await _friendshipRepository.SoftDeleteFriendshipAsync(reverseFriendship.Id);
            }

            return await _friendshipRepository.SoftDeleteFriendshipAsync(friendshipId);
        }

        public async Task<bool> RemoveFriendAsync(long userId, long friendId)
        {
            var friendship = await _friendshipRepository.GetFriendshipAsync(userId, friendId);
            if (friendship == null) return false;

            return await DeleteFriendshipAsync(friendship.Id, userId);
        }

        public async Task<bool> BlockUserAsync(long userId, long blockedUserId)
        {
            // 先删除现有好友关系（如果存在）
            var friendship = await _friendshipRepository.GetFriendshipAsync(userId, blockedUserId);
            if (friendship != null)
            {
                await _friendshipRepository.SoftDeleteFriendshipAsync(friendship.Id);
            }

            // 创建屏蔽关系
            var blockedFriendship = new Friendship
            {
                UserId = userId,
                FriendId = blockedUserId,
                Status = "blocked",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _friendshipRepository.CreateFriendshipAsync(blockedFriendship);
            _logger.LogInformation("User {UserId} blocked user {BlockedUserId}", userId, blockedUserId);
            return true;
        }

        public async Task<bool> UnblockUserAsync(long userId, long blockedUserId)
        {
            var blockedFriendship = await _friendshipRepository.GetFriendshipAsync(userId, blockedUserId);
            if (blockedFriendship == null || blockedFriendship.Status != "blocked")
            {
                return false;
            }

            return await _friendshipRepository.DeleteFriendshipAsync(blockedFriendship.Id);
        }

        public async Task<bool> FavoriteFriendAsync(long userId, long friendId, bool isFavorite)
        {
            var friendship = await _friendshipRepository.GetFriendshipAsync(userId, friendId);
            if (friendship == null) return false;

            friendship.IsFavorite = isFavorite;
            friendship.UpdatedAt = DateTime.UtcNow;
            await _friendshipRepository.UpdateFriendshipAsync(friendship);
            return true;
        }

        public async Task<bool> UpdateInteractionAsync(long userId, long friendId)
        {
            return await _friendshipRepository.UpdateInteractionAsync(userId, friendId);
        }

        public async Task<FriendRequestDTO?> GetFriendRequestByIdAsync(long id)
        {
            var request = await _friendshipRepository.GetFriendRequestByIdAsync(id);
            return request == null ? null : _mapper.Map<FriendRequestDTO>(request);
        }

        public async Task<IEnumerable<FriendRequestDTO>> GetFriendRequestsAsync(FriendRequestQueryParams queryParams)
        {
            var userId = queryParams.UserId ?? throw new ArgumentException("UserId is required");

            var requests = await _friendshipRepository.GetFriendRequestsByUserIdAsync(
                userId, queryParams.Type, queryParams.Status, queryParams.Page, queryParams.PageSize);

            return _mapper.Map<IEnumerable<FriendRequestDTO>>(requests);
        }

        public async Task<int> GetFriendRequestsCountAsync(FriendRequestQueryParams queryParams)
        {
            var userId = queryParams.UserId ?? throw new ArgumentException("UserId is required");
            return await _friendshipRepository.CountFriendRequestsAsync(userId, queryParams.Type, queryParams.Status);
        }

        public async Task<bool> CancelFriendRequestAsync(long requestId, long userId)
        {
            var request = await _friendshipRepository.GetFriendRequestByIdAsync(requestId);
            if (request == null || request.RequesterId != userId)
            {
                return false;
            }

            return await _friendshipRepository.CancelFriendRequestAsync(request.RequesterId, request.ReceiverId);
        }

        public async Task<bool> CancelFriendRequestByUserIdsAsync(long requesterId, long receiverId)
        {
            return await _friendshipRepository.CancelFriendRequestAsync(requesterId, receiverId);
        }

        public async Task<IEnumerable<FriendInfoDTO>> GetFriendSuggestionsAsync(long userId, int limit = 10)
        {
            // 简单的实现：返回空列表
            // 实际应该基于共同好友、兴趣等算法
            _logger.LogInformation("Getting friend suggestions for user {UserId}, limit {Limit}", userId, limit);
            return new List<FriendInfoDTO>();
        }

        public async Task<MutualFriendsResult> GetMutualFriendsAsync(long user1Id, long user2Id, int page = 1, int pageSize = 20)
        {
            var mutualFriendships = await _friendshipRepository.GetMutualFriendshipsAsync(user1Id, user2Id);
            var mutualFriendIds = mutualFriendships.Select(f => f.FriendId).ToList();

            var result = new MutualFriendsResult
            {
                User1Id = user1Id,
                User2Id = user2Id,
                MutualCount = mutualFriendIds.Count
            };

            // 获取好友信息
            foreach (var friendId in mutualFriendIds.Skip((page - 1) * pageSize).Take(pageSize))
            {
                var friendInfo = await GetUserInfoAsync(friendId);
                if (friendInfo != null)
                {
                    result.MutualFriends.Add(friendInfo);
                }
            }

            return result;
        }

        public async Task<FriendshipStatsDTO> GetFriendshipStatsAsync(long userId)
        {
            var stats = await _friendshipRepository.GetFriendshipStatsAsync(userId);
            var dto = _mapper.Map<FriendshipStatsDTO>(stats);
            dto.UserId = userId;
            return dto;
        }

        public async Task<bool> AreFriendsAsync(long user1Id, long user2Id)
        {
            return await _friendshipRepository.FriendshipExistsAsync(user1Id, user2Id);
        }

        public async Task<string> GetFriendshipStatusAsync(long user1Id, long user2Id)
        {
            var friendship = await _friendshipRepository.GetFriendshipAsync(user1Id, user2Id);
            return friendship?.Status ?? "none";
        }

        public Task<bool> ImportFriendshipsAsync(long userId, IEnumerable<long> friendIds)
        {
            _logger.LogInformation("Importing {Count} friendships for user {UserId}", friendIds.Count(), userId);
            // 实现略
            return Task.FromResult(true);
        }

        public Task<bool> ExportFriendshipsAsync(long userId, string format = "json")
        {
            _logger.LogInformation("Exporting friendships for user {UserId} in {Format} format", userId, format);
            return Task.FromResult(true);
        }

        public async Task<bool> CleanupExpiredRequestsAsync(DateTime cutoffDate)
        {
            _logger.LogInformation("Cleaning up friend requests expired before {CutoffDate}", cutoffDate);
            return await _friendshipRepository.CleanupExpiredFriendRequestsAsync(cutoffDate);
        }

        private async Task EnrichFriendInfoAsync(FriendshipDTO friendship)
        {
            try
            {
                var friendInfo = await GetUserInfoAsync(friendship.FriendId);
                friendship.FriendInfo = friendInfo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enrich friend info for user {FriendId}", friendship.FriendId);
            }
        }

        private async Task<FriendInfoDTO?> GetUserInfoAsync(long userId)
        {
            try
            {
                // 调用用户服务获取用户信息
                var response = await _httpClient.GetAsync($"{_userServiceUrl}/api/v1/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var userData = await response.Content.ReadFromJsonAsync<dynamic>();
                    // 解析用户信息并转换为FriendInfoDTO
                    // 简化实现
                    return new FriendInfoDTO
                    {
                        Id = userId,
                        Username = $"user{userId}",
                        DisplayName = $"User {userId}",
                        AvatarUrl = null,
                        Bio = null,
                        IsOnline = false,
                        LastActive = null,
                        MutualFriends = 0
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user info for user {UserId}", userId);
            }

            return null;
        }
    }
}