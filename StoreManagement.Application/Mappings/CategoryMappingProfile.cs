using AutoMapper;
using StoreManagement.Application.DTOs.Categories;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryResponse>();
        CreateMap<CreateCategoryRequest, Category>()
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());
    }
}