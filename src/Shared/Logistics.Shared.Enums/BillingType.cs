using System.ComponentModel;

namespace Logistics.Shared.Enums;

public enum BillingType
{
    [Description("Employee Payroll")]
    EmployeePayroll,
    
    [Description("Subscription Payment")]
    SubscriptionPayment,
    
    [Description("Other")]
    Other
}
