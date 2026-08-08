---
name: add-dispatch-tool
description: Add a new tool to the AI agents (dispatch and copilot). Use when the user wants to give an agent a new capability (e.g., "add a tool that returns load board listings older than 24h"). Walks through the three files that must change, the domain folder to pick, and the silently load-bearing registry metadata.
---

# Add an Agent Tool

Tools are auto-discovered via DI, and `AgentToolRegistry` is shared by the dispatch agent, the
TMS copilot, and the MCP server - add a tool once and every surface exposes it (each filtered by
its own feature + permission scope).

## Files that must change

1. **`src/Infrastructure/Logistics.Infrastructure.AI/Tools/{Domain}/{ToolName}Tool.cs`** - the tool implementation
2. **`src/Infrastructure/Logistics.Infrastructure.AI/Tools/AgentToolRegistry.cs`** - JSON schema + description **and the behavior metadata** (`RequiredPermission`, `DecisionType`, `RequiredFeature`, `DispatchAgent`)
3. **`test/Logistics.Infrastructure.AI.Tests/Tools/{Domain}/{ToolName}ToolTests.cs`** - unit test

**`Registrar.cs` is not on that list.** It scans the assembly for `IAgentTool`, so the tool
registers by existing. The catalogue entry in step 2 is the one that has no safety net at authoring
time - though `AgentToolRegistryParityTests` fails the build if a class and the catalogue disagree
in either direction.

Pick `{Domain}` by the Application module the tool dispatches into: `Dispatch/`, `Financial/`
(invoices, payments, tax, expenses), `Operations/` (loads, customers, maintenance), `LoadBoard/`,
`Intermodal/`. Which agent may call it is registry metadata, not folder membership - a `Financial/`
tool can still set `DispatchAgent = true`.

There is no separate write-tool list, and no `IsWrite` flag to set: a tool is a write exactly when
its `DecisionType` is not `Query`. The decision processor and the MCP description warning both read
that derived value.

## Step-by-step

### 1. Decide read vs write

- **Read tool**: pure query - always executes immediately. Examples: `get_unassigned_loads`, `check_hos_feasibility`, `search_loads`.
- **Write tool**: mutates state (assigns load, creates an invoice, books from load board). Always creates a `Suggested` decision awaiting dispatcher/user approval - it never executes inline.

A write tool is declared by giving it a `DecisionType` other than `Query` on its registry
definition (step 3). `IsWrite` is derived from that, so there is no second flag to forget.

### 2. Create the tool class

`Tools/{Domain}/{ToolName}Tool.cs`. Tool names use `snake_case`. Pattern:

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools.Operations;   // match the folder

internal sealed class GetSomethingTool(IMediator mediator) : IAgentTool
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

In `AgentToolRegistry.cs`, append to the `Tools` list. The JSON schema is what the LLM sees - descriptions matter:

```csharp
new("get_something",
    "Returns X for Y. Include 1-2 sentences describing inputs, outputs, and when to call this vs alternatives.",
    new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["some_id"] = Prop("string", "GUID of the entity")
        },
        ["required"] = new JsonArray("some_id")
    })
{
    RequiredFeature = TenantFeature.Loads,                // omit when ungated
    RequiredPermission = Permission.Load.Manage,          // ALWAYS - copilot scoping depends on it
    DecisionType = AgentDecisionType.AssignLoad,     // write tools; makes IsWrite true
    DispatchAgent = true                                  // fleet dispatch agent tools only
},
```

Group with other read tools or other write tools (look at the `── Read Tools ──` / `── Write Tools ──` comments).

- **Every tool declares `RequiredPermission`** - the copilot filters its catalogue by the calling
  user's permissions, and an undeclared permission bypasses that scoping (a registry test enforces this).
- **`DispatchAgent` decides whether the _fleet dispatch agent_ sees the tool**, independently of
  which permission it names. It defaults to false because it scopes the dispatch conversation's
  catalogue to fleet-relevant tools - an unset copilot-only write tool (e.g. `create_load_invoice`)
  simply never reaches a dispatch conversation. Set it only for tools that belong in a fleet dispatch
  run; a copilot-only tool leaves it off. The copilot sees every tool the calling user has the
  permission for either way.
- `DecisionType` both categorizes the tool in the audit trail and _is_ the write declaration;
  append to the `AgentDecisionType` enum when no existing value fits (append-only).
- If the tool's ids should link into the decision audit (`load_id`, `invoice_id`, ...), check
  `AgentDecisionProcessor.ExtractEntityIds` covers the input field name.

### 4. Write a unit test

`test/Logistics.Infrastructure.AI.Tests/Tools/{Domain}/{ToolName}ToolTests.cs`, using NSubstitute and
`MockQueryable.NSubstitute` for `IQueryable`-returning repositories. Cover at least missing input
(`JsonNode.Parse("""{}""")` → result contains `"error"`) and the happy path.

## Verification checklist

- [ ] Tool class created, implements `IAgentTool`, name is `snake_case`
- [ ] Placed in the `Tools/{Domain}/` folder matching the module it dispatches into
- [ ] Added to `AgentToolRegistry.Tools` list (otherwise the LLM never knows it exists)
- [ ] `RequiredPermission` set; **if write tool**: `DecisionType` set to a non-`Query` value
- [ ] `DispatchAgent = true` **only** if the fleet dispatch agent should call it
- [ ] Unit test added under `test/Logistics.Infrastructure.AI.Tests/Tools/{Domain}/`
- [ ] `dotnet build` passes
- [ ] `dotnet test --filter "{ToolName}ToolTests"` passes

## Common mistakes

- **Throwing instead of returning `{error}`**: the agent loop catches it, but the agent loses all context on what went wrong.
- **Verbose tool names or descriptions**: every tool definition is sent on every API call - keep descriptions tight.
- **Forgetting `RequiredPermission`**: a tool without a permission leaks into every copilot conversation regardless of role.
- **Forgetting `DispatchAgent = true` on a dispatch tool**: it is simply absent from the dispatch agent's catalogue, with no error - the agent just can't do the thing.

## Related

- `.claude/rules/backend/ai-agent.md` - overall AI agent conventions
- `docs/ai-dispatch.md` - dispatch agent architecture
- `docs/ai-copilot.md` - copilot architecture
- `feature-map.md` → AI dispatch / AI copilot rows
