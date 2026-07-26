---
name: add-llm-provider
description: Add a new LLM provider or model to the AI dispatch system. Use when adding a new model from an existing provider (e.g., a new Claude version) or wiring up a new OpenAI-compatible endpoint. Walks through the places that must change to keep pricing, quotas, and the admin model catalog in sync.
---

# Add an LLM Provider or Model

Providers sit behind the `ILlmProvider` adapter.

## Decide the path

- **New OpenAI-compatible provider** (DeepSeek-style): no SDK code - `OpenAILlmProvider` handles it via `BaseUrl`. Skip step 3.
- **New custom-SDK provider** (e.g. Gemini, Mistral): new `ILlmProvider` implementation in step 3.
- **New model from an existing provider** (e.g. a new Claude version): steps 4–6 only.

> The dispatch model is **global** (admin-selected) - no per-tenant selection, no per-plan tier gating.

## Files that must change (full provider)

1. `src/Core/Logistics.Domain.Primitives/Enums/AIDispatch/LlmProvider.cs` - add enum value
2. `src/Core/Logistics.Application.Abstractions/AI/LlmOptions.cs` - provider config section. Config is
   deliberately **not** in `Infrastructure.AI` (the application layer reads it) - see `.claude/rules/backend/ai-agent.md`
3. `src/Infrastructure/Logistics.Infrastructure.AI/Providers/{X}LlmProvider.cs` - only for non-OpenAI-compatible
4. `src/Infrastructure/Logistics.Infrastructure.AI/Providers/LlmProviderFactory.cs` - resolution case
5. `src/Infrastructure/Logistics.Infrastructure.AI/Services/LlmPricing.cs` - pricing, multiplier, tier, billing units
6. `src/Core/Logistics.Application.Abstractions/AIDispatch/LlmModelCatalog.cs` - add the model `{ Id, DisplayName, Provider }`

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

If the provider is OpenAI-compatible (most modern providers are), **skip this step** - `OpenAILlmProvider` handles it via `BaseUrl`.

If it requires a custom SDK, add `Providers/NewLlmProvider.cs`:

```csharp
internal sealed class NewLlmProvider(IOptions<LlmProviderOptions> options) : ILlmProvider
{
    public async Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct)
    {
        // Translate LlmRequest → provider SDK request
        // Translate provider response → LlmResponse (LlmTypes only - no SDK types leak out)
    }
}
```

Provider-specific SDK types **must not leak** outside this class. The agent loop uses only `LlmTypes` (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`).

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

For OpenAI-compatible providers, instantiate `OpenAILlmProvider` with the right `BaseUrl` from options.

### 5. Update `LlmPricing.cs`

**Three places** in this file. Miss any one and quota/billing breaks silently.

```csharp
private static readonly Dictionary<string, ModelPricing> Pricing = new()
{
    // existing entries
    ["new-model-1"] = new(0.50m, 2.0m, 0.05m), // input, output, cache-read per M tokens
};

public static int GetMultiplier(string model) => model switch
{
    "deepseek-..." or "claude-haiku-4-5" or "gpt-5.4-mini" or "new-model-1" => 1, // base = 1x
    "gpt-5.4" or "claude-sonnet-4-6" => 5, // premium = 5x
    "claude-opus-4-8" => 10, // ultra = 10x
    _ => 1
};

public static int GetOverageBillingUnits(string model) => model switch
{
    "gpt-5.4" or "claude-sonnet-4-6" => 2,
    "claude-opus-4-8" => 4,
    _ => 1 // ← matches GetMultiplier mapping
};
```

Decide the cost tier (1× / 5× / 10×), then keep billing units in step (1 / 2 / 4 at $0.20/unit). The tier only affects quota cost - it does **not** gate which plans can use the model (the model is global).

### 6. Add the model to `LlmModelCatalog`

In `src/Core/Logistics.Application.Abstractions/AIDispatch/LlmModelCatalog.cs`:

```csharp
public static readonly IReadOnlyList<LlmModelInfo> Models =
[
    // existing entries
    new("new-model-1", "New Model 1", LlmProvider.NewProvider),
];
```

This is the **single source** for the admin AI Settings dropdown (`GET /ai/settings`) and for validating the
selected model in `UpdateAISettingsCommand`. The admin UI populates automatically - no frontend change.

## Verification checklist

- [ ] Enum value added
- [ ] Config section + appsettings entry + env var documented
- [ ] (If custom SDK) Provider implementation, no SDK types leak
- [ ] Factory resolves the new provider
- [ ] **All three `LlmPricing` switches/dictionaries updated** (Pricing, GetMultiplier, GetOverageBillingUnits)
- [ ] `LlmModelCatalog` includes the model (id matches the `LlmPricing` keys)
- [ ] Admin AI Settings page shows the new model in the dropdown
- [ ] Selecting the model as the global model runs a dispatch session successfully

## Common mistakes

- **`GetMultiplier` and `GetOverageBillingUnits` out of sync**: a Premium model with multiplier=5 but billing=1 underbills overages.
- **`LlmModelCatalog` id ≠ `LlmPricing` key**: the catalog offers a model the pricing map doesn't know, so it falls back to default pricing/multiplier.
- **Forgetting `BaseUrl`** for OpenAI-compatible providers - `OpenAILlmProvider` defaults to OpenAI's endpoint and 401s.
- **SDK types leaking**: importing the provider SDK in any file other than `Providers/{X}LlmProvider.cs` breaks the abstraction.

## Related

- `.claude/rules/backend/ai-agent.md` - multi-provider architecture overview
- `docs/ai-dispatch.md` - agent architecture
