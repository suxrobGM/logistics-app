using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum InvoiceLineItemType
{
    [Description("Base Rate"), EnumMember(Value = "base_rate")]
    BaseRate,

    [Description("Fuel Surcharge"), EnumMember(Value = "fuel_surcharge")]
    FuelSurcharge,

    [Description("Detention"), EnumMember(Value = "detention")]
    Detention,

    [Description("Layover"), EnumMember(Value = "layover")]
    Layover,

    [Description("Lumper"), EnumMember(Value = "lumper")]
    Lumper,

    [Description("Accessorial"), EnumMember(Value = "accessorial")]
    Accessorial,

    [Description("Discount"), EnumMember(Value = "discount")]
    Discount,

    [Description("Tax"), EnumMember(Value = "tax")]
    Tax,

    [Description("Other"), EnumMember(Value = "other")]
    Other,

    // Payroll-specific line item types
    [Description("Base Pay"), EnumMember(Value = "base_pay")]
    BasePay,

    [Description("Bonus"), EnumMember(Value = "bonus")]
    Bonus,

    [Description("Deduction"), EnumMember(Value = "deduction")]
    Deduction,

    [Description("Reimbursement"), EnumMember(Value = "reimbursement")]
    Reimbursement,

    [Description("Adjustment"), EnumMember(Value = "adjustment")]
    Adjustment
}
