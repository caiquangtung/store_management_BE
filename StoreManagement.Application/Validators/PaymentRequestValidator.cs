using FluentValidation;
using StoreManagement.Application.DTOs.Order;

namespace StoreManagement.Application.Validators;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(BeValidPaymentMethod).WithMessage("Payment method must be one of: Cash, Card, BankTransfer, EWallet");
    }

    private bool BeValidPaymentMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method)) return false;

        var validMethods = new[] { "Cash", "Card", "BankTransfer", "EWallet" };
        return validMethods.Contains(method, StringComparer.OrdinalIgnoreCase);
    }
}
