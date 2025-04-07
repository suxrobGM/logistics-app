namespace Logistics.Domain.Entities;

public class BankAccountPaymentMethod : PaymentMethod
{
    public required string AccountNumber { get; set; }
    public required string BankName { get; set; }
    public required string AccountHolderName { get; set; }
    public required string SwiftCode { get; set; }
}