using AutoMapper;
using NotificationService.DTOs;
using NotificationService.Entities;
using NotificationService.Interfaces;

namespace NotificationService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Notification 映射
            CreateMap<Notification, NotificationDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.MetadataJson, opt => opt.MapFrom(src => src.MetadataJson));

            CreateMap<CreateNotificationRequest, Notification>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => NotificationStatus.Unread))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<SystemNotificationRequest, Notification>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => NotificationStatus.Unread))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UserNotificationRequest, Notification>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ReceiverId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => NotificationStatus.Unread))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // NotificationTemplate 映射
            CreateMap<NotificationTemplate, NotificationTemplateDTO>()
                .ForMember(dest => dest.VariablesJson, opt => opt.MapFrom(src => src.VariablesJson));

            CreateMap<CreateTemplateRequest, NotificationTemplate>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateTemplateRequest, NotificationTemplate>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // NotificationSettings 映射
            CreateMap<NotificationSettings, NotificationSettingsDTO>();

            CreateMap<UpdateNotificationSettingsRequest, NotificationSettings>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // 统计映射
            CreateMap<NotificationStats, NotificationStatsDTO>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()); // 在服务中设置
        }
    }
}