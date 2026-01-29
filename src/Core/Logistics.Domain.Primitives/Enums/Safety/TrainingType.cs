using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum TrainingType
{
    [Description("Initial Hiring Training")] [EnumMember(Value = "initial_hiring")]
    InitialHiring,

    [Description("Annual Refresher")] [EnumMember(Value = "annual")]
    Annual,

    [Description("Remedial Training")] [EnumMember(Value = "remedial")]
    Remedial,

    [Description("Safety Training")] [EnumMember(Value = "safety")]
    Safety,

    [Description("Hazmat Training")] [EnumMember(Value = "hazmat")]
    Hazmat,

    [Description("Defensive Driving")] [EnumMember(Value = "defensive")]
    Defensive,

    [Description("Load Securement")] [EnumMember(Value = "load_securement")]
    LoadSecurement,

    [Description("Customer Service")] [EnumMember(Value = "customer_service")]
    CustomerService,

    [Description("Equipment Training")] [EnumMember(Value = "equipment")]
    Equipment,

    [Description("Compliance Training")] [EnumMember(Value = "compliance")]
    Compliance,

    [Description("ELD Training")] [EnumMember(Value = "eld")]
    Eld,

    [Description("Other Training")] [EnumMember(Value = "other")]
    Other
}
