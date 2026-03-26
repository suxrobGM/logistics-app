using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDispatchSessionByIdHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetDispatchSessionByIdQuery, Result<DispatchSessionDto>>
{
    public async Task<Result<DispatchSessionDto>> Handle(
        GetDispatchSessionByIdQuery request, CancellationToken ct)
    {
        var session = await tenantUow.Repository<DispatchSession>()
            .GetByIdAsync(request.SessionId);

        if (session is null)
            return Result<DispatchSessionDto>.Fail("Session not found");

        return Result<DispatchSessionDto>.Ok(session.ToDtoWithDecisions());
    }
}
