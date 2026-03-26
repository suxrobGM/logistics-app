using Logistics.Application.Services;
using Logistics.Domain.Entities;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class ClaudeDispatchAgentService(HttpClient httpClient) : IDispatchAgentService
{
    public Task<DispatchSession> RunAsync(DispatchAgentRequest request, CancellationToken ct = default)
    {
        // TODO: Implement agent loop in Phase 2
        throw new NotImplementedException();
    }

    public Task CancelAsync(Guid sessionId, CancellationToken ct = default)
    {
        // TODO: Implement cancellation in Phase 2
        throw new NotImplementedException();
    }
}
