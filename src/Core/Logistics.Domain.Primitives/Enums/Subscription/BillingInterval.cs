using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum BillingInterval
{
    [Description("Daily"), EnumMember(Value = "day")] Day,
    [Description("Weekly"), EnumMember(Value = "week")] Week,
    [Description("Monthly"), EnumMember(Value = "monthly")] Month,
    [Description("Yearly"), EnumMember(Value = "year")] Year
}
