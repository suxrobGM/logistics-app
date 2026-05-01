---
paths:
  - "src/Infrastructure/Logistics.Infrastructure.AI/**/*.cs"
  - "src/Core/Logistics.Application/Commands/Dispatch/**/*.cs"
  - "src/Core/Logistics.Application/Queries/Dispatch/**/*.cs"
  - "src/Presentation/Logistics.McpServer/**/*.cs"
---

# AI Dispatch Agent Conventions

For step-by-step recipes, use the skills:

- `add-dispatch-tool` — add a new tool the agent can call
- `add-llm-provider` — add a new model or LLM provider

This file is conventions only.

## Project structure

All AI agent code lives in `src/Infrastructure/Logistics.Infrastructure.AI/`:

- `Providers/` — `ILlmProvider` interface, `LlmTypes`, `AnthropicLlmProvider`, `OpenAiLlmProvider`, `LlmProviderFactory`
- `Services/` — Agent loop (`DispatchAgentService`), `DispatchToolExecutor`, `DispatchToolRegistry`, `DispatchDecisionProcessor`, `DispatchConversationBuilder`, `LlmPricing`
- `Tools/` — Individual tool implementations (one per file, each implementing `IDispatchTool`)
- `Prompts/` — System prompt builders
- `Options/` — Configuration (`LlmOptions`, `LlmProviderOptions`)

## Multi-provider architecture

Provider-agnostic via the `ILlmProvider` adapter pattern:

- `AnthropicLlmProvider` — Claude API via `Anthropic.SDK` (prompt caching, extended thinking)
- `OpenAiLlmProvider` — OpenAI-compatible APIs via `OpenAI` SDK (OpenAI, DeepSeek, GLM via configurable `BaseUrl`)
- `LlmProviderFactory` — resolves provider from `LlmOptions.DefaultProvider`

**Provider-specific SDK types must not leak outside the provider classes.** The agent loop, tools, and decision processor use `LlmTypes` (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`) only.

## Tools

Each tool is its own class implementing `IDispatchTool`:

```csharp
internal sealed class GetSomethingTool(ITenantUnitOfWork tenantUow) : IDispatchTool
{
    public string Name => "get_something";
    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct) { /* ... */ }
}
```

`DispatchToolExecutor` builds a name → tool dictionary from DI-injected `IEnumerable<IDispatchTool>`. Tools must be registered in `Registrar.cs` with `services.AddScoped<IDispatchTool, MyTool>()`. Their schemas live in `DispatchToolRegistry.Tools`.

Tool names use **snake_case**. Schemas follow JSON Schema (compatible with both Claude and OpenAI function calling).

## Tool classification

- **Read tools** — pure queries. Always execute immediately in both modes.
- **Write tools** — mutate state. In `HumanInTheLoop` → create `Suggested` decisions for approval. In `Autonomous` → execute immediately.

A write tool's name **must** be added to `DispatchDecisionProcessor.WriteTools` HashSet. Missing this entry silently breaks HumanInTheLoop approvals.

## Agent loop pattern

`DispatchAgentService` loop: send message → receive tool calls → record decision → execute or suggest → send tool results → repeat until `end_turn`.

Max **25 iterations per session** to prevent runaway token usage.

## Configuration

`appsettings.json` `"Llm"` section with nested provider configs:

```json
{
  "Llm": {
    "DefaultProvider": "Anthropic",
    "Providers": {
      "Anthropic": { "ApiKey": "...", "Model": "claude-haiku-4-5" },
      "OpenAi": { "ApiKey": "...", "Model": "gpt-5.4-mini" },
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

## Model tier access

Models are classified into tiers via `LlmModelTier` enum. Each plan has `AllowedModelTier`:

| Model                              | Tier    | Quota multiplier | Overage units (at $0.20) |
| ---------------------------------- | ------- | ---------------- | ------------------------ |
| deepseek-v4-flash, deepseek-v4-pro | Base    | 1×               | 1 ($0.20)                |
| gpt-5.4-mini, claude-haiku-4-5     | Base    | 1×               | 1 ($0.20)                |
| gpt-5.4, claude-sonnet-4-6         | Premium | 5×               | 2 ($0.40)                |
| claude-opus-4-6                    | Ultra   | 10×              | 4 ($0.80)                |

Plans: Starter = Base, Professional = Premium, Enterprise = Ultra.

## Quota system

Weekly AI request quotas use multiplier-based counting (not flat session counts):

- `SubscriptionPlan.WeeklyAiRequestQuota` — weekly limit in request units
- `DispatchSession.RequestCost` — multiplier (1, 5, or 10) set from `LlmPricing.GetMultiplier()`
- `AiQuotaService` sums `RequestCost` across completed sessions for the week
- Tenant-facing API returns usage as a percentage (no raw numbers)
- Overage billing via Stripe: `LlmPricing.GetOverageBillingUnits()` returns 1 / 2 / 4 at $0.20/unit

`GetMultiplier`, `GetModelTier`, and `GetOverageBillingUnits` must agree on which tier each model belongs to. The `add-llm-provider` skill enforces this.

## Model selection priority

Resolution order in `DispatchConversationBuilder`:

1. **Tenant selection** — `TenantSettings.LlmModel` (set by tenant in AI Settings, or by admin in tenant-edit)
2. **System default** — `LlmProviderOptions.Model` from appsettings

Provider resolution: `TenantSettings.LlmProvider` → `LlmOptions.DefaultProvider`.
