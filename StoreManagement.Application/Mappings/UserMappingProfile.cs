using AutoMapper;
using StoreManagement.Application.DTOs.Users;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Mappings;

/// <summary>
/// User mapping profile for AutoMapper
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User -> UserResponse
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // CreateUserRequest -> User (excluding password hashing - handled in service)
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Password, opt => opt.Ignore()) // Password will be hashed in service
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Orders, opt => opt.Ignore());
    }
}
