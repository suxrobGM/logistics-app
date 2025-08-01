using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum PaymentMethodType
{
    [Description("Credit/Debit Card"), EnumMember(Value = "card")] 
    Card,
    
    [Description("US Bank Account"), EnumMember(Value = "us_bank_account")] 
    UsBankAccount,
    
    [Description("International Bank Account"), EnumMember(Value = "international_bank_account")] 
    InternationalBankAccount,
    
    [Description("Cash"), EnumMember(Value = "cash")]
    Cash,
    
    [Description("Check"), EnumMember(Value = "check")]
    Check,
}
