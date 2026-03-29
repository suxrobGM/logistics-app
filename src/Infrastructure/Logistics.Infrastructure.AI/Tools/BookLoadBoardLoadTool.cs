using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class BookLoadBoardLoadTool : IDispatchTool
{
    public string Name => "book_loadboard_load";

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        return Task.FromResult(JsonSerializer.Serialize(new
        {
            success = false,
            error = "Load board booking requires active load board integration configuration"
        }));
    }
}
