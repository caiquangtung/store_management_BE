using AutoMapper;
using StoreManagement.Application.DTOs.Customer;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<CreateCustomerRequest, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        CreateMap<UpdateCustomerRequest, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore());
            
        CreateMap<Customer, CustomerResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<Customer, CustomerResponse>();
    }
}
