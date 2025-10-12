using FluentValidation;
using StoreManagement.Application.DTOs.Promotion;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Validators;

public class CreatePromotionRequestValidator : AbstractValidator<CreatePromotionRequest>
{
    public CreatePromotionRequestValidator()
    {
        RuleFor(x => x.PromoCode)
            .NotEmpty().WithMessage("Promo code is required")
            .MaximumLength(50).WithMessage("Promo code cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("Promo code can only contain uppercase letters, numbers, underscores and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description cannot exceed 255 characters");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("Discount type must be either Percent or Fixed");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .Must((model, startDate) => startDate >= DateTime.Today)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount cannot be negative");

        RuleFor(x => x.UsageLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Usage limit cannot be negative");

        // Custom validation for percent discount
        When(x => x.DiscountType == DiscountType.Percent, () =>
        {
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100).WithMessage("Percent discount cannot exceed 100%");
        });
    }
}
