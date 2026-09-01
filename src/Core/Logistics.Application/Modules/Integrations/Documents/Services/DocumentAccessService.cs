using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

internal sealed class DocumentAccessService(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService) : IDocumentAccessService
{
    public async Task<DocumentCaller?> ResolveCallerAsync(CancellationToken ct = default)
    {
        if (currentUserService.GetUserId() is not { } callerId)
        {
            return null;
        }

        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(callerId, ct);

        return employee?.Role?.Name is { } roleName
            ? new DocumentCaller(
                callerId,
                IsManagement: roleName is TenantRoles.Owner or TenantRoles.Manager or TenantRoles.Dispatcher,
                IsDriver: roleName == TenantRoles.Driver)
            : null;
    }

    public async Task<bool> CanAccessAsync(
        DocumentCaller caller, Document document, CancellationToken ct = default)
    {
        if (caller.IsManagement)
        {
            return true;
        }

        if (!caller.IsDriver)
        {
            return false;
        }

        return document switch
        {
            EmployeeDocument ed => ed.EmployeeId == caller.CallerId,
            TruckDocument td => await DrivesTruckAsync(caller.CallerId, td.TruckId, ct),
            LoadDocument ld => await DrivesLoadAsync(caller.CallerId, ld.LoadId, ct),
            _ => false
        };
    }

    public async Task<bool> CanAccessOwnerAsync(
        DocumentCaller caller, DocumentOwnerType ownerType, Guid ownerId, CancellationToken ct = default)
    {
        return ownerType switch
        {
            DocumentOwnerType.Employee =>
                await tenantUow.Repository<Employee>().GetByIdAsync(ownerId, ct) is not null &&
                (caller.IsManagement || (caller.IsDriver && ownerId == caller.CallerId)),
            DocumentOwnerType.Truck => caller.IsManagement
                ? await tenantUow.Repository<Truck>().GetByIdAsync(ownerId, ct) is not null
                : caller.IsDriver && await DrivesTruckAsync(caller.CallerId, ownerId, ct),
            DocumentOwnerType.Load => caller.IsManagement
                ? await tenantUow.Repository<Load>().GetByIdAsync(ownerId, ct) is not null
                : caller.IsDriver && await DrivesLoadAsync(caller.CallerId, ownerId, ct),
            _ => false
        };
    }

    public async Task<List<TDocument>> FilterAccessibleAsync<TDocument>(
        DocumentCaller caller, List<TDocument> documents, CancellationToken ct = default)
        where TDocument : Document
    {
        if (caller.IsManagement)
        {
            return documents;
        }

        if (!caller.IsDriver || documents.Count == 0)
        {
            return [];
        }

        var truckIds = await tenantUow.Repository<Truck>()
            .Query()
            .Where(t => t.MainDriverId == caller.CallerId || t.SecondaryDriverId == caller.CallerId)
            .Select(t => t.Id)
            .ToHashSetAsync(ct);

        var loadIds = truckIds.Count == 0
            ? []
            : await tenantUow.Repository<Load>()
                .Query()
                .Where(l => l.AssignedTruckId != null && truckIds.Contains(l.AssignedTruckId.Value))
                .Select(l => l.Id)
                .ToHashSetAsync(ct);

        return documents.Where(d => d switch
        {
            EmployeeDocument ed => ed.EmployeeId == caller.CallerId,
            TruckDocument td => truckIds.Contains(td.TruckId),
            LoadDocument ld => loadIds.Contains(ld.LoadId),
            _ => false
        }).ToList();
    }

    private async Task<bool> DrivesTruckAsync(Guid driverId, Guid truckId, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckId, ct);
        return truck?.IsDrivenBy(driverId) == true;
    }

    private Task<bool> DrivesLoadAsync(Guid driverId, Guid loadId, CancellationToken ct)
    {
        return tenantUow.Repository<Load>()
            .Query()
            .AnyAsync(l => l.Id == loadId && l.AssignedTruck != null &&
                           (l.AssignedTruck.MainDriverId == driverId ||
                            l.AssignedTruck.SecondaryDriverId == driverId), ct);
    }
}
