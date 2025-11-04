using AutoMapper;
using StoreManagement.Application.DTOs.Purchase;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class PurchaseMappingProfile : Profile
{
    public PurchaseMappingProfile()
    {
        // Request -> Entity
        CreateMap<CreatePurchaseRequest, Purchase>()
            .ForMember(dest => dest.PurchaseItems, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.PurchaseStatus.Pending))
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()); // Sẽ tính trong service

        CreateMap<PurchaseItemRequest, PurchaseItem>()
            .ForMember(dest => dest.Subtotal, opt => opt.Ignore()); // Sẽ được DB tính toán

        // Entity -> Response
        CreateMap<Purchase, PurchaseResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));

        CreateMap<PurchaseItem, PurchaseItemResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
            .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Barcode : null));
    }
}