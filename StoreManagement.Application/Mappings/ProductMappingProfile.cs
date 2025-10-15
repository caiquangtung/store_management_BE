using AutoMapper;
using StoreManagement.Application.DTOs.Products;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Product -> ProductResponse
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.SupplierId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.Barcode))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath));  // New mapping

        // CreateProductRequest -> Product
        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore())  // Handled in service
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.Inventory, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

        // UpdateProductRequest -> Product
        CreateMap<UpdateProductRequest, Product>(MemberList.None);  // Only update provided fields in service
    }
}