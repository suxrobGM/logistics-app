using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CancelDispatchSessionHandler(
    IDispatchAgentService agentService) : IAppRequestHandler<CancelDispatchSessionCommand, Result>
{
    public async Task<Result> Handle(CancelDispatchSessionCommand request, CancellationToken ct)
    {
        await agentService.CancelAsync(request.SessionId, ct);
        return Result.Ok();
    }
}
