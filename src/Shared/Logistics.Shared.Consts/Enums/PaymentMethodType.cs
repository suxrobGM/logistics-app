using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum PaymentMethodType
{
    [Description("Credit/Debit Card")] Card,
    [Description("US Bank Account")] UsBankAccount,
    [Description("International Bank Account")] InternationalBankAccount,
}
