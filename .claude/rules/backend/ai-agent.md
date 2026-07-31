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
  The `AIDispatch*` prefix that remains is the domain (`AgentSession`, `AgentDecision`,
  `AgentAutonomyMode`, `AIDispatchPolicy`) and the dispatch-only service.

`AIDispatchSystemPrompt.Build` also varies by `TenantSettings.OperatingMode`: `SoloOperator` swaps the
fleet-framed lines (utilization, truck-to-truck comparison, the assignment table) and appends a
`## Fleet Profile` block. That heading is deliberately not `## Operating Mode` - that one is already
taken by `AgentAutonomyMode`, and reusing it makes the model conflate the two.

## Provider abstraction

`AnthropicLlmProvider` (Claude via `Anthropic.SDK` - prompt caching, adaptive thinking) and
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

The dispatch model and reasoning effort are **global**, set by an admin (`AI.Model` /
`AI.ReasoningEffort` in SystemSettings, resolved once per session by `LlmSessionSetup`). Tenants
never pick a model and never see model names; plans differ by **weekly budget only** - there is no
per-plan model tier and **no multiplier/tier system**. Do not reintroduce one.

- `LlmModelCatalog` (`Application.Abstractions/AI`) is the single source of selectable models and
  of each model's `ReasoningStyle` (`None` / `OpenAIEffort` / `AnthropicAdaptive`). Reasoning-flagged
  OpenAI models **must always receive an explicit `reasoning_effort`** (None maps to `"none"`) -
  their server default 400s with function tools on chat completions. `AnthropicAdaptive` models
  never receive a temperature (Sonnet 5 rejects non-default sampling) and replay thinking blocks
  in-turn via `LlmThinkingBlock`. Style-`None` models (DeepSeek, Haiku, unknown ids) never receive
  a reasoning parameter.
- `LlmPricing` is the single source of per-token pricing, keyed by the same ids. **Read the current
  numbers there** - do not copy them into docs, they change every model refresh. An **unknown**
  model is charged at Sonnet 5 rates (conservative fallback - see `DefaultPricing`).
- Quota is **cost-based**: `AgentSession.EstimatedCostUsd` (written in `AgentLoopRunner`'s finally,
  so failed/cancelled sessions count too) is summed by `AIQuotaService` for the week against
  `SubscriptionPlan.WeeklyAIBudgetUsd` (null = unlimited). The tenant-facing API returns a
  percentage only - never dollars or tokens.
- Overage: over-budget **completed dispatch** sessions report their raw cost to
  `IStripeUsageService`; `AIOverageBilling` (Infrastructure.Payments) owns the cost→Stripe-units
  conversion ($0.10/unit, `CostMarkup` 3×, min 1 unit). The copilot is hard-blocked instead and
  never bills overage.

Resolution order, once per session in `LlmSessionSetup`: `SystemSettings["AI.Model"]` (admin-set;
provider is derived from the catalog, never stored) → `LlmOptions.DefaultProvider` + `Model` from
appsettings. Reasoning effort follows the same shape via `SystemSettings["AI.ReasoningEffort"]` →
`LlmOptions.DefaultReasoningEffort`; both halves resolve through one owner each (`LlmModelResolver`
and `AISettingsResolver`) so the admin screen cannot report a setting the agents are not using.

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
- Learning creates no `AgentSession` row, so it never consumes tenant quota. Cost lands on
  `AIDispatchPolicy.GenerationCostUsd` and is never exposed to the tenant.

## Anthropic prompt caching

`AnthropicLlmProvider.BuildMessages` puts an ephemeral cache breakpoint on the **last content
block of the newest message**, alongside the one on the system prompt. Two things about it look
droppable and are not:

- **It must be re-placed on every call.** A breakpoint searches back only 20 content blocks for an
  existing entry, and one iteration with several parallel tool calls can add more than that. Set it
  once at the front of the transcript and long sessions silently stop hitting the cache.
- **It has to be the _last_ block.** The prefix caches up to the breakpoint, so marking an earlier
  block in a multi-result tool turn leaves the rest re-billed at full input price.

Caching flows straight into the budget: `LlmPricing` charges cache reads at a tenth of input, so a
hit lowers `EstimatedCostUsd` and therefore what the tenant's weekly budget and Stripe overage see.
Verify with `AgentSession.CacheReadTokens` - a session that stays at zero across iterations means
something invalidated the prefix (tool list changed, model switched, or the prompt is below the
per-model minimum: 1024 tokens on Sonnet 5, 4096 on Haiku 4.5, silently skipped below that).

OpenAI and DeepSeek cache automatically server-side with no breakpoints, which is why this lives in
the Anthropic provider rather than the shared loop.
