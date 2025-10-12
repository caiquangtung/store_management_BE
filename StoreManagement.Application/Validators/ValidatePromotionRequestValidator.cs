using FluentValidation;
using StoreManagement.Application.DTOs.Promotion;

namespace StoreManagement.Application.Validators;

public class ValidatePromotionRequestValidator : AbstractValidator<ValidatePromotionRequest>
{
    public ValidatePromotionRequestValidator()
    {
        RuleFor(x => x.PromoCode)
            .NotEmpty().WithMessage("Promo code is required")
            .MaximumLength(50).WithMessage("Promo code cannot exceed 50 characters");

        RuleFor(x => x.OrderAmount)
            .GreaterThan(0).WithMessage("Order amount must be greater than 0");
    }
}
