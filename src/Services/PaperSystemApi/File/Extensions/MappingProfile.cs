using AutoMapper;
using PaperSystemApi.File.DTOs;
using PaperSystemApi.File.Entities;

namespace PaperSystemApi.File.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // FileMetadata 映射
            CreateMap<FileMetadata, FileDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType));

            CreateMap<UploadFileRequest, FileMetadata>()
                .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => src.File.FileName))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.File.ContentType))
                .ForMember(dest => dest.SizeInBytes, opt => opt.MapFrom(src => src.File.Length))
                .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType ?? DetermineFileType(src.File.ContentType, Path.GetExtension(src.File.FileName))))
                .ForMember(dest => dest.StoragePath, opt => opt.Ignore()) // 在服务中设置
                .ForMember(dest => dest.FileId, opt => opt.Ignore()) // 在服务中生成
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => FileStatus.Uploaded))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateFileRequest, FileMetadata>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

        private string DetermineFileType(string contentType, string fileExtension)
        {
            if (contentType.StartsWith("image/")) return FileType.Image;
            if (contentType.StartsWith("video/")) return FileType.Video;
            if (contentType.StartsWith("audio/")) return FileType.Audio;
            if (contentType.Contains("pdf") || contentType.Contains("document") || contentType.Contains("text"))
                return FileType.Document;
            if (fileExtension == ".zip" || fileExtension == ".rar" || fileExtension == ".7z" || fileExtension == ".tar" || fileExtension == ".gz")
                return FileType.Archive;

            return FileType.Other;
        }
    }
}