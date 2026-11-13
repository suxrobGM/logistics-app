---
name: add-dispatch-tool
description: Add a new tool to the AI agents (dispatch, copilot, MCP). Use when the user wants an agent to gain a capability (e.g. "add a tool that returns load board listings older than 24h"). Covers the one file to add, its folder, and the metadata that silently decides who can call it.
---

# Add an agent tool

A tool is one class. `AgentToolCatalog` discovers it at startup and feeds DI and every surface, so `Registrar.cs` and `AgentToolRegistry.cs` never change. Rules behind this recipe: `.claude/rules/backend/ai-agent.md`.

## 1. Write the tool

`src/Infrastructure/Logistics.Infrastructure.AI/Tools/{Domain}/{Name}Tool.cs`. `{Domain}` is the module it calls: `Dispatch`, `Financial`, `Operations`, `Compliance`, `LoadBoard`, `Intermodal`, `Negotiation`.

```csharp
namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class GetSomethingTool(IMediator mediator)
    : AgentTool<GetSomethingTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("GUID of the load")]
        [AgentEntityId(AgentEntityKind.Load)]
        public required Guid LoadId { get; init; }

        [Description("Page number when a previous call returned truncated: true")]
        public int? Page { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_something",
        "Returns X for Y. One or two sentences: inputs, output, when to use it over alternatives.")
    {
        RequiredPermission = Permission.Load.View,   // always
        RequiredFeature = TenantFeature.Loads,       // when gated
        DecisionType = AgentDecisionType.AssignLoad, // writes only
        Surfaces = AgentSurfaces.All                 // omit for copilot only
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSomethingQuery { LoadId = input.LoadId }, ct);
        return result.IsSuccess ? ToolResult.Ok(result.Value) : ToolResult.Error(result.Error ?? "Unknown error");
    }
}
```

- The input record is the schema: snake_case names, `[Description]`s, `required` members. Never hand-write JSON Schema.
- Return through `ToolResult` (`Ok`, `Error`, `Paged`, `Written`, `Typed`); never throw. Cap lists at `ToolResult.MaxResults` and page the rest. Every byte costs tokens, so keep names, descriptions and payloads short.

## 2. Get the metadata right

None of these fail loudly when wrong.

- **`RequiredPermission`**: without it the tool reaches every copilot user regardless of role.
- **`DecisionType`**: anything but `Query` makes it a write, which becomes a suggestion awaiting approval on agent surfaces. Append to `AgentDecisionType` if nothing fits.
- **`Surfaces`**: defaults to `Copilot`. Add `Dispatch` for fleet dispatch runs (forgetting it means the dispatch agent simply can't see the tool). Add `Mcp` only if an API key may run it with no person and no approval; never for tools that use `TriggeredByUserId`, email third parties or move money.
- **`Destructive = true`** when it can overwrite or undo something the caller didn't name.
- **`[AgentEntityId]`** on entity id inputs, or the decision is never linked to its entity.
- Don't promise approval in the description; the registry adds that per surface.

## 3. Test

`test/Logistics.Infrastructure.AI.Tests/Tools/{Domain}/{Name}ToolTests.cs`. Call `ExecuteAsync(JsonNode, ct)` so binding is covered. Test a missing required input (result contains `"error"`) and the happy path. Naming is already covered by catalogue tests.

`dotnet build`, then `dotnet test --filter-class "*{Name}ToolTests"`.
