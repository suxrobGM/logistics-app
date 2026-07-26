---
name: add-dispatch-tool
description: Add a new tool to the AI dispatch agent. Use when the user wants to give the agent a new capability (e.g., "add a tool that returns load board listings older than 24h"). Walks through the four files that must change and the silently load-bearing WriteTools step.
---

# Add a Dispatch Tool

Tools are auto-discovered via DI, and `AIDispatchToolRegistry` is shared with the MCP server - add a
tool once and both surfaces expose it.

## Files that must change

1. **`src/Infrastructure/Logistics.Infrastructure.AI/Tools/{ToolName}Tool.cs`** - the tool implementation
2. **`src/Infrastructure/Logistics.Infrastructure.AI/Services/AIDispatchToolRegistry.cs`** - JSON schema + description for the LLM
3. **`src/Infrastructure/Logistics.Infrastructure.AI/Registrar.cs`** - DI registration
4. **`src/Infrastructure/Logistics.Infrastructure.AI/Services/AIDispatchDecisionProcessor.cs`** - write tools only
5. **`test/Logistics.Infrastructure.AI.Tests/Tools/{ToolName}ToolTests.cs`** - unit test

## Step-by-step

### 1. Decide read vs write

- **Read tool**: pure query - runs immediately in both Autonomous and HumanInTheLoop modes. Examples: `get_unassigned_loads`, `check_hos_feasibility`.
- **Write tool**: mutates state (assigns load, dispatches trip, books from load board). In HumanInTheLoop mode it creates a `Suggested` decision; in Autonomous mode it executes immediately.

A write tool **must** be added to the `WriteTools` HashSet (step 5). Miss it and the tool
auto-executes instead of creating a `Suggested` decision - HumanInTheLoop approvals break silently.

### 2. Create the tool class

`Tools/{ToolName}Tool.cs`. Tool names use `snake_case`. Pattern:

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetSomethingTool(ITenantUnitOfWork tenantUow) : IAIDispatchTool
{
    public string Name => "get_something";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        // 1. Parse and validate input
        var someId = input["some_id"]?.GetValue<string>();
        if (string.IsNullOrEmpty(someId))
            return JsonSerializer.Serialize(new { error = "Missing some_id" });

        // 2. Do the work via tenantUow / mediator / domain services
        // Inject IMediator if you need to dispatch a command/query

        // 3. Return JSON string. Keep payloads compact - every byte costs LLM tokens.
        return JsonSerializer.Serialize(new { /* fields */ });
    }
}
```

Conventions:

- `internal sealed class` with primary-constructor DI
- Tool name in `snake_case`, matching `Name` property
- Always return JSON string, never throw - surface errors as `{ error = "..." }`
- Inject the smallest dependency you need (`ITenantUnitOfWork`, `IMediator`, `IGeocodingService`, etc.) - tools are `Scoped`
- For write tools that map to existing commands, dispatch via `IMediator.Send(new XCommand(...), ct)`

### 3. Add the schema definition

In `AIDispatchToolRegistry.cs`, append to the `Tools` list. The JSON schema is what the LLM sees - descriptions matter:

```csharp
new("get_something",
    "Returns X for Y. Include 1-2 sentences describing inputs, outputs, and when to call this vs alternatives.",
    BuildSchema(new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["some_id"] = Prop("string", "GUID of the entity")
        },
        ["required"] = new JsonArray("some_id")
    })),
```

Group with other read tools or other write tools (look at the `── Read Tools ──` / `── Write Tools ──` comments).

### 4. Register in DI

In `Registrar.cs`, add to the `AddAIDispatchInfrastructure` method alongside the other tools:

```csharp
services.AddScoped<IAIDispatchTool, GetSomethingTool>();
```

### 5. If write tool, register in WriteTools

In `AIDispatchDecisionProcessor.cs`, add the tool name to the `WriteTools` HashSet:

```csharp
private static readonly HashSet<string> WriteTools =
[
    "assign_load_to_truck",
    "create_trip",
    "dispatch_trip",
    "book_loadboard_load",
    "get_something" // ← new write tool
];
```

### 6. Write a unit test

`test/Logistics.Infrastructure.AI.Tests/Tools/{ToolName}ToolTests.cs`, using NSubstitute and
`MockQueryable.NSubstitute` for `IQueryable`-returning repositories. Cover at least missing input
(`JsonNode.Parse("""{}""")` → result contains `"error"`) and the happy path.

## Verification checklist

- [ ] Tool class created, implements `IAIDispatchTool`, name is `snake_case`
- [ ] Registered in `Registrar.cs` (otherwise DI won't find it and `AIDispatchToolExecutor` returns "Unknown tool")
- [ ] Added to `AIDispatchToolRegistry.Tools` list (otherwise the LLM never knows it exists)
- [ ] **If write tool**: added to `AIDispatchDecisionProcessor.WriteTools` HashSet
- [ ] Unit test added under `test/Logistics.Infrastructure.AI.Tests/Tools/`
- [ ] `dotnet build` passes
- [ ] `dotnet test --filter "{ToolName}ToolTests"` passes

## Common mistakes

- **Throwing instead of returning `{error}`**: the agent loop catches it, but the agent loses all context on what went wrong.
- **Verbose tool names or descriptions**: every tool definition is sent on every API call - keep descriptions tight.
- **Not registering in `Registrar.cs`**: `AIDispatchToolExecutor.toolMap` is built from DI; an unregistered tool is invisible at runtime.

## Related

- `.claude/rules/backend/ai-agent.md` - overall AI agent conventions
- `docs/ai-dispatch.md` - agent architecture
- `feature-map.md` → AI dispatch row
