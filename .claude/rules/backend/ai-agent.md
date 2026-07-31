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

**Anthropic caching.** `BuildMessages` puts an ephemeral breakpoint on the last content block of the
newest message, alongside the system-prompt one. Both halves look droppable and are not: it must be
re-placed every call (a breakpoint searches back only 20 content blocks, and one iteration with
parallel tool calls can add more), and it must be the _last_ block (the prefix caches only up to it).
Cache reads bill at a tenth of input, so hits lower `EstimatedCostUsd` and therefore the tenant's
budget and overage. Verify with `AgentSession.CacheReadTokens`; a steady zero means the prefix was
invalidated (tools changed, model switched) or the prompt is under the per-model minimum - 1024
tokens on Sonnet 5, 4096 on Haiku 4.5, silently skipped below. OpenAI and DeepSeek cache
server-side, which is why this lives in the provider and not the loop.

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

- `LlmModelCatalog` owns the selectable models and each one's `ReasoningStyle`. `OpenAIEffort`
  models **must always get an explicit `reasoning_effort`** (`None` → `"none"`) - the server default
  400s with function tools. `AnthropicAdaptive` models never get a temperature (Sonnet 5 rejects
  non-default sampling) and replay thinking blocks in-turn via `LlmThinkingBlock`. `None` models
  (DeepSeek, Haiku, unknown ids) never get a reasoning parameter.
- `LlmPricing` owns per-token prices, keyed by the same ids. **Read the numbers there** - do not copy
  them into docs. An unknown model is charged at Sonnet 5 rates (conservative fallback).
- Quota is **cost-based**: `AgentSession.EstimatedCostUsd` (written in `AgentLoopRunner`'s finally,
  so failed and cancelled sessions count) summed for the week against
  `SubscriptionPlan.WeeklyAIBudgetUsd` (null = unlimited). The tenant API returns a percentage only.
- Overage: over-budget **completed dispatch** sessions report raw cost to `IStripeUsageService`;
  `AIOverageBilling` owns the cost→units conversion ($0.10/unit, 3× markup, min 1). The copilot is
  hard-blocked instead and never bills overage.

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
