using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Commands;

public class CreatePaymentMethodCommand : IAppRequest
{
    public PaymentMethodType Type { get; set; }
    public PaymentMethodVerificationStatus? VerificationStatus { get; set; }
    public Address BillingAddress { get; set; } = null!;
    public string? StripePaymentMethodId { get; set; }

    // Card-specific
    public string? CardHolderName { get; set; }
    public string? CardNumber { get; set; }
    public string? Cvc { get; set; }
    public int? ExpMonth { get; set; }
    public int? ExpYear { get; set; }

    // US Bank account-specific
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string? BankName { get; set; }
    public string? RoutingNumber { get; set; }
    public UsBankAccountHolderType? AccountHolderType { get; set; }
    public UsBankAccountType? AccountType { get; set; }
    public string? VerificationUrl { get; set; }

    // International Bank Account
    public string? SwiftCode { get; set; }
}
