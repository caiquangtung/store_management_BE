using FluentValidation;
using StoreManagement.Application.DTOs.Users;

namespace StoreManagement.Application.Validators;

/// <summary>
/// Validator for CreateUserRequest
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .Length(3, 50)
            .WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters");

        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .WithMessage("Full name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role specified");
    }
}
