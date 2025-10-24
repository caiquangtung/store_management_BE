using FluentValidation;
using StoreManagement.Application.DTOs.Users;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.Validators;

/// <summary>
/// Validator for UpdateUserRequest
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .WithMessage("Full name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role specified")
            .When(x => x.Role.HasValue);

        RuleFor(x => x.NewPassword)
            .MinimumLength(6)
            .WithMessage("New password must be at least 6 characters long")
            .MaximumLength(100)
            .WithMessage("New password must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.NewPassword));

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required if provided")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain alphanumeric characters and underscore")
            .When(x => !string.IsNullOrEmpty(x.Username));
        
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status specified.")
            // Ngăn chặn việc đặt trạng thái 'Deleted' qua endpoint này.
            // Trạng thái 'Deleted' chỉ nên được thiết lập bởi endpoint DELETE.
            .Must(status => status != EntityStatus.Deleted)
            .WithMessage("Cannot set status to 'Deleted' via update. Please use the DELETE endpoint.")
            .When(x => x.Status.HasValue);
    }
}
