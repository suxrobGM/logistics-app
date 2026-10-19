---
paths:
  - "src/Infrastructure/Logistics.Infrastructure.AI/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Integrations/AIDispatch/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Integrations/AICopilot/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Integrations/Agents/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Platform/AISettings/**/*.cs"
  - "src/Presentation/Logistics.McpServer/**/*.cs"
---

# AI Dispatch Agent Conventions

Traps only. Recipes are in the `add-dispatch-tool` and `add-llm-provider` skills; the end-to-end
picture is [docs/ai-dispatch.md](../../../docs/ai-dispatch.md).

## Where things live

`Logistics.Infrastructure.AI/` is grouped by capability, not layer. There is no `Services/`,
`Models/` or `Prompts/` folder - if you are about to create one, you want one of these:

| Folder | Holds |
| --- | --- |
| `Llm/` | `ILlmProvider`, factory, one-shot `LlmClient`, model resolution, pricing, sanitising |
| `Llm/Contracts/` | The `Llm*` wire records the rest of the project sees |
| `Llm/Providers/` | The only files allowed to touch a vendor SDK |
| `Agents/` | Shared runtime: loop runner, decision processor, cancellation, tool-call context, `AgentTurnService` (the one turn lifecycle every conversational agent runs through, parameterized by `IAgentSurface`), transcript codec/replay |
| `Agents/Dispatch/`, `Agents/Copilot/` | One folder per agent surface (each implements `IAgentSurface` for `AgentTurnService`) |
| `Tools/` | Tool base class, schema/binding (`AgentToolJson`), `ToolResult`, `DispatchUnits`, the catalogue |
| `Tools/{Dispatch,Financial,Operations,Compliance,LoadBoard,Intermodal,Negotiation}/` | Tools grouped by the Application module they dispatch into |

- **Config is not here.** `LlmOptions`, `LlmModelCatalog` and `AISettingsKeys` live in
  `Application.Abstractions/AI/` (the application layer reads them); the agent ports
  (`IAgentToolRegistry`, `IAgentToolExecutor`, `IAgentRunContext`, `AgentToolDefinition`) in
  `Application.Abstractions/Agents/`.
- **A prompt lives beside the agent that owns it**, with the `*SystemPrompt.cs` suffix. One-shot
  `ILlmClient` features are not agent code - prompt and workflow service live together, in
  `Logistics.Application/Modules/{Module}/.../Services/` or the infrastructure project owning it.
- **Shared machinery is `Agent*`, not `AIDispatch*`.** Registry, executor, tool contract, decision
  processor, turn service and cancellation serve dispatch, copilot and MCP alike (`AgentSession`,
  `AgentDecision`, `AgentConversation`, `AgentMessage`, `AgentTurnService`). `AIDispatch*` is what's
  left dispatch-only: `AIDispatchPolicy` and the dispatch service/surface.

`AIDispatchSystemPrompt.Build` varies by `Tenant.IsSolo` (the `SoloOperator` preset): solo swaps the
fleet-framed lines and appends `## Fleet Profile: SOLO OWNER-OPERATOR`.

## Providers

Three sit behind `ILlmProvider`, picked by `LlmProviderFactory` **on the `LlmProvider` enum, never
on the model** - the wire protocol belongs to the endpoint, not the model's reasoning style.
**SDK types must not leak past the provider class**; everything else sees `Llm/Contracts/`.

| Provider                      | Surface                                 | Serves                        |
| ----------------------------- | --------------------------------------- | ----------------------------- |
| `AnthropicLlmProvider`        | official `Anthropic` SDK (caching, adaptive thinking) | `Anthropic` |
| `OpenAIResponsesLlmProvider`  | `/v1/responses`                         | `OpenAI`                      |
| `OpenAICompatibleLlmProvider` | `/v1/chat/completions` via `BaseUrl`    | the rest (DeepSeek, GLM, ...) |

- **Providers use the factory's named `llm` client.** It carries only `RequestTimeoutSeconds` and no
  resilience handler (`RemoveAllResilienceHandlers` in `Registrar.cs`) - the host default's 30s
  total would cut agent turns. **The vendor SDKs are the only retry layer** (both retry 429 and
  5xx); `AgentLoopRunner` deliberately has none. Don't give a provider its own `HttpClient`; it
  would pick the host handler back up.
- **Providers translate their SDK's exceptions into `LlmProviderException`** (`LlmFailureKind`), and
  `LlmErrorSanitizer` classifies by kind. Never classify by exception type: Anthropic's SDK throws the
  same base type for a 401 and a 529, and an outage once read as "check the API key".
- **Stop reasons are `LlmStopReason`, mapped per provider.** `MaxTokens` must not run the reply's tool
  calls (they may be cut off); `PauseTurn` (Anthropic server tools) resends the conversation as it
  stands. Both are handled in `AgentLoopRunner`.
- **The official Anthropic SDK serializes an assigned `null`** as a JSON null rather than omitting
  the field. Set optional request fields (`Thinking`, `OutputConfig`, `Temperature`,
  `CacheControl`) only when they carry a value - see `BuildParameters`.
- **Anthropic server-tool blocks (web search) round-trip as `LlmProviderBlock`** - raw JSON, echoed
  back within the turn, never persisted. Dropping them breaks `pause_turn` and invalidates any later
  thinking block, whose signature covers the prefix that contained them.
- **Don't move OpenAI back onto chat completions.** Function tools plus any non-`none`
  `reasoning_effort` is a hard 400 there; that was the outage this split fixed.
- **Don't collapse the two OpenAI classes** without re-checking DeepSeek: as of Aug 2026 it serves
  Responses for `deepseek-flash` but not `deepseek-v4-pro`, and its
  Responses base URL drops the `/v1` the chat one carries.
- Responses defaults to `store: true`, so `StoredOutputEnabled = false` is load-bearing - tool
  outputs carry driver names, loads and HOS data.
- Reasoning items are **not** replayed across tool turns. Responses tolerates their absence but
  rejects one not immediately followed by its `function_call`. `LlmThinkingBlock` is Anthropic-only.

**Anthropic caching.** `BuildParameters` breakpoints the system prompt explicitly and sets the
request-level `CacheControl`, which the API applies to the last cacheable block. Don't swap that for
a fixed per-block marker: a breakpoint searches back only 20 blocks (one parallel-tool turn can
exceed that) and caches only up to itself, so it must sit on the newest block. Reads bill at a
tenth of input, so hits lower `EstimatedCostUsd` and the budget with it. `CacheReadTokens` stuck at
zero means an invalidated prefix (tools or model changed) or a prompt under the per-model minimum,
which is silently skipped. OpenAI and DeepSeek cache server-side, hence provider-level and not in
the loop.

**Keep every request append-only and deterministic.** The cache, and Anthropic's preserved-thinking
check, compare the prefix byte for byte. So: no date or per-request text in a system prompt (the
time comes from `AgentTranscriptReplay.SentStamp`), no in-memory notes added to a replayed message,
and the replay window moves in `WindowStep` jumps. History may only change at the start of a turn,
when no thinking block exists yet - that is why earlier turns' large tool results can be stubbed.

**The two vendors report cached tokens differently.** OpenAI's `input_tokens` **includes**
`cached_tokens`; Anthropic's **excludes** cache reads. `LlmPricing.Calculate` adds the input and
cache-read buckets, so both OpenAI-shaped providers must subtract before filling `LlmTokenUsage` -
that is what `OpenAIUsage.From` is for. Forward the raw counts and every cached token bills twice.

## Tools

`internal sealed`, deriving from `AgentTool<TInput>` and implementing `IAgentToolMetadata`,
**snake_case** name. Class name mirrors the tool name (`get_driver_hos_status` →
`GetDriverHosStatusTool`) so a transcript leads to the file.

**A tool is one file.** `AgentToolCatalog` scans the assembly at startup, reads each tool's static
`Definition`, generates its schema and feeds both DI and `AgentToolRegistry` - there is no catalogue
list and no registration to keep in step.

**The input type is the schema.** `AgentToolJson` exports `TInput` (snake_case names,
`[Description]` as the description, `required` members, enum value lists) and binds the reply back.
Never hand-write JSON Schema, and never read the raw `JsonNode` in a tool. Binding forgives quoted
numbers, unquoted text and casing, but a wrong type or unknown enum name fails the call naming the
property. `ToolInput`'s accessors are only for reading a **persisted** `AgentDecision.ToolInput`.

- On the agent surfaces, read tools always execute immediately and write tools always become
  `Suggested` decisions awaiting dispatcher/user approval - there is no unattended-execution path
  (`AgentDecisionProcessor`).
- **Behaviour metadata lives on the tool's static `Definition`** as named init properties:
  `RequiredPermission`, `DecisionType`, `RequiredFeature`, `Surfaces`, `Destructive`. `IsWrite` is
  **derived** (`DecisionType != Query`), so there is no separate flag to forget. Miss
  `RequiredPermission` and the tool leaks into every copilot conversation regardless of the caller's
  role.
- **`Surfaces` is opt-in and defaults to `Copilot` alone.** A tool nobody widened is under-exposed,
  never over-exposed. Naming `Dispatch` puts it in fleet dispatch runs, which have no caller to scope
  by; naming `Mcp` lets an API key run it with nobody to attribute it to and no approval step. Both
  are deliberate acts. Do not re-derive either from `RequiredPermission` - a dispatch tool may
  legitimately require `Permission.Trip.Manage`.
- One registry method per surface: `GetDispatchAgentTools`, `GetCopilotTools` (permission set is a
  required parameter, not a nullable flag - see `docs/ai-copilot.md`), `GetMcpTools`. Permissions do
  not apply to MCP: an API key authenticates a tenant, not a person.
- **`McpDenialReason` is the one MCP admission rule.** `GetMcpTools` is built from it rather than
  repeating it, because a client can call a name the catalogue never showed it. Put a new rule there
  and both paths get it.
- **Only the agent surfaces turn a write into a suggestion.** Over MCP a write executes immediately.
  Never write either promise into a tool's own description: `AgentToolRegistry` appends the approval
  sentence for the agents and the immediate-execution warning for MCP, one per surface method.
- **Mark an entity id with `[AgentEntityId]`** and `AgentDecisionProcessor` links the decision row to
  it. The wire key comes from the property, so a rename carries the audit link along. Skip the
  attribute and the link is simply never written - nothing fails.

`AgentLoopRunner` (shared by both surfaces) caps at **25 iterations per session**.

## Model selection and cost

Model and reasoning effort are **global**, admin-set (`AI.Model` / `AI.ReasoningEffort` in
SystemSettings), resolved once per session by `LlmSessionSetup`. Tenants never pick a model or see
model names; plans differ by **weekly budget only** - there is no tier or multiplier system. Do not
reintroduce one.

- `LlmModelCatalog` owns the selectable models and each `ReasoningStyle`: `OpenAIEffort` models
  **always get an explicit reasoning effort** (`None` → `"none"`) and are the reason OpenAI routes
  to `/v1/responses` - the same combination on chat completions is a 400 once function tools are
  present; `AnthropicAdaptive` models never get a temperature, always get adaptive thinking plus an explicit
  effort (`None` rides on `low` - omitting `thinking` still runs adaptive at the model's default), and
  replay thinking blocks via
  `LlmThinkingBlock` (**including redacted ones** - drop one and the next turn is rejected);
  `None` models get no reasoning parameter.
- `LlmPricing` owns per-token prices - **read the numbers there**, never copy them into docs.
  Unknown models charge at Sonnet 5.5 rates.
- Quota is **cost-based**: `AgentSession.EstimatedCostUsd` (written in `AgentLoopRunner`'s finally,
  so failed and cancelled sessions count) summed weekly against
  `SubscriptionPlan.WeeklyAIBudgetUsd`. The tenant API exposes a percentage and
  `OverageChargesUsd` - never budget dollars.
- **No tenant runs unmetered**: `WeeklyAIBudgetUsd` is required on every plan, and an unsubscribed
  tenant falls back to the Enterprise budget through `AIWeeklyBudget`
  (`Application.Abstractions/AIDispatch/`) - resolve a budget elsewhere and unsubscribed tenants
  vanish from the admin report.
- Overage: billed-not-blocked by default; `TenantSettings.BlockAIOverage` swaps billing for a hard
  pause. `AIOverageBilling` (`Application.Abstractions/Payments/Stripe/`) owns **both** halves of the
  rule (which sessions bill, cost→units) - re-derive either half and the displayed number
  contradicts the invoice. Details in [docs/ai-dispatch.md](../../../docs/ai-dispatch.md).

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
  immediately overwritten by `MarkExecuted()`, so **filtering on `Approved` finds nothing** - every
  executed write went through approval, so this filter is the whole population, not a carve-out.
- `GeneratedContent` is job-owned, `ManualContent` is dispatcher-owned. Never merge them.
- Policy text is **untrusted** (LLM output derived from dispatcher-typed rejection reasons).
  `AIDispatchSystemPrompt` sanitises and truncates at the injection point, not in callers.
- Truncation keeps **whole lines only** - a half-truncated rule reads as a different rule.
- Learning creates no `AgentSession`, so it never consumes quota. Its cost is only logged by
  `AIDispatchPolicyLearner` and is never exposed to the tenant.
