using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum PaymentMethodType
{
    [Description("Credit/Debit Card"), EnumMember(Value = "card")] 
    Card,
    
    [Description("US Bank Account"), EnumMember(Value = "us_bank_account")] 
    UsBankAccount,
    
    [Description("International Bank Account"), EnumMember(Value = "international_bank_account")] 
    InternationalBankAccount,
}
