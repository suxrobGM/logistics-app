using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// A single dispatch agent tool that can be executed.
/// Each tool handles one specific operation (e.g., get fleet overview, assign load).
/// </summary>
internal interface IDispatchTool
{
    string Name { get; }
    Task<string> ExecuteAsync(JsonNode input, CancellationToken ct);
}
