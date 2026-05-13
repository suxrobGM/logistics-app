using Logistics.Application.Abstractions.AiDispatch;
namespace Logistics.Application.Abstractions.AiDispatch;

/// <summary>
/// Executes agent tool calls by dispatching to MediatR commands/queries or domain services.
/// </summary>
public interface IAiDispatchToolExecutor
{
    Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default);
}
