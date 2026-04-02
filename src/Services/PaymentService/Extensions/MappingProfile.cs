using AutoMapper;
using PaymentService.DTOs;
using PaymentService.Entities;

namespace PaymentService.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // PaymentTransaction 映射
            CreateMap<PaymentTransaction, PaymentDetail>()
                .ForMember(dest => dest.Anonymous, opt => opt.MapFrom(src => src.Anonymous == "true"));

            // 如果需要，可以添加其他映射
            // CreateMap<CreatePaymentRequest, PaymentTransaction>()
            //     .ForMember(dest => dest.Anonymous, opt => opt.MapFrom(src => src.Anonymous ? "true" : "false"));

            // RefundTransaction 映射
            CreateMap<RefundTransaction, RefundDetail>();
        }
    }
}