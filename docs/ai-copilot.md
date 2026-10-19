# AI Copilot

A conversational agent in the TMS portal chat drawer. Users ask questions and delegate work across
the whole TMS - loads, invoices, payments, expenses, customers, and dispatch - and the
copilot answers with real data or proposes actions for approval. Available on every plan from
Starter up (`TenantFeature.AICopilot`), drawing on the same weekly quota as dispatch.

It reuses the [AI dispatch](ai-dispatch.md) platform end to end: the same tool registry, the same
agent loop (`AgentLoopRunner`), the same decision/approval machinery, and the same weekly quota.

## How a turn works

```text
1. User sends a message (POST ai/copilot/conversations/{id}/messages → 202)
   - handler checks ownership and the concurrency guard, persists the user
     message, marks the conversation Running, enqueues a Hangfire job
2. AICopilotTurnJob re-checks the feature flag and runs AICopilotService.RunTurnAsync,
   a thin adapter onto AgentTurnService (the shared turn lifecycle every conversational
   agent runs through) with the CopilotAgentSurface
3. The turn creates an AgentSession (Type = Copilot) - quota, tokens, and
   decisions ride the existing session machinery
4. CopilotAgentSurface resolves the caller's permissions and AICopilotConversationBuilder
   rebuilds the LLM messages from the persisted transcript, scoping the tool catalogue to
   those permissions
5. AgentLoopRunner iterates: read tools execute; write tools always become Suggested
   decisions awaiting dispatcher/user approval - there is no unattended execution path
6. Every appended message (tool_use ids included) is persisted to
   agent_messages; progress streams over /hubs/copilot
```

## Conversations and transcript

Conversations and messages are kind-agnostic entities shared with Phase 3's dispatch surface, not
copilot-only types:

- `AgentConversation` (tenant DB, `agent_conversations`): owned by one user (`CreatedById` - every
  handler verifies the caller), with an `Idle`/`Running` status acting as a concurrency guard, and
  a `Kind` (`Copilot` / `Dispatch`). Every copilot command/query filters `Kind == Copilot` so a
  future dispatch conversation never leaks into this surface. A Running conversation older than 15
  minutes is assumed crashed and may be taken over.
- `AgentMessage` (`agent_messages`): sequenced rows storing the provider content blocks as JSON
  (`ContentJson`, including `tool_use` ids so the exact sequence replays) plus a `DisplayText` the
  UI renders. Tool-result rows have no `DisplayText` and never leave the server.
- Replay (`AgentTranscriptReplay.LoadAsync`, shared with the dispatch builder) loads its window in
  SQL - never through the `Messages` navigation, which would lazy-load the whole transcript. The
  window is about the last 40 rows; its start moves in steps of 20 so the replayed prefix (and the
  provider's prompt cache) holds still between steps, and it cuts only at a plain user chat
  message - starting mid-turn would orphan a tool_use/tool_result pair and the provider rejects
  the request.
- Replay is a pure function of the stored rows, so the bytes match turn to turn: each user message
  carries a `[Sent: ...]` stamp from its `CreatedAt` (the system prompts carry no date), and an
  earlier turn's tool result over 1500 characters replays as a short "call the tool again" stub.
- The copilot follows the same admin-set reasoning effort as the dispatch agent. Thinking blocks
  are replayed within a turn's tool loop (`LlmThinkingBlock`), but the persisted transcript drops
  them - prior turns replay fine without them, only in-turn replay is required.

## Permission scoping

Two layers, both driven by `AgentToolDefinition.RequiredPermission`:

1. **Catalogue filtering** - the turn resolves the calling user's permission set
   (`GetCurrentUserPermissionsQuery`) and drops any tool they cannot use, so the model never sees it.
2. **Execution guard** - the decision processor re-checks at call time; a denied tool records a
   Failed decision with `permission_denied` and never executes.

Approving a suggestion re-checks again: the approver needs `Permission.Copilot.Manage` for the
endpoint **and** the tool's own permission (e.g. `Permission.Invoice.Manage`).

The dispatch agent's catalogue is a separate axis: it keeps only tools declaring
`AgentSurfaces.Dispatch`, which is what keeps copilot-only write tools (invoicing) out of dispatch
conversations. It is deliberately **not** derived from `RequiredPermission` - a dispatch tool may
legitimately require a non-`Dispatch.*` permission, and coupling the two would hide it silently.

## Approvals

Write tools always create `Suggested` decisions rendered as action cards in the chat. Approval
(`POST ai/copilot/decisions/{id}/approve`) executes the tool and appends a System note
("Approved and executed: ...") to the transcript; rejection appends the reason. The next turn
replays those notes, so the model knows what actually happened. Decisions stay in the same audit
trail as dispatch (`agent_decisions`, linked to the turn's session).

## API

All endpoints require the `AICopilot` feature and `Permission.Copilot.View` (reads) or
`Permission.Copilot.Manage` (writes). Conversations are always scoped to the calling user.

```text
POST   /ai/copilot/conversations                    Create a conversation
GET    /ai/copilot/conversations                    List my conversations (paged)
GET    /ai/copilot/conversations/{id}               Detail: messages + decisions
POST   /ai/copilot/conversations/{id}/messages      Send a message (202; turn runs async)
POST   /ai/copilot/conversations/{id}/cancel        Cancel the running turn
DELETE /ai/copilot/conversations/{id}               Delete (cascades messages, sessions, decisions)
POST   /ai/copilot/decisions/{id}/approve           Approve a suggested action (executes it)
POST   /ai/copilot/decisions/{id}/reject            Reject (body carries the reason)
```

## SignalR

`CopilotHub` at `/hubs/copilot`. Unlike the dispatch hub it is `[Authorize]`d: user and tenant ids
come from JWT claims only (websockets pass the token as the standard `access_token` query
parameter), and each connection auto-joins its private `copilot:{tenantId}:{userId}` group. Events:
`ReceiveCopilotMessage`, `ReceiveCopilotDecision`, `ReceiveCopilotTurnUpdate`.

## Quota and cost

Each turn is one `AgentSession`, drawing on the same weekly budget as dispatch. Past the budget a
turn is stamped `IsOverage` and metered to Stripe by `AgentOverageReporter`; the composer notice
shows the accrued amount (`AIQuotaStatusDto.OverageChargesUsd`) and warns from 80%. With
`TenantSettings.BlockAIOverage` on, over-budget sends fail with `AI_BUDGET_REACHED` and the
composer disables. The model is the admin-managed global one dispatch uses.

## Adding a tool

Use the `add-dispatch-tool` skill - the registry is shared, so a new tool lands on the copilot,
the dispatch agent (if dispatch-scoped), and the MCP server in one change. The copilot's v1 tools:

| Tool                                    | Type  | Wraps                                                                 |
| --------------------------------------- | ----- | --------------------------------------------------------------------- |
| `search_loads` / `get_load`             | Read  | `GetLoadsQuery` / `GetLoadByIdQuery`                                  |
| `search_customers`                      | Read  | `GetCustomersQuery`                                                   |
| `get_invoices` / `get_invoice`          | Read  | `GetInvoicesQuery` / `GetInvoiceByIdQuery`                            |
| `search_expenses` / `get_expense_stats` | Read  | `GetExpensesQuery` / `GetExpenseStatsQuery`                           |
| `get_detention`                         | Read  | `GetLoadDetentionQuery`, or open stops across the fleet without a load |
| `get_condition_reports`                 | Read  | `GetConditionReportsQuery` (defects with car hauler BOL codes)        |
| `create_load_invoice`                   | Write | `CreateLoadInvoiceCommand` (unpaid, one line per load charge)         |
| `send_invoice`                          | Write | `SendInvoiceCommand` (payment link, or remit-to notice if factored)   |
| `create_payment_link`                   | Write | `CreatePaymentLinkCommand` (refused for factored customers)           |

Plus every dispatch tool, for users holding `Permission.Dispatch.*`.

## Related

- [AI Dispatch](ai-dispatch.md) - the shared platform: providers, model selection, pricing, policy learning
- [MCP Server](mcp-server.md) - the same tools exposed to Claude Desktop, Cursor, and other MCP clients
