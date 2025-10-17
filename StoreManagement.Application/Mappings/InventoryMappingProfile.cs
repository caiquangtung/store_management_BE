using AutoMapper;
using StoreManagement.Application.DTOs.Inventory;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        // Product -> ProductInfo (NEW: Add this mapping to fix error)
        CreateMap<Product, ProductInfo>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.Barcode))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.SupplierId))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));

        // Inventory -> InventoryResponse
        CreateMap<Inventory, InventoryResponse>()
            .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src => src.InventoryId))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));  // Now maps via new Product -> ProductInfo

        // CreateInventoryRequest -> Inventory
        CreateMap<CreateInventoryRequest, Inventory>()
            .ForMember(dest => dest.InventoryId, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        // UpdateInventoryRequest -> Inventory
        CreateMap<UpdateInventoryRequest, Inventory>(MemberList.None);

        // Inventory -> LowStockResponse (only entity fields, computed fields set manually in service)
        CreateMap<Inventory, LowStockResponse>()
            .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src => src.InventoryId))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
            .ForMember(dest => dest.Threshold, opt => opt.Ignore())  // Set manually in service
            .ForMember(dest => dest.ReorderQuantity, opt => opt.Ignore())  // Set manually in service
            .ForMember(dest => dest.IsLowStock, opt => opt.Ignore());  // Set manually in service
    }
}