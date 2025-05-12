using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record PaymentMethodDto
{
    public Guid Id { get; set; }
    public PaymentMethodType Type { get; set; }
    public PaymentMethodVerificationStatus VerificationStatus { get; set; }
    public bool IsDefault { get; set; }
    public required AddressDto BillingAddress { get; set; }

    // Card-specific
    public string? CardHolderName { get; set; }
    public string? CardNumber { get; set; } // Masked card number
    public string? Cvc { get; set; } // Masked CVC
    public int? ExpMonth { get; set; }
    public int? ExpYear { get; set; }

    // US Bank account-specific
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string? BankName { get; set; }
    public string? RoutingNumber { get; set; }
    public UsBankAccountHolderType AccountHolderType { get; set; }
    public UsBankAccountType? AccountType { get; set; }
    public string? VerificationUrl { get; set; } // Stripe verification URL for ACH verification

    // International Bank Account
    public string? SwiftCode { get; set; }
}
