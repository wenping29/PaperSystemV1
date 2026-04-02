using AutoMapper;
using FriendshipService.DTOs;
using FriendshipService.Entities;
using FriendshipService.Interfaces;

namespace FriendshipService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Friendship 映射
            CreateMap<Friendship, FriendshipDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.MetadataJson, opt => opt.MapFrom(src => src.MetadataJson));

            CreateMap<UpdateFriendshipRequest, Friendship>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // FriendRequest 映射
            CreateMap<FriendRequest, FriendRequestDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<CreateFriendRequest, FriendRequest>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // 统计映射
            CreateMap<FriendshipStats, FriendshipStatsDTO>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // 在服务中设置
                .ForMember(dest => dest.MutualFriends, opt => opt.Ignore()); // 需要额外计算
        }
    }
}