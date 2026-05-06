---
name: add-dispatch-tool
description: Add a new tool to the AI dispatch agent. Use when the user wants to give the agent a new capability (e.g., "add a tool that returns load board listings older than 24h"). Walks through the four files that must change and the silently load-bearing WriteTools step.
---

# Add a Dispatch Tool

Adds a new tool that the AI dispatch agent can call. Tools are auto-discovered via DI: each tool is a class implementing `IDispatchTool`, registered in `Registrar.cs`, and described in `DispatchToolRegistry.cs` for the LLM.

## Files that must change

1. **`src/Infrastructure/Logistics.Infrastructure.AI/Tools/{ToolName}Tool.cs`** — the tool implementation
2. **`src/Infrastructure/Logistics.Infrastructure.AI/Services/DispatchToolRegistry.cs`** — JSON schema + description for the LLM
3. **`src/Infrastructure/Logistics.Infrastructure.AI/Registrar.cs`** — DI registration
4. **`src/Infrastructure/Logistics.Infrastructure.AI/Services/DispatchDecisionProcessor.cs`** — only if write tool, add name to `WriteTools` HashSet
5. **`tests/Logistics.Infrastructure.AI.Tests/Tools/{ToolName}ToolTests.cs`** — unit test

## Step-by-step

### 1. Decide read vs write

- **Read tool**: pure query — runs immediately in both Autonomous and HumanInTheLoop modes. Examples: `get_unassigned_loads`, `check_hos_feasibility`.
- **Write tool**: mutates state (assigns load, dispatches trip, books from load board). In HumanInTheLoop mode it creates a `Suggested` decision; in Autonomous mode it executes immediately.

If write tool, you **must** add the tool name to `WriteTools` HashSet in step 4 — missing this step breaks HumanInTheLoop approvals silently.

### 2. Create the tool class

`Tools/{ToolName}Tool.cs`. Tool names use `snake_case`. Pattern:

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetSomethingTool(ITenantUnitOfWork tenantUow) : IDispatchTool
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

        // 3. Return JSON string. Keep payloads compact — every byte costs LLM tokens.
        return JsonSerializer.Serialize(new { /* fields */ });
    }
}
```

Conventions:

- `internal sealed class` with primary-constructor DI
- Tool name in `snake_case`, matching `Name` property
- Always return JSON string, never throw — surface errors as `{ error = "..." }`
- Inject the smallest dependency you need (`ITenantUnitOfWork`, `IMediator`, `IGeocodingService`, etc.) — tools are `Scoped`
- For write tools that map to existing commands, dispatch via `IMediator.Send(new XCommand(...), ct)`

### 3. Add the schema definition

In `DispatchToolRegistry.cs`, append to the `Tools` list. The JSON schema is what the LLM sees — descriptions matter:

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

In `Registrar.cs`, add to the `AddDispatchAgentInfrastructure` method alongside the other tools:

```csharp
services.AddScoped<IDispatchTool, GetSomethingTool>();
```

### 5. If write tool, register in WriteTools

In `DispatchDecisionProcessor.cs`, add the tool name to the `WriteTools` HashSet:

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

**Skip this step for read tools.** Read tools always execute immediately.

### 6. Write a unit test

`tests/Logistics.Infrastructure.AI.Tests/Tools/{ToolName}ToolTests.cs`. Use NSubstitute and `MockQueryable.NSubstitute` for `IQueryable`-returning repositories. Pattern:

```csharp
public class GetSomethingToolTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly GetSomethingTool sut;

    public GetSomethingToolTests() => sut = new GetSomethingTool(tenantUow);

    [Fact]
    public async Task ExecuteAsync_MissingId_ReturnsError()
    {
        var input = JsonNode.Parse("""{}""")!;
        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        result.Should().Contain("\"error\"");
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsExpectedFields() { /* ... */ }
}
```

## Verification checklist

Before reporting done:

- [ ] Tool class created, implements `IDispatchTool`, name is `snake_case`
- [ ] Registered in `Registrar.cs` (otherwise DI won't find it and `DispatchToolExecutor` returns "Unknown tool")
- [ ] Added to `DispatchToolRegistry.Tools` list (otherwise the LLM never knows it exists)
- [ ] **If write tool**: added to `DispatchDecisionProcessor.WriteTools` HashSet
- [ ] Unit test added under `tests/Logistics.Infrastructure.AI.Tests/Tools/`
- [ ] `dotnet build` passes
- [ ] `dotnet test --filter "{ToolName}ToolTests"` passes

## Common mistakes

- **Missing `WriteTools` registration**: Tool runs in Autonomous mode but is silently auto-executed in HumanInTheLoop instead of creating a `Suggested` decision.
- **Throwing instead of returning `{error}`**: The agent loop catches exceptions but the agent loses the context of what went wrong.
- **Verbose tool names or descriptions**: Every tool definition is sent on every API call — keep descriptions tight.
- **Not registering in `Registrar.cs`**: `DispatchToolExecutor.toolMap` is built from DI; an unregistered tool is invisible at runtime.

## Related

- `.claude/rules/backend/ai-agent.md` — overall AI agent conventions
- `docs/ai-dispatch.md` — agent architecture
- `feature-map.md` → AI dispatch row
