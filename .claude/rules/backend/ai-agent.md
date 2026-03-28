# AI Dispatch Agent Conventions

## Project Structure

All AI agent code lives in `src/Infrastructure/Logistics.Infrastructure.AI/`:
- `Providers/` — `ILlmProvider` interface, `LlmTypes`, `AnthropicLlmProvider`, `OpenAiLlmProvider`, `LlmProviderFactory`
- `Services/` — Agent loop (`DispatchAgentService`), tool executor, tool registry, conversation builder, pricing
- `Tools/` — Individual tool implementations
- `Prompts/` — System prompt builders
- `Options/` — Configuration (`LlmOptions`, `LlmProviderOptions`)

## Multi-Provider Architecture

The agent is provider-agnostic via the `ILlmProvider` adapter pattern:
- `AnthropicLlmProvider` — Claude API via `Anthropic.SDK` (supports prompt caching, extended thinking)
- `OpenAiLlmProvider` — OpenAI-compatible APIs via `OpenAI` SDK (supports OpenAI, DeepSeek, GLM via configurable `BaseUrl`)
- `LlmProviderFactory` — Resolves provider from `LlmOptions.DefaultProvider`

Provider-specific SDK types never leak outside the provider classes. The agent loop, tools, and decision processor all use `LlmTypes` (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`, etc.).

## Adding a New Agent Tool

1. Add the tool definition in `DispatchToolRegistry.cs` with name, description, and JSON schema
2. Add the handler case in `DispatchToolExecutor.ExecuteToolAsync()` switch
3. If it's a write tool, add its name to `DispatchDecisionProcessor.WriteTools` HashSet
4. Tool names use `snake_case` (standard across all LLM providers)
5. Tool schemas follow JSON Schema format (compatible with both Claude and OpenAI function calling)

## Tool Classification

- **Read tools**: Always execute immediately in both modes. Used for querying fleet state.
- **Write tools**: In HumanInTheLoop mode → create `Suggested` decisions. In Autonomous mode → execute immediately.

## Agent Loop Pattern

The agent loop in `DispatchAgentService` follows: send message → receive tool calls → record decision → execute or suggest → send tool results → repeat until end_turn.

Max 25 iterations per session to prevent runaway token usage.

## Configuration

LLM config is in `appsettings.json` under `"Llm"` section with nested provider configs:

```json
{
  "Llm": {
    "DefaultProvider": "Anthropic",
    "Providers": {
      "Anthropic": { "ApiKey": "...", "Model": "claude-haiku-4-5" },
      "OpenAi": { "ApiKey": "...", "Model": "gpt-5.4-mini" },
      "DeepSeek": { "ApiKey": "...", "Model": "deepseek-chat", "BaseUrl": "https://api.deepseek.com/v1" }
    }
  }
}
```

API keys are passed via environment variables: `Llm__Providers__Anthropic__ApiKey`, `Llm__Providers__OpenAi__ApiKey`, `Llm__Providers__DeepSeek__ApiKey`.

## Model Tier Access

Models are classified into tiers via `LlmModelTier` enum. Each subscription plan has `AllowedModelTier`:

| Model | Tier | Quota Cost (multiplier) | Overage Billing Units |
|-------|------|------------------------|----------------------|
| deepseek-chat, deepseek-reasoner | Base | 1x | 1 ($0.20) |
| gpt-5.4-mini, claude-haiku-4-5 | Base | 1x | 1 ($0.20) |
| gpt-5.4, claude-sonnet-4-6 | Premium | 5x | 2 ($0.40) |
| claude-opus-4-6 | Ultra | 10x | 4 ($0.80) |

Plans: Starter=Base, Professional=Premium, Enterprise=Ultra.

## Quota System

Weekly AI request quotas use multiplier-based counting (not flat session counts):
- `SubscriptionPlan.WeeklyAiRequestQuota` — weekly limit in request units
- `DispatchSession.RequestCost` — multiplier used (1, 5, or 10), set from `LlmPricing.GetMultiplier()`
- `AiQuotaService` sums `RequestCost` across completed sessions for the week
- Tenant-facing API returns usage as a percentage (no raw numbers)
- Overage billing via Stripe: `LlmPricing.GetOverageBillingUnits()` returns 1/2/4 at $0.20/unit

## Model Selection Priority

Resolution order in `DispatchConversationBuilder`:
1. **Tenant selection** — `TenantSettings.LlmModel` (set by tenant in AI Settings, or by admin in tenant-edit)
2. **System default** — `LlmProviderOptions.Model` from appsettings

Provider resolution: `TenantSettings.LlmProvider` → `LlmOptions.DefaultProvider`

## Adding a New LLM Provider

1. If OpenAI-compatible: just add a new entry to `LlmProvider` enum and configure with `BaseUrl`
2. If custom SDK: create a new `ILlmProvider` implementation and add a case in `LlmProviderFactory`
3. Add pricing to `LlmPricing.cs` — include in `Pricing` dict, `GetMultiplier()`, `GetModelTier()`, `GetOverageBillingUnits()`
4. Add model to `UpdateTenantAiSettingsHandler.ModelTiers` dictionary
5. Add model options to admin portal `tenant-edit.ts` → `llmModelOptions`
6. Add model options to TMS portal AI Settings page filtered by tier
