using AutoMapper;
using PaperSystemApi.DTOs;
using PaperSystemApi.Models;

namespace PaperSystemApi.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile));

            CreateMap<UserProfile, UserProfileResponse>();

            CreateMap<User, UserListResponse>()
                .ForMember(dest => dest.role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.WritingCount, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.WritingCount : 0))
                .ForMember(dest => dest.FollowersCount, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.FollowersCount : 0));

            CreateMap<User, UserInfoDTO>();
            CreateMap<User, FriendInfoDTO>();

            CreateMap<Work, WorkResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<Work, WorkListResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<CreateWorkRequest, Work>();
            CreateMap<UpdateWorkRequest, Work>();

            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));

            CreateMap<Category, CategoryListResponse>();
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>();

            CreateMap<Template, TemplateResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<Template, TemplateListResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<CreateTemplateRequest, Template>();
            CreateMap<UpdateTemplateRequest, Template>();

            CreateMap<Friendship, FriendshipDTO>();
            CreateMap<FriendRequest, FriendRequestDTO>();

            CreateMap<User, UserInfo>();

            CreateMap<FileMetadata, FileDTO>();

            CreateMap<CreateActivityLogRequest, UserActivityLog>()
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType.ToString()));

            CreateMap<UserActivityLog, ActivityLogResponse>()
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType));
        }
    }
}