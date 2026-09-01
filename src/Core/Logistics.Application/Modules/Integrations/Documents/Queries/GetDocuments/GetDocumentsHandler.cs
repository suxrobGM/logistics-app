using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

internal sealed class GetDocumentsHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<GetDocumentsQuery, Result<IEnumerable<DocumentDto>>>
{
    public async Task<Result<IEnumerable<DocumentDto>>> Handle(
        GetDocumentsQuery req, CancellationToken ct)
    {
        if (await OwnerMissingAsync(req, ct) is { } ownerError)
        {
            return Result<IEnumerable<DocumentDto>>.Fail(ownerError);
        }

        var access = await DocumentAccess.ResolveAsync(tenantUow, currentUserService, ct);
        if (access is null)
        {
            return Result<IEnumerable<DocumentDto>>.Ok([]);
        }

        var documents = await FetchAsync(req, ct);
        var allowed = await DocumentAccess.FilterAccessibleAsync(tenantUow, access, documents, ct);

        return Result<IEnumerable<DocumentDto>>.Ok(allowed.Select(d => d.ToDto()).ToList());
    }

    private async Task<string?> OwnerMissingAsync(GetDocumentsQuery req, CancellationToken ct)
    {
        if (req.OwnerId is not { } ownerId)
        {
            return null;
        }

        return req.OwnerType switch
        {
            DocumentOwnerType.Load when await tenantUow.Repository<Load>().GetByIdAsync(ownerId, ct) is null
                => $"Could not find load with ID '{ownerId}'",
            DocumentOwnerType.Employee when await tenantUow.Repository<Employee>().GetByIdAsync(ownerId, ct) is null
                => $"Could not find employee with ID '{ownerId}'",
            DocumentOwnerType.Truck when await tenantUow.Repository<Truck>().GetByIdAsync(ownerId, ct) is null
                => $"Could not find truck with ID '{ownerId}'",
            _ => null
        };
    }

    private async Task<List<Document>> FetchAsync(GetDocumentsQuery req, CancellationToken ct)
    {
        var ownerId = req.OwnerId;
        var status = req.Status;
        var type = req.Type;

        return req switch
        {
            { OwnerType: DocumentOwnerType.Load, OwnerId: not null } =>
            [
                .. await tenantUow.Repository<LoadDocument>().GetListAsync(d =>
                    d.LoadId == ownerId &&
                    (!status.HasValue || d.Status == status) &&
                    (!type.HasValue || d.Type == type), ct)
            ],
            { OwnerType: DocumentOwnerType.Employee, OwnerId: not null } =>
            [
                .. await tenantUow.Repository<EmployeeDocument>().GetListAsync(d =>
                    d.EmployeeId == ownerId &&
                    (!status.HasValue || d.Status == status) &&
                    (!type.HasValue || d.Type == type), ct)
            ],
            { OwnerType: DocumentOwnerType.Truck, OwnerId: not null } =>
            [
                .. await tenantUow.Repository<TruckDocument>().GetListAsync(d =>
                    d.TruckId == ownerId &&
                    (!status.HasValue || d.Status == status) &&
                    (!type.HasValue || d.Type == type), ct)
            ],
            _ =>
            [
                .. await tenantUow.Repository<Document>().GetListAsync(d =>
                    (!status.HasValue || d.Status == status) &&
                    (!type.HasValue || d.Type == type), ct)
            ]
        };
    }
}
