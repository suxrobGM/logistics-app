using Logistics.Application.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadDocumentHandler : RequestHandler<UpdateLoadDocumentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateLoadDocumentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateLoadDocumentCommand req, CancellationToken cancellationToken)
    {
        // Get the document
        var document = await _tenantUow.Repository<LoadDocument>().GetByIdAsync(req.DocumentId);
        if (document is null)
        {
            return Result.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        // Check if document is deleted
        if (document.Status == DocumentStatus.Deleted)
        {
            return Result.Fail("Cannot update deleted document");
        }

        // Verify updater exists
        var updater = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UpdatedById);
        if (updater is null)
        {
            return Result.Fail($"Could not find employee with ID '{req.UpdatedById}'");
        }

        try
        {
            // Update fields if provided
            if (req.Type.HasValue)
            {
                document.Type = req.Type.Value;
                document.UpdatedAt = DateTime.UtcNow;
            }

            if (req.Description != null)
            {
                document.UpdateDescription(req.Description);
            }

            // Save changes
            var changes = await _tenantUow.SaveChangesAsync();

            return changes > 0 
                ? Result.Succeed() 
                : Result.Fail("No changes were made to the document");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update document: {ex.Message}");
        }
    }
}