using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class
    GetDocumentsHandler : RequestHandler<GetDocumentsQuery, Result<IEnumerable<DocumentDto>>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetDocumentsHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<IEnumerable<DocumentDto>>> HandleValidated(
        GetDocumentsQuery req, CancellationToken ct)
    {
        // Verify the owner exists
        switch (req.OwnerType)
        {
            case DocumentOwnerType.Load when req.OwnerId is not null:
                if (await _tenantUow.Repository<Load>().GetByIdAsync(req.OwnerId.Value, ct) is null)
                {
                    return Result<IEnumerable<DocumentDto>>.Fail($"Could not find load with ID '{req.OwnerId}'");
                }

                break;

            case DocumentOwnerType.Employee when req.OwnerId is not null:
                if (await _tenantUow.Repository<Employee>().GetByIdAsync(req.OwnerId.Value, ct) is null)
                {
                    return Result<IEnumerable<DocumentDto>>.Fail($"Could not find employee with ID '{req.OwnerId}'");
                }

                break;
        }


        // Fetch and filter by owner + optional status/type
        List<DocumentDto> dtos;
        if (req is { OwnerType: DocumentOwnerType.Load, OwnerId: not null })
        {
            var docs = await _tenantUow.Repository<LoadDocument>()
                .GetListAsync(d =>
                    d.LoadId == req.OwnerId &&
                    (!req.Status.HasValue || d.Status == req.Status) &&
                    (!req.Type.HasValue || d.Type == req.Type), ct);

            dtos = docs.Select(d => d.ToDto()).ToList();
        }
        else if (req is { OwnerType: DocumentOwnerType.Employee, OwnerId: not null })
        {
            var docs = await _tenantUow.Repository<EmployeeDocument>()
                .GetListAsync(d =>
                    d.EmployeeId == req.OwnerId &&
                    (!req.Status.HasValue || d.Status == req.Status) &&
                    (!req.Type.HasValue || d.Type == req.Type), ct);

            dtos = docs.Select(d => d.ToDto()).ToList();
        }
        else
        {
            var docs = await _tenantUow.Repository<Document>()
                .GetListAsync(d =>
                    (!req.Status.HasValue || d.Status == req.Status) &&
                    (!req.Type.HasValue || d.Type == req.Type), ct);

            dtos = docs.Select(d => d.ToDto()).ToList();
        }

        return Result<IEnumerable<DocumentDto>>.Succeed(dtos);
    }
}
