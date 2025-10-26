using FluentValidation;
using StoreManagement.Application.DTOs.Order;

namespace StoreManagement.Application.Validators;

public class AddOrderItemRequestValidator : AbstractValidator<AddOrderItemRequest>
{
    public AddOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required and must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1")
            .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10000");
    }
}
