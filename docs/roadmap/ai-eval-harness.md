# AI Eval Harness

- **Status**: Planned
- **Priority**: P1 - the admin can swap the global model live with zero measurement; also a marketing weapon ("9X% agreement with senior dispatchers")
- **Effort**: M
- **Category**: AI differentiation / quality infrastructure

## Why

The global dispatch model is set in the admin portal (`SystemSettings["AI.Model"]`) and applies
fleet-wide instantly. Today "did DeepSeek get worse than Sonnet?" is unanswerable. Recorded real
sessions + dispatcher outcomes are ground truth we already store.

## What to build

- Session recorder: persist full tool-call transcripts (inputs/outputs per iteration) for replay - extend `AIDispatchSession` or a sidecar entity; scrub PII before storing as fixtures.
- Replay runner: feed a recorded initial state (unassigned loads, available trucks, HOS data) to a candidate model via mocked tool responses; compare produced assignments against the dispatcher-approved outcome. Lives in `tests/Logistics.Infrastructure.AI.Tests/` as an opt-in suite + a CLI/admin trigger.
- Scoring: assignment agreement rate, HOS-violation count (hard fail), tokens + cost per session via `LlmPricing`.
- Admin portal report: per-model scorecard next to the model picker in `admin-portal/pages/ai-settings/` - make the dropdown an informed decision.
- Gate: block (or warn on) switching the global model to one scoring below threshold on the current fixture set.

## Acceptance

Running the eval suite against two models produces a comparable scorecard (agreement %, violations, cost/session); the admin model page shows the latest scores.

## Notes

_(add dated implementation notes here)_
