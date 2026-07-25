using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class CancelAIDispatchSessionHandler(
    IAIDispatchService agentService) : IAppRequestHandler<CancelAIDispatchSessionCommand, Result>
{
    public async Task<Result> Handle(CancelAIDispatchSessionCommand request, CancellationToken ct)
    {
        var cancelled = await agentService.CancelAsync(request.SessionId, ct);

        return cancelled
            ? Result.Ok()
            : Result.Fail("Session not found or is not currently running");
    }
}
