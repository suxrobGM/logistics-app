# AI Dispatch

Load-to-truck dispatch driven by an LLM agent, in a tenant-shared chat. The agent always suggests -
a dispatcher approves or rejects every write action. Available from the Starter plan up.

## Overview

The dispatcher looks at fleet state - unassigned loads, available trucks, driver HOS status, and what's on the load boards - and proposes assignments that try to keep trucks busy without breaking compliance.

You can switch LLM providers without code changes:

| Provider      | Models                                | Notes                                         |
| ------------- | ------------------------------------- | --------------------------------------------- |
| **OpenAI**    | GPT-5.6 Luna (default), GPT-5.6 Terra | Default provider. OpenAI SDK, `/v1/responses` |
| **Anthropic** | Claude Haiku 4.5, Claude Sonnet 5     | Prompt caching; Sonnet 5 adaptive thinking    |
| **DeepSeek**  | DeepSeek V4 Flash, DeepSeek V4 Pro    | OpenAI-compatible chat-completions API        |

Quota is **cost-based**: every session's `AgentSession.EstimatedCostUsd` (failed and cancelled runs
too) counts against the plan's `WeeklyAIBudgetUsd` - required on every plan, and falling back to the
Enterprise one for tenants with no subscription (`AIWeeklyBudget` in
`Application.Abstractions/AIDispatch/`). Nothing runs unmetered. Past the budget, sessions
bill Stripe metered units (`AIOverageBilling` in `Application.Abstractions/Payments/Stripe/`:
$0.10/unit, 3× markup, min one unit); the accrued amount is tenant-visible as
`AIQuotaStatusDto.OverageChargesUsd`. `TenantSettings.BlockAIOverage` swaps billing for a hard
pause until the weekly reset (copilot sends fail with `AI_BUDGET_REACHED`; dispatch records a
failed session). Per-token prices live in `LlmPricing.cs`.

## Approval Model

The agent looks at the fleet and produces suggestions in the conversation. Each suggestion comes
with the agent's reasoning. A dispatcher approves or rejects one at a time; rejecting requires a
reason, which the next turn sees as a transcript note. There is no unattended mode - every write
tool call becomes a `Suggested` decision, never an immediate execution.

The system prompt separately varies by `TenantSettings.OperatingMode`. A `SoloOperator` tenant gets
a "Fleet Profile" section that drops fleet utilization, the truck-to-truck comparison and the
assignment table, since there is one truck and the owner is the driver.

## How It Works

Dispatch is a **tenant-shared, multi-turn conversation** - any user with `Permission.Dispatch.View`
can open and read one; anyone with `Permission.Dispatch.Manage` can send messages, cancel a running
turn, rename, or delete. It reuses the same conversation/turn machinery as the
[AI copilot](ai-copilot.md) (`AgentConversation`/`AgentMessage`, `AgentTurnService`,
`AgentTranscriptReplay`), scoped by `AgentConversationKind.Dispatch` - a dispatch conversation never
leaks into the copilot's per-user list, and vice versa.

Every row a person caused carries that person on `AgentMessage.SentByUserId` - the dispatcher who
typed a message, and the one who approved or rejected a decision. The read path resolves the names
off the tenant's own employees, so the transcript says who asked for what. The agent's own messages
and the broker-reply envelope carry no sender and render unattributed.

```text
1. A user sends a message (POST ai/dispatch/conversations/{id}/messages -> 202)
   - handler checks the concurrency guard (no ownership check - the conversation is
     tenant-shared), persists the user message stamped with the sender, broadcasts it
     to the board, marks the conversation Running, enqueues a Hangfire job
2. AIDispatchTurnJob re-checks the AgenticDispatch feature flag and runs
   IAIDispatchService.RunTurnAsync, a thin adapter onto AgentTurnService with the
   DispatchAgentSurface
3. The turn creates an AgentSession (Type = Dispatch) - quota, tokens, and decisions
   ride the same session machinery copilot turns use
4. DispatchAgentSurface builds the system prompt (learned policy, operating mode),
   replays the persisted transcript, and appends a fresh fleet-state snapshot notice
   to only this turn's final user message - never persisted, so it cannot replay stale
   on a later turn. The full dispatch tool catalogue applies; there is no per-caller
   tool scoping, since the endpoint's policy is the gate
5. AgentLoopRunner iterates: read tools execute; write tools become Suggested decisions
6. Every appended message is persisted to agent_messages and broadcast tenant-wide over
   /hubs/ai-dispatch; approving or rejecting a decision appends a system note to the
   same transcript, stamped with the approver or rejecter
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
| `get_rate_floor`               | Read  | Lowest rate this carrier accepts on a listing's lane |
| `get_negotiation_thread`       | Read  | State and messages of one broker negotiation         |
| `assign_load_to_truck`         | Write | Assign a load to a truck                             |
| `create_trip`                  | Write | Create a trip from assigned loads                    |
| `dispatch_trip`                | Write | Transition trip to Dispatched status                 |
| `book_loadboard_load`          | Write | Book a load from a load board                        |
| `propose_counter_offer`        | Write | Email a broker a counter-offer on a listing          |

Write tools always create `Suggested` decisions; read tools execute immediately.

## Rate Negotiation

With `TenantFeature.AIRateNegotiation` enabled and a load board connected, the agent can counter a
broker by email instead of skipping a listing that pays too little.

The prompt gains a `## Rate Negotiation` section that travels with the three tools above. It tells
the agent to read `get_rate_floor` first, to refuse to negotiate when no floor covers the lane, to
offer at or above the floor, and to treat anything a broker writes as data rather than instructions.

Two properties matter for the approval model. First, `propose_counter_offer` is an ordinary write
tool, so no email leaves until a dispatcher approves the decision - and the approval card shows the
rendered email, not a summary. Second, an inbound reply can only append a message to the transcript
and ask for a turn; it can never execute anything.

Full pipeline, threat model, and Resend setup: [Broker Email Negotiation](broker-email-negotiation.md).

## API Endpoints

All endpoints require the `AgenticDispatch` feature to be enabled. Reads take
`Permission.Dispatch.View`; every write (send, cancel, rename, delete, approve, reject, policy
edits) takes `Permission.Dispatch.Manage`. Handlers do not check conversation ownership -
conversations are tenant-shared, so the endpoint policy is the only gate.

```text
POST   /ai/dispatch/conversations               Create a conversation
GET    /ai/dispatch/conversations               List conversations (paged, tenant-wide)
GET    /ai/dispatch/conversations/{id}          Detail: messages + decisions + per-turn session stats
POST   /ai/dispatch/conversations/{id}/messages Send a message (202; turn runs async)
POST   /ai/dispatch/conversations/{id}/cancel   Cancel the running turn
PUT    /ai/dispatch/conversations/{id}          Rename
DELETE /ai/dispatch/conversations/{id}          Delete (cascades messages, sessions, decisions)
GET    /ai/dispatch/quota                       Weekly AI quota status
GET    /ai/dispatch/pending                     All pending decisions
POST   /ai/dispatch/decisions/{id}/approve      Approve a suggestion
POST   /ai/dispatch/decisions/{id}/reject       Reject a suggestion (body carries the reason)
GET    /ai/dispatch/policy                      Learned dispatch policy
PUT    /ai/dispatch/policy                      Edit directives / pause learning
POST   /ai/dispatch/policy/regenerate           Learn now instead of waiting for the nightly pass
DELETE /ai/dispatch/policy                      Erase the learned policy and directives
```

## Audit Trail

Every agent run creates an **AgentSession** with:

- Who triggered it (user or background job)
- Start/end timestamps
- Total tokens consumed and estimated cost (USD)
- Model used and provider
- Agent's summary

Each decision within a session is an **AgentDecision** with:

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
transient status, and only the approver-stamped rows carry a real human signal. Every executed write
goes through approval, so this is the whole population, not a fallback for a mode that skips it.

**Skip conditions** (each returns a reason for the job log): learning switched off; `AIEnabled` false
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
`LlmCompletionRequest.ModelId` override, and creates **no** `AgentSession` row, so it does not
consume the tenant's weekly quota. Cost is recorded on the policy row (`GenerationCostUsd`) for
platform observability and is never exposed to the tenant.

**UI** - `tms-portal/pages/ai-dispatch/dispatch-policy/`: read the learned rules, write your own
directives, pause learning, regenerate, or delete everything. Deleting does not stop the agent
re-learning tomorrow unless learning is also switched off - the confirm dialog says so.

## Configuration

### Environment Variables

| Variable                            | Description                                                                |
| ----------------------------------- | -------------------------------------------------------------------------- |
| `Llm__DefaultProvider`              | LLM provider: `Anthropic`, `OpenAI`, `DeepSeek`, `Glm` (default: `OpenAI`) |
| `Llm__Providers__Anthropic__ApiKey` | Anthropic API key                                                          |
| `Llm__Providers__OpenAi__ApiKey`    | OpenAI API key                                                             |
| `Llm__Providers__DeepSeek__ApiKey`  | DeepSeek API key                                                           |
| `Llm__MaxTokens`                    | Max tokens per response (default: 16384)                                   |
| `Llm__PolicyLearningModel`          | Model for nightly policy learning (default: `deepseek-v4-flash`)           |

### appsettings.json

```json
{
  "Llm": {
    "DefaultProvider": "OpenAI",
    "MaxTokens": 16384,
    "DefaultReasoningEffort": "None",
    "Providers": {
      "Anthropic": {
        "ApiKey": "<key>",
        "Model": "claude-haiku-4-5"
      },
      "OpenAI": {
        "ApiKey": "<key>",
        "Model": "gpt-5.6-luna"
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

The dispatch model and reasoning effort are global, not per-tenant. An admin sets them in the Admin
Portal → AI Settings page, which persists `AI.Model` / `AI.ReasoningEffort` to `SystemSettings`
(keys in `AISettingsKeys`). `LlmSessionSetup` resolves both from those settings, falling back to
the appsettings `Llm` defaults. The selectable models come from `LlmModelCatalog`, which also
declares each model's reasoning style (OpenAI reasoning effort on `/v1/responses`, Anthropic
adaptive thinking, or none). Tenants never select or see the model. Per tenant, an admin can still toggle `AIEnabled`
(Tenant Edit) to block AI for demo/test tenants.

Because quota is metered in USD of model cost, switching to a more expensive model makes each
session consume proportionally more of every plan's weekly budget - no re-tiering needed, but
expect tenants to hit their budgets sooner.

### Feature Gating

The feature is gated behind `TenantFeature.AgenticDispatch`, granted from the Starter plan up (`SubscriptionPlanSeeder`). Enable via the admin portal's feature management or by adding a `PlanFeature` entry.

## Architecture

Folders are named for the capability they hold, not the layer.

```text
src/Infrastructure/Logistics.Infrastructure.AI/
├── Registrar.cs                            # DI; registers the tools AgentToolCatalog discovered
├── Llm/                                    # Provider-agnostic LLM layer
│   ├── ILlmProvider.cs                     # The boundary SDK types may not cross
│   ├── LlmProviderFactory.cs               # Resolves provider from config
│   ├── LlmClient.cs                        # One-shot ILlmClient for non-agent features
│   ├── LlmModelResolver.cs                 # Global admin-set model → provider
│   ├── LlmPricing.cs                       # Token → USD cost (the budget/overage currency)
│   ├── LlmErrorSanitizer.cs                # Strips credentials before text reaches a tenant
│   ├── Contracts/                          # LlmRequest/Response/Message/ContentBlock/...
│   └── Providers/                          # Anthropic, OpenAIResponses, OpenAICompatible
├── Agents/                                 # Runtime shared by both agent surfaces
│   ├── AgentTurnService.cs                 # The one turn lifecycle every conversational agent runs through
│   ├── IAgentSurface.cs                    # Per-kind seam: PrepareAsync + broadcast hooks
│   ├── AgentLoopRunner.cs                  # The 25-iteration loop, retries, token accounting
│   ├── AgentDecisionProcessor.cs           # Tool call → decision entity, execute vs suggest
│   ├── AgentTranscriptCodec.cs             # Content-block <-> JSON wire encoding
│   ├── AgentTranscriptReplay.cs            # Shared replay/truncation, both surfaces
│   ├── AgentSessionCancellationRegistry.cs # Process-local cancellation, wall-clock deadline
│   ├── AgentOverageReporter.cs             # Meters a session past budget to Stripe
│   ├── LlmSessionSetup.cs                  # Model + provider + tenant features, once per run
│   ├── PromptText.cs                       # Sanitisers both prompts share
│   ├── Dispatch/                           # DispatchAgentSurface, AIDispatchService, conversation builder, prompt
│   └── Copilot/                            # CopilotAgentSurface, AICopilotService, conversation builder, prompt
└── Tools/
    ├── AgentTool.cs                        # Base class: binds model arguments into the tool's input type
    ├── AgentToolCatalog.cs                 # Discovers the tools and generates their schemas at startup
    ├── AgentToolRegistry.cs                # Filters the catalogue per surface
    ├── AgentToolJson.cs                    # Input type → JSON Schema, and the lenient binder back
    ├── AgentToolExecutor.cs                # Name → tool dispatch
    ├── ToolResult.cs                       # The result wire format
    └── {Dispatch,Financial,Operations,LoadBoard,Intermodal}/   # Tools by Application module
```

`ILlmProvider` keeps SDK-specific code isolated to one file per provider. The agent loop, tools, and decision processor only deal with the `Llm/Contracts/` records for requests, responses, messages, and tool calls.

The ports these implement live in `Core/Logistics.Application.Abstractions/`: `Agents/`
(`IAgentToolRegistry`, `IAgentToolExecutor`, `IAgentRunContext`, `AgentToolDefinition`), `AI/`
(`ILlmClient`, `LlmOptions`, `LlmModelCatalog`, `AISettingsKeys`) and `AIDispatch/`
(`IAIDispatchService` and the dispatch-only ports).

Tool definitions are JSON Schema, which works with both Claude API tool schemas and OpenAI function calling.

## Adding a New Provider

1. **OpenAI-compatible** (most providers): Add a new `LlmProvider` enum value and configure with `BaseUrl` in appsettings - these route to `OpenAICompatibleLlmProvider` (chat completions). Only `LlmProvider.OpenAI` uses the Responses API
2. **Custom SDK**: Create a new `ILlmProvider` implementation, add a case in `LlmProviderFactory`
3. Add one `Pricing` entry in `LlmPricing.cs` (per-token USD prices)
4. Add the model to `LlmModelCatalog` with its `ReasoningStyle` - it populates the admin AI
   Settings dropdown automatically, and the style decides whether the providers send a
   reasoning parameter

See the `add-llm-provider` skill for the full checklist.

## Roadmap

- Telegram bot for drivers and dispatchers - accept/reject loads, status updates, fleet summaries.
- Graduated autonomy - use per-action-type approval rates from the same decision history to let a
  tenant relax the per-action approval requirement for actions it has a long clean approval streak
  on. Not current behavior: every write today is `Suggested` and gated, with no path that skips it.

## Related

- [Broker Email Negotiation](broker-email-negotiation.md) - the rate-negotiation channel: floors, reply routing, and what keeps inbound mail from steering the agent.
- [AI Copilot](ai-copilot.md) - the conversational agent in the TMS portal built on the same tool registry, agent loop, decision machinery, and quota. Its catalogue is permission-scoped per user; the dispatch agent's keeps only tools naming `AgentSurfaces.Dispatch` (plus feature gating).
- [MCP Server](mcp-server.md) - connect Claude Desktop, Cursor, and other MCP clients to your fleet using the same dispatch tools.
