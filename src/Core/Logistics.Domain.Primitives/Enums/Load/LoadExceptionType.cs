namespace Logistics.Domain.Primitives.Enums;

/// <summary>
///     Represents the type of exception or issue encountered during load transportation.
/// </summary>
public enum LoadExceptionType
{
    Delay,
    Accident,
    WeatherDelay,
    MechanicalFailure,
    RouteChange,
    CustomerRequest,
    Other
}
