using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum BillingType
{
    [Description("Employee Payroll"), EnumMember(Value = "employee_payroll")]
    EmployeePayroll,

    [Description("Subscription Payment"), EnumMember(Value = "subscription_payment")]
    SubscriptionPayment,

    [Description("Other"), EnumMember(Value = "other")]
    Other
}
