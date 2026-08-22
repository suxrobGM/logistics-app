using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

/// <summary>
/// Reads names from the tenant's own employees: <c>Employee.Id</c> is the user id
/// (<see cref="Employee.CreateEmployeeFromUser"/>), so no master round trip is needed.
/// </summary>
internal sealed class AgentSenderDirectory(ITenantUnitOfWork tenantUow) : IAgentSenderDirectory
{
    public async Task<string?> GetNameAsync(Guid? userId, CancellationToken ct)
    {
        if (userId is not { } id)
            return null;

        var names = await GetNamesAsync([id], ct);
        return names.GetValueOrDefault(id);
    }

    public async Task<Dictionary<Guid, string>> GetNamesAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken ct)
    {
        if (userIds.Count == 0)
            return [];

        // Projected, not materialized: full rows would carry lazy-loading proxies for a name.
        var rows = await tenantUow.Repository<Employee>().Query()
            .Where(e => userIds.Contains(e.Id))
            .Select(e => new { e.Id, e.FirstName, e.LastName })
            .ToListAsync(ct);

        // Same formatting as Employee.GetFullName, which needs a whole entity.
        return rows.ToDictionary(r => r.Id, r => string.Join(" ", r.FirstName, r.LastName));
    }
}
