using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum PaymentMethodType
{
    [Description("Credit/Debit Card")]
    Card,

    [Description("US Bank Account")]
    UsBankAccount,

    InternationalBankAccount,
    Cash,
    Check
}
