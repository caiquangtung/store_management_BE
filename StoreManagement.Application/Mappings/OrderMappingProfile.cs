using AutoMapper;
using StoreManagement.Application.DTOs.Order;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // ✅ Order -> OrderResponse
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
            .ForMember(dest => dest.PromoId, opt => opt.MapFrom(src => src.PromoId))
            .ForMember(dest => dest.PromoCode, opt => opt.MapFrom(src => src.Promotion != null ? src.Promotion.PromoCode : null))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount))
            .ForMember(dest => dest.FinalAmount, opt => opt.MapFrom(src => (src.TotalAmount ?? 0) - src.DiscountAmount))
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));

        // ✅ OrderItem -> OrderItemResponse
        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(dest => dest.OrderItemId, opt => opt.MapFrom(src => src.OrderItemId))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId ?? 0))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImagePath : null))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal));

        // ✅ Payment -> PaymentResponse
        CreateMap<Payment, PaymentResponse>()
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId))
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate));

        // ✅ CreateOrderRequest -> Order
        CreateMap<CreateOrderRequest, Order>()
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.PromoId, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Promotion, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore());

        // ✅ UpdateOrderRequest -> Order
        CreateMap<UpdateOrderRequest, Order>(MemberList.None);
    }
}
