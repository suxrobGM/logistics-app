using Logistics.Application.Services;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class DispatchToolRegistry : IDispatchToolRegistry
{
    public IReadOnlyList<DispatchToolDefinition> GetToolDefinitions()
    {
        // TODO: Register all dispatch tools in Phase 2
        return [];
    }
}
