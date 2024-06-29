using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum PaymentMethod
{
    [Description("Bank Account")]
    BankAccount,
    
    [Description("Credit Card")]
    CreditCard,
    
    [Description("Cash")]
    Cash
}
