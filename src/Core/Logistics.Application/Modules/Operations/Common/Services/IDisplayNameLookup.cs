namespace Logistics.Application.Modules.Operations.Common.Services;

/// <summary>
///     Names for a page of rows in one query each. Lazy loading would read every row's employee or truck
///     separately, so list handlers collect the ids first and look them up here.
/// </summary>
public interface IDisplayNameLookup : IApplicationService
{
    /// <summary>Employees' full names by id. Null ids and missing employees are left out.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetEmployeeNamesAsync(IEnumerable<Guid?> ids, CancellationToken ct);

    /// <summary>Truck numbers by id. Null ids and missing trucks are left out.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetTruckNumbersAsync(IEnumerable<Guid?> ids, CancellationToken ct);
}
