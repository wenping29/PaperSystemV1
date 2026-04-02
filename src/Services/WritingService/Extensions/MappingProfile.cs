using AutoMapper;
using System.Text.Json;
using WritingService.DTOs;
using WritingService.Entities;
using WritingService.Interfaces;

namespace WritingService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Work 映射
            CreateMap<Work, WorkResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            CreateMap<Work, WorkListResponse>()
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished));

            CreateMap<CreateWorkRequest, Work>()
                .ForMember(dest => dest.TagsJson, opt => opt.MapFrom(src => MappingHelper.SerializeTags(src.Tags)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => WorkStatus.Draft))
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(_ => false));

            var updateMap = CreateMap<UpdateWorkRequest, Work>();
            updateMap.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            updateMap.ForMember(dest => dest.TagsJson, opt => opt.MapFrom(src => MappingHelper.SerializeTags(src.Tags)));
            updateMap.ForMember(dest => dest.Status, opt => opt.MapFrom(src => MappingHelper.ParseWorkStatus(src.Status)));

            // Category 映射
            CreateMap<Category, CategoryResponse>();
            CreateMap<Category, CategoryListResponse>();

            CreateMap<CreateCategoryRequest, Category>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            var updateCategoryMap = CreateMap<UpdateCategoryRequest, Category>();
            updateCategoryMap.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Template 映射
            CreateMap<Template, TemplateResponse>()
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic));

            CreateMap<Template, TemplateListResponse>();

            CreateMap<CreateTemplateRequest, Template>()
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic));

            var updateTemplateMap = CreateMap<UpdateTemplateRequest, Template>();
            updateTemplateMap.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // WorkVersion 映射
            CreateMap<WorkVersion, WorkVersionResponse>();

            // WorkCollaborator 映射
            CreateMap<WorkCollaborator, CollaboratorResponse>();
        }
    }

    public static class MappingHelper
    {
        public static string? SerializeTags(string[]? tags)
        {
            return tags != null ? JsonSerializer.Serialize(tags) : null;
        }

        public static WorkStatus? ParseWorkStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return null;

            if (Enum.TryParse<WorkStatus>(status, true, out var parsedStatus))
                return parsedStatus;

            return null;
        }
    }
}