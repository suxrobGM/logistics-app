using System.ComponentModel;

namespace Logistics.Shared.Enums;

public enum PaymentMethod
{
    [Description("Bank Account")]
    BankAccount,
    
    [Description("Credit Card")]
    CreditCard,
    
    [Description("Cash")]
    Cash
}
