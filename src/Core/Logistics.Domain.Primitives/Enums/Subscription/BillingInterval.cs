using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum BillingInterval
{
    [Description("Daily")] Day,
    [Description("Weekly")] Week,
    [Description("Monthly")] Month,
    [Description("Yearly")] Year
}
