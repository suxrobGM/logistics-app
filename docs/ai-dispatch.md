# AI-Powered Dispatch

Autonomous and semi-autonomous load-to-truck dispatch with pluggable LLM providers. Available on the **Enterprise** plan.

## Overview

The agentic dispatcher analyzes your fleet state — unassigned loads, available trucks, driver HOS status, and load board opportunities — then optimizes assignments to maximize fleet utilization while ensuring compliance.

Supports multiple LLM providers out of the box:

| Provider      | Models                                 | Notes                                                  |
| ------------- | -------------------------------------- | ------------------------------------------------------ |
| **Anthropic** | Claude Sonnet 4.6, Haiku 4.5, Opus 4.6 | Default. Supports prompt caching and extended thinking |
| **OpenAI**    | GPT-5.4 Mini, GPT-5.4, GPT-5.4 Nano    | Via official OpenAI SDK                                |
| **DeepSeek**  | DeepSeek V4 Flash, DeepSeek V4 Pro     | OpenAI-compatible API                                  |

## Operating Modes

### Human-in-the-Loop (Default)

The agent analyzes the fleet and creates **suggestions** that dispatchers review in the TMS portal. Each suggestion includes the agent's reasoning. Dispatchers approve or reject individually or in bulk.

### Autonomous (Experimental)

The agent executes assignments immediately without human approval. Recommended only after validating the agent's suggestions in human-in-the-loop mode. Marked with an experimental badge in the UI.

## How It Works

```text
1. Gather fleet state (loads, trucks, drivers, HOS)
2. Send context + tools to LLM provider
3. Agent reasons about optimal assignments
4. Agent calls tools (assign load, create trip, etc.)
   - Human mode: tools create suggestions
   - Autonomous mode: tools execute immediately
5. Agent searches load boards for capacity gaps
6. Session completes with summary
```

## Agent Tools

| Tool                           | Type  | Description                                          |
| ------------------------------ | ----- | ---------------------------------------------------- |
| `get_unassigned_loads`         | Read  | All Draft loads not in any trip                      |
| `get_available_trucks`         | Read  | Available trucks with driver HOS data                |
| `get_driver_hos_status`        | Read  | Detailed HOS for a specific driver                   |
| `check_hos_feasibility`        | Read  | Can a driver complete a trip given HOS remaining?    |
| `batch_check_hos_feasibility`  | Read  | Batch HOS feasibility for multiple driver-load pairs |
| `calculate_distance`           | Read  | Driving distance between two points                  |
| `calculate_assignment_metrics` | Read  | Revenue/deadhead analysis for a potential assignment |
| `optimize_trip_stops`          | Read  | Optimize stop ordering for multi-load trips          |
| `search_loadboard`             | Read  | Search DAT/Truckstop/123Loadboard for opportunities  |
| `assign_load_to_truck`         | Write | Assign a load to a truck                             |
| `create_trip`                  | Write | Create a trip from assigned loads                    |
| `dispatch_trip`                | Write | Transition trip to Dispatched status                 |
| `book_loadboard_load`          | Write | Book a load from a load board                        |

Write tools create suggestions in Human-in-the-Loop mode and execute immediately in Autonomous mode.

## API Endpoints

All endpoints require `Permission.Dispatch.View` or `Permission.Dispatch.Manage` and the `AgenticDispatch` feature to be enabled.

```text
POST   /dispatch/agent/run                         Trigger on-demand agent run
POST   /dispatch/agent/cancel/{sessionId}           Cancel a running session
GET    /dispatch/agent/sessions                     List sessions (paged)
GET    /dispatch/agent/sessions/{sessionId}         Session detail with decisions
GET    /dispatch/agent/pending                      All pending decisions
POST   /dispatch/agent/decisions/{id}/approve       Approve a suggestion
POST   /dispatch/agent/decisions/{id}/reject        Reject a suggestion
```

## Audit Trail

Every agent run creates a **DispatchSession** with:

- Mode (HumanInTheLoop / Autonomous)
- Who triggered it (user or background job)
- Start/end timestamps
- Total tokens consumed and estimated cost (USD)
- Model used and provider
- Agent's summary

Each decision within a session is a **DispatchDecision** with:

- Tool called and input parameters
- Agent's reasoning
- Status (Suggested / Approved / Rejected / Executed / Failed)
- Related entity IDs (load, truck, trip)
- Who approved/rejected and when

## Configuration

### Environment Variables

| Variable                            | Description                                                                   |
| ----------------------------------- | ----------------------------------------------------------------------------- |
| `Llm__DefaultProvider`              | LLM provider: `Anthropic`, `OpenAi`, `DeepSeek`, `Glm` (default: `Anthropic`) |
| `Llm__Providers__Anthropic__ApiKey` | Anthropic API key                                                             |
| `Llm__Providers__OpenAi__ApiKey`    | OpenAI API key                                                                |
| `Llm__Providers__DeepSeek__ApiKey`  | DeepSeek API key                                                              |
| `Llm__MaxTokens`                    | Max tokens per response (default: 8192)                                       |

### appsettings.json

```json
{
  "Llm": {
    "DefaultProvider": "Anthropic",
    "MaxTokens": 8192,
    "Providers": {
      "Anthropic": {
        "ApiKey": "<key>",
        "Model": "claude-haiku-4-5"
      },
      "OpenAi": {
        "ApiKey": "<key>",
        "Model": "gpt-5.4-mini"
      },
      "DeepSeek": {
        "ApiKey": "<key>",
        "Model": "deepseek-chat",
        "BaseUrl": "https://api.deepseek.com/v1"
      }
    }
  }
}
```

### Per-Tenant Overrides

Admins can override the model per tenant via the Admin Portal → Tenant Edit → AI Dispatch Settings. The `TenantSettings.LlmModel` field accepts any model ID from the configured providers. Extended thinking can also be toggled per tenant.

### Feature Gating

The feature is gated behind `TenantFeature.AgenticDispatch`, available on the Enterprise plan only. Enable via the admin portal's feature management or by adding a `PlanFeature` entry.

## Architecture

```text
src/Infrastructure/Logistics.Infrastructure.AI/
├── Registrar.cs                         # DI registration
├── Options/
│   └── LlmOptions.cs                   # Multi-provider configuration
├── Providers/
│   ├── ILlmProvider.cs                  # Provider-agnostic interface
│   ├── LlmTypes.cs                      # Request/response/message types
│   ├── AnthropicLlmProvider.cs          # Anthropic SDK adapter
│   ├── OpenAiLlmProvider.cs             # OpenAI-compatible adapter
│   └── LlmProviderFactory.cs            # Resolves provider from config
├── Services/
│   ├── DispatchAgentService.cs          # Agent loop orchestration
│   ├── DispatchConversationBuilder.cs   # Builds provider-agnostic conversation
│   ├── DispatchDecisionProcessor.cs     # Tool call → decision entity processing
│   ├── DispatchToolExecutor.cs          # Maps tool calls to MediatR
│   ├── DispatchToolRegistry.cs          # Tool definitions (JSON Schema)
│   └── LlmPricing.cs                   # Token → USD cost calculator
├── Tools/                               # Individual IDispatchTool implementations
└── Prompts/
    └── DispatchSystemPrompt.cs          # Dynamic system prompt builder
```

The provider abstraction (`ILlmProvider`) keeps all SDK-specific code isolated. The agent loop, tools, and decision processor work exclusively with `LlmTypes` — provider-agnostic records for requests, responses, messages, and tool calls.

Tool definitions use JSON Schema, compatible with both Claude API tool schemas and OpenAI function calling.

## Adding a New Provider

1. **OpenAI-compatible** (most providers): Add a new `LlmProvider` enum value and configure with `BaseUrl` in appsettings
2. **Custom SDK**: Create a new `ILlmProvider` implementation, add a case in `LlmProviderFactory`
3. Add model pricing to `LlmPricing.cs`
4. Add model options to admin portal `tenant-edit.ts` → `llmModelOptions`

## Future Roadmap

- **Telegram Bot**: Driver and dispatcher interaction via Telegram (accept/reject loads, status updates, fleet summaries)
- **Learning from overrides**: Track dispatcher corrections to improve future suggestions

## Related

- [MCP Server](mcp-server.md) — connect Claude Desktop, Cursor, and other AI tools to your fleet using the same dispatch tools
