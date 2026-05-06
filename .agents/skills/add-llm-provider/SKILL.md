---
name: add-llm-provider
description: Add a new LLM provider or model to the AI dispatch system. Use when adding a new model from an existing provider (e.g., a new Claude version) or wiring up a new OpenAI-compatible endpoint. Walks through the seven places that must change to keep pricing, quotas, and UI dropdowns in sync.
---

# Add an LLM Provider or Model

The AI dispatch agent supports multiple LLM providers via the `ILlmProvider` adapter pattern. New OpenAI-compatible providers (DeepSeek, GLM, etc.) reuse `OpenAiLlmProvider` with a custom `BaseUrl`. New non-compatible providers need a new `ILlmProvider` implementation.

## Decide the path

- **New OpenAI-compatible provider** (DeepSeek-style): no SDK code needed — just a new `LlmProvider` enum value and config. Skip step 3.
- **New custom-SDK provider** (e.g., Gemini, Mistral): create a new `ILlmProvider` implementation in step 3.
- **New model from an existing provider** (e.g., new Claude version): only steps 4–7 needed.

## Files that must change (full provider)

1. `src/Core/Logistics.Domain.Primitives/Enums/Llm/LlmProvider.cs` — add enum value
2. `src/Infrastructure/Logistics.Infrastructure.AI/Options/LlmOptions.cs` — provider config section
3. `src/Infrastructure/Logistics.Infrastructure.AI/Providers/{X}LlmProvider.cs` — only for non-OpenAI-compatible
4. `src/Infrastructure/Logistics.Infrastructure.AI/Providers/LlmProviderFactory.cs` — resolution case
5. `src/Infrastructure/Logistics.Infrastructure.AI/Services/LlmPricing.cs` — pricing, multiplier, tier, billing units
6. `src/Core/Logistics.Application/Commands/Tenant/UpdateTenantAiSettings/UpdateTenantAiSettingsHandler.cs` — `ModelTiers` dictionary
7. UI dropdowns:
   - `src/Client/Logistics.Angular/projects/admin-portal/src/app/pages/.../tenant-edit.ts` → `llmModelOptions`
   - `src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/settings/ai-settings/` (filtered by allowed tier)

## Step-by-step

### 1. Add enum value

```csharp
public enum LlmProvider
{
    Anthropic,
    OpenAi,
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

Then in `appsettings.json`:

```json
{
  "Llm": {
    "Providers": {
      "NewProvider": {
        "ApiKey": "...",
        "Model": "new-model-1",
        "BaseUrl": "https://api.newprovider.com/v1"
      }
    }
  }
}
```

API key passed via env var: `Llm__Providers__NewProvider__ApiKey`.

### 3. (Custom SDK only) Create `ILlmProvider` implementation

If the provider is OpenAI-compatible (most modern providers are), **skip this step** — `OpenAiLlmProvider` handles it via `BaseUrl`.

If it requires a custom SDK, add `Providers/NewLlmProvider.cs`:

```csharp
internal sealed class NewLlmProvider(IOptions<LlmProviderOptions> options) : ILlmProvider
{
    public async Task<LlmResponse> SendMessageAsync(LlmRequest request, CancellationToken ct)
    {
        // Translate LlmRequest → provider SDK request
        // Translate provider response → LlmResponse (LlmTypes only — no SDK types leak out)
    }
}
```

Provider-specific SDK types **must not leak** outside this class. The agent loop uses only `LlmTypes` (`LlmRequest`, `LlmResponse`, `LlmToolUseBlock`).

### 4. Resolve in `LlmProviderFactory`

```csharp
public ILlmProvider GetProvider(LlmProvider provider) => provider switch
{
    LlmProvider.Anthropic => anthropic,
    LlmProvider.OpenAi => openai,
    LlmProvider.DeepSeek => deepseek,        // OpenAI-compatible reuse
    LlmProvider.NewProvider => newProvider,  // ← here
    _ => throw new NotSupportedException($"Unknown provider: {provider}")
};
```

For OpenAI-compatible providers, instantiate `OpenAiLlmProvider` with the right `BaseUrl` from options.

### 5. Update `LlmPricing.cs`

**Four places** in this file. Miss any one and quota/billing breaks silently.

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
    "claude-opus-4-6" => 10, // ultra = 10x
    _ => 1
};

public static LlmModelTier GetModelTier(string model) => model switch
{
    "gpt-5.4" or "claude-sonnet-4-6" => LlmModelTier.Premium,
    "claude-opus-4-6" => LlmModelTier.Ultra,
    _ => LlmModelTier.Base // ← new-model-1 falls through to Base
};

public static int GetOverageBillingUnits(string model) => model switch
{
    "gpt-5.4" or "claude-sonnet-4-6" => 2,
    "claude-opus-4-6" => 4,
    _ => 1 // ← matches GetMultiplier mapping
};
```

Decide tier first (Base / Premium / Ultra), then multiplier (1 / 5 / 10), then billing units (1 / 2 / 4 at $0.20/unit).

### 6. Update `ModelTiers` dictionary

In `UpdateTenantAiSettingsHandler`:

```csharp
private static readonly Dictionary<string, LlmModelTier> ModelTiers = new()
{
    // existing entries
    ["new-model-1"] = LlmModelTier.Base,
};
```

This is what the tenant settings handler uses to validate that the user's plan tier allows the selected model. Mismatch with `GetModelTier` = silently broken validation.

### 7. Wire the UI dropdowns

**Admin portal** (`tenant-edit.ts`) — admin can set any model:

```typescript
const llmModelOptions = [
  // existing
  { label: "New Model 1", value: "new-model-1", provider: "NewProvider" },
];
```

**TMS portal AI Settings** — filter the same list by the tenant's `AllowedModelTier`. The page already does this filtering; just make sure the new model is in the source list.

## Verification checklist

- [ ] Enum value added
- [ ] Config section + appsettings entry + env var documented
- [ ] (If custom SDK) Provider implementation, no SDK types leak
- [ ] Factory resolves the new provider
- [ ] **All four `LlmPricing` switches/dictionaries updated** (Pricing, GetMultiplier, GetModelTier, GetOverageBillingUnits)
- [ ] `ModelTiers` dictionary in `UpdateTenantAiSettingsHandler` agrees with `GetModelTier`
- [ ] Admin portal dropdown updated
- [ ] TMS portal AI settings page shows the model (filtered correctly by plan tier)
- [ ] Test that selecting the model with an insufficient plan tier is rejected

## Common mistakes

- **`GetMultiplier` and `GetOverageBillingUnits` out of sync**: a Premium model with multiplier=5 but billing=1 underbills overages.
- **`ModelTiers` and `GetModelTier` out of sync**: handler accepts a model the system thinks is Base while the agent loads it as Premium.
- **Forgetting `BaseUrl`** for OpenAI-compatible providers — `OpenAiLlmProvider` defaults to OpenAI's endpoint and 401s.
- **SDK types leaking**: importing the provider SDK in any file other than `Providers/{X}LlmProvider.cs` breaks the abstraction.

## Related

- `.claude/rules/backend/ai-agent.md` — multi-provider architecture overview
- `docs/ai-dispatch.md` — agent architecture
