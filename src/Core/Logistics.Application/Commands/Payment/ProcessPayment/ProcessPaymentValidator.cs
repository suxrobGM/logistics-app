using FluentValidation;
using Logistics.Shared;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class ProcessPaymentValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentValidator()
    {
        RuleFor(i => i.PaymentId).NotEmpty();
        RuleFor(i => i.BillingAddress).NotEmpty();
        
        When(i => i.PaymentMethod == PaymentMethod.CreditCard, () =>
        {
            RuleFor(i => i.CardholderName).NotEmpty();
            RuleFor(i => i.CardNumber).NotEmpty().Matches(RegexPatterns.CreditCardNumber);
            RuleFor(i => i.CardExpirationDate).NotEmpty().Matches(RegexPatterns.CardExpirationDate);
            RuleFor(i => i.CardCvv).NotEmpty().Matches(RegexPatterns.CardCvv);
        });
        
        When(i => i.PaymentMethod == PaymentMethod.BankAccount, () =>
        {
            RuleFor(i => i.BankName).NotEmpty();
            RuleFor(i => i.BankAccountNumber).NotEmpty();
            RuleFor(i => i.BankRoutingNumber).NotEmpty().Matches(RegexPatterns.BankRoutingNumber);
        });
    }
}
