---
name: add-dispatch-tool
description: Add a new tool to the AI agents (dispatch and copilot). Use when the user wants to give an agent a new capability (e.g., "add a tool that returns load board listings older than 24h"). Walks through the one file that must change, the domain folder to pick, and the silently load-bearing tool metadata.
---

# Add an Agent Tool

A tool is one class. It declares its catalogue entry, its input type and its behaviour together, and
`AgentToolCatalog` discovers it at startup. The catalogue is shared by the dispatch agent, the TMS
copilot and the MCP server, each filtered by its own feature and permission scope.

## Files that must change

1. **`src/Infrastructure/Logistics.Infrastructure.AI/Tools/{Domain}/{ToolName}Tool.cs`** - the tool
2. **`test/Logistics.Infrastructure.AI.Tests/Tools/{Domain}/{ToolName}ToolTests.cs`** - unit test

**`Registrar.cs` and `AgentToolRegistry.cs` are not on that list.** DI registration and the
catalogue are both driven by the assembly scan, so a tool joins every surface by existing.

Pick `{Domain}` by the Application module the tool dispatches into: `Dispatch/`, `Financial/`
(invoices, payments, tax, expenses), `Operations/` (loads, customers, maintenance), `LoadBoard/`,
`Intermodal/`. Which surfaces may call it is metadata, not folder membership - a `Financial/` tool can
still name `AgentSurfaces.Dispatch`.

There is no separate write-tool list and no `IsWrite` flag: a tool is a write exactly when its
`DecisionType` is not `Query`.

## Step-by-step

### 1. Decide read vs write

- **Read tool**: pure query - always executes immediately. Examples: `get_unassigned_loads`, `check_hos_feasibility`, `search_loads`.
- **Write tool**: mutates state (assigns a load, creates an invoice, books from a load board). On the agent surfaces it always creates a `Suggested` decision awaiting approval, never executing inline. Over MCP it runs immediately, and the surface says so in the description it publishes.

Declare it by giving it a `DecisionType` other than `Query`. `IsWrite` derives from that.

### 2. Write the tool

`Tools/{Domain}/{ToolName}Tool.cs`. Tool names are `snake_case`, and the class name mirrors the tool
name (a test enforces both) so a transcript leads to the file.

```csharp
using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Operations;   // match the folder

internal sealed class GetSomethingTool(IMediator mediator)
    : AgentTool<GetSomethingTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("GUID of the entity")]
        public required Guid SomeId { get; init; }

        [Description("Page number when a previous call returned truncated: true")]
        public int? Page { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_something",
        "Returns X for Y. Include 1-2 sentences describing inputs, outputs, and when to call this vs alternatives.")
    {
        RequiredFeature = TenantFeature.Loads,          // omit when ungated
        RequiredPermission = Permission.Load.View,      // ALWAYS - copilot scoping depends on it
        DecisionType = AgentDecisionType.AssignLoad,    // write tools only; makes IsWrite true
        Surfaces = AgentSurfaces.All                    // omit to stay on the copilot alone
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        // Arguments arrive bound and validated. Do the work, then return JSON.
        // Keep payloads compact - every byte costs tokens. Cap lists at ToolResult.MaxResults and
        // return ToolResult.Paged instead of everything.
        return ToolResult.Ok(new { /* fields */ });
    }
}
```

**The input type is the schema.** `AgentToolJson` exports it: snake_case names, `[Description]` as
the description, `required` members as the required list, enums as a value list, `DateTime` as a
date-time string. Never hand-write JSON Schema. A `required` property that the model omits fails the
call before your code runs; an optional one is nullable and yours to check.

Other conventions:

- `internal sealed class` with primary-constructor DI; tools are `Scoped`
- Return JSON through `ToolResult` (`Ok`, `Error`, `Paged`, `Written`, `Typed`) and never throw - the loop catches exceptions, but the agent loses all context on what went wrong
- Inject the smallest dependency you need (`IMediator`, `ITenantUnitOfWork`, a domain service)
- For tools that map to existing commands/queries, dispatch via `IMediator.Send(new XCommand(...), ct)`

### 3. Check the metadata

- **Every tool declares `RequiredPermission`** - the copilot filters its catalogue by the calling
  user's permissions, and an undeclared permission bypasses that scoping (a test enforces this).
- **`Surfaces` decides which catalogues publish the tool**, independently of which permission it
  names. It defaults to `Copilot`, so a tool you say nothing about reaches only a named user with the
  right permission. Add `Dispatch` for fleet dispatch runs. Add `Mcp` only if an API key may run it
  with nobody to attribute it to and no approval step - `AgentSurfaces.All` is both.
- `DecisionType` both categorizes the tool in the audit trail and _is_ the write declaration; append
  to the `AgentDecisionType` enum when no existing value fits (append-only).
- **Leave `Mcp` off** when the tool attributes its work to `IAgentRunContext.TriggeredByUserId`, or
  when it emails a third party or moves money. An API key names a tenant, not a person, and there is
  no dispatcher to approve the call.
- **`Destructive = true`** when the tool can overwrite or undo something the caller did not name.
  MCP clients read it to decide whether a call may be auto-approved.
- **Do not promise approval in the description.** The registry appends that sentence for the agent
  surfaces, because it is not true of MCP.
- Mark an entity id input with `[AgentEntityId(AgentEntityKind.Load)]` and the decision row links to
  it. A catalogue test fails if a known link key is left unmarked.

### 4. Write a unit test

`test/Logistics.Infrastructure.AI.Tests/Tools/{Domain}/{ToolName}ToolTests.cs`, using NSubstitute and
`MockQueryable.NSubstitute` for `IQueryable`-returning repositories. Call the tool the way a surface
does - `ExecuteAsync(JsonNode, ct)` - so the test covers binding too. Cover at least a missing
required input (result contains `"error"`) and the happy path. Do not assert the tool's name: the
catalogue tests already pin naming.

## Verification checklist

- [ ] Tool derives from `AgentTool<TInput>` and implements `IAgentToolMetadata`
- [ ] Name is `snake_case` and the class name mirrors it
- [ ] Placed in the `Tools/{Domain}/` folder matching the module it dispatches into
- [ ] Every input property carries a `[Description]`; required ones use the `required` keyword
- [ ] `RequiredPermission` set; **if write tool**: `DecisionType` set to a non-`Query` value
- [ ] `Surfaces` names `Dispatch` and/or `Mcp` **only** if those surfaces should call it
- [ ] Entity id inputs carry `[AgentEntityId]`
- [ ] Unit test added under `test/Logistics.Infrastructure.AI.Tests/Tools/{Domain}/`
- [ ] `dotnet build` passes
- [ ] `dotnet test --filter "{ToolName}ToolTests"` passes

## Common mistakes

- **Hand-writing a JSON schema**: it is generated from the input type; a second copy can only disagree.
- **Throwing instead of returning `ToolResult.Error`**: the agent loop catches it, but the agent loses all context on what went wrong.
- **Verbose tool names or descriptions**: every definition is sent on every API call - keep descriptions tight.
- **Forgetting `RequiredPermission`**: the tool leaks into every copilot conversation regardless of role.
- **Forgetting `Surfaces` on a dispatch tool**: it is simply absent from the dispatch agent's catalogue, with no error - the agent just can't do the thing.

## Related

- `.claude/rules/backend/ai-agent.md` - overall AI agent conventions
- `docs/ai-dispatch.md` - dispatch agent architecture
- `docs/ai-copilot.md` - copilot architecture
- `docs/mcp-server.md` - the MCP surface
- `feature-map.md` → AI dispatch / AI copilot rows
