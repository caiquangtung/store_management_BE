using FluentValidation;
using StoreManagement.Application.DTOs.Inventory;

namespace StoreManagement.Application.Validators;

public class UpdateInventoryRequestValidator : AbstractValidator<UpdateInventoryRequest>
{
    public UpdateInventoryRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0")
            .When(x => !x.SetToZero);
    }
}