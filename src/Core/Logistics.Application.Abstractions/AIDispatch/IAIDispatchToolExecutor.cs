using Logistics.Application.Abstractions.AIDispatch;
namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Executes agent tool calls by dispatching to MediatR commands/queries or domain services.
/// </summary>
public interface IAIDispatchToolExecutor
{
    Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default);
}
