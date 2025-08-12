using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateDocumentHandler : RequestHandler<UpdateDocumentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateDocumentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateDocumentCommand req, CancellationToken ct)
    {
        var document = await _tenantUow.Repository<Document>().GetByIdAsync(req.DocumentId, ct);
        if (document is null)
            return Result.Fail($"Could not find document with ID '{req.DocumentId}'");

        if (document.Status == DocumentStatus.Deleted)
            return Result.Fail("Cannot update deleted document");

        var updater = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UpdatedById, ct);
        if (updater is null)
            return Result.Fail($"Could not find employee with ID '{req.UpdatedById}'");

        if (req.Type.HasValue)
        {
            document.Type = req.Type.Value;
            document.LastModifiedAt = DateTime.UtcNow;
        }

        if (req.Description != null) document.UpdateDescription(req.Description);

        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
