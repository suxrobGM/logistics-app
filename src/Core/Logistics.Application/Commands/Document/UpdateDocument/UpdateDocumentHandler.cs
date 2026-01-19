using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateDocumentHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateDocumentCommand, Result>
{
    public async Task<Result> Handle(
        UpdateDocumentCommand req, CancellationToken ct)
    {
        var document = await tenantUow.Repository<Document>().GetByIdAsync(req.DocumentId, ct);
        if (document is null)
        {
            return Result.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        if (document.Status == DocumentStatus.Deleted)
        {
            return Result.Fail("Cannot update deleted document");
        }

        var updater = await tenantUow.Repository<Employee>().GetByIdAsync(req.UpdatedById, ct);
        if (updater is null)
        {
            return Result.Fail($"Could not find employee with ID '{req.UpdatedById}'");
        }

        if (req.Type.HasValue)
        {
            document.Type = req.Type.Value;
            document.UpdatedAt = DateTime.UtcNow;
        }

        if (req.Description != null)
        {
            document.UpdateDescription(req.Description);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
