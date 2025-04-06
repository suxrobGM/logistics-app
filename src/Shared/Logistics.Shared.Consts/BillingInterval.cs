using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum BillingInterval
{
    [Description("Daily")] Day,
    [Description("Weekly")] Week,
    [Description("Monthly")] Month,
    [Description("Yearly")] Year
}