using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

internal sealed class GetDocumentsHandler(
    ITenantUnitOfWork tenantUow,
    IDocumentAccessService documentAccess)
    : IAppRequestHandler<GetDocumentsQuery, Result<IEnumerable<DocumentDto>>>
{
    public async Task<Result<IEnumerable<DocumentDto>>> Handle(
        GetDocumentsQuery req, CancellationToken ct)
    {
        var caller = await documentAccess.ResolveCallerAsync(ct);
        if (caller is null)
        {
            return Result<IEnumerable<DocumentDto>>.Ok([]);
        }

        if (req.OwnerId.HasValue != req.OwnerType.HasValue)
        {
            return Result<IEnumerable<DocumentDto>>.Ok([]);
        }

        var owner = req is { OwnerId: { } ownerId, OwnerType: { } ownerType }
            ? (Id: ownerId, Type: ownerType)
            : ((Guid Id, DocumentOwnerType Type)?)null;

        if (owner is { } scope &&
            !await documentAccess.CanAccessOwnerAsync(caller, scope.Type, scope.Id, ct))
        {
            return Result<IEnumerable<DocumentDto>>.Ok([]);
        }

        var documents = await FetchAsync(req, ct);
        var allowed = owner is not null
            ? documents
            : await documentAccess.FilterAccessibleAsync(caller, documents, ct);

        return Result<IEnumerable<DocumentDto>>.Ok(allowed.Select(d => d.ToDto()).ToList());
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
