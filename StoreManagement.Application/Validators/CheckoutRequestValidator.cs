using FluentValidation;
using StoreManagement.Application.DTOs.Order;

namespace StoreManagement.Application.Validators;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(BeValidPaymentMethod).WithMessage("Payment method must be one of: Cash, Card, BankTransfer, EWallet");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0");
    }

    private bool BeValidPaymentMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method)) return false;

        var validMethods = new[] { "Cash", "Card", "BankTransfer", "EWallet" };
        return validMethods.Contains(method, StringComparer.OrdinalIgnoreCase);
    }
}
