using FluentValidation;
using StoreManagement.Application.DTOs.Suppliers;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.Validators;

public class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required")
            .MaximumLength(100).WithMessage("Supplier name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status specified.")
            .Must(status => status != EntityStatus.Deleted)
                .WithMessage("Cannot set status to 'Deleted' via update. Please use the DELETE endpoint.")
            .When(x => x.Status.HasValue);
    }
}