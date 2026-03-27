using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum TenantFeature
{
    Dashboard,
    Employees,
    Loads,
    Trucks,
    Customers,
    Invoices,
    Payments,

    [Description("ELD / HOS")]
    Eld,

    LoadBoard,
    Messages,
    Notifications,

    [Description("Safety & Compliance")]
    Safety,

    Expenses,
    Payroll,
    Timesheets,
    Maintenance,
    Trips,
    Reports,

    [Description("AI Dispatch")]
    AgenticDispatch,

    [Description("Priority Support")]
    PrioritySupport,

    [Description("API Access")]
    ApiAccess,

    [Description("Telegram Bot")]
    TelegramBot
}
