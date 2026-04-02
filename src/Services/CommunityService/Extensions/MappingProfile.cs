using AutoMapper;
using CommunityService.DTOs;
using CommunityService.Entities;

namespace CommunityService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Post 映射
            CreateMap<Post, PostResponse>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()));

            CreateMap<Post, PostListResponse>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

            CreateMap<CreatePostRequest, Post>()
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore()) // 从用户上下文中获取
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => PostStatus.Pending))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    NewMethod(src)))
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src =>
                    NewMethod1(src)));

            var map = CreateMap<UpdatePostRequest, Post>();
            map.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            map.ForMember(dest => dest.Category, opt => opt.MapFrom(src =>NewMethod3(src)))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src =>NewMethod4(src)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>NewMethod5(src)));

            // Comment 映射
            CreateMap<Comment, CommentResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateCommentRequest, Comment>()
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore()); // 从用户上下文中获取

            CreateMap<UpdateCommentRequest, Comment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Like 映射
            CreateMap<Like, LikeResponse>()
                .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => src.TargetType.ToString()));

            CreateMap<CreateLikeRequest, Like>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // 从用户上下文中获取
                .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src =>
                    NewMethod2(src)));

            // Collection 映射
            CreateMap<Collection, CollectionResponse>();

            CreateMap<CreateCollectionRequest, Collection>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()); // 从用户上下文中获取

            CreateMap<UpdateCollectionRequest, Collection>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

        private static PostStatus? NewMethod5(UpdatePostRequest src)
        {
            return !string.IsNullOrEmpty(src.Status) && Enum.TryParse<PostStatus>(src.Status, true, out var status) ? status : (PostStatus?)null;
        }

        private static PostVisibility? NewMethod4(UpdatePostRequest src)
        {
            return !string.IsNullOrEmpty(src.Visibility) && Enum.TryParse<PostVisibility>(src.Visibility, true, out var visibility) ? visibility : (PostVisibility?)null;
        }

        private static PostCategory? NewMethod3(UpdatePostRequest src)
        {
            return !string.IsNullOrEmpty(src.Category) && Enum.TryParse<PostCategory>(src.Category, true, out var category) ? category : (PostCategory?)null;
        }

        private static LikeTargetType NewMethod2(CreateLikeRequest src)
        {
            return Enum.TryParse<LikeTargetType>(src.TargetType, true, out var targetType) ? targetType : LikeTargetType.Post;
        }

        private static PostVisibility NewMethod1(CreatePostRequest src)
        {
            return Enum.TryParse<PostVisibility>(src.Visibility, true, out var visibility) ? visibility : PostVisibility.Public;
        }

        private static PostCategory NewMethod(CreatePostRequest src)
        {
            return Enum.TryParse<PostCategory>(src.Category, true, out var category) ? category : PostCategory.Other;
        }
    }
}