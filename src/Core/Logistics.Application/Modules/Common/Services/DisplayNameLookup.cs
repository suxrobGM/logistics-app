using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Common.Services;

public sealed class DisplayNameLookup(ITenantUnitOfWork tenantUow) : IDisplayNameLookup
{
    public async Task<IReadOnlyDictionary<Guid, string>> GetEmployeeNamesAsync(
        IEnumerable<Guid?> ids, CancellationToken ct)
    {
        var wanted = Distinct(ids);
        if (wanted.Length == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var employees = await tenantUow.Repository<Employee>().Query()
            .Where(e => wanted.Contains(e.Id))
            .Select(e => new { e.Id, e.FirstName, e.LastName })
            .ToListAsync(ct);

        // Same format as Employee.GetFullName, which can't run inside the query.
        return employees.ToDictionary(e => e.Id, e => string.Join(" ", e.FirstName, e.LastName));
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetTruckNumbersAsync(
        IEnumerable<Guid?> ids, CancellationToken ct)
    {
        var wanted = Distinct(ids);
        if (wanted.Length == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return await tenantUow.Repository<Truck>().Query()
            .Where(t => wanted.Contains(t.Id))
            .Select(t => new { t.Id, t.Number })
            .ToDictionaryAsync(t => t.Id, t => t.Number, ct);
    }

    private static Guid[] Distinct(IEnumerable<Guid?> ids) => ids.OfType<Guid>().Distinct().ToArray();
}
