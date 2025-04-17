using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum TrialPeriod
{
    [Description("None")] None,
    [Description("7 Days")] SevenDays,
    [Description("14 Days")] FourteenDays,
    [Description("30 Days")] ThirtyDays
}