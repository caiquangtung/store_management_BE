using FluentValidation;
using StoreManagement.Application.DTOs.Users;

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
    }
}
