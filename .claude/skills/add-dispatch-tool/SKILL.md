---
name: add-dispatch-tool
description: Add a new tool to the AI agents (dispatch and copilot). Use when the user wants to give an agent a new capability (e.g., "add a tool that returns load board listings older than 24h"). Walks through the four files that must change and the silently load-bearing IsWrite metadata.
---

# Add an Agent Tool

Tools are auto-discovered via DI, and `AIDispatchToolRegistry` is shared by the dispatch agent, the
TMS copilot, and the MCP server - add a tool once and every surface exposes it (each filtered by
its own feature + permission scope).

## Files that must change

1. **`src/Infrastructure/Logistics.Infrastructure.AI/Tools/{ToolName}Tool.cs`** - the tool implementation
2. **`src/Infrastructure/Logistics.Infrastructure.AI/Services/AIDispatchToolRegistry.cs`** - JSON schema + description **and the behavior metadata** (`IsWrite`, `RequiredPermission`, `DecisionType`, `RequiredFeature`)
3. **`src/Infrastructure/Logistics.Infrastructure.AI/Registrar.cs`** - DI registration
4. **`test/Logistics.Infrastructure.AI.Tests/Tools/{ToolName}ToolTests.cs`** - unit test

There is no separate write-tool list anymore - `IsWrite` on the registry definition is the single
registration point, read by the decision processor and the MCP description warning alike.

## Step-by-step

### 1. Decide read vs write

- **Read tool**: pure query - runs immediately in both Autonomous and HumanInTheLoop modes. Examples: `get_unassigned_loads`, `check_hos_feasibility`, `search_loads`.
- **Write tool**: mutates state (assigns load, creates an invoice, books from load board). In HumanInTheLoop mode it creates a `Suggested` decision; in Autonomous mode it executes immediately.

A write tool **must** carry `IsWrite: true` on its registry definition (step 3). Miss it and the
tool auto-executes instead of creating a `Suggested` decision - approvals break silently.

### 2. Create the tool class

`Tools/{ToolName}Tool.cs`. Tool names use `snake_case`. Pattern:

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetSomethingTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "get_something";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        // 1. Parse and validate input - the ToolInput extensions coerce LLM-authored JSON leniently
        if (input.GetGuid("some_id") is not { } someId)
            return JsonSerializer.Serialize(new { error = "Invalid or missing some_id" });

        // 2. Do the work via mediator / tenantUow / domain services

        // 3. Return JSON string. Keep payloads compact - every byte costs LLM tokens.
        //    Cap list results (~20) and set a truncated flag instead of returning everything.
        return JsonSerializer.Serialize(new { /* fields */ });
    }
}
```

Conventions:

- `internal sealed class` with primary-constructor DI
- Tool name in `snake_case`, matching `Name` property
- Always return JSON string, never throw - surface errors as `{ error = "..." }`
- Inject the smallest dependency you need (`IMediator`, `ITenantUnitOfWork`, `IGeocodingService`, etc.) - tools are `Scoped`
- For tools that map to existing commands/queries, dispatch via `IMediator.Send(new XCommand(...), ct)`

### 3. Add the schema definition + metadata

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
    }),
    TenantFeature.Loads,                                  // omit when ungated
    IsWrite: true,                                        // write tools only
    RequiredPermission: Permission.Load.Manage,           // ALWAYS - copilot scoping depends on it
    DecisionType: AIDispatchDecisionType.AssignLoad),     // write tools; add an enum value if new
```

Group with other read tools or other write tools (look at the `── Read Tools ──` / `── Write Tools ──` comments).

- **Every tool declares `RequiredPermission`** - the copilot filters its catalogue by the calling
  user's permissions, and an undeclared permission bypasses that scoping (a registry test enforces this).
- Write tools also declare `DecisionType` so the audit trail categorizes them; append to the
  `AIDispatchDecisionType` enum when no existing value fits (append-only).
- If the tool's ids should link into the decision audit (`load_id`, `invoice_id`, ...), check
  `AIDispatchDecisionProcessor.ExtractEntityIds` covers the input field name.

### 4. Register in DI

In `Registrar.cs`, add alongside the other tools:

```csharp
services.AddScoped<IAIDispatchTool, GetSomethingTool>();
```

### 5. Write a unit test

`test/Logistics.Infrastructure.AI.Tests/Tools/{ToolName}ToolTests.cs`, using NSubstitute and
`MockQueryable.NSubstitute` for `IQueryable`-returning repositories. Cover at least missing input
(`JsonNode.Parse("""{}""")` → result contains `"error"`) and the happy path.

## Verification checklist

- [ ] Tool class created, implements `IAIDispatchTool`, name is `snake_case`
- [ ] Registered in `Registrar.cs` (otherwise DI won't find it and `AIDispatchToolExecutor` returns "Unknown tool")
- [ ] Added to `AIDispatchToolRegistry.Tools` list (otherwise the LLM never knows it exists)
- [ ] `RequiredPermission` set; **if write tool**: `IsWrite: true` + `DecisionType` set
- [ ] Unit test added under `test/Logistics.Infrastructure.AI.Tests/Tools/`
- [ ] `dotnet build` passes
- [ ] `dotnet test --filter "{ToolName}ToolTests"` passes

## Common mistakes

- **Throwing instead of returning `{error}`**: the agent loop catches it, but the agent loses all context on what went wrong.
- **Verbose tool names or descriptions**: every tool definition is sent on every API call - keep descriptions tight.
- **Not registering in `Registrar.cs`**: `AIDispatchToolExecutor.toolMap` is built from DI; an unregistered tool is invisible at runtime.
- **Forgetting `RequiredPermission`**: the dispatch agent's catalogue is scoped to `Permission.Dispatch.*`; a tool without a permission leaks into every copilot conversation regardless of role.

## Related

- `.claude/rules/backend/ai-agent.md` - overall AI agent conventions
- `docs/ai-dispatch.md` - dispatch agent architecture
- `docs/ai-copilot.md` - copilot architecture
- `feature-map.md` → AI dispatch / AI copilot rows
