using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum TenantFeature
{
    [Description("Dashboard"), EnumMember(Value = "dashboard")]
    Dashboard,

    [Description("Employees"), EnumMember(Value = "employees")]
    Employees,

    [Description("Loads"), EnumMember(Value = "loads")]
    Loads,

    [Description("Trucks"), EnumMember(Value = "trucks")]
    Trucks,

    [Description("Customers"), EnumMember(Value = "customers")]
    Customers,

    [Description("Invoices"), EnumMember(Value = "invoices")]
    Invoices,

    [Description("Payments"), EnumMember(Value = "payments")]
    Payments,

    [Description("ELD / HOS"), EnumMember(Value = "eld")]
    Eld,

    [Description("Load Board"), EnumMember(Value = "loadboard")]
    LoadBoard,

    [Description("Messages"), EnumMember(Value = "messages")]
    Messages,

    [Description("Notifications"), EnumMember(Value = "notifications")]
    Notifications,

    [Description("Safety & Compliance"), EnumMember(Value = "safety")]
    Safety,

    [Description("Expenses"), EnumMember(Value = "expenses")]
    Expenses,

    [Description("Payroll"), EnumMember(Value = "payroll")]
    Payroll,

    [Description("Timesheets"), EnumMember(Value = "timesheets")]
    Timesheets,

    [Description("Maintenance"), EnumMember(Value = "maintenance")]
    Maintenance,

    [Description("Trips"), EnumMember(Value = "trips")]
    Trips,

    [Description("Reports"), EnumMember(Value = "reports")]
    Reports
}
