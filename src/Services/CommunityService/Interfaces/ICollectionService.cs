using CommunityService.DTOs;
using CommunityService.Entities;

namespace CommunityService.Interfaces
{
    public interface ICollectionService
    {
        Task<CollectionResponse?> GetCollectionByIdAsync(long id);
        Task<IEnumerable<CollectionResponse>> GetCollectionsAsync(CollectionQueryParams queryParams);
        Task<CollectionResponse> CreateCollectionAsync(CreateCollectionRequest request, long userId);
        Task<CollectionResponse?> UpdateCollectionAsync(long id, UpdateCollectionRequest request, long userId);
        Task<bool> DeleteCollectionAsync(long id, long userId);
        Task<bool> DeleteCollectionByPostAsync(long postId, long userId);
        Task<bool> HasCollectedAsync(long userId, long postId);
        Task<int> GetCollectionCountByUserAsync(long userId);
        Task<int> GetCollectionCountByPostAsync(long postId);
        Task<IEnumerable<CollectionResponse>> GetCollectionsByUserIdAsync(long userId, int page, int pageSize);
        Task<CollectionStatsResponse> GetCollectionStatsAsync(long userId);
    }

    public class CollectionStatsResponse
    {
        public long UserId { get; set; }
        public int TotalCollections { get; set; }
        public int PublicPostCollections { get; set; }
        public int PrivatePostCollections { get; set; }
        public DateTime FirstCollectionAt { get; set; }
        public DateTime LastCollectionAt { get; set; }
    }
}