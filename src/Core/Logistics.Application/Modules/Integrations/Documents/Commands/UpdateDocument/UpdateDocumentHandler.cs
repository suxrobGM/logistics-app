using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class UpdateDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IDocumentAccessService documentAccess)
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

        var caller = await documentAccess.ResolveCallerAsync(ct);
        if (caller is null || !await documentAccess.CanAccessAsync(caller, document, ct))
        {
            return Result.Fail("Document not found or access denied.");
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
