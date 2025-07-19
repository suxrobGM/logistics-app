using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum BillingInterval
{
    [Description("Daily"), EnumMember(Value = "day")] Day,
    [Description("Weekly"), EnumMember(Value = "week")] Week,
    [Description("Monthly"), EnumMember(Value = "monthly")] Month,
    [Description("Yearly"), EnumMember(Value = "year")] Year
}