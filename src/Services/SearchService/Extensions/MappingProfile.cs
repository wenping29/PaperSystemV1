using AutoMapper;
using SearchService.DTOs;
using SearchService.Entities;

namespace SearchService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // SearchHistory 映射
            CreateMap<SearchHistory, SearchHistoryDTO>()
                .ForMember(dest => dest.SearchType, opt => opt.MapFrom(src => src.SearchType));

            CreateMap<SearchRequest, SearchHistory>()
                .ForMember(dest => dest.Query, opt => opt.MapFrom(src => src.Query))
                .ForMember(dest => dest.SearchType, opt => opt.MapFrom(src => src.SearchType))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.ResponseTime, opt => opt.MapFrom(_ => TimeSpan.Zero))
                .ForMember(dest => dest.IsSuccessful, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.ResultCount, opt => opt.MapFrom(_ => 0));

            // PopularSearchTerm 映射已在查询中处理，无需额外配置
        }
    }
}