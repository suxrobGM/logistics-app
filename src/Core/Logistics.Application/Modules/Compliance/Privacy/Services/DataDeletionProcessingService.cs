using Logistics.Application.Abstractions.Privacy;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Privacy.Services;

internal sealed class DataDeletionProcessingService(
    IMasterUnitOfWork masterUow,
    IDataAnonymizer anonymizer,
    ILogger<DataDeletionProcessingService> logger) : IDataDeletionProcessingService
{
    public async Task ProcessDueAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var due = await masterUow.Repository<DataDeletionRequest>()
            .Query()
            .Where(r => r.Status == DataDeletionStatus.Pending && r.ScheduledFor <= now)
            .OrderBy(r => r.ScheduledFor)
            .ToListAsync(ct);

        foreach (var request in due)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await anonymizer.AnonymizeUserAsync(request.UserId, ct);

                request.Status = DataDeletionStatus.Processed;
                request.ProcessedAt = DateTime.UtcNow;
                await masterUow.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // Leave the request Pending so the next run retries; log loudly so ops can investigate.
                logger.LogError(ex,
                    "Failed to process deletion request '{RequestId}' for user '{UserId}'.",
                    request.Id, request.UserId);
            }
        }
    }
}
