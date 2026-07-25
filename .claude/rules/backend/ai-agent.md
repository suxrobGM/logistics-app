---
paths:
  - "src/Infrastructure/Logistics.Infrastructure.AI/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Integrations/AIDispatch/**/*.cs"
  - "src/Core/Logistics.Application/Modules/Platform/AISettings/**/*.cs"
  - "src/Presentation/Logistics.McpServer/**/*.cs"
---

# AI Dispatch Agent Conventions

For step-by-step recipes, use the skills:

- `add-dispatch-tool` - add a new tool the agent can call
- `add-llm-provider` - add a new model or LLM provider

This file is conventions only.

## Project structure

The agent loop and everything provider-specific lives in `src/Infrastructure/Logistics.Infrastructure.AI/`:

- `Providers/` - `ILlmProvider` interface, `LlmTypes`, `AnthropicLlmProvider`, `OpenAILlmProvider`, `LlmProviderFactory`
- `Services/` - Agent loop (`AIDispatchService`), `AIDispatchToolExecutor`, `AIDispatchToolRegistry`, `AIDispatchDecisionProcessor`, `AIDispatchConversationBuilder`, `LlmPricing`
- `Tools/` - Individual tool implementations (one per file, each implementing `IAIDispatchTool`)
- `Prompts/` - The agent's own system prompt builders

Configuration (`LlmOptions`, `LlmProviderOptions`) is **not** here - the application layer reads it,
so it lives in `Application.Abstractions/AI/`.

A one-shot `ILlmClient` feature is **not** agent code: its workflow service and its prompt live
together in `Logistics.Application/Modules/{Module}/.../Services/` (policy learning) or in the
infrastructure project that owns the feature (`Infrastructure.Documents/PdfImport/DispatchSheetPrompt`).
Only prompts the dispatch agent itself sends belong in `Infrastructure.AI/Prompts/`.

## Multi-provider architecture

Provider-agnostic via the `ILlmProvider` adapter pattern:

- `AnthropicLlmProvider` - Claude API via `Anthropic.SDK` (prompt caching, extended thinking)
- `OpenAILlmProvider` - OpenAI-compatible APIs via `OpenAI` SDK (OpenAI, DeepSeek, GLM via configurable `BaseUrl`)
- `LlmProviderFactory` - resolves provider from `LlmOptions.DefaultProvider`

**Provider-specific SDK types must not leak outside the provider classes.** The agent loop, tools, and decision processor use `LlmTypes` (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`) only.

## Tools

Each tool is its own class implementing `IAIDispatchTool`:

```csharp
internal sealed class GetSomethingTool(ITenantUnitOfWork tenantUow) : IAIDispatchTool
{
    public string Name => "get_something";
    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct) { /* ... */ }
}
```

`AIDispatchToolExecutor` builds a name → tool dictionary from DI-injected `IEnumerable<IAIDispatchTool>`. Tools must be registered in `Registrar.cs` with `services.AddScoped<IAIDispatchTool, MyTool>()`. Their schemas live in `AIDispatchToolRegistry.Tools`.

Tool names use **snake_case**. Schemas follow JSON Schema (compatible with both Claude and OpenAI function calling).

## Tool classification

- **Read tools** - pure queries. Always execute immediately in both modes.
- **Write tools** - mutate state. In `HumanInTheLoop` → create `Suggested` decisions for approval. In `Autonomous` → execute immediately.

A write tool's name **must** be added to `AIDispatchDecisionProcessor.WriteTools` HashSet. Missing this entry silently breaks HumanInTheLoop approvals.

## Agent loop pattern

`AIDispatchService` loop: send message → receive tool calls → record decision → execute or suggest → send tool results → repeat until `end_turn`.

Max **25 iterations per session** to prevent runaway token usage.

## Configuration

`appsettings.json` `"Llm"` section with nested provider configs:

```json
{
  "Llm": {
    "DefaultProvider": "Anthropic",
    "Providers": {
      "Anthropic": { "ApiKey": "...", "Model": "claude-haiku-4-5" },
      "OpenAI": { "ApiKey": "...", "Model": "gpt-5.4-mini" },
      "DeepSeek": {
        "ApiKey": "...",
        "Model": "deepseek-v4-flash",
        "BaseUrl": "https://api.deepseek.com/v1"
      }
    }
  }
}
```

API keys via env vars: `Llm__Providers__{Provider}__ApiKey`.

## Global model (admin-managed)

The dispatch model is **global**, set by an admin in the admin portal - tenants do not pick a model and
never see model names. Plans differ by **quota only**, not by model tier (there is no per-plan model
gating / `AllowedModelTier`).

`LlmModelCatalog` (in `Application.Abstractions/AIDispatch`) is the single source of selectable models
(`Id`, `DisplayName`, `Provider`). `LlmPricing` keeps pricing, the cost multiplier, and overage units keyed
by the same ids:

| Model                              | Multiplier | Overage units (at $0.20) |
| ---------------------------------- | ---------- | ------------------------ |
| deepseek-v4-flash, deepseek-v4-pro | 1×         | 1 ($0.20)                |
| gpt-5.4-mini, claude-haiku-4-5     | 1×         | 1 ($0.20)                |
| gpt-5.4, claude-sonnet-4-6         | 5×         | 2 ($0.40)                |
| claude-opus-4-8                    | 10×        | 4 ($0.80)                |

## Quota system

Weekly AI request quotas use multiplier-based counting (not flat session counts):

- `SubscriptionPlan.WeeklyAIRequestQuota` - weekly limit in request units (null = unlimited). Admin-editable
  live from the admin portal AI Settings page.
- `AIDispatchSession.RequestCost` - multiplier (1, 5, or 10) set from `LlmPricing.GetMultiplier()`
- `AIQuotaService` sums `RequestCost` across completed sessions for the week
- Tenant-facing API returns usage as a percentage (no raw numbers, no model/tier names)
- Overage billing via Stripe: `LlmPricing.GetOverageBillingUnits()` returns 1 / 2 / 4 at $0.20/unit

`GetMultiplier` and `GetOverageBillingUnits` must agree on each model's cost tier (1×→1 unit, 5×→2, 10×→4). The `add-llm-provider` skill enforces this.

## Model selection priority

Resolution order in `AIDispatchConversationBuilder` (no tenant override):

1. **Global system setting** - `SystemSettings["AI.Model"]` (key in `AISettingsKeys`), set via the
   admin `UpdateAISettingsCommand`. The provider is derived from the model via `LlmModelCatalog`,
   never stored.
2. **System default** - `LlmOptions.DefaultProvider` + `LlmProviderOptions.Model` from appsettings.

Extended thinking is likewise global: `SystemSettings["AI.ExtendedThinking"]` → `LlmOptions.EnableExtendedThinking`.
Only honored by providers that support it.

### Per-request override (one-shot calls only)

`LlmCompletionRequest.ModelId` lets a **one-shot** `ILlmClient` call pick a specific `LlmModelCatalog`
model - used by the nightly policy learning pass to stay on a cheap tier. `LlmModelResolver` honors it
only when the id is in the catalog **and** that provider has an API key configured; otherwise it logs a
warning and falls back to the global model. The API-key check is load-bearing: without it, an install
that configured only one provider would fail every night.

The **agent loop has no override** and must not get one - `AIDispatchConversationBuilder` always calls
`ResolveAsync(config)`. Tenants do not pick models and never see model names.

## Learned dispatch policy

`AIDispatchPolicy` (one row per tenant) holds a short markdown policy the nightly
`AIDispatchPolicyLearningJob` derives from approve/reject history, injected into the system prompt
between `## HOS Rules` and `## Workflow` as _strong defaults that rank below the hard constraints_.

Non-obvious rules when touching this:

- The human-approved population is `Status == Executed && ApprovedByUserId != null`. `Approve()` is
  immediately overwritten by `MarkExecuted()`, so filtering on `Approved` finds nothing, and autonomous
  executions carry no human signal.
- `GeneratedContent` is job-owned, `ManualContent` is dispatcher-owned. Never merge them.
- Policy text is **untrusted** - it is LLM output derived from dispatcher-typed rejection reasons.
  `AIDispatchSystemPrompt` sanitises and truncates it; keep that at the injection point, not in callers.
- Truncation keeps whole lines only. A half-truncated rule reads as a different rule.
- Learning creates **no** `AIDispatchSession` row, so it never consumes the tenant's weekly quota.
  Cost lands on `AIDispatchPolicy.GenerationCostUsd` and is never exposed to the tenant.

See [docs/ai-dispatch.md](../../../docs/ai-dispatch.md) for the full picture.
