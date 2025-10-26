using FluentValidation;
using StoreManagement.Application.DTOs.Order;

namespace StoreManagement.Application.Validators;

public class ApplyPromotionRequestValidator : AbstractValidator<ApplyPromotionRequest>
{
    public ApplyPromotionRequestValidator()
    {
        RuleFor(x => x.PromoCode)
            .NotEmpty().WithMessage("Promo code is required")
            .MaximumLength(50).WithMessage("Promo code must not exceed 50 characters")
            .Matches("^[A-Z0-9]+$").WithMessage("Promo code must contain only uppercase letters and numbers");
    }
}
