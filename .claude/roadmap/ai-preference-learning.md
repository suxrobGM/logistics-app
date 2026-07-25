# AI Preference Learning (learn from approve/reject)

- **Status**: Done
- **Priority**: P1 - top-pick differentiator: labeled training data already accumulates in every tenant DB and is currently ignored
- **Effort**: M
- **Category**: AI differentiation

## Why

Every `AiDispatchDecision` carries an approve/reject outcome (+ rejection reason), yet session 100
is no smarter than session 1. Distilling this history into per-tenant dispatch policy makes the agent
demonstrably improve weekly - no TMS competitor has a learning dispatcher, and the accumulated policy
becomes switching-cost moat.

## What to build

- Nightly Hangfire job (`Jobs/`, mirror `PayrollGenerationJob`) that runs an LLM learning pass over the last N decisions + outcomes + rejection reasons → a short "dispatch policy" markdown doc stored per tenant (new entity, e.g. `Entities/AiDispatch/AiDispatchPolicy.cs`).
- `AiDispatchConversationBuilder` injects the current policy doc into the system prompt (bounded, e.g. ≤ 1k tokens; keep `AiDispatchSystemPrompt.Build` clean by passing it as a parameter).
- Policy page in `tms-portal/pages/ai-dispatch/`: dispatcher can read, edit, or delete learned rules (transparency = trust; also GDPR-friendly).
- Learning cost counts against tenant quota via existing `LlmPricing`/`AiQuotaService` or runs on the cheapest model tier.
- Feeds [ai-graduated-autonomy](ai-graduated-autonomy.md): approval-rate stats per action type come from the same decision history.

## Acceptance

After a week of consistent rejections for a pattern (e.g. deadhead > 80 mi), new sessions stop suggesting that pattern and the policy page shows the learned rule in plain language.

## Notes

**2026-07-25 - shipped.** Named "policy learning" throughout, not "flywheel"/"distillation" - the
user-facing surface is **Dispatch Policy**. Decisions worth remembering:

- **Prerequisite found during planning:** `RejectionReason` was null in every tenant DB. Both reject
  buttons sent `body: {}`, and `ReplanAiDispatchSessionHandler` had been printing "no reason given"
  for every rejection. Shipped `reject-decision-dialog` (required reason + quick picks) first; without
  it there is no labelled signal and the acceptance criterion is unreachable.
- **`Approved` is a transient status.** `Approve()` is immediately overwritten by `MarkExecuted()`, so
  the human-approved population is `Executed && ApprovedByUserId != null`. Autonomous executions have
  no approver - including them would train the agent on its own output.
- **Two content columns, not a manual-edit flag.** `GeneratedContent` (job-owned) +
  `ManualContent` (dispatcher-owned). A single column plus a flag forces a choice between clobbering
  dispatcher text and freezing the loop on first edit.
- **Off-quota, not quota-counted.** No `AiDispatchSession` row is created, so `AiQuotaService` never
  sees it. Runs on `Llm:PolicyLearningModel` (`deepseek-v4-flash`) via the new optional
  `LlmCompletionRequest.ModelId`, which falls back to the global model when the id is unknown _or its
  provider has no API key_ - that second condition matters on single-provider installs.
- **Daily 04:00 UTC, not weekly.** A `LastDecisionAt` watermark makes a quiet night free, and daily
  means a new preference reaches the agent within 24h of the rejection that taught it. The watermark is
  deliberately not advanced on LLM failure.
- **Prompt placement is between `## HOS Rules` and `## Workflow`.** Position carries authority. The
  section says preferences are STRONG DEFAULTS ranking below hard constraints, and the text is
  sanitised as untrusted - it is LLM output derived from dispatcher-typed reasons.
- **Truncation keeps whole lines only.** A half-truncated rule reads as a different rule, so a line
  that will not fit is dropped (in both the learner and the prompt builder).
- Deferred: `ui-tabs` edit/preview split (a single Preview toggle covers it) and nav sub-items (the
  page is reached from a header button, which cannot disturb the pending-decisions badge).
