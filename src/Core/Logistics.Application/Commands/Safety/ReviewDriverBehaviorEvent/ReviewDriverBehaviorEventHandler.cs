using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ReviewDriverBehaviorEventHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser)
    : IAppRequestHandler<ReviewDriverBehaviorEventCommand, Result<DriverBehaviorEventDto>>
{
    public async Task<Result<DriverBehaviorEventDto>> Handle(ReviewDriverBehaviorEventCommand req, CancellationToken ct)
    {
        var behaviorEvent = await tenantUow.Repository<DriverBehaviorEvent>().GetByIdAsync(req.Id, ct);

        if (behaviorEvent is null)
        {
            return Result<DriverBehaviorEventDto>.Fail($"Could not find driver behavior event with ID '{req.Id}'");
        }

        if (behaviorEvent.IsReviewed)
        {
            return Result<DriverBehaviorEventDto>.Fail("This event has already been reviewed.");
        }

        behaviorEvent.IsReviewed = true;
        behaviorEvent.ReviewedById = currentUser.GetUserId();
        behaviorEvent.ReviewedAt = DateTime.UtcNow;
        behaviorEvent.ReviewNotes = req.ReviewNotes;
        behaviorEvent.IsDismissed = req.IsDismissed;

        tenantUow.Repository<DriverBehaviorEvent>().Update(behaviorEvent);
        await tenantUow.SaveChangesAsync(ct);

        return Result<DriverBehaviorEventDto>.Ok(behaviorEvent.ToDto());
    }
}
