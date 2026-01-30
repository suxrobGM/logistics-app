using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDriverBehaviorEventByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetDriverBehaviorEventByIdQuery, Result<DriverBehaviorEventDto>>
{
    public async Task<Result<DriverBehaviorEventDto>> Handle(GetDriverBehaviorEventByIdQuery req, CancellationToken ct)
    {
        var behaviorEvent = await tenantUow.Repository<DriverBehaviorEvent>().GetByIdAsync(req.Id, ct);

        if (behaviorEvent is null)
        {
            return Result<DriverBehaviorEventDto>.Fail($"Could not find driver behavior event with ID '{req.Id}'");
        }

        return Result<DriverBehaviorEventDto>.Ok(behaviorEvent.ToDto());
    }
}
