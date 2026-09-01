using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;

namespace Logistics.Application.Modules.Integrations.Documents;

/// <summary>
/// The caller's role, resolved once so a handler does not re-query it per document.
/// </summary>
internal sealed record DocumentAccessContext(Guid CallerId, string? RoleName)
{
    public bool IsManagement =>
        RoleName is TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher;

    public bool IsDriver => RoleName == TenantRoles.Driver;
}

/// <summary>
/// Per-record authorization for the staff-facing DocumentController, reads and writes alike.
/// Management handles every document in the tenant, a driver only those tied to them. Everyone
/// else is denied here and must use a properly-scoped portal endpoint.
/// </summary>
internal static class DocumentAccess
{
    /// <summary>Null when the caller is not an employee of this tenant, which denies them.</summary>
    public static async Task<DocumentAccessContext?> ResolveAsync(
        ITenantUnitOfWork tenantUow, ICurrentUserService currentUserService, CancellationToken ct)
    {
        if (currentUserService.GetUserId() is not { } callerId)
        {
            return null;
        }

        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(callerId, ct);
        return employee?.Role?.Name is { } roleName
            ? new DocumentAccessContext(callerId, roleName)
            : null;
    }

    public static async Task<bool> CanAccessAsync(
        ITenantUnitOfWork tenantUow, DocumentAccessContext ctx, Document document, CancellationToken ct)
    {
        if (ctx.IsManagement)
        {
            return true;
        }

        if (!ctx.IsDriver)
        {
            return false;
        }

        return document switch
        {
            EmployeeDocument ed => ed.EmployeeId == ctx.CallerId,
            TruckDocument td => await DrivesTruckAsync(tenantUow, ctx.CallerId, td.TruckId, ct),
            LoadDocument ld => await DrivesLoadAsync(tenantUow, ctx.CallerId, ld.LoadId, ct),
            _ => false
        };
    }

    /// <summary>
    /// The create path: an upload names its owner before any document exists, so a driver is
    /// checked against the target load, truck, or employee record instead.
    /// </summary>
    public static async Task<bool> CanAccessOwnerAsync(
        ITenantUnitOfWork tenantUow,
        DocumentAccessContext ctx,
        DocumentOwnerType ownerType,
        Guid ownerId,
        CancellationToken ct)
    {
        if (ctx.IsManagement)
        {
            return true;
        }

        if (!ctx.IsDriver)
        {
            return false;
        }

        return ownerType switch
        {
            DocumentOwnerType.Employee => ownerId == ctx.CallerId,
            DocumentOwnerType.Truck => await DrivesTruckAsync(tenantUow, ctx.CallerId, ownerId, ct),
            DocumentOwnerType.Load => await DrivesLoadAsync(tenantUow, ctx.CallerId, ownerId, ct),
            _ => false
        };
    }

    /// <summary>
    /// Reads a driver's truck and load sets once rather than per document, which is why this does
    /// not just loop <see cref="CanAccessAsync"/>.
    /// </summary>
    public static async Task<List<TDocument>> FilterAccessibleAsync<TDocument>(
        ITenantUnitOfWork tenantUow,
        DocumentAccessContext ctx,
        List<TDocument> documents,
        CancellationToken ct)
        where TDocument : Document
    {
        if (ctx.IsManagement)
        {
            return documents;
        }

        if (!ctx.IsDriver || documents.Count == 0)
        {
            return [];
        }

        var truckIds = (await tenantUow.Repository<Truck>()
                .GetListAsync(t => t.MainDriverId == ctx.CallerId || t.SecondaryDriverId == ctx.CallerId, ct))
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
            EmployeeDocument ed => ed.EmployeeId == ctx.CallerId,
            TruckDocument td => truckIds.Contains(td.TruckId),
            LoadDocument ld => loadIds.Contains(ld.LoadId),
            _ => false
        }).ToList();
    }

    private static async Task<bool> DrivesTruckAsync(
        ITenantUnitOfWork tenantUow, Guid driverId, Guid truckId, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckId, ct);
        return truck is not null &&
               (truck.MainDriverId == driverId || truck.SecondaryDriverId == driverId);
    }

    private static async Task<bool> DrivesLoadAsync(
        ITenantUnitOfWork tenantUow, Guid driverId, Guid loadId, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(loadId, ct);
        return load?.AssignedTruckId is { } truckId &&
               await DrivesTruckAsync(tenantUow, driverId, truckId, ct);
    }
}
