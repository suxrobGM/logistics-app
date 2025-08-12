using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class UsBankAccountPaymentMethod : PaymentMethod
{
    public override PaymentMethodType Type { get; protected set; } = PaymentMethodType.UsBankAccount;
    public required string BankName { get; set; }
    public required string AccountHolderName { get; set; }
    public required string AccountNumber { get; set; }
    public required string RoutingNumber { get; set; }
    public UsBankAccountHolderType AccountHolderType { get; set; }
    public UsBankAccountType AccountType { get; set; }

    /// <summary>
    ///     Stripe verification URL for ACH verification
    /// </summary>
    public string? VerificationUrl { get; set; }
}
