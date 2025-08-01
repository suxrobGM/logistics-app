using FluentValidation;
using Logistics.Application.Constants;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(i => i.Type).IsInEnum();
        RuleFor(i => i.VerificationStatus).IsInEnum();
        RuleFor(i => i.BillingAddress).NotEmpty();
        
        When(i => i.Type == PaymentMethodType.Card, () =>
        {
            var currentYear = DateTime.UtcNow.Year;
            RuleFor(i => i.CardHolderName).NotEmpty();
            RuleFor(i => i.CardNumber).NotEmpty().Matches(RegexPatterns.CreditCardNumber);
            RuleFor(i => i.Cvc).NotEmpty().Matches(RegexPatterns.CardCvc);
            RuleFor(i => i.ExpMonth).NotEmpty().GreaterThan(0).LessThanOrEqualTo(12);
            RuleFor(i => i.ExpYear).NotEmpty().GreaterThanOrEqualTo(currentYear)
                .WithMessage($"Expiration year must be greater than or equal to {currentYear}");
        });

        When(i => i.Type == PaymentMethodType.UsBankAccount, () =>
        {
            RuleFor(i => i.BankName).NotEmpty();
            RuleFor(i => i.AccountNumber).NotEmpty();
            RuleFor(i => i.RoutingNumber).NotEmpty().Matches(RegexPatterns.BankRoutingNumber);
            RuleFor(i => i.AccountHolderName).NotEmpty();
            RuleFor(i => i.AccountHolderType).IsInEnum();
            RuleFor(i => i.AccountType).IsInEnum();
        });
        
        When(i => i.Type == PaymentMethodType.InternationalBankAccount, () =>
        {
            RuleFor(i => i.SwiftCode).NotEmpty();
            RuleFor(i => i.AccountHolderName).NotEmpty();
            RuleFor(i => i.AccountNumber).NotEmpty();
        });
    }
}
