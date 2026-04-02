using AutoMapper;
using Microsoft.Extensions.Logging;
using CommunityService.DTOs;
using CommunityService.Entities;
using CommunityService.Interfaces;

namespace CommunityService.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CollectionService> _logger;

        public CollectionService(
            ICollectionRepository collectionRepository,
            IPostRepository postRepository,
            IMapper mapper,
            ILogger<CollectionService> logger)
        {
            _collectionRepository = collectionRepository;
            _postRepository = postRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CollectionResponse?> GetCollectionByIdAsync(long id)
        {
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null) return null;

            return _mapper.Map<CollectionResponse>(collection);
        }

        public async Task<IEnumerable<CollectionResponse>> GetCollectionsAsync(CollectionQueryParams queryParams)
        {
            var collections = await _collectionRepository.GetAllAsync(
                queryParams.Page, queryParams.PageSize, queryParams.UserId, queryParams.PostId);

            return _mapper.Map<IEnumerable<CollectionResponse>>(collections);
        }

        public async Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request, long userId)
        {
            // 验证帖子是否存在
            var postExists = await _postRepository.ExistsAsync(request.PostId);
            if (!postExists)
            {
                throw new ArgumentException("Post not found");
            }

            var alreadyCollected = await _collectionRepository.ExistsByUserAndPostAsync(userId, request.PostId);
            if (alreadyCollected)
            {
                throw new InvalidOperationException("Already collected");
            }

            var collection = new Collection
            {
                UserId = userId,
                PostId = request.PostId,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdCollection = await _collectionRepository.CreateAsync(collection);
            await _postRepository.IncrementCollectionCountAsync(request.PostId, 1);

            return _mapper.Map<CollectionResponse>(createdCollection);
        }

        public async Task<CollectionResponse?> UpdateCollectionAsync(long id, UpdateCollectionRequest request, long userId)
        {
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null) return null;

            if (collection.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this collection");
            }

            _mapper.Map(request, collection);
            collection.UpdatedAt = DateTime.UtcNow;

            var updatedCollection = await _collectionRepository.UpdateAsync(collection);
            return _mapper.Map<CollectionResponse>(updatedCollection);
        }

        public async Task<bool> DeleteCollectionAsync(long id, long userId)
        {
            var collection = await _collectionRepository.GetByIdAsync(id);
            if (collection == null) return false;

            if (collection.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this collection");
            }

            var result = await _collectionRepository.DeleteAsync(id);
            if (result)
            {
                await _postRepository.IncrementCollectionCountAsync(collection.PostId, -1);
            }
            return result;
        }

        public async Task<bool> DeleteCollectionByPostAsync(long postId, long userId)
        {
            return await _collectionRepository.DeleteByUserAndPostAsync(userId, postId);
        }

        public async Task<bool> HasCollectedAsync(long userId, long postId)
        {
            return await _collectionRepository.ExistsByUserAndPostAsync(userId, postId);
        }

        public async Task<int> GetCollectionCountByUserAsync(long userId)
        {
            return await _collectionRepository.GetCountByUserAsync(userId);
        }

        public async Task<int> GetCollectionCountByPostAsync(long postId)
        {
            return await _collectionRepository.GetCountByPostAsync(postId);
        }

        public async Task<IEnumerable<CollectionResponse>> GetCollectionsByUserIdAsync(long userId, int page, int pageSize)
        {
            var collections = await _collectionRepository.GetByUserIdAsync(userId, page, pageSize);
            return _mapper.Map<IEnumerable<CollectionResponse>>(collections);
        }

        public async Task<CollectionStatsResponse> GetCollectionStatsAsync(long userId)
        {
            var totalCollections = await _collectionRepository.GetCountByUserAsync(userId);
            var collections = await _collectionRepository.GetByUserIdAsync(userId, 1, int.MaxValue);

            var publicPostCollections = collections.Count(c => c.Post?.Visibility == PostVisibility.Public);
            var privatePostCollections = totalCollections - publicPostCollections;

            var firstCollection = collections.OrderBy(c => c.CreatedAt).FirstOrDefault();
            var lastCollection = collections.OrderByDescending(c => c.CreatedAt).FirstOrDefault();

            return new CollectionStatsResponse
            {
                UserId = userId,
                TotalCollections = totalCollections,
                PublicPostCollections = publicPostCollections,
                PrivatePostCollections = privatePostCollections,
                FirstCollectionAt = firstCollection?.CreatedAt ?? DateTime.UtcNow,
                LastCollectionAt = lastCollection?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}