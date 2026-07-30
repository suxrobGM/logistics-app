---
paths:
  - "src/Infrastructure/Logistics.Infrastructure.AI/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Integrations/AIDispatch/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Platform/AISettings/**/*.cs"
  - "src/Presentation/Logistics.McpServer/**/*.cs"
---

# AI Dispatch Agent Conventions

Conventions and traps only. For step-by-step recipes use the skills - `add-dispatch-tool` (new tool)
and `add-llm-provider` (new model or provider). For how the system works end to end, read
[docs/ai-dispatch.md](../../../docs/ai-dispatch.md).

## Where things live

`src/Infrastructure/Logistics.Infrastructure.AI/` is grouped by capability, not by layer. There is
no `Services/`, `Models/` or `Prompts/` folder - if you are about to create one, you want one of
these instead:

| Folder                                                        | Holds                                                                                                                           |
| ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| `Llm/`                                                        | Provider-agnostic LLM layer: `ILlmProvider`, the factory, the one-shot `LlmClient`, model resolution, pricing, error sanitising |
| `Llm/Contracts/`                                              | The `Llm*` wire records the rest of the project sees                                                                            |
| `Llm/Providers/`                                              | The only files allowed to touch a vendor SDK                                                                                    |
| `Agents/`                                                     | The runtime both agents share: loop runner, decision processor, session cancellation, tool-call context, prompt sanitisers      |
| `Agents/Dispatch/`, `Agents/Copilot/`                         | One folder per agent surface                                                                                                    |
| `Tools/`                                                      | Tool contract, the `ToolInput`/`ToolResult` kit, `DispatchUnits`, and the catalogue                                             |
| `Tools/{Dispatch,Financial,Operations,LoadBoard,Intermodal}/` | Tools grouped by the Application module they dispatch into                                                                      |

Three placement rules that get broken:

- **Config is not here.** `LlmOptions` / `LlmProviderOptions` live in `Application.Abstractions/AI/`,
  because the application layer reads them. So do `LlmModelCatalog` and `AISettingsKeys`; the agent
  ports (`IAgentToolRegistry`, `IAgentToolExecutor`, `IAgentRunContext`, `AgentToolDefinition`) live
  in `Application.Abstractions/Agents/`.
- **An agent's prompt lives beside the agent that owns it**, under `Agents/Dispatch/` or
  `Agents/Copilot/`, and the `*SystemPrompt.cs` suffix is what keeps them findable. A one-shot
  `ILlmClient` feature is not agent code - its workflow service and prompt live together, either in
  `Logistics.Application/Modules/{Module}/.../Services/` (policy learning) or in the infrastructure
  project that owns the feature (`Infrastructure.Documents/PdfImport/DispatchSheetPrompt`).
- **Shared machinery is named `Agent*`, not `AIDispatch*`.** The registry, executor, tool contract,
  decision processor and cancellation registry serve the dispatch agent, the copilot and MCP alike.
  The `AIDispatch*` prefix that remains is the domain (`AIDispatchSession`, `AIDispatchDecision`,
  `AIDispatchMode`, `AIDispatchPolicy`) and the dispatch-only service.

`AIDispatchSystemPrompt.Build` also varies by `TenantSettings.OperatingMode`: `SoloOperator` swaps the
fleet-framed lines (utilization, truck-to-truck comparison, the assignment table) and appends a
`## Fleet Profile` block. That heading is deliberately not `## Operating Mode` - that one is already
taken by `AIDispatchMode`, and reusing it makes the model conflate the two.

## Provider abstraction

`AnthropicLlmProvider` (Claude via `Anthropic.SDK` - prompt caching, extended thinking) and
`OpenAILlmProvider` (any OpenAI-compatible endpoint via configurable `BaseUrl`) both sit behind
`ILlmProvider`, resolved by `LlmProviderFactory`.

**Provider SDK types must not leak past the provider class.** The agent loop, tools, and decision
processor see the `Llm/Contracts/` records (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`) only.

## Tools

`internal sealed`, implementing `IAgentTool`, **snake_case** `Name`, JSON Schema that works for
both Claude and OpenAI function calling. Class name mirrors the tool name (`get_driver_hos_status`
→ `GetDriverHosStatusTool`) so a transcript leads straight to the file. Schemas go in
`AgentToolRegistry.Tools`, which is shared with the MCP server.

**DI needs no edit** - `Registrar` scans the assembly for `IAgentTool`, so a new tool registers by
existing. The catalogue entry is the one thing you must not forget, and
`AgentToolRegistryParityTests` fails if class and catalogue disagree in either direction.

- Read tools are pure queries and always execute immediately.
- Write tools mutate state: `HumanInTheLoop` turns them into `Suggested` decisions, `Autonomous`
  executes them.
- **Behavior metadata lives on the registry definition**, as named init properties:
  `RequiredPermission`, `DecisionType`, `RequiredFeature`, `DispatchAgent`. There is no separate
  write-tool list. `IsWrite` is **derived** - a tool is a write exactly when its `DecisionType` is
  not `Query`, so the two cannot drift and there is no separate flag to forget. Miss
  `RequiredPermission` and the tool leaks into every copilot conversation regardless of the
  caller's role.
- The registry has one method per surface: `GetDispatchAgentTools` (keeps only tools declaring
  `DispatchAgent = true`), `GetCopilotTools` (requires the caller's permission set - see
  `docs/ai-copilot.md`), and `GetAllTools` for the MCP server, which gates per call instead.
  Permission scoping is not optional on the copilot path; it is a parameter, not a nullable flag.
- **`DispatchAgent` defaults to false, and that default is the safe one.** The dispatch agent can
  run `Autonomous`, so a tool it should not have executes with no dispatcher approval. Opting a
  tool in is deliberate. Do **not** re-derive this from `RequiredPermission` - a dispatch tool is
  free to require `Permission.Trip.Manage`, and coupling the two silently hides it.

The agent loop (`AgentLoopRunner`, shared by dispatch and copilot) caps at **25 iterations per session**.

## Model selection and cost

The dispatch model is **global**, set by an admin. Tenants never pick a model and never see model
names; plans differ by **quota only** - there is no per-plan model tier.

- `LlmModelCatalog` (`Application.Abstractions/AI`) is the single source of selectable models.
- `LlmPricing` is the single source of pricing, cost multipliers, and overage units, keyed by the same
  ids. **Read the current numbers there** - do not copy them into docs, they change every model refresh.
- Tier is declared once per model, via the `Base`/`Premium`/`Ultra` factory used in the `Pricing`
  dictionary; `GetMultiplier` and `GetOverageBillingUnits` both read it, so they cannot disagree.
  The one thing to preserve: an **unknown** model falls back to base tier for quota and overage but
  to Sonnet rates for cost. That asymmetry is deliberate - see the comment on `DefaultPricing`.
- Quota counting is multiplier-based, not flat session counts: `AIDispatchSession.RequestCost` comes
  from `GetMultiplier()`, `AIQuotaService` sums it for the week, and the tenant-facing API returns a
  percentage only.

Resolution order in `AIDispatchConversationBuilder`: `SystemSettings["AI.Model"]` (admin-set; provider
is derived from the catalog, never stored) → `LlmOptions.DefaultProvider` + `Model` from appsettings.
Extended thinking follows the same shape via `SystemSettings["AI.ExtendedThinking"]`.

**Per-request override is for one-shot calls only.** `LlmCompletionRequest.ModelId` lets an
`ILlmClient` call pin a cheap model (the nightly policy learning pass does this). `LlmModelResolver`
honors it only when the id is in the catalog **and** that provider has an API key configured. That
API-key check is load-bearing: without it, an install that configured a single provider fails every
night. The agent loop has no override and must not get one.

## Learned dispatch policy

`AIDispatchPolicy` (one row per tenant) holds a short markdown policy that the nightly
`AIDispatchPolicyLearningJob` derives from approve/reject history. It is injected into the system
prompt between `## HOS Rules` and `## Workflow` as _strong defaults ranking below the hard constraints_.

Traps, all of which look fine in review:

- The human-approved population is `Status == Executed && ApprovedByUserId != null`. `Approve()` is
  immediately overwritten by `MarkExecuted()`, so **filtering on `Approved` finds nothing** - and
  autonomous executions carry no human signal at all.
- `GeneratedContent` is job-owned, `ManualContent` is dispatcher-owned. Never merge them.
- Policy text is **untrusted** - it is LLM output derived from dispatcher-typed rejection reasons.
  `AIDispatchSystemPrompt` sanitises and truncates it. Keep that at the injection point, not in callers.
- Truncation keeps **whole lines only**. A half-truncated rule reads as a different rule.
- Learning creates no `AIDispatchSession` row, so it never consumes tenant quota. Cost lands on
  `AIDispatchPolicy.GenerationCostUsd` and is never exposed to the tenant.
