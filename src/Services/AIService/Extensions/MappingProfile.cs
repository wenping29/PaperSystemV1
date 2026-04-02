using AutoMapper;
using AIService.DTOs;
using AIService.Entities;

namespace AIService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 可以在这里添加DTO和实体之间的映射配置
            // 例如：CreateMap<AIAssistantRequest, AIAuditLog>();
            // 但当前审计日志存储JSON，不需要复杂映射

            // 如果需要，可以添加其他映射
        }
    }
}