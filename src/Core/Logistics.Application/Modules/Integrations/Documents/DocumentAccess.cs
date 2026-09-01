using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;

namespace Logistics.Application.Modules.Integrations.Documents;

internal sealed record DocumentAccessContext(Guid CallerId, string? RoleName)
{
    public bool IsManagement =>
        RoleName is TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher;

    public bool IsDriver => RoleName == TenantRoles.Driver;
}

internal static class DocumentAccess
{
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

    public static async Task<bool> CanAccessOwnerAsync(
        ITenantUnitOfWork tenantUow,
        DocumentAccessContext ctx,
        DocumentOwnerType ownerType,
        Guid ownerId,
        CancellationToken ct)
    {
        return ownerType switch
        {
            DocumentOwnerType.Employee =>
                await tenantUow.Repository<Employee>().GetByIdAsync(ownerId, ct) is not null &&
                (ctx.IsManagement || ctx.IsDriver && ownerId == ctx.CallerId),
            DocumentOwnerType.Truck => ctx.IsManagement
                ? await tenantUow.Repository<Truck>().GetByIdAsync(ownerId, ct) is not null
                : ctx.IsDriver && await DrivesTruckAsync(tenantUow, ctx.CallerId, ownerId, ct),
            DocumentOwnerType.Load => ctx.IsManagement
                ? await tenantUow.Repository<Load>().GetByIdAsync(ownerId, ct) is not null
                : ctx.IsDriver && await DrivesLoadAsync(tenantUow, ctx.CallerId, ownerId, ct),
            _ => false
        };
    }

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
