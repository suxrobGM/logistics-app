# AI Dispatch

Load-to-truck dispatch driven by an LLM agent. Runs in two modes: human-in-the-loop or autonomous. Available on the Enterprise plan.

## Overview

The dispatcher looks at fleet state - unassigned loads, available trucks, driver HOS status, and what's on the load boards - and proposes assignments that try to keep trucks busy without breaking compliance.

You can switch LLM providers without code changes:

| Provider      | Models                                 | Notes                                                  |
| ------------- | -------------------------------------- | ------------------------------------------------------ |
| **Anthropic** | Claude Sonnet 4.6, Haiku 4.5, Opus 4.8 | Default. Supports prompt caching and extended thinking |
| **OpenAI**    | GPT-5.4 Mini, GPT-5.4, GPT-5.4 Nano    | Via official OpenAI SDK                                |
| **DeepSeek**  | DeepSeek V4 Flash, DeepSeek V4 Pro     | OpenAI-compatible API                                  |

Quota cost is multiplier-based: **DeepSeek V4 Flash and Pro, GPT-5.4 Mini, and Claude Haiku 4.5 are all 1×**; GPT-5.4 and Claude Sonnet 4.6 are 5×; Claude Opus 4.8 is 10×.

## Operating Modes

### Human-in-the-Loop (default)

The agent looks at the fleet and produces suggestions a dispatcher reviews in the TMS portal. Each suggestion comes with the agent's reasoning. Dispatchers approve or reject one at a time, or in bulk.

### Autonomous (experimental)

The agent assigns loads on its own, no human approval. I'd only flip this on after running in human-in-the-loop for a while and trusting what the agent picks. The UI flags it as experimental.

Separately from those two, the system prompt varies by `TenantSettings.OperatingMode`. A `SoloOperator` tenant gets a "Fleet Profile" section that drops fleet utilization, the truck-to-truck comparison and the assignment table, since there is one truck and the owner is the driver.

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
| `get_container_status`         | Read  | ISO 6346 lookup: status, terminal, seal, B/L, load   |
| `get_terminal_info`            | Read  | UN/LOCODE lookup: name, type, country, address       |
| `search_loadboard`             | Read  | Search DAT/Truckstop/123Loadboard for opportunities  |
| `assign_load_to_truck`         | Write | Assign a load to a truck                             |
| `create_trip`                  | Write | Create a trip from assigned loads                    |
| `dispatch_trip`                | Write | Transition trip to Dispatched status                 |
| `book_loadboard_load`          | Write | Book a load from a load board                        |

Write tools create suggestions in Human-in-the-Loop mode and execute immediately in Autonomous mode.

## API Endpoints

All endpoints require `Permission.Dispatch.View` or `Permission.Dispatch.Manage` and the `AgenticDispatch` feature to be enabled.

```text
POST   /ai/dispatch/run                         Trigger on-demand agent run
POST   /ai/dispatch/cancel/{sessionId}           Cancel a running session
GET    /ai/dispatch/sessions                     List sessions (paged)
GET    /ai/dispatch/sessions/{sessionId}         Session detail with decisions
GET    /ai/dispatch/pending                      All pending decisions
POST   /ai/dispatch/decisions/{id}/approve       Approve a suggestion
POST   /ai/dispatch/decisions/{id}/reject        Reject a suggestion (body carries the reason)
GET    /ai/dispatch/policy                       Learned dispatch policy
PUT    /ai/dispatch/policy                       Edit directives / pause learning
POST   /ai/dispatch/policy/regenerate            Learn now instead of waiting for the nightly pass
DELETE /ai/dispatch/policy                       Erase the learned policy and directives
```

## Audit Trail

Every agent run creates a **AIDispatchSession** with:

- Mode (HumanInTheLoop / Autonomous)
- Who triggered it (user or background job)
- Start/end timestamps
- Total tokens consumed and estimated cost (USD)
- Model used and provider
- Agent's summary

Each decision within a session is a **AIDispatchDecision** with:

- Tool called and input parameters
- Agent's reasoning
- Status (Suggested / Approved / Rejected / Executed / Failed)
- Related entity IDs (load, truck, trip)
- Who approved/rejected and when

The rejection **reason** is required in the UI, not optional: it is the labelled signal the nightly
learning pass below uses. A bare rejection teaches the agent nothing.

## Learning From Approvals & Rejections

A nightly Hangfire job (`AIDispatchPolicyLearningJob`, `Cron.Daily(4)`) turns each tenant's
approve/reject history into a short markdown **dispatch policy** that is injected into the agent's
system prompt, so the agent stops re-suggesting patterns the dispatcher keeps refusing.

**Storage** - one `AIDispatchPolicy` row per tenant (`ai_dispatch_policies`), with two content columns:

| Column             | Owner      | Behaviour                                              |
| ------------------ | ---------- | ------------------------------------------------------ |
| `GeneratedContent` | The job    | Overwritten on every successful pass                   |
| `ManualContent`    | Dispatcher | Never touched by the job, and outranks learned content |

The split is what lets the policy keep regenerating without either clobbering dispatcher text or
freezing the moment someone edits it.

**What it reads** (`AIDispatchPolicyLearner`, in `Logistics.Application`): non-`Query` decisions from
the last 60 days that are either `Rejected` or `Executed` **with a non-null `ApprovedByUserId`**. That
last condition matters - `Approve()` is immediately overwritten by `MarkExecuted()`, so `Approved` is a
transient status, and autonomous-mode executions have no approver and no human signal. Counting them
would train the agent on its own output.

**Skip conditions** (each returns a reason for the job log): learning switched off; `LlmEnabled` false
for the tenant; fewer than 15 qualifying decisions or fewer than 3 rejections; no new decisions since
the `LastDecisionAt` watermark; a manual regenerate within 10 minutes of the last run. `force: true`
(the regenerate endpoint) bypasses only the last two. The watermark is what makes the job's
`[AutomaticRetry]` safe and a quiet night free - and it is deliberately **not** advanced when the LLM
call fails, so the next run retries the same history.

**Bounding** - the prompt asks for ≤ 8 bullets / ≤ 400 words, `MaxTokens` is 1024, and the stored text
is capped at 4000 chars. `AIDispatchSystemPrompt` caps it again at the injection point, keeping whole
lines only: a half-truncated rule reads as a different rule, so a line that will not fit is dropped.

**Where it lands in the prompt** - between `## HOS Rules` and `## Workflow`. Position carries authority,
so the hard constraints are read first and the workflow steps then act on both. The section states that
preferences are STRONG DEFAULTS ranking _below_ hard constraints and below this run's instructions. The
text is treated as untrusted (control characters stripped): it is LLM output derived from
dispatcher-typed rejection reasons, which is a prompt-injection path.

**Cost** - learning runs on `Llm:PolicyLearningModel` (default `deepseek-v4-flash`) via the optional
`LlmCompletionRequest.ModelId` override, and creates **no** `AIDispatchSession` row, so it does not
consume the tenant's weekly quota. Cost is recorded on the policy row (`GenerationCostUsd`) for
platform observability and is never exposed to the tenant.

**UI** - `tms-portal/pages/ai-dispatch/dispatch-policy/`: read the learned rules, write your own
directives, pause learning, regenerate, or delete everything. Deleting does not stop the agent
re-learning tomorrow unless learning is also switched off - the confirm dialog says so.

## Configuration

### Environment Variables

| Variable                            | Description                                                                   |
| ----------------------------------- | ----------------------------------------------------------------------------- |
| `Llm__DefaultProvider`              | LLM provider: `Anthropic`, `OpenAI`, `DeepSeek`, `Glm` (default: `Anthropic`) |
| `Llm__Providers__Anthropic__ApiKey` | Anthropic API key                                                             |
| `Llm__Providers__OpenAi__ApiKey`    | OpenAI API key                                                                |
| `Llm__Providers__DeepSeek__ApiKey`  | DeepSeek API key                                                              |
| `Llm__MaxTokens`                    | Max tokens per response (default: 16384)                                      |
| `Llm__PolicyLearningModel`          | Model for nightly policy learning (default: `deepseek-v4-flash`)              |

### appsettings.json

```json
{
  "Llm": {
    "DefaultProvider": "Anthropic",
    "MaxTokens": 16384,
    "ThinkingBudgetTokens": 16384,
    "Providers": {
      "Anthropic": {
        "ApiKey": "<key>",
        "Model": "claude-haiku-4-5"
      },
      "OpenAI": {
        "ApiKey": "<key>",
        "Model": "gpt-5.4-mini"
      },
      "DeepSeek": {
        "ApiKey": "<key>",
        "Model": "deepseek-v4-flash",
        "BaseUrl": "https://api.deepseek.com/v1"
      }
    }
  }
}
```

### Global model (admin-managed)

The dispatch model is global, not per-tenant. An admin sets it in the Admin Portal → AI Settings page,
which persists `AI.Model` / `AI.Provider` / `AI.ExtendedThinking` to `SystemSettings` (keys in
`AISettingsKeys`). `AIDispatchConversationBuilder` resolves the model from those settings, falling back to
the appsettings `Llm` defaults. The selectable models come from `LlmModelCatalog`. Tenants never select or
see the model. Per tenant, an admin can still toggle `LlmEnabled` (Tenant Edit) to block AI for demo/test
tenants.

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
│   ├── OpenAILlmProvider.cs             # OpenAI-compatible adapter
│   └── LlmProviderFactory.cs            # Resolves provider from config
├── Services/
│   ├── AIDispatchService.cs          # Agent loop orchestration
│   ├── AIDispatchConversationBuilder.cs   # Builds provider-agnostic conversation
│   ├── AIDispatchDecisionProcessor.cs     # Tool call → decision entity processing
│   ├── AIDispatchToolExecutor.cs          # Maps tool calls to MediatR
│   ├── AIDispatchToolRegistry.cs          # Tool definitions (JSON Schema)
│   └── LlmPricing.cs                   # Token → USD cost calculator
├── Tools/                               # Individual IAIDispatchTool implementations
└── Prompts/
    └── AIDispatchSystemPrompt.cs          # Dynamic system prompt builder
```

`ILlmProvider` keeps SDK-specific code isolated to one file per provider. The agent loop, tools, and decision processor only deal with `LlmTypes`, the provider-agnostic records for requests, responses, messages, and tool calls.

Tool definitions are JSON Schema, which works with both Claude API tool schemas and OpenAI function calling.

## Adding a New Provider

1. **OpenAI-compatible** (most providers): Add a new `LlmProvider` enum value and configure with `BaseUrl` in appsettings
2. **Custom SDK**: Create a new `ILlmProvider` implementation, add a case in `LlmProviderFactory`
3. Add model pricing to `LlmPricing.cs` (Pricing, GetMultiplier, GetOverageBillingUnits)
4. Add the model to `LlmModelCatalog` - it populates the admin AI Settings dropdown automatically

See the `add-llm-provider` skill for the full checklist.

## Roadmap

- Telegram bot for drivers and dispatchers - accept/reject loads, status updates, fleet summaries.
- Graduated autonomy - use per-action-type approval rates from the same decision history to widen what the agent may execute unattended.

## Related

- [AI Copilot](ai-copilot.md) - the conversational agent in the TMS portal built on the same tool registry, agent loop, decision machinery, and quota. Its catalogue is permission-scoped per user; the dispatch agent's is scoped to `Permission.Dispatch.*`.
- [MCP Server](mcp-server.md) - connect Claude Desktop, Cursor, and other MCP clients to your fleet using the same dispatch tools.
