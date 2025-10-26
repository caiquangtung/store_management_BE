using FluentValidation;
using StoreManagement.Application.DTOs.Categories;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CategoryName));
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status specified.")
            .Must(status => status != EntityStatus.Deleted)
                .WithMessage("Cannot set status to 'Deleted' via update. Please use the DELETE endpoint.")
            .When(x => x.Status.HasValue);
    }
}