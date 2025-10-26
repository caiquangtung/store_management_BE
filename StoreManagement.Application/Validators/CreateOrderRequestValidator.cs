using FluentValidation;
using StoreManagement.Application.DTOs.Order;

namespace StoreManagement.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID must be greater than 0")
            .When(x => x.CustomerId.HasValue);
    }
}
