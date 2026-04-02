using AutoMapper;
using Microsoft.Extensions.Logging;
using CommunityService.DTOs;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LikeService> _logger;

        public LikeService(
            ILikeRepository likeRepository,
            IMapper mapper,
            ILogger<LikeService> logger)
        {
            _likeRepository = likeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<LikeResponse?> GetLikeByIdAsync(long id)
        {
            var like = await _likeRepository.GetByIdAsync(id);
            if (like == null) return null;

            return _mapper.Map<LikeResponse>(like);
        }

        public async Task<IEnumerable<LikeResponse>> GetLikesAsync(LikeQueryParams queryParams)
        {
            var likes = await _likeRepository.GetAllAsync(
                queryParams.Page, queryParams.PageSize, queryParams.UserId,
                queryParams.TargetType, queryParams.TargetId);

            return _mapper.Map<IEnumerable<LikeResponse>>(likes);
        }

        public async Task<LikeResponse> CreateLikeAsync(CreateLikeRequest request, long userId)
        {
            if (!Enum.TryParse<LikeTargetType>(request.TargetType, true, out var targetType))
            {
                throw new ArgumentException("Invalid target type");
            }

            var alreadyLiked = await _likeRepository.ExistsByUserAndTargetAsync(userId, targetType, request.TargetId);
            if (alreadyLiked)
            {
                throw new InvalidOperationException("Already liked");
            }

            var like = new Like
            {
                UserId = userId,
                TargetType = targetType,
                TargetId = request.TargetId,
                CreatedAt = DateTime.UtcNow
            };

            var createdLike = await _likeRepository.CreateAsync(like);
            return _mapper.Map<LikeResponse>(createdLike);
        }

        public async Task<bool> DeleteLikeAsync(long id, long userId)
        {
            var like = await _likeRepository.GetByIdAsync(id);
            if (like == null) return false;

            if (like.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this like");
            }

            return await _likeRepository.DeleteAsync(id);
        }

        public async Task<bool> DeleteLikeByTargetAsync(DeleteLikeRequest request, long userId)
        {
            if (!Enum.TryParse<LikeTargetType>(request.TargetType, true, out var targetType))
            {
                throw new ArgumentException("Invalid target type");
            }

            return await _likeRepository.DeleteByUserAndTargetAsync(userId, targetType, request.TargetId);
        }

        public async Task<bool> HasLikedAsync(long userId, LikeTargetType targetType, long targetId)
        {
            return await _likeRepository.ExistsByUserAndTargetAsync(userId, targetType, targetId);
        }

        public async Task<int> GetLikeCountAsync(LikeTargetType targetType, long targetId)
        {
            return await _likeRepository.GetCountByTargetAsync(targetType, targetId);
        }

        public async Task<LikeStatsResponse> GetLikeStatsAsync(long userId)
        {
            var postLikesCount = await _likeRepository.GetCountByUserAsync(userId, LikeTargetType.Post);
            var commentLikesCount = await _likeRepository.GetCountByUserAsync(userId, LikeTargetType.Comment);
            var totalLikesCount = postLikesCount + commentLikesCount;

            var likes = await _likeRepository.GetAllAsync(1, 1, userId, null, null);
            var firstLike = likes.OrderBy(l => l.CreatedAt).FirstOrDefault();
            var lastLike = likes.OrderByDescending(l => l.CreatedAt).FirstOrDefault();

            return new LikeStatsResponse
            {
                UserId = userId,
                PostLikesCount = postLikesCount,
                CommentLikesCount = commentLikesCount,
                TotalLikesCount = totalLikesCount,
                FirstLikeAt = firstLike?.CreatedAt ?? DateTime.UtcNow,
                LastLikeAt = lastLike?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}