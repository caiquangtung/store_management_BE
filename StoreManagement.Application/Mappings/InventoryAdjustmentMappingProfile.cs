using AutoMapper;
using StoreManagement.Application.DTOs.InventoryAdjustment;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class InventoryAdjustmentMappingProfile : Profile
{
    public InventoryAdjustmentMappingProfile()
    {
        // Request -> Entity
        CreateMap<CreateAdjustmentRequest, InventoryAdjustment>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Entity -> Response
        CreateMap<InventoryAdjustment, AdjustmentResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
            .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));
    }
}