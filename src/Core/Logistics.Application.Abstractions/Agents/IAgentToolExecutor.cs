namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// Executes agent tool calls by dispatching to commands/queries or domain services.
/// </summary>
public interface IAgentToolExecutor
{
    Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default);
}
