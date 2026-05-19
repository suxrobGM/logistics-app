using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

internal sealed class CancelDataDeletionHandler(
    IMasterUnitOfWork masterUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<CancelDataDeletionCommand, Result>
{
    public async Task<Result> Handle(CancelDataDeletionCommand req, CancellationToken ct)
    {
        var userId = currentUserService.GetUserId();
        if (userId is null)
        {
            return Result.Fail("User not authenticated.");
        }

        var request = await masterUow.Repository<DataDeletionRequest>().GetByIdAsync(req.Id, ct);
        if (request is null || request.UserId != userId.Value)
        {
            return Result.Fail("Deletion request not found.");
        }

        if (request.Status != DataDeletionStatus.Pending)
        {
            return Result.Fail($"Cannot cancel a deletion request in '{request.Status}' state.");
        }

        var now = DateTime.UtcNow;
        if (request.ScheduledFor <= now)
        {
            return Result.Fail("The grace period has elapsed; this deletion can no longer be cancelled.");
        }

        request.Status = DataDeletionStatus.Cancelled;
        request.CancelledAt = now;

        var user = await masterUow.Repository<User>().GetByIdAsync(userId.Value, ct);
        user?.DeletionRequestedAt = null;

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
