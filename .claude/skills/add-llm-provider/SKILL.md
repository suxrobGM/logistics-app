---
name: add-llm-provider
description: Add a new LLM provider or model to the AI dispatch system. Use when adding a new model from an existing provider (e.g., a new Claude version) or wiring up a new OpenAI-compatible endpoint. Walks through the places that must change to keep pricing, quotas, and the admin model catalog in sync.
---

# Add an LLM Provider or Model

Providers sit behind the `ILlmProvider` adapter.

## Decide the path

- **New OpenAI-compatible provider** (DeepSeek-style): no SDK code - `OpenAICompatibleLlmProvider` handles it via `BaseUrl`. Skip step 3.
- **New custom-SDK provider** (e.g. Gemini, Mistral): new `ILlmProvider` implementation in step 3.
- **New model from an existing provider** (e.g. a new Claude version): steps 4–6 only.

> The dispatch model is **global** (admin-selected) - no per-tenant selection, no per-plan tier gating.

## Files that must change (full provider)

1. `src/Core/Logistics.Domain.Primitives/Enums/AIDispatch/LlmProvider.cs` - add enum value
2. `src/Core/Logistics.Application.Abstractions/AI/LlmOptions.cs` - provider config section. Config is
   deliberately **not** in `Infrastructure.AI` (the application layer reads it) - see `.claude/rules/backend/ai-agent.md`
3. `src/Infrastructure/Logistics.Infrastructure.AI/Llm/Providers/{X}LlmProvider.cs` - only for non-OpenAI-compatible
4. `src/Infrastructure/Logistics.Infrastructure.AI/Llm/LlmProviderFactory.cs` - resolution case
5. `src/Infrastructure/Logistics.Infrastructure.AI/Llm/LlmPricing.cs` - pricing, multiplier, tier, billing units
6. `src/Core/Logistics.Application.Abstractions/AI/LlmModelCatalog.cs` - add the model `{ Id, DisplayName, Provider }`

## Step-by-step

### 1. Add enum value

```csharp
public enum LlmProvider
{
    Anthropic,
    OpenAI,
    DeepSeek,
    NewProvider // ← here
}
```

### 2. Add provider config section

In `LlmOptions.cs`:

```csharp
public record LlmProviderOptions
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
    public string? BaseUrl { get; set; } // null for native SDKs
}
```

Binds from `Llm:Providers:{Name}` in `appsettings.json`; the API key comes from the env var
`Llm__Providers__{Name}__ApiKey`, never a committed file.

### 3. (Custom SDK only) Create `ILlmProvider` implementation

If the provider is OpenAI-compatible (most modern providers are), **skip this step** - `OpenAICompatibleLlmProvider` handles it via `BaseUrl`.

If it requires a custom SDK, add `Llm/Providers/NewLlmProvider.cs`:

```csharp
internal sealed class NewLlmProvider(IOptions<LlmProviderOptions> options) : ILlmProvider
{
    public async Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct)
    {
        // Translate LlmRequest → provider SDK request
        // Translate provider response → LlmResponse (Llm/Contracts only - no SDK types leak out)
    }
}
```

Provider-specific SDK types **must not leak** outside this class. The agent loop uses only the `Llm/Contracts/` records (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`).

### 4. Resolve in `LlmProviderFactory`

```csharp
public ILlmProvider GetProvider(LlmProvider provider) => provider switch
{
    LlmProvider.Anthropic => anthropic,
    LlmProvider.OpenAI => openai,
    LlmProvider.DeepSeek => deepseek,        // OpenAI-compatible reuse
    LlmProvider.NewProvider => newProvider,  // ← here
    _ => throw new NotSupportedException($"Unknown provider: {provider}")
};
```

For OpenAI-compatible providers, instantiate `OpenAICompatibleLlmProvider` with the right `BaseUrl` from options.

### 5. Update `LlmPricing.cs`

**One entry** - per-token USD prices. The weekly budget and Stripe overage both derive from the
cost this computes; there is no tier or multiplier to pick.

```csharp
private static readonly Dictionary<string, ModelPricing> Pricing = new()
{
    // existing entries
    ["new-model-1"] = new(0.50m, 2.0m, 0.05m), // input, output, cache-read per M tokens
};
```

### 6. Add the model to `LlmModelCatalog`

In `src/Core/Logistics.Application.Abstractions/AI/LlmModelCatalog.cs`:

```csharp
public static readonly IReadOnlyList<LlmModelInfo> Models =
[
    // existing entries
    new("new-model-1", "New Model 1", LlmProvider.NewProvider, ReasoningStyle.OpenAIEffort),
];
```

**Pick the `ReasoningStyle` deliberately** - it decides whether providers send a reasoning
parameter for the admin-set effort level:

- `OpenAIEffort` - takes a reasoning effort, which OpenAI only accepts alongside function tools on
  `/v1/responses` (the GPT-5.6 Luna 400). Put these under `LlmProvider.OpenAI` - that enum value is
  what selects the Responses provider.
- `AnthropicAdaptive` - Claude adaptive thinking + effort (Sonnet 5 class). Also suppresses
  temperature, which these models reject.
- `None` (default) - no reasoning parameter is ever sent. For models without a reasoning control,
  and for compatible endpoints that reject it (DeepSeek).

This is the **single source** for the admin AI Settings dropdown (`GET /ai/settings`) and for validating the
selected model in `UpdateAISettingsCommand`. The admin UI populates automatically - no frontend change.

## Verification checklist

- [ ] Enum value added
- [ ] Config section + appsettings entry + env var documented
- [ ] (If custom SDK) Provider implementation, no SDK types leak
- [ ] Factory resolves the new provider
- [ ] **`LlmPricing.Pricing` entry added** (per-token USD prices)
- [ ] `LlmModelCatalog` includes the model (id matches the `LlmPricing` keys) with the right `ReasoningStyle`
- [ ] Admin AI Settings page shows the new model in the dropdown
- [ ] Selecting the model as the global model runs a dispatch session successfully

## Common mistakes

- **Wrong `ReasoningStyle`**: a flagged non-reasoning endpoint rejects the parameter. Unsure? Start with `None`.
- **`LlmProvider.OpenAI` for a non-OpenAI endpoint**: that value selects the Responses provider, and only OpenAI/Azure serve `/v1/responses`. Everything else 404s.
- **Catalog id ≠ `LlmPricing` key**: silently falls back to Sonnet 5 rates.
- **No `BaseUrl`** on a compatible provider: defaults to OpenAI's endpoint and 401s.
- **Raw token counts**: OpenAI folds cached tokens into `input_tokens`, so fill `LlmTokenUsage` via `OpenAIUsage.From` or every cache hit bills twice.
- **SDK types leaking**: importing the provider SDK in any file other than `Llm/Providers/{X}LlmProvider.cs` breaks the abstraction.

## Related

- `.claude/rules/backend/ai-agent.md` - multi-provider architecture overview
- `docs/ai-dispatch.md` - agent architecture
