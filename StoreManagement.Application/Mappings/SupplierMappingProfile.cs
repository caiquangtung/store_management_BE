using AutoMapper;
using StoreManagement.Application.DTOs.Suppliers;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class SupplierMappingProfile : Profile
{
    public SupplierMappingProfile()
    {
        CreateMap<Supplier, SupplierResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        CreateMap<CreateSupplierRequest, Supplier>()
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());
    }
}