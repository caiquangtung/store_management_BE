using AutoMapper;
using StoreManagement.Application.DTOs.Promotion;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class PromotionMappingProfile : Profile
{
    public PromotionMappingProfile()
    {
        CreateMap<CreatePromotionRequest, Promotion>()
            .ForMember(dest => dest.PromoId, opt => opt.Ignore())
            .ForMember(dest => dest.UsedCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "active"))
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        CreateMap<UpdatePromotionRequest, Promotion>()
            .ForMember(dest => dest.PromoId, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        CreateMap<Promotion, PromotionResponse>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                src.Status.ToLower() == "active" &&
                src.StartDate <= DateTime.Now &&
                src.EndDate >= DateTime.Now))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src =>
                src.EndDate < DateTime.Now))
            .ForMember(dest => dest.IsUsageLimitReached, opt => opt.MapFrom(src =>
                src.UsageLimit > 0 && src.UsedCount >= src.UsageLimit));
    }
}
