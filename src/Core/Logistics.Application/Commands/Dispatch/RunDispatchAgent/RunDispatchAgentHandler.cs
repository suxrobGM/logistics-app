using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class RunDispatchAgentHandler(
    IDispatchAgentService agentService,
    ICurrentUserService currentUser) : IAppRequestHandler<RunDispatchAgentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RunDispatchAgentCommand request, CancellationToken ct)
    {
        var agentRequest = new DispatchAgentRequest(
            TenantId: Guid.Empty, // Resolved by tenant middleware, not needed here
            Mode: request.Mode,
            TriggeredByUserId: currentUser.GetUserId());

        var session = await agentService.RunAsync(agentRequest, ct);
        return Result<Guid>.Ok(session.Id);
    }
}
