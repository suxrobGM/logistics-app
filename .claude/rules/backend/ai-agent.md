---
paths:
  - "src/Infrastructure/Logistics.Infrastructure.AI/**/*.cs"
  - "src/Core/Logistics.Application/Modules/AI/**/*.cs"
  - "src/Presentation/Logistics.McpServer/**/*.cs"
---

# AI Agents

Traps only. Recipes: `add-dispatch-tool`, `add-llm-provider`. Full picture: [docs/ai-dispatch.md](../../../docs/ai-dispatch.md).

## Layout

`Logistics.Infrastructure.AI/` is grouped by capability. There is no `Services/`, `Models/` or `Prompts/`.

| Folder | Holds |
| --- | --- |
| `Llm/` | `ILlmProvider`, factory, one-shot `LlmClient`, model resolution, sanitising |
| `Llm/Contracts/` | The `Llm*` records everything outside `Providers/` sees |
| `Llm/Providers/` | The only files that may touch a vendor SDK |
| `Agents/` | Shared runtime: loop runner, decision processor, `AgentTurnService` (one turn lifecycle, parameterized by `IAgentSurface`), transcripts |
| `Agents/Dispatch/`, `Agents/Copilot/` | One folder per surface |
| `Tools/{Domain}/` | Tools, grouped by the Application module they call |

- Config (`LlmOptions`, `LlmModelCatalog`, `AISettingsKeys`) and the agent ports live in `Application.Abstractions/AI/`.
- A prompt sits beside its agent as `*SystemPrompt.cs`. One-shot `ILlmClient` features keep prompt and service together in their module.
- Shared machinery is named `Agent*`. `AIDispatch*` is only what is dispatch-specific (`AIDispatchPolicy`, the dispatch surface).
- `AIDispatchSystemPrompt.Build` varies on `Tenant.IsSolo`.

## Providers

`LlmProviderFactory` picks by the `LlmProvider` enum, never by model: the wire protocol belongs to the endpoint.

| Provider | Endpoint | Serves |
| --- | --- | --- |
| `AnthropicLlmProvider` | official SDK | Anthropic |
| `OpenAIResponsesLlmProvider` | `/v1/responses` | OpenAI |
| `OpenAICompatibleLlmProvider` | `/v1/chat/completions` at `BaseUrl` | the rest (DeepSeek, GLM) |

- **Use the factory's `llm` client.** It has no resilience handler, because the host's 30s total would cut agent turns. The vendor SDKs are the only retry layer; `AgentLoopRunner` has none on purpose.
- **Translate SDK exceptions to `LlmProviderException`** and classify by `LlmFailureKind`. Anthropic throws one type for a 401 and a 529; an outage once read as "check the API key".
- **Stop reasons:** `MaxTokens` must not run the reply's tool calls (they may be truncated). `PauseTurn` resends the conversation as is.
- **The Anthropic SDK serializes an assigned `null`.** Set `Thinking`, `OutputConfig`, `Temperature`, `CacheControl` only when they have a value.
- **Server-tool blocks (web search) round-trip as `LlmProviderBlock`** within a turn and are never persisted. Dropping one breaks `pause_turn` and invalidates later thinking signatures.
- **Keep OpenAI on Responses.** Function tools plus any reasoning effort is a 400 on chat completions. Don't merge the two OpenAI classes: DeepSeek serves Responses for some models only, at a base URL without `/v1`.
- `StoredOutputEnabled = false` is load-bearing: tool outputs contain driver, load and HOS data.
- Reasoning items are not replayed across tool turns. `LlmThinkingBlock` is Anthropic-only.
- **Web search is always on** (OpenAI `CreateWebSearchTool()`, Anthropic `WebSearchFor(model)`, 3 uses). The user rejected an enable flag; don't add one. Compatible endpoints have none. Per-search fees are not in `LlmModelCatalog`, so searched turns undercount cost.

## Caching and determinism

- Anthropic: `BuildParameters` breakpoints the system prompt and sets request-level `CacheControl` (applies to the last block). Don't switch to per-block markers: a breakpoint looks back only 20 blocks and caches only up to itself.
- `CacheReadTokens` stuck at zero means the prefix changed (tools or model) or the prompt is under the model's minimum.
- **Requests are append-only and byte-stable.** No dates or per-request text in system prompts (time comes from `AgentTranscriptLoader.SentStamp`), no in-memory notes on replayed messages, replay window moves in `WindowStep` jumps. History may change only at the start of a turn.
- OpenAI's `input_tokens` includes cached tokens; Anthropic's excludes them. OpenAI-shaped providers fill `LlmTokenUsage` through `OpenAIUsage.From`, or cache hits bill twice.

## Tools

`internal sealed`, `AgentTool<TInput>` + `IAgentToolMetadata`, snake_case name mirrored by the class name. One file per tool; `AgentToolCatalog` discovers it, so there is no list to update.

- **The input type is the schema.** Never hand-write JSON Schema or read the raw `JsonNode`. `ToolInput` accessors are only for persisted `AgentDecision.ToolInput`.
- Metadata on the static `Definition`: `RequiredPermission`, `DecisionType`, `RequiredFeature`, `Surfaces`, `Destructive`. A write is any `DecisionType` other than `Query`. Missing `RequiredPermission` leaks the tool to every copilot user.
- **`Surfaces` defaults to `Copilot`.** `Dispatch` means unattended fleet runs; `Mcp` means an API key with no person and no approval. Both are deliberate; don't derive them from the permission.
- On agent surfaces, writes become `Suggested` decisions. Exception: auto-approve for types in `AIDispatchPolicy.AutoApproveEligibleTypes` once the tenant opts in and the clean streak reaches `AutoApproveStreak` (revertable for `AgentDecision.RevertWindow`). Never make anything eligible that spends money, contacts someone outside the carrier or notifies a driver.
- Over MCP writes execute immediately. `McpDenialReason` is the only MCP admission rule. Don't promise approval in a tool description; `AgentToolRegistry` appends the right sentence per surface.
- `[AgentEntityId]` on an id input links the decision to the entity. Without it the link is silently missing.
- `AgentLoopRunner` caps a session at 25 iterations.

## Models and cost

- Model and reasoning effort are global, admin-set (`AI.Model`, `AI.ReasoningEffort`), resolved per session by `LlmSessionSetup`. Tenants never pick a model; plans differ only by weekly budget. No tiers or multipliers.
- `LlmModelCatalog` owns models, prices and `ReasoningStyle`. Never copy prices into docs. Unknown models bill at Sonnet 5.5 rates. `AnthropicAdaptive` models get no temperature and must replay every thinking block, redacted ones included.
- Quota is cost-based: `AgentSession.EstimatedCostUsd` (written in the runner's `finally`, so failures count) summed weekly against `SubscriptionPlan.WeeklyAIBudgetUsd`. Tenants see a percentage and `OverageChargesUsd`, never budget dollars.
- Every plan has a budget; unsubscribed tenants use the Enterprise budget via `AIWeeklyBudget`. Resolve budgets anywhere else and they vanish from the admin report.
- Overage bills by default; `TenantSettings.BlockAIOverage` pauses instead. `AIOverageBilling` owns both halves of the rule.
- `LlmCompletionRequest.ModelId` overrides the model for one-shot calls only, and only when the model is in the catalog and its provider has a key.

## Learned dispatch policy

`AIDispatchPolicyLearningJob` writes a markdown policy per tenant from approve/reject history, injected between `## HOS Rules` and `## Workflow`.

- Human-approved means `Status == Executed && ApprovedByUserId != null`. `Approve()` is immediately followed by `MarkExecuted()`, so filtering on `Approved` finds nothing.
- `GeneratedContent` is the job's, `ManualContent` the dispatcher's. Never merge them.
- Policy text is untrusted. `AIDispatchSystemPrompt` sanitises and truncates it at injection, keeping whole lines only.
- Learning creates no `AgentSession` and uses no quota.

The MCP server pins `Stateless = true` (`Logistics.McpServer/Registrar.cs`) although it is the SDK default, so an upgrade can't change it silently.
