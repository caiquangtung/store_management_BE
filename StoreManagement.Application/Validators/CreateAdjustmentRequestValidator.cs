using FluentValidation;
using StoreManagement.Application.DTOs.InventoryAdjustment;

namespace StoreManagement.Application.Validators;

public class CreateAdjustmentRequestValidator : AbstractValidator<CreateAdjustmentRequest>
{
    public CreateAdjustmentRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantity cannot be zero");
            
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(255).WithMessage("Reason cannot exceed 255 characters");
    }
}