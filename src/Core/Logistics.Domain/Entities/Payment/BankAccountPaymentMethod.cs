namespace Logistics.Domain.Entities;

/// <summary>
/// International Bank Account Payment Method
/// </summary>
public class BankAccountPaymentMethod : PaymentMethod
{
    public required string BankName { get; set; }
    public required string AccountNumber { get; set; }
    public required string AccountHolderName { get; set; }
    public required string SwiftCode { get; set; }
}