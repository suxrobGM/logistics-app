using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
///     Represents the type of exception or issue encountered during load transportation.
/// </summary>
public enum LoadExceptionType
{
    [Description("Delay")] [EnumMember(Value = "delay")]
    Delay,

    [Description("Accident")] [EnumMember(Value = "accident")]
    Accident,

    [Description("Weather Delay")] [EnumMember(Value = "weather_delay")]
    WeatherDelay,

    [Description("Mechanical Failure")] [EnumMember(Value = "mechanical_failure")]
    MechanicalFailure,

    [Description("Route Change")] [EnumMember(Value = "route_change")]
    RouteChange,

    [Description("Customer Request")] [EnumMember(Value = "customer_request")]
    CustomerRequest,

    [Description("Other")] [EnumMember(Value = "other")]
    Other
}
