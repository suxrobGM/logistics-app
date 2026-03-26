namespace Logistics.Application.Services;

/// <summary>
/// Executes agent tool calls by dispatching to MediatR commands/queries or domain services.
/// </summary>
public interface IDispatchToolExecutor
{
    Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default);
}
