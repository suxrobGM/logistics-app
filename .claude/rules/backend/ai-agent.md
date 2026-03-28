# AI Dispatch Agent Conventions

## Project Structure

All AI agent code lives in `src/Infrastructure/Logistics.Infrastructure.AI/`:
- `Providers/` — `ILlmProvider` interface, `LlmTypes`, `AnthropicLlmProvider`, `OpenAiLlmProvider`, `LlmProviderFactory`
- `Services/` — Agent loop (`DispatchAgentService`), tool executor, tool registry, conversation builder, pricing
- `Tools/` — Individual tool implementations
- `Prompts/` — System prompt builders
- `Options/` — Configuration (`LlmOptions`, `LlmProviderOptions`, `LlmProviderType`)

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

## Adding a New LLM Provider

1. If OpenAI-compatible: just add a new entry to `LlmProviderType` enum and configure with `BaseUrl`
2. If custom SDK: create a new `ILlmProvider` implementation and add a case in `LlmProviderFactory`
3. Add pricing to `LlmPricing.cs`
4. Add model options to admin portal `tenant-edit.ts` → `llmModelOptions`
