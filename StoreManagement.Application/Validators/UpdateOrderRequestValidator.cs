using FluentValidation;
using StoreManagement.Application.DTOs.Order;

namespace StoreManagement.Application.Validators;

public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID must be greater than 0")
            .When(x => x.CustomerId.HasValue);
    }
}
