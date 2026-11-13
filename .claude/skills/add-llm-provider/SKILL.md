---
name: add-llm-provider
description: Add a new LLM provider or model to the AI agents. Use when adding a model from an existing provider (e.g. a new Claude version) or wiring a new OpenAI-compatible or custom-SDK endpoint. Covers the catalog entry that drives pricing, quotas and the admin model list.
---

# Add an LLM provider or model

Background: `.claude/rules/backend/ai-agent.md`. The model is global and admin-selected; there is no per-tenant choice or tier.

| Adding | Steps |
| --- | --- |
| A model from an existing provider | 3 |
| An OpenAI-compatible endpoint (DeepSeek-style) | 1, 3 |
| A custom-SDK provider (Gemini-style) | 1, 2, 3 |

## 1. Enum and config

Add the value to `LlmProvider` (`src/Core/Logistics.Domain.Primitives/Enums/AI/Agents/LlmProvider.cs`). Config binds from `Llm:Providers:{Name}` (`ApiKey`, `Model`, `BaseUrl`) into `LlmOptions.Providers`; no new options class. The key comes from the env var `Llm__Providers__{Name}__ApiKey`, never a committed file. A compatible provider without `BaseUrl` hits OpenAI and 401s.

## 2. Custom SDK only

- `Llm/Providers/{Name}LlmProvider.cs` implementing `ILlmProvider`. SDK types stay inside this file; everything else uses `Llm/Contracts/`.
- Use the `HttpClient` the factory passes (the `llm` client). Your own client would pick up the host's 30s resilience timeout.
- Translate SDK exceptions to `LlmProviderException` and stop reasons to `LlmStopReason`.
- Add an arm to the `switch` in `LlmProviderFactory.Create`. Unlisted values fall through to `OpenAICompatibleLlmProvider`.

## 3. Catalog entry

Add the model to `LlmModelCatalog.Models` (`src/Core/Logistics.Application.Abstractions/AI/`), with prices in USD per million tokens (input, output, cache read, cache write) and the vendor's pricing page in the comment above the list:

```csharp
new("new-model-1", "New Model 1", LlmProvider.NewProvider, new(0.50m, 2.0m, 0.05m, 0.625m), ReasoningStyle.None),
```

This one entry feeds the admin AI Settings dropdown, model validation, budgets and overage billing. No frontend change.

Pick `ReasoningStyle` deliberately:

- `OpenAIEffort`: takes a reasoning effort, which OpenAI accepts with tools only on `/v1/responses`. Use only under `LlmProvider.OpenAI`; other endpoints 404 there.
- `AnthropicAdaptive`: adaptive thinking plus effort, no temperature; `None` effort is sent as `low`. Check `AnthropicLlmProvider.WebSearchFor`, since the newer search tool only works on some models.
- `None`: no reasoning parameter. Use it when unsure and for endpoints that reject one (DeepSeek).

The catalog id must equal the model id the provider sends, or cost silently falls back to Sonnet 5.5 rates. OpenAI-shaped providers fill `LlmTokenUsage` through `OpenAIUsage.From`.

## Verify

Select the model in the admin AI Settings page and run a dispatch session end to end.
