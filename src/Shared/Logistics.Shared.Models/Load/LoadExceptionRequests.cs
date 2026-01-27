using Logistics.Domain.Primitives.Enums;

/// <summary>
///     Request model for assigning a load to a truck.
/// </summary>
public record AssignLoadRequest(Guid? TruckId);

/// <summary>
///     Request model for bulk assigning loads to a truck.
/// </summary>
public record BulkAssignRequest(Guid[] LoadIds, Guid TruckId);

/// <summary>
///     Request model for reporting a load exception.
/// </summary>
public record ReportExceptionRequest(LoadExceptionType Type, string Reason);
