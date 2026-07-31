---
paths:
  - "src/Infrastructure/Logistics.Infrastructure.AI/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Integrations/AIDispatch/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Platform/AISettings/**/*.cs"
  - "src/Presentation/Logistics.McpServer/**/*.cs"
---

# AI Dispatch Agent Conventions

Traps only. Recipes are in the `add-dispatch-tool` and `add-llm-provider` skills; the end-to-end
picture is [docs/ai-dispatch.md](../../../docs/ai-dispatch.md).

## Where things live

`Logistics.Infrastructure.AI/` is grouped by capability, not layer. There is no `Services/`,
`Models/` or `Prompts/` folder - if you are about to create one, you want one of these:

| Folder                                                        | Holds                                                                                |
| ------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `Llm/`                                                        | `ILlmProvider`, factory, one-shot `LlmClient`, model resolution, pricing, sanitising |
| `Llm/Contracts/`                                              | The `Llm*` wire records the rest of the project sees                                 |
| `Llm/Providers/`                                              | The only files allowed to touch a vendor SDK                                         |
| `Agents/`                                                     | Shared runtime: loop runner, decision processor, cancellation, tool-call context     |
| `Agents/Dispatch/`, `Agents/Copilot/`                         | One folder per agent surface                                                         |
| `Tools/`                                                      | Tool contract, `ToolInput`/`ToolResult`, `DispatchUnits`, the catalogue              |
| `Tools/{Dispatch,Financial,Operations,LoadBoard,Intermodal}/` | Tools grouped by the Application module they dispatch into                           |

- **Config is not here.** `LlmOptions`, `LlmModelCatalog` and `AISettingsKeys` live in
  `Application.Abstractions/AI/` (the application layer reads them); the agent ports
  (`IAgentToolRegistry`, `IAgentToolExecutor`, `IAgentRunContext`, `AgentToolDefinition`) in
  `Application.Abstractions/Agents/`.
- **A prompt lives beside the agent that owns it**, with the `*SystemPrompt.cs` suffix. One-shot
  `ILlmClient` features are not agent code - prompt and workflow service live together, in
  `Logistics.Application/Modules/{Module}/.../Services/` or the infrastructure project owning it.
- **Shared machinery is `Agent*`, not `AIDispatch*`.** Registry, executor, tool contract, decision
  processor and cancellation serve dispatch, copilot and MCP alike. `AIDispatch*` is the domain
  (`AgentSession`, `AgentDecision`, `AgentAutonomyMode`, `AIDispatchPolicy`) and the dispatch service.

`AIDispatchSystemPrompt.Build` varies by `TenantSettings.OperatingMode`: `SoloOperator` swaps the
fleet-framed lines and appends `## Fleet Profile`. That heading is deliberately not `## Operating
Mode` - `AgentAutonomyMode` owns that one, and reusing it makes the model conflate the two.

## Providers

`AnthropicLlmProvider` (prompt caching, adaptive thinking) and `OpenAILlmProvider` (any
OpenAI-compatible endpoint via `BaseUrl`) sit behind `ILlmProvider`, resolved by `LlmProviderFactory`.
**Provider SDK types must not leak past the provider class** - everything else sees `Llm/Contracts/`.

**Anthropic caching.** `BuildMessages` breakpoints the newest message's last block, plus the system
prompt. Both details are load-bearing: **newest** (a breakpoint searches back only 20 blocks, and one
parallel-tool turn can exceed that) and **last** (the prefix caches only up to it). Reads bill at a
tenth of input, so hits lower `EstimatedCostUsd` and the budget with it. `CacheReadTokens` stuck at
zero means an invalidated prefix (tools or model changed) or a prompt under the per-model minimum -
1024 tokens on Sonnet 5, 4096 on Haiku 4.5, silently skipped. OpenAI and DeepSeek cache server-side,
hence provider-level and not in the loop.

## Tools

`internal sealed`, implementing `IAgentTool`, **snake_case** `Name`, JSON Schema valid for both
Claude and OpenAI function calling. Class name mirrors the tool name (`get_driver_hos_status` →
`GetDriverHosStatusTool`) so a transcript leads to the file.

**DI needs no edit** - `Registrar` scans for `IAgentTool`, so a tool registers by existing. The
`AgentToolRegistry.Tools` catalogue entry (shared with the MCP server) is the one thing you must not
forget; `AgentToolRegistryParityTests` fails if class and catalogue disagree either way.

- Read tools always execute immediately. Write tools become `Suggested` decisions under
  `HumanInTheLoop` and execute under `Autonomous`.
- **Behaviour metadata lives on the registry definition** as named init properties:
  `RequiredPermission`, `DecisionType`, `RequiredFeature`, `DispatchAgent`. `IsWrite` is **derived**
  (`DecisionType != Query`), so there is no separate flag to forget. Miss `RequiredPermission` and
  the tool leaks into every copilot conversation regardless of the caller's role.
- One registry method per surface: `GetDispatchAgentTools`, `GetCopilotTools` (permission set is a
  required parameter, not a nullable flag - see `docs/ai-copilot.md`), `GetAllTools` for MCP, which
  gates per call instead.
- **`DispatchAgent` defaults to false, and that is the safe default** - the dispatch agent can run
  `Autonomous`, so a tool it should not have executes with no approval. Do not re-derive it from
  `RequiredPermission`; a dispatch tool may legitimately require `Permission.Trip.Manage`.

`AgentLoopRunner` (shared by both surfaces) caps at **25 iterations per session**.

## Model selection and cost

Model and reasoning effort are **global**, admin-set (`AI.Model` / `AI.ReasoningEffort` in
SystemSettings), resolved once per session by `LlmSessionSetup`. Tenants never pick a model or see
model names; plans differ by **weekly budget only** - there is no tier or multiplier system. Do not
reintroduce one.

- `LlmModelCatalog` owns the selectable models and each `ReasoningStyle`: `OpenAIEffort` models
  **always get an explicit `reasoning_effort`** (`None` → `"none"`; the server default 400s with
  function tools); `AnthropicAdaptive` models never get a temperature and replay thinking blocks
  via `LlmThinkingBlock`; `None` models get no reasoning parameter.
- `LlmPricing` owns per-token prices - **read the numbers there**, never copy them into docs.
  Unknown models charge at Sonnet 5 rates.
- Quota is **cost-based**: `AgentSession.EstimatedCostUsd` (written in `AgentLoopRunner`'s finally,
  so failed and cancelled sessions count) summed weekly against
  `SubscriptionPlan.WeeklyAIBudgetUsd` (null = unlimited). The tenant API exposes a percentage and
  `OverageChargesUsd` - never budget dollars.
- Overage: billed-not-blocked by default. `AIOverageBilling`
  (`Application.Abstractions/Payments/Stripe/`) owns **both** halves of the rule - `Billable` /
  `IsBillable` (completed over-budget sessions only) and cost→units ($0.10/unit, 3× markup, min 1).
  `AgentOverageReporter` meters Stripe through it and `AIQuotaService` sums the tenant-visible
  `OverageChargesUsd` through it; re-deriving either half makes the displayed number contradict the
  invoice. `IsOverage` is stamped at session start - the crossing run is free and
  failed/cancelled runs never bill; both priced into the markup. `TenantSettings.BlockAIOverage`
  swaps billing for a hard pause: `OverageBlocked` gates copilot sends (`AIBudgetReached`, not an
  upgrade code) and fail-fasts dispatch sessions, which must keep `IsOverage` false or they'd bill.

Resolution is setting → appsettings default for both halves, each through one owner
(`LlmModelResolver`, `AISettingsResolver`) so the admin screen cannot report a setting the agents
are not using. The provider is derived from the model via the catalog, never stored.

**Per-request override is for one-shot calls only.** `LlmCompletionRequest.ModelId` lets an
`ILlmClient` call pin a cheap model (the nightly learning pass does). `LlmModelResolver` honours it
only when the id is in the catalog **and** that provider has an API key - without that check, an
install with a single configured provider fails every night. The agent loop has no override.

## Learned dispatch policy

`AIDispatchPolicy` (one row per tenant) holds a short markdown policy the nightly
`AIDispatchPolicyLearningJob` derives from approve/reject history, injected into the system prompt
between `## HOS Rules` and `## Workflow` as defaults ranking below the hard constraints. Traps:

- The human-approved population is `Status == Executed && ApprovedByUserId != null`. `Approve()` is
  immediately overwritten by `MarkExecuted()`, so **filtering on `Approved` finds nothing** - and
  autonomous executions carry no human signal at all.
- `GeneratedContent` is job-owned, `ManualContent` is dispatcher-owned. Never merge them.
- Policy text is **untrusted** (LLM output derived from dispatcher-typed rejection reasons).
  `AIDispatchSystemPrompt` sanitises and truncates at the injection point, not in callers.
- Truncation keeps **whole lines only** - a half-truncated rule reads as a different rule.
- Learning creates no `AgentSession`, so it never consumes quota. Cost lands on
  `AIDispatchPolicy.GenerationCostUsd` and is never exposed to the tenant.
