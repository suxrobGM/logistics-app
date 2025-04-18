﻿using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class CreatePaymentMethodCommand : IRequest<Result>
{
    public string TenantId { get; set; } = null!;
    public PaymentMethodType Type { get; set; }
    public Address BillingAddress { get; set; } = null!;

    // Card-specific
    public string? CardHolderName { get; set; }
    public string? CardBrand { get; set; } // Visa, MasterCard, etc.
    public string? CardNumber { get; set; }
    public string? Cvc { get; set; }
    public int? ExpMonth { get; set; }
    public int? ExpYear { get; set; }
    public CardFundingType? FundingType { get; set; }

    // US Bank account-specific
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string? BankName { get; set; }
    public string? RoutingNumber { get; set; }
    public UsBankAccountHolderType? AccountHolderType { get; set; }
    public UsBankAccountType? AccountType { get; set; }

    // International Bank Account
    public string? SwiftCode { get; set; }
}
