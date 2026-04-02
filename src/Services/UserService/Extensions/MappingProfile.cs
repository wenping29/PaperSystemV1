using AutoMapper;
using UserService.DTOs;
using UserService.Entities;

namespace UserService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User 映射
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<UserProfile, UserProfileResponse>();

            CreateMap<CreateUserRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // 在服务中处理
                .ForMember(dest => dest.Role, opt => opt.MapFrom(_ => UserRole.User))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => UserStatus.Active));

            CreateMap<UpdateUserRequest, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // UserProfile 映射
            CreateMap<UpdateProfileRequest, UserProfile>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // 其他映射配置...

            // ActivityLog 映射
            CreateMap<CreateActivityLogRequest, UserActivityLog>();
            CreateMap<UserActivityLog, ActivityLogResponse>()
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType.ToString()))
                .ForMember(dest => dest.User, opt => opt.Ignore()); // 在服务中手动映射
        }
    }
}