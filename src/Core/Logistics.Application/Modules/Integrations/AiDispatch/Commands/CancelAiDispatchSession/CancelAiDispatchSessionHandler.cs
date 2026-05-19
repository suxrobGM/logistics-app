using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

internal sealed class CancelAiDispatchSessionHandler(
    IAiDispatchService agentService) : IAppRequestHandler<CancelAiDispatchSessionCommand, Result>
{
    public async Task<Result> Handle(CancelAiDispatchSessionCommand request, CancellationToken ct)
    {
        var cancelled = await agentService.CancelAsync(request.SessionId, ct);

        return cancelled
            ? Result.Ok()
            : Result.Fail("Session not found or is not currently running");
    }
}
