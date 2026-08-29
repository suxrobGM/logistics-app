using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Roles;

namespace Logistics.Application.Modules.Integrations.Documents;

/// <summary>
/// Per-record authorization for documents on the staff-facing DocumentController. Without it, any
/// authenticated user could pull any document in the tenant by id (employee driver-license PII,
/// every customer's BOLs/PODs). Management sees all; a driver sees only documents tied to them
/// (their own employee record, their assigned trucks, and loads on those trucks); anyone else is
/// denied here and must use a properly-scoped portal endpoint.
/// </summary>
internal static class DocumentAccess
{
    public static async Task<bool> CanAccessAsync(
        ITenantUnitOfWork tenantUow, Guid? callerId, Document document, CancellationToken ct)
    {
        if (callerId is not { } caller)
        {
            return false;
        }

        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(caller, ct);
        var roleName = employee?.Role?.Name;

        // Not an employee of this tenant (e.g. a customer-portal user) - deny on the staff controller.
        if (roleName is null)
        {
            return false;
        }

        // Owner / Manager / Dispatcher legitimately manage all documents.
        if (roleName is TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher)
        {
            return true;
        }

        // Driver: only documents tied to them.
        if (roleName == TenantRoles.Driver)
        {
            return document switch
            {
                EmployeeDocument ed => ed.EmployeeId == caller,
                TruckDocument td => await IsDriverOfTruckAsync(tenantUow, caller, td.TruckId, ct),
                LoadDocument ld => await IsDriverOfLoadAsync(tenantUow, caller, ld.LoadId, ct),
                _ => false
            };
        }

        return false;
    }

    /// <summary>
    /// Filters a document list down to what the caller may see. Management sees everything; a driver
    /// sees only documents tied to them; anyone else sees nothing on this staff-facing surface.
    /// Resolves the caller's role once and (for drivers) their truck/load set once, rather than
    /// re-querying per document.
    /// </summary>
    public static async Task<List<TDocument>> FilterAccessibleAsync<TDocument>(
        ITenantUnitOfWork tenantUow, Guid? callerId, List<TDocument> documents, CancellationToken ct)
        where TDocument : Document
    {
        if (callerId is not { } caller || documents.Count == 0)
        {
            return [];
        }

        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(caller, ct);
        var roleName = employee?.Role?.Name;

        if (roleName is TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher)
        {
            return documents;
        }

        if (roleName != TenantRoles.Driver)
        {
            return [];
        }

        // Driver: resolve their trucks (and the loads on them) once, then filter in memory.
        var truckIds = (await tenantUow.Repository<Truck>()
                .GetListAsync(t => t.MainDriverId == caller || t.SecondaryDriverId == caller, ct))
            .Select(t => t.Id)
            .ToHashSet();

        var loadIds = truckIds.Count == 0
            ? []
            : (await tenantUow.Repository<Load>()
                .GetListAsync(l => l.AssignedTruckId != null && truckIds.Contains(l.AssignedTruckId.Value), ct))
            .Select(l => l.Id)
            .ToHashSet();

        return documents.Where(d => d switch
        {
            EmployeeDocument ed => ed.EmployeeId == caller,
            TruckDocument td => truckIds.Contains(td.TruckId),
            LoadDocument ld => loadIds.Contains(ld.LoadId),
            _ => false
        }).ToList();
    }

    private static async Task<bool> IsDriverOfTruckAsync(
        ITenantUnitOfWork tenantUow, Guid driverId, Guid truckId, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckId, ct);
        return truck is not null &&
               (truck.MainDriverId == driverId || truck.SecondaryDriverId == driverId);
    }

    private static async Task<bool> IsDriverOfLoadAsync(
        ITenantUnitOfWork tenantUow, Guid driverId, Guid loadId, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(loadId, ct);
        if (load?.AssignedTruckId is not { } truckId)
        {
            return false;
        }

        return await IsDriverOfTruckAsync(tenantUow, driverId, truckId, ct);
    }
}
