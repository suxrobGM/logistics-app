using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class SearchLoadBoardTool : IAIDispatchTool
{
    public string Name => "search_loadboard";

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var originCity = input.GetString("origin_city");
        var originState = input.GetString("origin_state");

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            message = $"Load board search for {originCity}, {originState} - requires active load board integration. Check tenant's load board configuration.",
            loads = Array.Empty<object>()
        }));
    }
}
