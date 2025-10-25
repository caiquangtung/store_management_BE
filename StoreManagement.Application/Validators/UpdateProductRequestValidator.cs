using FluentValidation;
using StoreManagement.Application.DTOs.Products;
using Microsoft.AspNetCore.Http;
namespace StoreManagement.Application.Validators;
using StoreManagement.Domain.Enums;
public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ProductName));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required")
            .MaximumLength(20).WithMessage("Unit must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Unit));

        RuleFor(x => x.Barcode)
            .MaximumLength(50).WithMessage("Barcode must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Barcode));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier ID must be greater than 0")
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.Image)
            .Must(BeAValidImage).WithMessage("Only JPG, JPEG, PNG images are allowed")
            .When(x => x.Image != null);
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status specified.")
            .Must(status => status != EntityStatus.Deleted)
                .WithMessage("Cannot set status to 'Deleted' via update. Please use the DELETE endpoint.")
            .When(x => x.Status.HasValue);
    }

    private bool BeAValidImage(IFormFile file)
    {
        if (file == null) return true;
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}