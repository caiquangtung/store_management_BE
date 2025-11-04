using FluentValidation;
using StoreManagement.Application.DTOs.Purchase;

namespace StoreManagement.Application.Validators;

public class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier ID must be valid")
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Purchase must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Product ID must be valid");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
            item.RuleFor(i => i.PurchasePrice).GreaterThanOrEqualTo(0).WithMessage("Purchase price cannot be negative");
        });
    }
}