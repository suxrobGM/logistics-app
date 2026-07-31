# AI Copilot

A conversational agent in the TMS portal chat drawer. Users ask questions and delegate work across
the whole TMS - loads, invoices, payments, expenses, maintenance, customers, and dispatch - and the
copilot answers with real data or proposes actions for approval. Available on every plan from
Starter up (`TenantFeature.AICopilot`), drawing on the same weekly quota as dispatch.

It reuses the [AI dispatch](ai-dispatch.md) platform end to end: the same tool registry, the same
agent loop (`AgentLoopRunner`), the same decision/approval machinery, and the same weekly quota.

## How a turn works

```text
1. User sends a message (POST ai/copilot/conversations/{id}/messages → 202)
   - handler checks ownership and the concurrency guard, persists the user
     message, marks the conversation Running, enqueues a Hangfire job
2. AICopilotTurnJob re-checks the feature flag and runs AICopilotService.RunTurnAsync
3. The turn creates an AgentSession (Type = Copilot) - quota, tokens, and
   decisions ride the existing session machinery
4. AICopilotConversationBuilder rebuilds the LLM messages from the persisted
   transcript and scopes the tool catalogue to the calling user's permissions
5. AgentLoopRunner iterates: read tools execute; write tools become Suggested
   decisions (always HumanInTheLoop - the copilot has no autonomous mode)
6. Every appended message (tool_use ids included) is persisted to
   ai_copilot_messages; progress streams over /hubs/copilot
```

## Conversations and transcript

- `AICopilotConversation` (tenant DB, `ai_copilot_conversations`): owned by one user
  (`CreatedById` - every handler verifies the caller), with an `Idle`/`Running` status acting as a
  concurrency guard. A Running conversation older than 15 minutes is assumed crashed and may be
  taken over.
- `AICopilotMessage` (`ai_copilot_messages`): sequenced rows storing the provider content blocks as
  JSON (`ContentJson`, including `tool_use` ids so the exact sequence replays) plus a `DisplayText`
  the UI renders. Tool-result rows have no `DisplayText` and never leave the server.
- Replay truncates to the last 30 rows, cutting only at a plain user chat message - starting
  mid-turn would orphan a tool_use/tool_result pair and the provider rejects the request.
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
`DispatchAgent: true`, which is what keeps copilot write tools (invoicing) out of autonomous
dispatch runs. It is deliberately **not** derived from `RequiredPermission` - a dispatch tool may
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

Each turn is one `AgentSession`, so copilot turns and dispatch runs draw on the same weekly plan
budget in USD of estimated model cost. Neither blocks at the budget: a turn started past it is
stamped `IsOverage` and metered to Stripe on completion by `AgentOverageReporter`. The composer
warns from 80% of the allowance. The model is the same admin-managed global one dispatch uses.

## Adding a tool

Use the `add-dispatch-tool` skill - the registry is shared, so a new tool lands on the copilot,
the dispatch agent (if dispatch-scoped), and the MCP server in one change. The copilot's v1 tools:

| Tool                                    | Type  | Wraps                                                                 |
| --------------------------------------- | ----- | --------------------------------------------------------------------- |
| `search_loads` / `get_load`             | Read  | `GetLoadsQuery` / `GetLoadByIdQuery`                                  |
| `search_customers`                      | Read  | `GetCustomersQuery`                                                   |
| `get_invoices` / `get_invoice`          | Read  | `GetInvoicesQuery` / `GetInvoiceByIdQuery`                            |
| `search_expenses` / `get_expense_stats` | Read  | `GetExpensesQuery` / `GetExpenseStatsQuery`                           |
| `get_upcoming_maintenance`              | Read  | `GetUpcomingMaintenanceQuery` (date-based only)                       |
| `create_load_invoice`                   | Write | `CreateLoadInvoiceCommand` (unpaid, amount defaults to delivery cost) |
| `send_invoice`                          | Write | `SendInvoiceCommand` (emails with a payment link)                     |
| `create_payment_link`                   | Write | `CreatePaymentLinkCommand`                                            |

Plus every dispatch tool, for users holding `Permission.Dispatch.*`.

## Related

- [AI Dispatch](ai-dispatch.md) - the shared platform: providers, model selection, pricing, policy learning
- [MCP Server](mcp-server.md) - the same tools exposed to Claude Desktop, Cursor, and other MCP clients
