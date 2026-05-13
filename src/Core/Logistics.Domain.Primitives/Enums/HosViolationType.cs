using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum HosViolationType
{
    // FMCSA (US) — 1–99
    [Description("11-Hour Driving Limit")]
    Driving11Hour = 1,

    [Description("14-Hour On-Duty Limit")]
    OnDuty14Hour = 2,

    [Description("30-Minute Break Required")]
    Break30Minute = 3,

    [Description("70-Hour/8-Day Cycle Limit")]
    Cycle70Hour = 4,

    [Description("34-Hour Restart Required")]
    RestartRequired = 5,

    [Description("Form and Manner Violation")]
    FormAndMannerViolation = 6,

    // EU 561/2006 — 100–199
    [Description("4.5h Continuous Driving Without Break")]
    EuContinuousDriving4_5h = 100,

    [Description("9-Hour Daily Driving Limit")]
    EuDailyDriving9h = 101,

    [Description("56-Hour Weekly Driving Limit")]
    EuWeeklyDriving56h = 102,

    [Description("90-Hour Biweekly Driving Limit")]
    EuBiweeklyDriving90h = 103,

    [Description("11-Hour Daily Rest Required")]
    EuDailyRest11h = 104,

    [Description("45-Hour Weekly Rest Required")]
    EuWeeklyRest45h = 105,

    [Description("Tachograph Form and Manner")]
    EuFormAndManner = 199
}
