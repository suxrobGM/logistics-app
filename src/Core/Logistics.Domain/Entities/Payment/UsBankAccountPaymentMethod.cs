using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class UsBankAccountPaymentMethod : PaymentMethod
{
    public required string AccountNumber { get; set; }
    public required string BankName { get; set; }
    public required string RoutingNumber { get; set; }
    public UsBankAccountHolderType AccountHolderType { get; set; }
    public required string AccountHolderName { get; set; }
    public UsBankAccountType AccountType { get; set; }
}