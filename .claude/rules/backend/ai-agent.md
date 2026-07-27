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

`src/Infrastructure/Logistics.Infrastructure.AI/` holds the agent loop and everything
provider-specific: `Providers/`, `Services/`, `Tools/` (one class per file), `Prompts/`.

Two placement rules that get broken:

- **Config is not here.** `LlmOptions` / `LlmProviderOptions` live in `Application.Abstractions/AI/`,
  because the application layer reads them.
- **Only the dispatch agent's own prompts go in `Prompts/`.** A one-shot `ILlmClient` feature is not
  agent code - its workflow service and prompt live together, either in
  `Logistics.Application/Modules/{Module}/.../Services/` (policy learning) or in the infrastructure
  project that owns the feature (`Infrastructure.Documents/PdfImport/DispatchSheetPrompt`).

`AIDispatchSystemPrompt.Build` also varies by `TenantSettings.OperatingMode`: `SoloOperator` swaps the
fleet-framed lines (utilization, truck-to-truck comparison, the assignment table) and appends a
`## Fleet Profile` block. That heading is deliberately not `## Operating Mode` - that one is already
taken by `AIDispatchMode`, and reusing it makes the model conflate the two.

## Provider abstraction

`AnthropicLlmProvider` (Claude via `Anthropic.SDK` - prompt caching, extended thinking) and
`OpenAILlmProvider` (any OpenAI-compatible endpoint via configurable `BaseUrl`) both sit behind
`ILlmProvider`, resolved by `LlmProviderFactory`.

**Provider SDK types must not leak past the provider class.** The agent loop, tools, and decision
processor see `LlmTypes` (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`) only.

## Tools

`internal sealed`, implementing `IAIDispatchTool`, **snake_case** `Name`, JSON Schema that works for
both Claude and OpenAI function calling. Registered in `Registrar.cs`; schemas in
`AIDispatchToolRegistry.Tools`, which is shared with the MCP server.

- Read tools are pure queries and always execute immediately.
- Write tools mutate state: `HumanInTheLoop` turns them into `Suggested` decisions, `Autonomous`
  executes them.
- **A write tool's name must be added to the `AIDispatchDecisionProcessor.WriteTools` HashSet.**
  Miss it and HumanInTheLoop approvals break silently - the tool just executes.

The agent loop caps at **25 iterations per session**.

## Model selection and cost

The dispatch model is **global**, set by an admin. Tenants never pick a model and never see model
names; plans differ by **quota only** - there is no per-plan model tier.

- `LlmModelCatalog` (`Application.Abstractions/AIDispatch`) is the single source of selectable models.
- `LlmPricing` is the single source of pricing, cost multipliers, and overage units, keyed by the same
  ids. **Read the current numbers there** - do not copy them into docs, they change every model refresh.
- `GetMultiplier` and `GetOverageBillingUnits` must agree on each model's tier. `add-llm-provider`
  enforces this.
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
