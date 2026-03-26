using Logistics.Application.Services;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class DispatchToolExecutor : IDispatchToolExecutor
{
    public Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default)
    {
        // TODO: Implement tool dispatch via MediatR in Phase 2
        throw new NotImplementedException();
    }
}
