using AutoMapper;
using PaperSystemApi.Chat.DTOs;
using PaperSystemApi.Chat.Entities;

namespace PaperSystemApi.Chat.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User 映射
            CreateMap<UserEntity, UserInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl));

            // Message 映射
            CreateMap<Message, MessageResponse>()
                .ForMember(dest => dest.Sender, opt => opt.Ignore())
                .ForMember(dest => dest.Receiver, opt => opt.Ignore())
                .ForMember(dest => dest.ChatRoom, opt => opt.Ignore())
                .ForMember(dest => dest.ParentMessage, opt => opt.Ignore());

            CreateMap<Message, MessageListResponse>()
                .ForMember(dest => dest.Sender, opt => opt.Ignore())
                .ForMember(dest => dest.IsOwnMessage, opt => opt.Ignore());

            CreateMap<Message, MessageInfo>()
                .ForMember(dest => dest.Sender, opt => opt.Ignore());

            // ChatRoom 映射
            CreateMap<ChatRoom, ChatRoomResponse>()
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserMember, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore());

            CreateMap<ChatRoom, ChatRoomListResponse>()
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserMember, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessage, opt => opt.Ignore())
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore());

            CreateMap<ChatRoom, ChatRoomInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl));

            // ChatRoomMember 映射
            CreateMap<ChatRoomMember, ChatRoomMemberInfo>()
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // 请求映射
            CreateMap<SendMessageRequest, Message>()
                .ForMember(dest => dest.SenderId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Sent"))
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<CreateChatRoomRequest, ChatRoom>()
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.LastActivityAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}