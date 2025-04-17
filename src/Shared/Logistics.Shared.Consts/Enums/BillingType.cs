using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum BillingType
{
    [Description("Employee Payroll")]
    EmployeePayroll,
    
    [Description("Subscription Payment")]
    SubscriptionPayment,
    
    [Description("Other")]
    Other
}
