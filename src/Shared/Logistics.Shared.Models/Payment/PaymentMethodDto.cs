using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record PaymentMethodDto
{
    public required string Id { get; set; }
    public PaymentMethodType Type { get; set; }
    public bool IsDefault { get; set; }
    public required AddressDto BillingAddress { get; set; }

    // Card-specific
    public string? CardHolderName { get; set; }
    public string? CardBrand { get; set; }
    public string? CardNumber { get; set; } // Masked card number
    public string? Cvv { get; set; } // Masked CVV
    public int? ExpMonth { get; set; }
    public int? ExpYear { get; set; }
    public CardFundingType FundingType { get; set; }

    // US Bank account-specific
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string? BankName { get; set; }
    public string? RoutingNumber { get; set; }
    public UsBankAccountHolderType AccountHolderType { get; set; }
    public UsBankAccountType? AccountType { get; set; }

    // International Bank Account
    public string? SwiftCode { get; set; }
}
