using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Documents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Documents.Commands;

internal sealed class UpdateDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IDocumentAccessService documentAccess)
    : IRequestHandler<UpdateDocumentCommand, Result>
{
    public async Task<Result> Handle(
        UpdateDocumentCommand req, CancellationToken ct)
    {
        var document = await tenantUow.Repository<Document>().GetByIdAsync(req.DocumentId, ct);
        if (document is null)
        {
            return Result.Fail($"Could not find document with ID '{req.DocumentId}'", ErrorCodes.NotFound);
        }

        var caller = await documentAccess.ResolveCallerAsync(ct);
        if (caller is null || !await documentAccess.CanAccessAsync(caller, document, ct))
        {
            return Result.Fail("Document not found or access denied.", ErrorCodes.NotFound);
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
