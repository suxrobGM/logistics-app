namespace Logistics.Application.Modules.Common.Services;

/// <summary>Batches a page's name lookups so list handlers avoid a lazy load per row.</summary>
public interface IDisplayNameLookup : IApplicationService
{
    /// <summary>Employees' full names by id. Null ids and missing employees are left out.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetEmployeeNamesAsync(IEnumerable<Guid?> ids, CancellationToken ct);

    /// <summary>Truck numbers by id. Null ids and missing trucks are left out.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetTruckNumbersAsync(IEnumerable<Guid?> ids, CancellationToken ct);
}
