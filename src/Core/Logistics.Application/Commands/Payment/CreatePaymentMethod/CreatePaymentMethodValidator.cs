using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(i => i.TenantId).NotEmpty();
        RuleFor(i => i.Type).IsInEnum();
        RuleFor(i => i.BillingAddress).NotEmpty();
        
        When(i => i.Type == PaymentMethodType.Card, () =>
        {
            var currentYear = DateTime.UtcNow.Year;
            RuleFor(i => i.FundingType).IsInEnum();
            RuleFor(i => i.CardHolderName).NotEmpty();
            RuleFor(i => i.CardNumber).NotEmpty().Matches(RegexPatterns.CreditCardNumber);
            RuleFor(i => i.Cvv).NotEmpty().Matches(RegexPatterns.CardCvv);
            RuleFor(i => i.CardBrand).NotEmpty();
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
